using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CasaHeights.Data;
using CasaHeights.Models;
using Microsoft.AspNetCore.Authorization;
using System;
using System.Linq;
using System.Threading.Tasks;

namespace CasaHeights.Controllers
{
    [Authorize] // All logged-in users can access
    public class FacilityUserController : Controller
    {
        private readonly AppDbContext _context;

        public FacilityUserController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index()
        {
            var facilities = await _context.Facilities
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .ToListAsync();

            return View(facilities);
        }

        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(m => m.Id == id && m.IsActive);

            if (facility == null)
            {
                return NotFound();
            }

            // Get upcoming reservations for this facility
            var upcomingReservations = await _context.Reservations
                .Where(r => r.FacilityId == id &&
                           r.StartTime >= DateTime.Now &&
                           r.Status == ReservationStatus.Approved)
                .OrderBy(r => r.StartTime)
                .Take(5)
                .ToListAsync();

            ViewBag.UpcomingReservations = upcomingReservations;

            return View(facility);
        }

        // AJAX endpoint to check availability
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int facilityId, DateTime date)
        {
            try
            {
                var facility = await _context.Facilities.FindAsync(facilityId);
                if (facility == null || !facility.IsActive)
                {
                    return Json(new { success = false, message = "Facility not found or unavailable" });
                }

                // Check if maintenance day
                if (facility.MaintenanceDay.HasValue && (int)date.DayOfWeek == (int)facility.MaintenanceDay.Value)
                {
                    return Json(new { success = false, message = "This day is reserved for maintenance" });
                }

                // Get existing reservations for this date
                var existingReservations = await _context.Reservations
                    .Where(r => r.FacilityId == facilityId &&
                            r.StartTime.Date == date.Date &&
                            (r.Status == ReservationStatus.Approved || r.Status == ReservationStatus.Pending))
                    .Select(r => new { Start = r.StartTime.TimeOfDay, End = r.EndTime.TimeOfDay })
                    .ToListAsync();

                // Generate hourly time slots
                var timeSlots = new List<object>();
                
                // Create a base date to add the timespans to (for AM/PM formatting)
                DateTime baseDate = date.Date;
                
                // Loop through hours and create slots
                for (var hour = facility.OpeningTime.Hours; hour < facility.ClosingTime.Hours; hour++)
                {
                    // Create TimeSpans for start and end times
                    TimeSpan startTime = new TimeSpan(hour, 0, 0);
                    TimeSpan endTime = new TimeSpan(hour + 1, 0, 0);
                    
                    // Check if this time slot overlaps with any existing reservation
                    bool isAvailable = !existingReservations.Any(r => 
                        (r.Start < endTime && r.End > startTime)
                    );
                    
                    // Create DateTime objects for formatting
                    DateTime startDateTime = baseDate.Add(startTime);
                    DateTime endDateTime = baseDate.Add(endTime);
                    
                    timeSlots.Add(new
                    {
                        start = startDateTime.ToString("h:mm tt"),
                        end = endDateTime.ToString("h:mm tt"),
                        rawStartHour = hour,
                        rawEndHour = hour + 1,
                        available = isAvailable
                    });
                }

                return Json(new { 
                success = true, 
                slots = timeSlots,
                minHours = facility.MinimumReservationHours, // Direct assignment
                maxHours = facility.MaximumReservationHours, // Direct assignment
                rate = facility.HourlyRate,
                date = date.ToString("yyyy-MM-dd")
            });
            }
            catch (Exception ex)
            {
                // Log the exception details
                return Json(new { success = false, message = "An error occurred while checking availability: " + ex.Message });
            }
        }
    }
}