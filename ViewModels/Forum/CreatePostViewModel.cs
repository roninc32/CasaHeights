using CasaHeights.Models.Forum;
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.ViewModels.Forum
{
    // ViewModels/Forum/CreatePostViewModel.cs
    public class CreatePostViewModel
    {
        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 5, 
            ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [MinLength(10, ErrorMessage = "Content must be at least 10 characters long")]
        public string Content { get; set; }

        public List<ForumCategory> Categories { get; set; } = new();
        
        [Required(ErrorMessage = "Please select at least one category")]
        public List<int> SelectedCategories { get; set; } = new();

        [Display(Name = "Tags")]
        public string Tags { get; set; }

        // Add this for tag descriptions
        public Dictionary<string, string> TagDescriptions { get; set; } = new();
    }
}