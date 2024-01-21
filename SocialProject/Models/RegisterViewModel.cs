using System.ComponentModel.DataAnnotations;

namespace SocialProject.WebUI.Models
{
    public class RegisterViewModel
    {
        [Required]
        public string? Username { get; set; }
        [Required(ErrorMessage = "Please enter a password")]
        [DataType(DataType.Password)]
        public string? Password { get; set; }
        [Required]
        public string? Email { get; set; }
        public IFormFile? File { get; set; }
        public string? ImageUrl { get; set; } = "user-1.jpg";
    }
}
