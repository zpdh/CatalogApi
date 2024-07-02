using CatalogApi.Data;
using CatalogApi.Models;
using CatalogApi.Pagination;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace CatalogApi.Repositories;

/*Replaced handmade paged list methods with X.PagedList lib, for testing purposes.
 * Both methods work fine and as intended */

public class CategoryRepository : CrudRepository<Category>, ICategoryRepository
{
    public CategoryRepository(DataContext context) : base(context) { }

    public async Task<IPagedList<Category>> GetCategoriesAsync(CategoriesParameters parameters)
    {
        var categories = await GetAllAsync();
        categories = categories.OrderBy(x => x.CategoryId);
            
        return await categories.ToPagedListAsync(parameters.PageNumber, parameters.PageSize);
        //return PagedList<Category>.ToPagedList(categories.AsQueryable(),parameters.PageNumber, parameters.PageSize);
    }

    public async Task<IPagedList<Category>> GetCategoriesFilteredByNameAsync(CategoriesNameFilter filter)
    {
        var categories = await GetAllAsync();

        if (!string.IsNullOrEmpty(filter.Name))
        {
            categories = categories.Where(obj => obj.Name.Contains(filter.Name, StringComparison.OrdinalIgnoreCase));
        }

        return await categories.ToPagedListAsync(filter.PageNumber, filter.PageSize);
            //return PagedList<Category>.ToPagedList(categories.AsQueryable(), filter.PageNumber, filter.PageSize);
    }
}
