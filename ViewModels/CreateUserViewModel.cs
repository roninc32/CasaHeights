using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.ViewModels;
public class CreateUserViewModel
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

    [Required(ErrorMessage = "Role is required")]
    [RegularExpression("^(User|Staff)$", ErrorMessage = "Invalid role selected")]
    public string Role { get; set; }

    [Phone(ErrorMessage = "Invalid phone number format")]
    [Display(Name = "Phone Number")]
    public string PhoneNumber { get; set; }

    [Required(ErrorMessage = "House number is required")]
    [Display(Name = "House Number")]
    public string HouseNumber { get; set; }

    [Display(Name = "Profile Picture")]
    public string? ProfilePicture { get; set; }

    public List<string> Roles { get; set; } = new();
}