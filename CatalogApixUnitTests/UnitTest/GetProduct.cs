using CatalogApi.Controllers;
using CatalogApi.DataTransferObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestProject1.UnitTest;

public class GetProduct : IClassFixture<ProductsUnitTestController>
{
    private readonly ProductsController _controller;

    public GetProduct(ProductsUnitTestController controller)
    {
        _controller = new ProductsController(controller.UnitOfWork, NullLogger<ProductsController>.Instance,
            controller.Mapper);
    }

    [Fact]
    public async Task GetById_OkResult()
    {
        //Arrange
        var id = 1;
        
        //Act
        var data = await _controller.GetByIdAsync(id);
        
        //Assert (xUnit)
        /*
        var result = Assert.IsType<OkObjectResult>(data.Result);
            Assert.Equal(200, result.StatusCode);
        */
            
        //Assert (FluentAssertions)
        data.Result.Should().BeOfType<OkObjectResult>().Which.StatusCode.Should().Be(200);
    }
    
    [Fact]
    public async Task GetById_NotFoundResult()
    {
        //Arrange
        var id = 9999;
        
        //Act
        var data = await _controller.GetByIdAsync(id);
            
        //Assert (FluentAssertions)
        data.Result.Should().BeOfType<NotFoundObjectResult>().Which.StatusCode.Should().Be(404);
    }
    
    [Fact]
    public async Task GetById_BadRequestResult()
    {
        //Arrange
        var id = -213;
        
        //Act
        var data = await _controller.GetByIdAsync(id);
            
        //Assert (FluentAssertions)
        data.Result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }
    
    
    [Fact]
    public async Task GetList()
    {
        //Act
        var data = await _controller.GetAsync();
            
        //Assert (FluentAssertions)
        data.Result.Should().BeOfType<OkObjectResult>().Which.Value.Should().BeAssignableTo<IEnumerable<ProductDTO>>()
            .And.NotBeNull();
    }
}