using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CasaHeights.Models
{
    public class Facility
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        [Required]
        public string Description { get; set; }

        [Required]
        public string Location { get; set; }

        [Required]
        public int Capacity { get; set; }

        [Display(Name = "Opening Time")]
        [DataType(DataType.Time)]
        public TimeSpan OpeningTime { get; set; } = new TimeSpan(8, 0, 0); // Default 8:00 AM

        [Display(Name = "Closing Time")]
        [DataType(DataType.Time)]
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(22, 0, 0); // Default 10:00 PM

        [Display(Name = "Minimum Reservation Hours")]
        [Range(1, 24)]
        public int MinimumReservationHours { get; set; } = 1;

        [Display(Name = "Maximum Reservation Hours")]
        [Range(1, 24)]
        public int MaximumReservationHours { get; set; } = 4;

        [Display(Name = "Hourly Rate")]
        [DataType(DataType.Currency)]
        [Range(0, 10000)]
        [Column(TypeName = "decimal(18, 2)")]
        public decimal HourlyRate { get; set; }

        [Display(Name = "Maintenance Day")]
        public DayOfWeek? MaintenanceDay { get; set; }

        public bool IsActive { get; set; } = true;

        [Display(Name = "Image URL")]
        public string? ImageUrl { get; set; }

        // Navigation property
        public virtual ICollection<Reservation> Reservations { get; set; } = new List<Reservation>();
    }
}