using iron_revolution_center_api.Data.Interface;
using iron_revolution_center_api.Data.Interfaces;
using iron_revolution_center_api.Data.Service;
using iron_revolution_center_api.Data.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http.Features;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;
using System.Net;
using System.Text;

namespace iron_revolution_center_api
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            #region JWT Configuration
            var jwtSettings = builder.Configuration.GetSection("JwtSettings");
            var secretKey = Encoding.UTF8.GetBytes(jwtSettings["SecretKey"]);

            builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
                .AddJwtBearer(options =>
                {
                    options.TokenValidationParameters = new TokenValidationParameters
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateLifetime = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = jwtSettings["Issuer"],
                        ValidAudience = jwtSettings["Audience"],
                        IssuerSigningKey = new SymmetricSecurityKey(secretKey)
                    };
                });

            builder.Services.AddAuthorization();
            #endregion

            builder.Services.AddControllers();
            // Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
            builder.Services.AddEndpointsApiExplorer();
            builder.Services.AddSwaggerGen();

            // connection
            var connectionString = builder.Configuration["CONNECTION_STRING"];
            builder.Services.AddSingleton<IMongoClient>(sp => new MongoClient(connectionString));
            builder.Services.AddScoped<IMongoDatabase>(sp =>
            {
                var client = sp.GetRequiredService<IMongoClient>();
                return client.GetDatabase("iron-revolution-center-db");
            });

            // services
            builder.Services.AddScoped<iBranchesService, BranchesService>(); // branches office
            builder.Services.AddScoped<iClientsService, ClientsService>(); // clients
            builder.Services.AddScoped<iExercisesService, ExercisesService>(); // exerecises
            builder.Services.AddScoped<iMembershipsService, MembershipsService>(); // memberships
            builder.Services.AddScoped<iRolesInterface, RolesService>(); // roles
            builder.Services.AddScoped<iEmployeesService, EmployeesService>(); // staff
            builder.Services.AddScoped<iUsersService, UsersService>(); // users
            builder.Services.AddScoped<iActivity_CenterService, Activity_CenterService>(); // activity center 
            builder.Services.AddScoped<iStatisticsService, StatisticsService>();

            builder.Services.AddCors(options =>
            {
                options.AddPolicy("AllowFrontend", policy =>
                {
                    policy.WithOrigins("http://localhost:4200", "https://iron-revolution-center-api.onrender.com")
                        .AllowAnyHeader()
                        .AllowAnyMethod();
                });
            });

            var app = builder.Build();

            // Configure the HTTP request pipeline.
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }
            else
            {
                app.UseHttpsRedirection();
            }

            app.UseCors("AllowFrontend");

            app.UseHttpsRedirection();

            app.UseAuthentication();

            app.UseAuthorization();

            app.MapControllers();

            app.Run();
        }
    }
}
