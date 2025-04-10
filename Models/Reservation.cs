using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaHeights.Models
{
    public enum ReservationStatus
    {
        Pending,
        Approved,
        Rejected,
        Cancelled,
        Completed
    }

    public class Reservation
    {
        public int Id { get; set; }

        [Required]
        public int FacilityId { get; set; }

        [Required]
        public string UserId { get; set; }

        // Adding Date property that maps to ReservationDate
        [NotMapped]
        public DateTime Date 
        { 
            get => ReservationDate;
            set => ReservationDate = value;
        }

        [Required]
        [Display(Name = "Reservation Date")]
        [DataType(DataType.Date)]
        public DateTime ReservationDate { get; set; } = DateTime.Now;

        [Required]
        [Display(Name = "Start Time")]
        public DateTime StartTime { get; set; }

        [Required]
        [Display(Name = "End Time")]
        public DateTime EndTime { get; set; }

        [Required]
        [StringLength(500)]
        [Display(Name = "Purpose of Reservation")]
        public string Purpose { get; set; }

        [Display(Name = "Number of Attendees")]
        [Range(1, 1000)]
        public int? AttendeeCount { get; set; }

        // Notes property that maps to SpecialRequests
        [NotMapped]
        public string Notes
        {
            get => SpecialRequests;
            set => SpecialRequests = value;
        }

        [Display(Name = "Special Requests")]
        public string? SpecialRequests { get; set; }

        [Required]
        public ReservationStatus Status { get; set; } = ReservationStatus.Pending;

        // TotalCost property that maps to TotalAmount
        [NotMapped]
        public decimal TotalCost
        {
            get => TotalAmount;
            set => TotalAmount = value;
        }

        [Display(Name = "Total Amount")]
        [DataType(DataType.Currency)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal TotalAmount { get; set; }

        // Adding CreatedDate property
        [Display(Name = "Created Date")]
        public DateTime CreatedDate { get; set; } = DateTime.Now;

        // Adding ApprovalDate property
        [Display(Name = "Approval Date")]
        public DateTime? ApprovalDate { get; set; }

        [Display(Name = "Approved/Rejected By")]
        public string? ProcessedById { get; set; }

        [Display(Name = "Approval/Rejection Date")]
        public DateTime? ProcessedDate { get; set; }

        [Display(Name = "Comments")]
        public string? AdminComments { get; set; }

        // Adding RejectionDate property
        [Display(Name = "Rejection Date")]
        public DateTime? RejectionDate { get; set; }

        // Adding RejectionReason property
        [Display(Name = "Rejection Reason")]
        public string? RejectionReason { get; set; }

        // Adding CancellationDate property
        [Display(Name = "Cancellation Date")]
        public DateTime? CancellationDate { get; set; }

        // Adding CancellationReason property
        [Display(Name = "Cancellation Reason")]
        public string? CancellationReason { get; set; }

        [Display(Name = "Payment Status")]
        public bool IsPaid { get; set; } = false;

        [Display(Name = "Payment Date")]
        public DateTime? PaymentDate { get; set; }

        [Display(Name = "Payment Method")]
        public string? PaymentMethod { get; set; }

        [Display(Name = "Payment Reference")]
        public string? PaymentReference { get; set; }

        [ForeignKey("FacilityId")]
        public virtual Facility Facility { get; set; }

        [ForeignKey("UserId")]
        public virtual Users User { get; set; }

        [ForeignKey("ProcessedById")]
        public virtual Users? ProcessedBy { get; set; }

        [NotMapped]
        public double DurationHours => (EndTime - StartTime).TotalHours;
    }
}