using System.ComponentModel.DataAnnotations;

namespace Application.DTOs
{
    public class RegisterDto
    {
        [Required(ErrorMessage = "Username is required")]
        [StringLength(50, MinimumLength = 4)]
        public string Username { get; set; }
        [Required(ErrorMessage = "First name is required")]
        [StringLength(100, MinimumLength = 1)]
        public string FirstName { get; set; }
        [Required(ErrorMessage = "Last name is required")]
        [StringLength(100, MinimumLength = 1)]
        public string LastName { get; set; }
        [Required(ErrorMessage = "Email is required")]
        [EmailAddress]
        public string Email { get; set; }
        [Required(ErrorMessage = "Password is required")]
        [StringLength(100, MinimumLength = 8)]
        public string Password { get; set; }
        [Required(ErrorMessage = "Phonenumber is required")]
        [RegularExpression(@"^(\+98|0)?9\d{9}$", ErrorMessage = "Invalid phone number format for Iran.")]
        public string Phone { get; set; }
    }
}
