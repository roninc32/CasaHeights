// ViewModels/ReservationCreateViewModel.cs
using System.ComponentModel.DataAnnotations;

namespace CasaHeights.ViewModels
{
    public class ReservationCreateViewModel
    {
        public int FacilityId { get; set; }

        [Required(ErrorMessage = "Facility name is required")]
        public string FacilityName { get; set; }

        // Make ImageUrl optional
        public string? ImageUrl { get; set; }

        [Required(ErrorMessage = "Please select a date")]
        [DataType(DataType.Date)]
        public DateTime Date { get; set; }

        [Required(ErrorMessage = "Please select start hour")]
        [Range(0, 23, ErrorMessage = "Start hour must be between 0 and 23")]
        public int StartHour { get; set; }

        [Required(ErrorMessage = "Please select start minute")]
        [Range(0, 59, ErrorMessage = "Start minute must be between 0 and 59")]
        public int StartMinute { get; set; }

        [Required(ErrorMessage = "Please select end hour")]
        [Range(0, 23, ErrorMessage = "End hour must be between 0 and 23")]
        public int EndHour { get; set; }

        [Required(ErrorMessage = "Please select end minute")]
        [Range(0, 59, ErrorMessage = "End minute must be between 0 and 59")]
        public int EndMinute { get; set; }

        [Required(ErrorMessage = "Please enter the purpose of reservation")]
        [StringLength(500, ErrorMessage = "Purpose cannot exceed 500 characters")]
        public string Purpose { get; set; }

        [Required(ErrorMessage = "Please enter number of attendees")]
        [Range(1, 1000, ErrorMessage = "Attendee count must be between 1 and 1000")]
        public int AttendeeCount { get; set; }

        public string? Notes { get; set; }

        [Required]
        [Range(0, double.MaxValue, ErrorMessage = "Hourly rate must be greater than 0")]
        public decimal HourlyRate { get; set; }

        [Required]
        [Range(1, 24, ErrorMessage = "Minimum hours must be between 1 and 24")]
        public int MinimumHours { get; set; }

        [Required]
        [Range(1, 24, ErrorMessage = "Maximum hours must be between 1 and 24")]
        public int MaximumHours { get; set; }

        public TimeSpan OpeningTime { get; set; } = new TimeSpan(8, 0, 0); // Default to 8:00 AM
        public TimeSpan ClosingTime { get; set; } = new TimeSpan(22, 0, 0); // Default to 10:00 PM

        // Formatted display properties
        public string FormattedOpeningTime => OpeningTime != default
        ? DateTime.Today.Add(OpeningTime).ToString("hh:mm tt")
        : "08:00 AM";

        public string FormattedClosingTime => ClosingTime != default
            ? DateTime.Today.Add(ClosingTime).ToString("hh:mm tt")
            : "10:00 PM";
    }
}