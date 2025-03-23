using System.ComponentModel.DataAnnotations;

namespace CasaHeights.ViewModels
{
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Name is required")]
        [Display(Name = "Full Name")]
        public string Name { get; set; }

        [Required(ErrorMessage = "Email is required")]
        [EmailAddress(ErrorMessage = "Invalid email address")]
        public string Email { get; set; }

        [Required(ErrorMessage = "Password is required")]
        [StringLength(40, MinimumLength = 8, ErrorMessage = "Password must be between {2} and {1} characters")]
        [DataType(DataType.Password)]
        public string Password { get; set; }

        [Required(ErrorMessage = "Confirm Password is required")]
        [DataType(DataType.Password)]
        [Display(Name = "Confirm Password")]
        [Compare("Password", ErrorMessage = "Passwords do not match")]
        public string ConfirmPassword { get; set; }

        [Phone(ErrorMessage = "Invalid phone number format")]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required(ErrorMessage = "House number is required")]
        [Display(Name = "House Number")]
        [RegularExpression(@"^[A-Za-z0-9\s-]+$", ErrorMessage = "House number can only contain letters, numbers, spaces and hyphens")]
        [StringLength(50, ErrorMessage = "House number cannot be longer than 50 characters")]
        public string HouseNumber { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfilePicture { get; set; }
    }
}