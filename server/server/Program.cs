using Microsoft.EntityFrameworkCore;
using Serilog;
using server.Data;
using server.Hubs;
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

            // Cors policy
            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowAll",
                    policy => policy
                        .AllowAnyHeader()
                        .AllowAnyMethod()
                        .WithOrigins("http://localhost:5173")
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


            app.MapHub<GameHub>("/gamehub");


            app.MapControllers();

            app.Run();
        }
    }
}
