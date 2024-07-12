using AutoMapper;
using CatalogApi.Controllers;
using CatalogApi.Data;
using CatalogApi.DataTransferObjects;
using CatalogApi.Logging;
using CatalogApi.Repositories;
using Microsoft.CodeAnalysis.Elfie.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging.Abstractions;
using NuGet.Common;

namespace TestProject1.UnitTest;

public class ProductsUnitTestController
{
    public IUnitOfWork UnitOfWork;
    public IMapper Mapper;
    private static DbContextOptions<DataContext> DbContextOptions { get; }

    public static string ConnectionString = "Server=localhost;DataBase=CatalogDB;Uid=root;Pwd=1234";

    static ProductsUnitTestController()
    {
        DbContextOptions = new DbContextOptionsBuilder<DataContext>().UseMySql(ConnectionString,
            ServerVersion.AutoDetect(ConnectionString)).Options;
    }

    public ProductsUnitTestController()
    {
        var config = new MapperConfiguration(cfg =>
            cfg.AddProfile(new ProductDTOMappingProfile()));
        
        Mapper = config.CreateMapper();

        var context = new DataContext(DbContextOptions);
        UnitOfWork = new UnitOfWork(context);
    }
}