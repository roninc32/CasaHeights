using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.Models
{
    public class Users : IdentityUser
    {
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        [Required]
        [Display(Name = "House Number")]
        [StringLength(50)]
        public string HouseNumber { get; set; }

        [Display(Name = "Profile Picture")]
        public string? ProfilePicture { get; set; }

        [Display(Name = "Created Date")]
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;

        [Display(Name = "Last Updated")]
        public DateTime DateUpdated { get; set; } = DateTime.UtcNow;
    }
}
