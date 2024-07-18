using CatalogApi.DataTransferObjects;
using CatalogApi.Extensions;
using CatalogApi.Models;
using CatalogApi.Pagination;
using CatalogApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace CatalogApi.Controllers;

[Route("api/v{version:apiVersion}/[controller]")]
[ApiConventionType(typeof(DefaultApiConventions))]
[Produces("application/json")]
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

    /// <summary>
    /// Gets list of category objects from database
    /// </summary>
    /// <returns>Returns list of categories</returns>
    [HttpGet]
    [AllowAnonymous]
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

    /// <summary>
    /// Gets a single category object from database
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Returns single category</returns>
    [HttpGet]
    [Route("{id:int:min(1)}")]
    [AllowAnonymous]
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

    /// <summary>
    /// Gets list of category objects from database while paginating according to the informed config
    /// </summary>
    /// <returns>Returns list of categories</returns>
    [HttpGet]
    [Route("pagination")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<CategoryDTO>>> GetAsync([FromQuery] CategoriesParameters parameters)
    {
        var categories = await _uof.CategoryRepository.GetCategoriesAsync(parameters);

        if (categories == null || !categories.Any())
        {
            _logger.LogWarning("Could not find any categories");
            return NotFound("Could not find any categories");
        }

        return ObtainCategory(categories);
    }

    /// <summary>
    /// Gets list of category objects from database that meet the filter options
    /// </summary>
    /// <returns>Returns list of categories</returns>
    [HttpGet]
    [Route("pagination/filter")]
    [AllowAnonymous]
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
    
    /// <summary>
    /// Adds an object of the category type to the database
    /// </summary>
    /// <param name="categoryDto"></param>
    /// <returns>Returns the category object added to database</returns>
    [HttpPost]
    //[Authorize(Policy = "AdminOnly")]
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

    /// <summary>
    /// Edits an existing category in the database
    /// </summary>
    /// <param name="id"></param>
    /// <param name="categoryDto"></param>
    /// <returns>Returns the object edited</returns>
    [HttpPut]
    [Route("{id:int:min(1)}")]
    //[Authorize(Policy = "AdminOnly")]
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

    /// <summary>
    /// Deletes a category object from the database
    /// </summary>
    /// <param name="id"></param>
    /// <returns>Returns the deleted object</returns>
    [HttpDelete]
    [Route("{id:int:min(1)}")]
    //[Authorize(Policy = "AdminOnly")]
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