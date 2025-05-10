using CasaHeights.Models.Forum;
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.ViewModels.Forum
{
    public class PostDetailViewModel
    {
        public ForumPost Post { get; set; }
        public List<ForumComment> Comments { get; set; } = new();

        // For adding new comments
        [Required(ErrorMessage = "Comment content is required")]
        [MinLength(2, ErrorMessage = "Comment must be at least 2 characters long")]
        public string NewCommentContent { get; set; }

        // For reporting
        public string ReportReason { get; set; }
        public ForumIndexViewModel SidebarModel { get; set; } = new();
    }
}