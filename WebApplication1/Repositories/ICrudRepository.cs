using System.Linq.Expressions;

namespace CatalogApi.Repositories
{
    public interface ICrudRepository<T> where T : class
    {
        Task<IEnumerable<T>> GetAllAsync();
        Task<T?> GetByIdAsync(Expression<Func<T, bool>> expression);
        T Create(T entity);
        T Update(T entity);
        T Delete(T entity);
    }
}
