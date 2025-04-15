// Models/ServiceRequest.cs
using CasaHeights.Models;
using CasaHeights.Data;
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.Models
{
    public class ServiceRequest
    {
        public int Id { get; set; }

        [Required]
        public ServiceRequestType RequestType { get; set; }

        [Required]
        [StringLength(100)]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(1000)]
        public string Description { get; set; } = string.Empty;

        [Required]
        public PriorityLevel Priority { get; set; }

        [StringLength(255)]
        public string? Location { get; set; }

        public DateTime CreatedDate { get; set; }

        public string? AttachmentUrl { get; set; }

        public ServiceRequestStatus Status { get; set; }

        [Required]
        public string ResidentId { get; set; } = string.Empty;

        // Navigation property using your Users class
        public Users? Resident { get; set; }

        // Additional properties for staff handling
        public string? StaffId { get; set; }
        public Users? AssignedStaff { get; set; }
        public DateTime? ProcessedDate { get; set; }
        public string? StaffNotes { get; set; }
    }

    public enum ServiceRequestType
    {
        Maintenance,
        Security,
        Housekeeping,
        General
    }

    public enum PriorityLevel
    {
        Low,
        Medium,
        High,
        Emergency
    }

    public enum ServiceRequestStatus
    {
        New,
        InProgress,
        OnHold,
        Completed,
        Cancelled
    }
}
