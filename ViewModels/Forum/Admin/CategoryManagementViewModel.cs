using CasaHeights.Models.Forum;
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.ViewModels.Forum.Admin
{
    public class CategoryManagementViewModel
    {
        public List<ForumCategory> Categories { get; set; } = new();
        
        // For adding/editing categories
        public ForumCategory EditingCategory { get; set; }
        
        [Required(ErrorMessage = "Category name is required")]
        [StringLength(50, MinimumLength = 2, 
            ErrorMessage = "Category name must be between 2 and 50 characters")]
        public string NewCategoryName { get; set; }
        
        [StringLength(200, ErrorMessage = "Description cannot exceed 200 characters")]
        public string NewCategoryDescription { get; set; }
        
        public int NewCategoryDisplayOrder { get; set; }
    }
}