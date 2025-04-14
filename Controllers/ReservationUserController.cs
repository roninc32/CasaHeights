using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CasaHeights.Data;
using CasaHeights.Models;
using CasaHeights.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using System.Security.Claims;

namespace CasaHeights.Controllers
{
    [Authorize]
    public class ReservationUserController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ReservationUserController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: ReservationUser/MyReservations
        public async Task<IActionResult> MyReservations()
        {
            // Check for success message in session as a backup
            if (HttpContext.Session.TryGetValue("LastBookingSuccess", out byte[] messageBytes) && 
                TempData["SuccessMessage"] == null)
            {
                string message = System.Text.Encoding.UTF8.GetString(messageBytes);
                TempData["SuccessMessage"] = message;
                // Clear the session variable after using it
                HttpContext.Session.Remove("LastBookingSuccess");
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            System.Diagnostics.Debug.WriteLine($"Loading reservations for user ID: {userId}");

            var reservations = await _context.Reservations
                .Include(r => r.Facility)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();
                
            System.Diagnostics.Debug.WriteLine($"Found {reservations.Count} reservations for user");

            return View(reservations);
        }

        // GET: ReservationUser/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Facility)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // Controllers/ReservationUserController.cs
        [HttpGet]
        public async Task<IActionResult> Create(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var facility = await _context.Facilities
                .FirstOrDefaultAsync(f => f.Id == id && f.IsActive);

            if (facility == null)
            {
                return NotFound();
            }

            var viewModel = new ReservationCreateViewModel
            {
                FacilityId = facility.Id,
                FacilityName = facility.Name,
                ImageUrl = facility.ImageUrl,
                HourlyRate = facility.HourlyRate,
                MinimumHours = facility.MinimumReservationHours,
                MaximumHours = facility.MaximumReservationHours,
                // Ensure valid TimeSpan values
                OpeningTime = facility.OpeningTime != default ? facility.OpeningTime : new TimeSpan(8, 0, 0),
                ClosingTime = facility.ClosingTime != default ? facility.ClosingTime : new TimeSpan(22, 0, 0),
                Date = DateTime.Today.AddDays(1)
            };

            return View(viewModel);
        }

        // POST: ReservationUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel viewModel)
        {
            // Debug logging
            System.Diagnostics.Debug.WriteLine($"Received booking form submission:");
            System.Diagnostics.Debug.WriteLine($"FacilityId: {viewModel.FacilityId}");
            System.Diagnostics.Debug.WriteLine($"FacilityName: {viewModel.FacilityName}");
            System.Diagnostics.Debug.WriteLine($"MinHours: {viewModel.MinimumHours}");
            System.Diagnostics.Debug.WriteLine($"MaxHours: {viewModel.MaximumHours}");

            if (!ModelState.IsValid)
            {
                var errors = string.Join(", ", ModelState.Values
                    .SelectMany(v => v.Errors)
                    .Select(e => e.ErrorMessage));
                return Json(new { success = false, message = errors });
            }

            try
            {
                if (!ModelState.IsValid)
                {
                    return Json(new { 
                        success = false, 
                        message = string.Join(" ", ModelState.Values
                            .SelectMany(v => v.Errors)
                            .Select(e => e.ErrorMessage))
                    });
                }

                var facility = await _context.Facilities.FindAsync(viewModel.FacilityId);
                if (facility == null || !facility.IsActive)
                {
                    return Json(new { 
                        success = false, 
                        message = "Selected facility is no longer available."
                    });
                }

                // Calculate start and end times
                var startTime = new DateTime(
                    viewModel.Date.Year, viewModel.Date.Month, viewModel.Date.Day,
                    viewModel.StartHour, viewModel.StartMinute, 0);
                
                var endTime = new DateTime(
                    viewModel.Date.Year, viewModel.Date.Month, viewModel.Date.Day,
                    viewModel.EndHour, viewModel.EndMinute, 0);

                // Validate times
                if (startTime < DateTime.Now)
                {
                    return Json(new { 
                        success = false, 
                        message = "You cannot book a facility in the past."
                    });
                }

                // Check for maintenance day
                if (facility.MaintenanceDay.HasValue && (int)viewModel.Date.DayOfWeek == (int)facility.MaintenanceDay.Value)
                {
                    return Json(new { 
                        success = false, 
                        message = $"The facility is unavailable on {facility.MaintenanceDay}s due to scheduled maintenance."
                    });
                }

                // Calculate and validate duration
                double durationHours = (endTime - startTime).TotalHours;
                if (durationHours < facility.MinimumReservationHours)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Minimum reservation duration is {facility.MinimumReservationHours} hours."
                    });
                }

                if (durationHours > facility.MaximumReservationHours)
                {
                    return Json(new { 
                        success = false, 
                        message = $"Maximum reservation duration is {facility.MaximumReservationHours} hours."
                    });
                }

