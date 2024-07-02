using CatalogApi.Data;
using Microsoft.EntityFrameworkCore;
using System.Linq.Expressions;

namespace CatalogApi.Repositories
{
    public class CrudRepository<T> : ICrudRepository<T> where T : class
    {
        protected readonly DataContext _context;

        public CrudRepository(DataContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await _context.Set<T>().AsNoTracking().ToListAsync();
        }

        public async Task<T?> GetByIdAsync(Expression<Func<T, bool>> expression)
        {
            return await _context.Set<T>().FirstOrDefaultAsync(expression);
        }

        public T Create(T entity)
        {
            _context.Set<T>().Add(entity);

            return entity;
        }

        public T Update(T entity)
        {
            _context.Update(entity);

            return entity;
        }

        public T Delete(T entity)
        {
            _context.Set<T>().Remove(entity);

            return entity;
        }
    }
}
