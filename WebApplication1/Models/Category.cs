using System.Collections.ObjectModel;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace CatalogApi.Models;

[Table("Categories")]
public class Category
{
    [Key]
    public int CategoryId { get; set; }

    [Required]
    [StringLength(32, ErrorMessage = "The name must atleast 4 characters and, at most 32 characters."), MinLength(4)]
    public string? Name { get; set; }

    [StringLength(256)]
    public string? ImageURL { get; set; }

    [JsonIgnore]
    public ICollection<Product>? Products { get; set; }

    public Category()
    {
        Products = new Collection<Product>();
    }
}

