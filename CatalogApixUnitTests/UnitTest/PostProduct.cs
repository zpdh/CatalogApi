using CatalogApi.Controllers;
using CatalogApi.DataTransferObjects;
using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging.Abstractions;

namespace TestProject1.UnitTest;

public class PostProduct : IClassFixture<ProductsUnitTestController>
{
    private readonly ProductsController _controller;

    public PostProduct(ProductsUnitTestController controller)
    {
        _controller = new ProductsController(controller.UnitOfWork, NullLogger<ProductsController>.Instance,
            controller.Mapper);
    }

    [Fact]
    public async Task Post_CreatedResult()
    {
        //Arrange
        var newProdDto = new ProductDTO
        {
            Name = "placeholder",
            Description = "placeholder",
            Price = 10,
            CategoryId = 1,
            ImageURL = "placeholder"
        };
        //Act
        var data = await _controller.PostAsync(newProdDto);
        
        //Assert (FluentAssertions)
        data.Result.Should().BeOfType<CreatedResult>().Which.StatusCode.Should().Be(201);
    }    
    [Fact]
    public async Task Post_BadRequestResult()
    {
        //Arrange
        //Act
        var data = await _controller.PostAsync(null);
        
        //Assert (FluentAssertions)
        data.Result.Should().BeOfType<BadRequestObjectResult>().Which.StatusCode.Should().Be(400);
    }    
}