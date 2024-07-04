using System.ComponentModel.DataAnnotations;

namespace CatalogApi.DataTransferObjects
{
    public class RegisterDTO
    {
        [Required(ErrorMessage = "Username field is required.")]
        public string? Username { get; set; }

        [EmailAddress]
        [Required(ErrorMessage = "Email field is required.")]
        public string? Email { get; set; }

        [Required(ErrorMessage = "Password field is required.")]
        public string? Password { get; set; }
    }
}
