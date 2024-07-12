using CatalogApi.Controllers;
using CatalogApi.DataTransferObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestProject1.UnitTest;

public class PutProduct : IClassFixture<ProductsUnitTestController>
{
    private readonly ProductsController _controller;

    public PutProduct(ProductsUnitTestController controller)
    {
        _controller = new ProductsController(controller.UnitOfWork, NullLogger<ProductsController>.Instance,
            controller.Mapper);
    }
    
    [Fact]
    public async Task PutProduct_OkResult()
    {
        var prod = new ProductDTO
        {
            Name = "placeholder",
            Description = "placeholder",
            Price = 10,
            CategoryId = 1,
            ImageURL = "placeholder",
            ProductId = 16
        }; 
        var data = await _controller.PutAsync(16, prod);

        data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }
    
    [Fact]
    public async Task PutProduct_BadRequestResult()
    {
        var prod = new ProductDTO
        {
            Name = "placeholder",
            ProductId = 9999
        };
        var data = await _controller.PutAsync(1, prod);

        data.Result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
}