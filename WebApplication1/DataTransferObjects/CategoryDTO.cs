using System.ComponentModel.DataAnnotations;

namespace CatalogApi.DataTransferObjects;

public class CategoryDTO
{
    public int CategoryId { get; set; }

    [Required]
    [StringLength(32, ErrorMessage = "The name must atleast 4 characters and, at most 32 characters."), MinLength(4)]
    public string? Name { get; set; }

    [StringLength(256)]
    public string? ImageURL { get; set; }

}
