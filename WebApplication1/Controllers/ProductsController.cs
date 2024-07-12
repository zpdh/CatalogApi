using Asp.Versioning;
using AutoMapper;
using CatalogApi.DataTransferObjects;
using CatalogApi.Models;
using CatalogApi.Pagination;
using CatalogApi.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.JsonPatch;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using X.PagedList;

namespace CatalogApi.Controllers;

[ApiController]
[Produces("application/json")]
[ApiConventionType(typeof(DefaultApiConventions))]
[Route("api/v{version:apiVersion}/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IUnitOfWork _uof;
    private readonly ILogger<ProductsController> _logger;
    private readonly IMapper _mapper;

    public ProductsController(IUnitOfWork uof, ILogger<ProductsController> logger, IMapper mapper)
    {
        _uof = uof;
        _logger = logger;
        _mapper=mapper;
    }
    
    [HttpGet]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAsync()
    {
        var products = await _uof.ProductRepository.GetAllAsync();

        if (products == null)
        {
            _logger.LogWarning("Could not find any products");
            return NotFound("Could not find any products");
        }

        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Ok(productsDto);
    }

    [HttpGet]
    [Route("{id:int:min(1)}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDTO>> GetByIdAsync(int id)
    {
        if (id <= 0) return BadRequest("Invalid id");
        
        var product = await _uof.ProductRepository.GetByIdAsync(x => x.ProductId == id);

        if (product == null)
        {
            _logger.LogWarning($"Could not find product. Id: {id}");
            return NotFound($"Could not find product. Id: {id}");
        }

        var productDto = _mapper.Map<ProductDTO>(product);

        return Ok(productDto);
    }

    [HttpGet]
    [Route("/category/{categoryId}")]
    [AllowAnonymous]
    public async Task<ActionResult<ProductDTO>> GetProductsByCategoryAsync(int categoryId)
    {
        var products = await _uof.ProductRepository.GetProductsByCategoryAsync(categoryId);

        if (products == null)
        {
            _logger.LogWarning
            ($"Could not find products category. Id: {categoryId}");
            return NotFound($"Could not find products category. Id: {categoryId}");
        }

        var productsDto = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Ok(productsDto);
    }

    [HttpGet]
    [Route("pagination")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetAsync([FromQuery] ProductsParameters parameters)
    {
        var products = await _uof.ProductRepository.GetProductsAsync(parameters);

        if (products == null)
        {
            _logger.LogWarning("Could not find any products");
            return NotFound("Could not find any products");
        }

        return ObtainProducts(products);
    }

    [HttpGet]
    [Route("pagination/filter")]
    [AllowAnonymous]
    public async Task<ActionResult<IEnumerable<ProductDTO>>> GetFilteredAsync([FromQuery] ProductsPriceFilter filter)
    {
        var products = await _uof.ProductRepository.GetProductsFilteredByPriceAsync(filter);

        if (products == null)
        {
            _logger.LogWarning("Could not find any products");
            return NotFound("Could not find any products");
        }

        return ObtainProducts(products);
    }

    private ActionResult<IEnumerable<ProductDTO>> ObtainProducts(IPagedList<Product> products)
    {
        var metadata = new
        {
            products.Count,
            products.PageSize,
            products.PageCount,
            products.TotalItemCount,
            products.HasNextPage,
            products.HasPreviousPage
        };

        Response.Headers.Append("Pagination", JsonConvert.SerializeObject(metadata));

        var productsDTO = _mapper.Map<IEnumerable<ProductDTO>>(products);

        return Ok(productsDTO);
    }

    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ProductDTO>> PostAsync(ProductDTO productDto)
    {
        if (productDto == null)
        {
            _logger.LogWarning("Invalid data");
            return BadRequest("Error: Invalid data");
        }

        var product = _mapper.Map<Product>(productDto);

        _uof.ProductRepository.Create(product);
        await _uof.CommitAsync();

        return Created("Products", productDto);
    }

    [HttpPatch]
    [Route("{id}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ProductUpdateResponseDTO>> PatchAsync(int id, JsonPatchDocument<ProductUpdateRequestDTO> patchProductDto)
    {
        if (patchProductDto == null || id <= 0) return BadRequest();

        var product = await _uof.ProductRepository.GetByIdAsync(x => x.ProductId == id);

        var productUpdateRequest = _mapper.Map<ProductUpdateRequestDTO>(product);

        patchProductDto.ApplyTo(productUpdateRequest, ModelState);

        if (!ModelState.IsValid || TryValidateModel(productUpdateRequest)) return BadRequest(ModelState);

        _mapper.Map(productUpdateRequest, product);

        _uof.ProductRepository.Update(product!);
        await _uof.CommitAsync();

        return Ok(_mapper.Map<ProductUpdateResponseDTO>(product));
    }

    [HttpPut]
    [Route("{id:int:min(1)}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ProductDTO>> PutAsync(int id, ProductDTO productDto)
    {
        if (productDto.ProductId != id)
        {
            _logger.LogWarning("Invalid data");
            return BadRequest("Error: Invalid data");
        }

        var product = _mapper.Map<Product>(productDto);

        _uof.ProductRepository.Update(product);
        await _uof.CommitAsync();

        return Ok(productDto);
    }

    [HttpDelete]
    [Route("{id:int:min(1)}")]
    [Authorize(Policy = "AdminOnly")]
    public async Task<ActionResult<ProductDTO>> DeleteAsync(int id)
    {
        var product = await _uof.ProductRepository.GetByIdAsync(x => x.ProductId == id);

        if (product == null)
        {
            _logger.LogWarning($"Could not find product. Id: {id}");
            return NotFound($"Could not find product. Id: {id}");
        }

        _uof.ProductRepository.Delete(product);
        await _uof.CommitAsync();

        var productDto = _mapper.Map<ProductDTO>(product);

        return Ok(productDto);
    }
}
