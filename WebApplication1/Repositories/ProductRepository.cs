using CatalogApi.Data;
using CatalogApi.Models;
using CatalogApi.Pagination;
using Microsoft.EntityFrameworkCore;
using X.PagedList;

namespace CatalogApi.Repositories
{
    public class ProductRepository : CrudRepository<Product>, IProductRepository
    {
        public ProductRepository(DataContext context) : base(context) { }

        public async Task<IPagedList<Product>> GetProductsAsync(ProductsParameters parameters)
        {
            var products = await GetAllAsync();               
            return await products.OrderBy(x => x.ProductId).ToPagedListAsync(parameters.PageNumber, parameters.PageSize);
            //PagedList<Product>.ToPagedList(products.OrderBy(x => x.ProductId).AsQueryable(), parameters.PageNumber, parameters.PageSize);
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(int categoryId)
        {
            var products = await GetAllAsync();
            return products.Where(x => x.CategoryId == categoryId);
        }

        public async Task<IPagedList<Product>> GetProductsFilteredByPriceAsync(ProductsPriceFilter filter)
        {
            var products = await GetAllAsync();

            if (filter.Price.HasValue && !string.IsNullOrEmpty(filter.PriceCriteria))
            {
                var priceCriteria = filter.PriceCriteria.ToLower();
                switch(priceCriteria)
                {
                    case "equals":
                        products = products.Where(obj => obj.Price == filter.Price)
                                           .OrderBy(obj => obj.Price);
                        break;

                    case "under":
                        products = products.Where(obj => obj.Price <= filter.Price)
                                           .OrderBy(obj => obj.Price);
                        break;

                    case "over":
                        products = products.Where(obj => obj.Price >= filter.Price)
                                           .OrderBy(obj => obj.Price);
                        break;
                }
            }

            return await products.ToPagedListAsync(filter.PageNumber, filter.PageSize);
            //return PagedList<Product>.ToPagedList(products.AsQueryable(), filter.PageNumber, filter.PageSize);
        }
    }
}
