using CatalogApi.Controllers;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestProject1.UnitTest;

public class DeleteProduct : IClassFixture<ProductsUnitTestController>
{
    private readonly ProductsController _controller;

    public DeleteProduct(ProductsUnitTestController controller)
    {
        _controller = new ProductsController(controller.UnitOfWork, NullLogger<ProductsController>.Instance,
            controller.Mapper);
    }

    [Fact]
    public async Task DeleteProduct_OkResult()
    {
        var data = await _controller.DeleteAsync(18);
        
        data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }
    
    [Fact]
    public async Task DeleteProduct_NotFoundResult()
    {
        var data = await _controller.DeleteAsync(21312314);
        
        data.Result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
}