using System.ComponentModel.DataAnnotations;

namespace CatalogApi.DataTransferObjects;

public class ProductDTO
{
    public int ProductId { get; set; }

    [Required(ErrorMessage = "The name field is required")]
    [StringLength(32, ErrorMessage = "The name must atleast 4 characters and, at most 32 characters."), MinLength(4)]
    public string? Name { get; set; }

    [Required(ErrorMessage = "The description field is required")]
    [StringLength(128, ErrorMessage = "The description can only contain up to 128 characters.")]
    public string? Description { get; set; }

    [Required(ErrorMessage = "The price field is required")]
    [Range(0.01, 10000.00, ErrorMessage = "The price field has a minimum value of 0.01 and a maximum value of 10000.00.")]
    public decimal Price { get; set; }

    [StringLength(256)]
    public string? ImageURL { get; set; }

    public int CategoryId { get; set; }
}
