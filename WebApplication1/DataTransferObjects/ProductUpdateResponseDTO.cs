using System.ComponentModel.DataAnnotations;

namespace CatalogApi.DataTransferObjects;

public class ProductUpdateResponseDTO
{
    public int ProductId { get; set; }
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public int Stock { get; set; }
    public string? ImageURL { get; set; }
    public DateTime RegistrationDate { get; set; }
    public int CategoryId { get; set; }
}
