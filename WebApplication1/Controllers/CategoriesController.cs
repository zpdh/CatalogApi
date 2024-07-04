using CatalogApi.Data;
using CatalogApi.DataTransferObjects;
using CatalogApi.Extensions;
using CatalogApi.Filters;
using CatalogApi.Models;
using CatalogApi.Pagination;
using CatalogApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Newtonsoft.Json;
using X.PagedList;

namespace CatalogApi.Controllers;

[Route("v1/api/[controller]")]
[ApiController]
public class CategoriesController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    private readonly ILogger<CategoriesController> _logger;

    public CategoriesController(IUnitOfWork uof, ILogger<CategoriesController> logger)
    {
        _uof = uof;
        _logger = logger;
    }

    [HttpGet]
    [Authorize]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAsync()
    {
        var categories = await _uof.CategoryRepository.GetAllAsync();

        if (categories == null || !categories.Any())
        {
            _logger.LogWarning("Could not find any categories");
            return NotFound("Could not find any categories");
        }

        return Ok(categories.ToDTOList());
    }

    [HttpGet]
    [Route("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryDTO>> GetByIdAsync(int id)
    {
        var category = await _uof.CategoryRepository.GetByIdAsync(x => x.CategoryId == id);

        if (category == null)
        {
            _logger.LogWarning($"Could not find category. Id: {id}");
            return NotFound($"Could not find category. Id: {id}");
        }

        return Ok(category.ToDTO());
    }

    [HttpGet]
    [Route("pagination")]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAsync([FromQuery]CategoriesParameters parameters)
    {
        var categories = await _uof.CategoryRepository.GetCategoriesAsync(parameters);

        if (categories == null || !categories.Any())
        {
            _logger.LogWarning("Could not find any categories");
            return NotFound("Could not find any categories");
        }

        return ObtainCategory(categories);
    }

    [HttpGet]
    [Route("pagination/filter")]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetFiltered([FromQuery] CategoriesNameFilter filter)
    {
        var categories = await _uof.CategoryRepository.GetCategoriesFilteredByNameAsync(filter);

        if (categories == null || !categories.Any())
        {
            _logger.LogWarning("Could not find any categories");
            return NotFound("Could not find any categories");
        }

        return ObtainCategory(categories);
    }

    private ActionResult<IEnumerable<CategoryDTO>> ObtainCategory(IPagedList<Category> categories)
    {
        var metadata = new
        {
            categories.Count,
            categories.PageSize,
            categories.PageCount,
            categories.TotalItemCount,
            categories.HasNextPage,
            categories.HasPreviousPage
        };

        Response.Headers.Append("Pagination", JsonConvert.SerializeObject(metadata));

        return Ok(categories.ToDTOList());
    }

    [HttpPost]
    public async Task<ActionResult<CategoryDTO>> Post(CategoryDTO categoryDto)
    {
        if (categoryDto == null)
        {
            _logger.LogWarning("Invalid data");
            return BadRequest("Error: Invalid data");
        }

        _uof.CategoryRepository.Create(categoryDto.ToCategory());
        await _uof.CommitAsync();

        return new CreatedAtRouteResult(new { id = categoryDto.CategoryId }, categoryDto);
    }

    [HttpPut]
    [Route("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryDTO>> PutAsync(int id, CategoryDTO categoryDto)
    {
        if (categoryDto.CategoryId != id)
        {
            _logger.LogWarning("Invalid data");
            return BadRequest("Error: Invalid data");
        }

        _uof.CategoryRepository.Update(categoryDto.ToCategory());
        await _uof.CommitAsync();

        return Ok(categoryDto);
    }

    [HttpDelete]
    [Route("{id:int:min(1)}")]
    public async Task<ActionResult<CategoryDTO>> DeleteAsync(int id)
    {
        var category = await _uof.CategoryRepository.GetByIdAsync(x => x.CategoryId == id);

        if (category == null)
        {
            _logger.LogWarning($"Could not find category. Id: {id}");
            return NotFound($"Could not find category. Id: {id}");
        }

        _uof.CategoryRepository.Delete(category);
        await _uof.CommitAsync();

        return Ok(category.ToDTO());
    }
}
