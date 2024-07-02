
using CatalogApi.Data;
using CatalogApi.DataTransferObjects;
using CatalogApi.Extensions;
using CatalogApi.Filters;
using CatalogApi.Logging;
using CatalogApi.Repositories;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;

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

        string? mySqlConnection =
        builder.Configuration.GetConnectionString("DefaultConnection");

        builder.Services.AddDbContext<DataContext>(options =>
        options.UseMySql(mySqlConnection,
        ServerVersion.AutoDetect(mySqlConnection)));

        builder.Logging.AddProvider(new CustomerLoggerProvider(new CustomerLoggerProviderConfiguration()
        {
            LogLevel = LogLevel.Information
        }));

        builder.Services.AddScoped<ApiLoggingFilter>();

        builder.Services.AddScoped<ICategoryRepository, CategoryRepository>();
        builder.Services.AddScoped<IProductRepository, ProductRepository>();
        builder.Services.AddScoped(typeof(ICrudRepository<>), typeof(CrudRepository<>));
        builder.Services.AddScoped<IUnitOfWork, UnitOfWork>();
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

        app.UseAuthorization();


        app.MapControllers();

        app.Run();
    }
}
