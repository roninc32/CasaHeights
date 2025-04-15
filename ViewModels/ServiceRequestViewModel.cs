using System;
using System.ComponentModel.DataAnnotations;
using CasaHeights.Models;

namespace CasaHeights.ViewModels
{
    public class ServiceRequestViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Please select a request type")]
        [Display(Name = "Request Type")]
        public ServiceRequestType RequestType { get; set; }

        [Required(ErrorMessage = "Please enter a title")]
        [StringLength(100, ErrorMessage = "Title cannot exceed 100 characters")]
        public string Title { get; set; } = string.Empty;  // Remove duplicate

        [Required(ErrorMessage = "Please enter a description")]
        [StringLength(1000, ErrorMessage = "Description cannot exceed 1000 characters")]
        public string Description { get; set; } = string.Empty;  // Remove duplicate

        [Required(ErrorMessage = "Please select a priority level")]
        [Display(Name = "Priority Level")]
        public PriorityLevel Priority { get; set; }

        [Display(Name = "Location")]
        [StringLength(255)]
        public string? Location { get; set; }

        public DateTime CreatedDate { get; set; } = DateTime.Now;

        public string? AttachmentUrl { get; set; }

        public ServiceRequestStatus Status { get; set; } = ServiceRequestStatus.New;

        // Add ResidentId property to match with ServiceRequest model
        public string? ResidentId { get; set; }

        // Add UserId property that is used in controller
        public string? UserId { get; set; }

        // Navigation property
        public Users? Resident { get; set; }
        public Users? AssignedStaff { get; set; }
    }
}
