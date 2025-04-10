using System;
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.ViewModels
{
    public class ReservationCreateViewModel
    {
        // Facility details
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public decimal HourlyRate { get; set; }
        public int MinimumHours { get; set; }
        public int MaximumHours { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan ClosingTime { get; set; }
        public DayOfWeek? MaintenanceDay { get; set; }
        public string ImageUrl { get; set; }

        // Reservation details
        [Required]
        [Display(Name = "Date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required]
        [Display(Name = "Start Hour")]
        [Range(0, 23, ErrorMessage = "Start hour must be between 0 and 23")]
        public int StartHour { get; set; }

        [Required]
        [Display(Name = "Start Minute")]
        [Range(0, 59, ErrorMessage = "Start minute must be between 0 and 59")]
        public int StartMinute { get; set; }

        [Required]
        [Display(Name = "End Hour")]
        [Range(0, 23, ErrorMessage = "End hour must be between 0 and 23")]
        public int EndHour { get; set; }

        [Required]
        [Display(Name = "End Minute")]
        [Range(0, 59, ErrorMessage = "End minute must be between 0 and 59")]
        public int EndMinute { get; set; }

        [Required]
        [Display(Name = "Purpose of Reservation")]
        [StringLength(200, ErrorMessage = "Purpose cannot be longer than 200 characters")]
        public string Purpose { get; set; }

        [Required]
        [Display(Name = "Number of Attendees")]
        [Range(1, 500, ErrorMessage = "Number of attendees must be between 1 and 500")]
        public int AttendeeCount { get; set; }

        [Display(Name = "Additional Notes")]
        [StringLength(500, ErrorMessage = "Notes cannot be longer than 500 characters")]
        public string Notes { get; set; }

        // Helper properties for display
        public string FormattedOpeningTime => DateTime.Today.Add(OpeningTime).ToString("h:mm tt");
        public string FormattedClosingTime => DateTime.Today.Add(ClosingTime).ToString("h:mm tt");
    }
}