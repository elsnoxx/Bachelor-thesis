using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Serilog;
using server.Data;
using server.Helpers;
using server.Hubs;
using server.Models.DB;
using server.Repositories;
using server.Repositories.Interfaces;
using server.Services.DbServices;
using server.Services.DbServices.Interfaces;
using server.Services.GameServices;
using server.Services.Utils;
using server.Services.Utils.Interfaces;

namespace server
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Logs setting
            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Debug()
                .WriteTo.Console()
                .CreateLogger();

            // Add services to the container.

            builder.Services.AddControllers();
            // Include SignalR service
            builder.Services.AddSignalR();

            // Health checks
            builder.Services.AddHealthChecks()
                    .AddNpgSql(builder.Configuration.GetConnectionString("DefaultConnection"),
                               name: "PostgreSQL",
                               failureStatus: HealthStatus.Unhealthy,
                               tags: new[] { "db", "postgres" });

            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // Database connection
            builder.Services.AddDbContext<AppDbContext>(options =>
                    options.UseNpgsql(builder.Configuration.GetConnectionString("DefaultConnection"))
                           .EnableSensitiveDataLogging()
                           .LogTo(Console.WriteLine, LogLevel.Information));

            // In your Program.cs, configure JSON serialization to handle cycles:
            //builder.Services.AddControllers()
            //    .AddJsonOptions(options =>
            //    {
            //        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.Preserve;
            //        options.JsonSerializerOptions.MaxDepth = 64; // Optional: increase max depth if needed
            //    });

            builder.Services.AddAutoMapper(typeof(Program));

            // Singleton Service
            builder.Services.AddSingleton<GameManager>();

            // Scoped Services for DB operations
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<IGameRoomRepository, GameRoomRepository>();
            builder.Services.AddScoped<ISesionRepository, SesionRepository>();

            builder.Services.AddScoped<IUserDbServices, UserDbServices>();
            builder.Services.AddScoped<IAuthDbService, AuthDbService>();
            builder.Services.AddScoped<IGameRoomService, GameRoomService>();
            builder.Services.AddScoped<IJwtUtils, JwtUtils>();

            builder.Services.AddScoped<FileHelper>();


            // Cors policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:5173", "https://localhost:443", "https://localhost")
                        .AllowCredentials());
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseCors("AllowAll");

            app.UseHttpsRedirection();

            app.UseAuthorization();

            //SignalR endpoint
            app.MapHub<GameHub>("/gamehub");
            // Health check endpoint
            app.MapHealthChecks("/health", new HealthCheckOptions
            {
                ResponseWriter = async (context, report) =>
                {
                    context.Response.ContentType = "application/json";
                    var json = System.Text.Json.JsonSerializer.Serialize(new
                    {
                        status = report.Status.ToString(),
                        checks = report.Entries.Select(e => new {
                            name = e.Key,
                            status = e.Value.Status.ToString(),
                            description = e.Value.Description
                        }),
                        duration = report.TotalDuration.TotalMilliseconds
                    });
                    await context.Response.WriteAsync(json);
                }
            });


            app.MapControllers();

            DBMigrate(app);

            app.Run();
        }

        public static void DBMigrate(IHost app)
        {
            using (var scope = app.Services.CreateScope())
            {
                var dbContext = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                dbContext.Database.EnsureCreated();

                if (!dbContext.Users.Any())
                {
                    dbContext.Users.Add(new User
                    {
                        Id = Guid.NewGuid(),
                        Username = "admin",
                        Email = "admin@example.com",
                        PasswordHash =  BCrypt.Net.BCrypt.HashPassword("admin"),
                    });

                    dbContext.SaveChangesAsync();
                }
            }

        }
    }
}
