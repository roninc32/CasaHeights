using System.ComponentModel.DataAnnotations;

public class EditUserViewModel
{
    public string Id { get; set; }

    [Required(ErrorMessage = "Name is required")]
    public string Name { get; set; }

    [Required(ErrorMessage = "Email is required")]
    [EmailAddress]
    public string Email { get; set; }

    [Phone]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Display(Name = "House Number")]
    public string HouseNumber { get; set; }

    [Display(Name = "Profile Picture")]
    public IFormFile? ProfilePicture { get; set; }

    public string? ExistingProfilePicture { get; set; }

    [StringLength(40, MinimumLength = 8, ErrorMessage = "Password must be between {2} and {1} characters")]
    [DataType(DataType.Password)]
    public string? NewPassword { get; set; }

    [DataType(DataType.Password)]
    [Display(Name = "Confirm New Password")]
    [Compare("NewPassword", ErrorMessage = "Passwords do not match")]
    public string? ConfirmNewPassword { get; set; }

    [Required(ErrorMessage = "Role is required")]
    public string Role { get; set; }

    public List<string> Roles { get; set; } = new();
}
