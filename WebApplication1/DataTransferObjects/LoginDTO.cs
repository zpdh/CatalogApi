using System.ComponentModel.DataAnnotations;

namespace CatalogApi.DataTransferObjects
{
    public class LoginDTO
    {
        [Required(ErrorMessage = "Username field is required.")]
        public string? Username { get; set; }

        [Required(ErrorMessage = "Password field is required.")]
        public string? Password { get; set; }
    }
}
