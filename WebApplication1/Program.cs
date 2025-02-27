using System.Reflection;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using Asp.Versioning;
using CatalogApi.Data;
using CatalogApi.DataTransferObjects;
using CatalogApi.Extensions;
using CatalogApi.Filters;
using CatalogApi.Logging;
using CatalogApi.Models;
using CatalogApi.RateLimitOptions;
using CatalogApi.Repositories;
using CatalogApi.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

namespace CatalogApi;

internal static class Program
{
    internal static void Main(string[] args)
    {
        var builder = WebApplication.CreateBuilder(args);

        // Add services to the container.

        builder.Services.AddControllers(options => options.Filters.Add(typeof(ApiExceptionFilter)))
            .AddJsonOptions(options => options.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles)
            .AddNewtonsoftJson();

        builder.Services.AddCors(options =>
        {
            options.AddPolicy("CorsPolicyOne",
                policy =>
                {
                    policy.WithOrigins("https://localhost:44386/").WithMethods("GET", "POST", "PUT", "DELETE")
                        .AllowAnyHeader();
                });
        });

        builder.Services.AddEndpointsApiExplorer();
        builder.Services.AddSwaggerGen(options =>
        {
            options.SwaggerDoc("v1", new OpenApiInfo
            {
                Version = "v1",
                Title = "CatalogApi",
                Description = "Products and categories catalog",
                TermsOfService = new Uri("https://example.com/terms"),
                Contact = new OpenApiContact
                {
                    Name = "Bob Brown",
                    Email = "bobbrown@gmail.com",
                    Url = new Uri("https://github.com/zpdh")
                },
                License = new OpenApiLicense
                {
                    Name = "MIT License",
                    Url = new Uri("https://opensource.org/licenses/MIT")
                }
            });

            options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme()
            {
                Name = "Authorization",
                Type = SecuritySchemeType.ApiKey,
                Scheme = "Bearer",
                BearerFormat = "JWT",
                In = ParameterLocation.Header,
                Description = "Bearer JWT "
            });
            options.AddSecurityRequirement(new OpenApiSecurityRequirement
            {
                {
                    new OpenApiSecurityScheme
                    {
                        Reference = new OpenApiReference
                        {
                            Type = ReferenceType.SecurityScheme,
                            Id = "Bearer"
                        }
                    },
                    new string[] { }
                }
            });
            var xmlFileName = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
            options.IncludeXmlComments(Path.Combine(AppContext.BaseDirectory, xmlFileName));
        });

        builder.Services.AddIdentity<ApplicationUser, IdentityRole>().AddEntityFrameworkStores<DataContext>()
            .AddDefaultTokenProviders();

        string? mySqlConnection =
            builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<DataContext>(options =>
            options.UseMySql(mySqlConnection,
                ServerVersion.AutoDetect(mySqlConnection)));

        //JWT Bearer authentication & authorization
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
        builder.Services.AddAuthorization(options =>
        {
            options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin"));
            options.AddPolicy("SuperAdminOnly", policy => policy.RequireRole("SuperAdmin")
                .RequireClaim("id", "bobbrown"));
            options.AddPolicy("ExclusivePolicy", policy => policy.RequireAssertion(handler =>
                handler.User.HasClaim(claim => claim is { Type: "id", Value: "bobbrown" } ||
                                               handler.User.IsInRole("SuperAdmin"))));
        });

        builder.Logging.AddProvider(new CustomerLoggerProvider(new CustomerLoggerProviderConfiguration()
        {
            LogLevel = LogLevel.Information
        }));

        var rateLimitOptions = new MyRateLimitOptions();
        builder.Configuration.GetSection("RateLimitValues").Bind(rateLimitOptions);
        builder.Services.AddRateLimiter(options =>
        {
            options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;

            options.GlobalLimiter = PartitionedRateLimiter.Create<HttpContext, string>(httpcontext =>
                RateLimitPartition.GetFixedWindowLimiter(httpcontext.User.Identity?.Name ??
                                                         httpcontext.Request.Headers.Host.ToString(),
                    _ => new FixedWindowRateLimiterOptions
                    {
                        AutoReplenishment = rateLimitOptions.AutoReplenishment,
                        PermitLimit = rateLimitOptions.PermitLimit,
                        QueueLimit = rateLimitOptions.QueueLimit,
                        Window = TimeSpan.FromSeconds(rateLimitOptions.Window)
                    }));
        });

        builder.Services.AddApiVersioning(options =>
        {
            options.DefaultApiVersion = new ApiVersion(1, 0);
            options.AssumeDefaultVersionWhenUnspecified = true;
            options.ReportApiVersions = true;
            options.ApiVersionReader = ApiVersionReader.Combine(new UrlSegmentApiVersionReader());
        }).AddApiExplorer(options =>
        {
            options.GroupNameFormat = "'v'VVV";
            options.SubstituteApiVersionInUrl = true;
        });

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
        app.UseStaticFiles();
        app.UseRouting();

        app.UseRateLimiter();

        app.UseCors();

        app.UseAuthorization();
        app.MapControllers();

        app.Run();
    }
}