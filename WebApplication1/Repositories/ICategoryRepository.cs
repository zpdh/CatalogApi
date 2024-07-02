using CatalogApi.Models;
using CatalogApi.Pagination;
using X.PagedList;

namespace CatalogApi.Repositories
{
    public interface ICategoryRepository : ICrudRepository<Category>
    {
        Task<IPagedList<Category>> GetCategoriesAsync(CategoriesParameters parameters);

        Task<IPagedList<Category>> GetCategoriesFilteredByNameAsync(CategoriesNameFilter filter);
    }
}
