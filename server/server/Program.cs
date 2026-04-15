using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.IdentityModel.Tokens;
using Serilog;
using server.BackgroundWorkers;
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
using System.Text;

namespace server
{
    public static class Program
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

            builder.Services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
                options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
            })
            .AddJwtBearer(options =>
            {
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"])),
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ClockSkew = TimeSpan.Zero
                };

                options.Events = new JwtBearerEvents
                {
                    OnAuthenticationFailed = context =>
                    {
                        Console.WriteLine("Authentication failed: " + context.Exception.Message);
                        return Task.CompletedTask;
                    },
                    OnTokenValidated = context =>
                    {
                        Console.WriteLine("Token validated for: " + context.Principal.Identity.Name);
                        return Task.CompletedTask;
                    }
                };
            });

            builder.Services.AddAuthorization();

            builder.Services.AddAutoMapper(typeof(Program));

            

            // Scoped Services for DB operations
            builder.Services.AddScoped<IUserRepository, UserRepository>();
            builder.Services.AddScoped<ITokenRepository, TokenRepository>();
            builder.Services.AddScoped<IGameRoomRepository, GameRoomRepository>();
            builder.Services.AddScoped<ISessionRepository, SessionRepository>();
            builder.Services.AddScoped<IStatisticRepository, StatisticRepository>();
            builder.Services.AddScoped<IBiofeedbackRepository, BiofeedbackRepository>();

            builder.Services.AddScoped<IUserService, UserServices>();
            builder.Services.AddScoped<IAuthService, AuthService>();
            builder.Services.AddScoped<IGameRoomService, GameRoomService>();
            builder.Services.AddScoped<IStatisticService, StatisticService>();
            builder.Services.AddScoped<IJwtUtils, JwtUtils>();

            builder.Services.AddSingleton<BallanceGameService>();
            builder.Services.AddSingleton<EnergyBattleGameServices>();
            builder.Services.AddSingleton<BalloonGameService>();

            builder.Services.AddScoped<FileHelper>();
            builder.Services.AddSingleton<DbWriteQueue>();
            builder.Services.AddHostedService<DbWriterWorker>();

            // Singleton Service
            builder.Services.AddSingleton<GameManager>();

            // Cors policy
            builder.Services.AddCors(options =>
            {
            options.AddPolicy("AllowAll",
                policy => policy
                    .AllowAnyHeader()
                    .AllowAnyMethod()
                    .WithOrigins(
                        "http://localhost:5173",
                        "http://localhost:3000",
                        "http://localhost:5217",
                        "http://localhost",
                        "http://localhost:80",
                        "https://localhost:443", 
                        "https://localhost"
                    )
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
            app.UseAuthentication();
            app.UseAuthorization();

            //SignalR endpoint
            app.MapHub<GameHub>("api/gamehub");
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
                
                dbContext.Database.Migrate();

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
