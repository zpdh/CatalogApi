using System.ComponentModel.DataAnnotations;

namespace CatalogApi.DataTransferObjects;

public class ProductUpdateRequestDTO : IValidatableObject
{
    [Range(1, 1000, ErrorMessage = "The stock must be between 1 and 1000.")]
    public int Stock { get; set; }
    public DateTime RegistrationDate { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        if (RegistrationDate <= DateTime.Now.Date)
        {
            yield return new ValidationResult("The date must be previous.", [nameof(this.RegistrationDate)]);
        }
    }
}
