using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Pagination
{
    public class PagedList<T> : List<T> where T : class
    {
        public int CurrentPage { get; private set; }
        public int TotalPages { get; private set; }
        public int PageSize { get; private set; }
        public int TotalCount { get; private set; }

        public bool HasPrevious => CurrentPage > 1;
        public bool HasNext => CurrentPage < TotalPages;

        public PagedList(List<T> values, int count, int pageNumber, int pageSize)
        {
            TotalCount = count;
            PageSize = pageSize;
            CurrentPage = pageNumber;
            TotalPages = (int)Math.Ceiling(count / (double)pageSize);

            AddRange(values);
        }

        /* Raw method; no extra frameworks/libs
         
        public static PagedList<T> ToPagedList(IQueryable<T> values, int pageNumber, int pageSize)
        {
            var count = values.Count();
            var items = values.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToList();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }

        
        public static async Task<PagedList<T>> ToPagedListAsync(IQueryable<T> values, int pageNumber, int pageSize)
        {
            var count = await values.CountAsync();
            var items = await values.Skip((pageNumber - 1) * pageSize).Take(pageSize).ToListAsync();

            return new PagedList<T>(items, count, pageNumber, pageSize);
        }
        */
    }
}
