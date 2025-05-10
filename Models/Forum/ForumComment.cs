using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaHeights.Models.Forum
{
    public class ForumComment
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Comment content is required")]
        [MinLength(2, ErrorMessage = "Comment must be at least 2 characters long")]
        public string Content { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }

        // Relations
        [Required]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public Users Author { get; set; }

        public int PostId { get; set; }
        public ForumPost Post { get; set; }

        // Moderation
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletionReason { get; set; }
    }
}