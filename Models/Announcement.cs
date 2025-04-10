using System;
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.Models
{
    public class Announcement
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; }

        [Required]
        public string Content { get; set; }

        [Required]
        public DateTime PostedDate { get; set; }

        [Required]
        public DateTime ExpiryDate { get; set; }

        public bool IsUrgent { get; set; }

        public bool SendEmail { get; set; }

        public bool SendSMS { get; set; }

        // Navigation property for who created the announcement
        // Remove any [Required] attributes here
        public string? CreatedById { get; set; }
        public Users? CreatedBy { get; set; }
    }
}