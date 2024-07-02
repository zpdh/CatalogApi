namespace CatalogApi.Pagination
{
    public class ProductsPriceFilter : QueryParameters
    {
        public decimal? Price { get; set; }

        public string? PriceCriteria { get; set; } // "under", "equals" or "over"
    }
}
