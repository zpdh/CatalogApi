
using CatalogApi.Data;
using CatalogApi.DataTransferObjects;
using CatalogApi.Extensions;
using CatalogApi.Filters;
using CatalogApi.Logging;
using CatalogApi.Repositories;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using CatalogApi.Models;
using CatalogApi.Services;

namespace WebApplication1;

public class Program
{
    public static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers(options => options.Filters.Add(typeof(ApiExceptionFilter)))
        .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
        .AddNewtonsoftJson();

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen();

        builder.Services.AddAuthorization();
        builder.Services.AddAuthentication("Bearer").AddJwtBearer();

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<DataContext>()
               .AddDefaultTokenProviders();

        string? mySqlConnection =
        builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<DataContext>(options =>
        options.UseMySql(mySqlConnection,
        ServerVersion.AutoDetect(mySqlConnection)));

        //JWT Bearer authentication stuff
        var secretKey = builder.Configuration["JWT:SecretKey"] ?? throw new ArgumentException("Invalid secret key");
        builder.Services.AddAuthentication(options =>
        {
            options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
            options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
        }).AddJwtBearer(options =>
        {
            options.SaveToken = true;
            options.RequireHttpsMetadata = false;
            options.TokenValidationParameters = new TokenValidationParameters()
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = true,
                ValidateIssuerSigningKey = true,
                ClockSkew = TimeSpan.Zero,
                ValidAudience = builder.Configuration["JWT:ValidAudience"],
                ValidIssuer = builder.Configuration["JWT:ValidIssuer"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
            };
        });


        builder.Logging.AddProvider(new CustomerLoggerProvider(new CustomerLoggerProviderConfiguration()
        {
            LogLevel = LogLevel.Information
        }));

        builder.Services.AddScoped<ApiLoggingFilter>();
        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped(typeof(ICrudRepository<>), typeof(CrudRepository<>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
        builder.Services.AddScoped<ITokenService, TokenService>();

        builder.Services.AddAutoMapper(typeof(ProductDTOMappingProfile));

        var app = builder.Build();

        // Configure the HTTP request pipeline.
        if (app.Environment.IsDevelopment())
        {
            app.UseSwagger();
            app.UseSwaggerUI();
            app.ConfigureExceptionHandler();
        }

        app.UseHttpsRedirection();

        app.MapControllers();

        app.Run();
    }
}