                // Check for conflicts
                var conflictingReservations = await _context.Reservations
                    .Where(r => r.FacilityId == viewModel.FacilityId && 
                            r.Status != ReservationStatus.Cancelled &&
                            r.Status != ReservationStatus.Rejected &&
                            r.ReservationDate.Date == viewModel.Date.Date &&
                            ((r.StartTime <= startTime && r.EndTime > startTime) ||
                            (r.StartTime < endTime && r.EndTime >= endTime) ||
                            (r.StartTime >= startTime && r.EndTime <= endTime)))
                    .ToListAsync();

                if (conflictingReservations.Any())
                {
                    return Json(new { 
                        success = false, 
                        message = "The selected time slot conflicts with an existing reservation."
                    });
                }

                // Get current user ID
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    return Json(new { 
                        success = false, 
                        message = "Unable to identify user. Please try logging out and back in."
                    });
                }

                // Create and save the reservation
                var reservation = new Reservation
                {
                    FacilityId = viewModel.FacilityId,
                    UserId = userId,
                    ReservationDate = viewModel.Date,
                    StartTime = startTime,
                    EndTime = endTime,
                    TotalAmount = facility.HourlyRate * (decimal)durationHours,
                    Status = ReservationStatus.Pending,
                    Purpose = viewModel.Purpose,
                    AttendeeCount = viewModel.AttendeeCount,
                    SpecialRequests = viewModel.Notes,
                    CreatedDate = DateTime.Now
                };

                _context.Reservations.Add(reservation);
                await _context.SaveChangesAsync();

                return Json(new { 
                    success = true, 
                    message = $"Your reservation for {facility.Name} has been submitted and is pending approval.",
                    redirectUrl = Url.Action("MyReservations", "ReservationUser")
                });
            }
                catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Error: {ex.Message}");
                return Json(new { 
                    success = false, 
                    message = "An error occurred while processing your reservation." 
                });
            }
        }

        // GET: ReservationUser/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .Include(r => r.Facility)
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != ReservationStatus.Pending && 
                reservation.Status != ReservationStatus.Approved)
            {
                TempData["ErrorMessage"] = "You can only cancel pending or approved reservations.";
                return RedirectToAction(nameof(MyReservations));
            }

            if (reservation.StartTime <= DateTime.Now.AddHours(24))
            {
                TempData["ErrorMessage"] = "Reservations can only be cancelled at least 24 hours in advance.";
                return RedirectToAction(nameof(MyReservations));
            }

            return View(reservation);
        }

        // POST: ReservationUser/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id, string cancellationReason)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var reservation = await _context.Reservations
                .FirstOrDefaultAsync(m => m.Id == id && m.UserId == userId);

            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != ReservationStatus.Pending && 
                reservation.Status != ReservationStatus.Approved)
            {
                TempData["ErrorMessage"] = "You can only cancel pending or approved reservations.";
                return RedirectToAction(nameof(MyReservations));
            }

            if (reservation.StartTime <= DateTime.Now.AddHours(24))
            {
                TempData["ErrorMessage"] = "Reservations can only be cancelled at least 24 hours in advance.";
                return RedirectToAction(nameof(MyReservations));
            }

            reservation.Status = ReservationStatus.Cancelled;
            reservation.CancellationReason = cancellationReason;
            reservation.CancellationDate = DateTime.Now;

            _context.Update(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your reservation has been cancelled successfully.";
            return RedirectToAction(nameof(MyReservations));
        }

        // GET: ReservationUser/CheckAvailability
        [HttpGet]
        public async Task<IActionResult> CheckAvailability(int facilityId, DateTime date)
        {
            var facility = await _context.Facilities.FindAsync(facilityId);
            if (facility == null)
            {
                return Json(new { success = false, message = "Facility not found." });
            }

            // Check if the selected date is a maintenance day
            if (facility.MaintenanceDay.HasValue && (int)date.DayOfWeek == (int)facility.MaintenanceDay.Value)
            {
                return Json(new { 
                    success = false, 
                    message = $"The facility is unavailable on {facility.MaintenanceDay}s due to scheduled maintenance." 
                });
            }

            // Get all bookings for the selected date
            var bookings = await _context.Reservations
                .Where(r => r.FacilityId == facilityId &&
                       r.ReservationDate.Date == date.Date &&
                       (r.Status == ReservationStatus.Pending || r.Status == ReservationStatus.Approved))
                .ToListAsync();

            // Generate time slots
            var openingHour = facility.OpeningTime.Hours;
            var closingHour = facility.ClosingTime.Hours;
            var slots = new List<object>();

            for (int hour = openingHour; hour < closingHour; hour++)
            {
                var slotStart = new DateTime(date.Year, date.Month, date.Day, hour, 0, 0);
                var slotEnd = slotStart.AddHours(1);

                bool isAvailable = !bookings.Any(b => 
                    (b.StartTime < slotEnd && b.EndTime > slotStart));

                slots.Add(new { 
                    hour = hour,
                    start = slotStart.ToString("h:mm tt"),
                    end = slotEnd.ToString("h:mm tt"),
                    available = isAvailable
                });
            }

            return Json(new { success = true, slots = slots });
        }
    }
}