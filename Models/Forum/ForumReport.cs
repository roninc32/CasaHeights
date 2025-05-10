using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using CasaHeights.Models.Forum.Enums;

namespace CasaHeights.Models.Forum
{
    public class ForumReport
    {
        public int Id { get; set; }

        [Required]
        public string Reason { get; set; }

        public string? AdditionalDetails { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Reporter
        [Required]
        public string ReporterId { get; set; }

        [ForeignKey("ReporterId")]
        public Users Reporter { get; set; }

        // Reported Content
        public int? PostId { get; set; }
        public ForumPost Post { get; set; }

        public int? CommentId { get; set; }
        public ForumComment Comment { get; set; }

        // Report Status
        public ReportStatus Status { get; set; } = ReportStatus.Pending;
        public string? ModeratorId { get; set; }
        public DateTime? ReviewedAt { get; set; }
        public string? ModeratorNotes { get; set; }
    }
}