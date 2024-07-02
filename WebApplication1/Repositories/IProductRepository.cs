using CatalogApi.Models;
using CatalogApi.Pagination;
using X.PagedList;

namespace CatalogApi.Repositories
{
    public interface IProductRepository : ICrudRepository<Product>
    {
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId);

        Task<IPagedList<Product>> GetProductsAsync(ProductsParameters parameters);

        Task<IPagedList<Product>> GetProductsFilteredByPriceAsync(ProductsPriceFilter filter);
    }
}
