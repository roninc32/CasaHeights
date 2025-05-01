using CasaHeights.Models.Forum.Enums;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaHeights.Models.Forum
{
    public class ForumPost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Title is required")]
        [StringLength(200, MinimumLength = 5, ErrorMessage = "Title must be between 5 and 200 characters")]
        public string Title { get; set; }

        [Required(ErrorMessage = "Content is required")]
        [MinLength(10, ErrorMessage = "Content must be at least 10 characters long")]
        public string Content { get; set; }

        // Metadata
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ModifiedAt { get; set; }
        public bool IsDeleted { get; set; }
        public bool IsPinned { get; set; }
        public PostStatus Status { get; set; } = PostStatus.Active;

        // User Relations
        [Required]
        public string AuthorId { get; set; }

        [ForeignKey("AuthorId")]
        public Users Author { get; set; }

        // Navigation Properties
        public virtual ICollection<ForumComment> Comments { get; set; } = new List<ForumComment>();
        public virtual ICollection<ForumCategory> Categories { get; set; } = new List<ForumCategory>();
        public virtual ICollection<ForumTag> Tags { get; set; } = new List<ForumTag>();

        // Metrics
        [NotMapped]
        public int CommentCount => Comments?.Count(c => !c.IsDeleted) ?? 0;

        // Moderation
        public string? DeletedBy { get; set; }
        public DateTime? DeletedAt { get; set; }
        public string? DeletionReason { get; set; }
    }
}