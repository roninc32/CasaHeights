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
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var reservations = await _context.Reservations
                .Include(r => r.Facility)
                .Where(r => r.UserId == userId)
                .OrderByDescending(r => r.ReservationDate)
                .ToListAsync();

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

        // GET: ReservationUser/Create
        public async Task<IActionResult> Create(int? id, DateTime? date)
        {
            if (id == null)
            {
                return RedirectToAction("Index", "FacilityUser");
            }

            var facility = await _context.Facilities.FindAsync(id);
            if (facility == null || !facility.IsActive)
            {
                TempData["ErrorMessage"] = "Facility not found or is currently unavailable.";
                return RedirectToAction("Index", "FacilityUser");
            }

            var viewModel = new ReservationCreateViewModel
            {
                FacilityId = facility.Id,
                FacilityName = facility.Name,
                HourlyRate = facility.HourlyRate,
                MinimumHours = facility.MinimumReservationHours,
                MaximumHours = facility.MaximumReservationHours,
                OpeningTime = facility.OpeningTime,
                ClosingTime = facility.ClosingTime,
                MaintenanceDay = facility.MaintenanceDay,
                ImageUrl = facility.ImageUrl,
                Date = date ?? DateTime.Today.AddDays(1),
                // Set default start time to facility opening time
                StartHour = facility.OpeningTime.Hours,
                StartMinute = 0,
                // Set default end time to 1 hour after opening
                EndHour = facility.OpeningTime.Hours + 1,
                EndMinute = 0
            };

            return View(viewModel);
        }

        // POST: ReservationUser/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ReservationCreateViewModel viewModel)
        {
            if (ModelState.IsValid)
            {
                var facility = await _context.Facilities.FindAsync(viewModel.FacilityId);
                if (facility == null || !facility.IsActive)
                {
                    TempData["ErrorMessage"] = "Selected facility is no longer available.";
                    return RedirectToAction("Index", "FacilityUser");
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
                    ModelState.AddModelError("", "You cannot book a facility in the past.");
                    return View(viewModel);
                }

                // Check for maintenance day
                if (facility.MaintenanceDay.HasValue && (int)viewModel.Date.DayOfWeek == (int)facility.MaintenanceDay.Value)
                {
                    ModelState.AddModelError("Date", $"The facility is unavailable on {facility.MaintenanceDay}s due to scheduled maintenance.");
                    return View(viewModel);
                }

                // Calculate duration
                double durationHours = (endTime - startTime).TotalHours;

                // Validate duration
                if (durationHours < facility.MinimumReservationHours)
                {
                    ModelState.AddModelError("", $"Minimum reservation duration is {facility.MinimumReservationHours} hours.");
                    return View(viewModel);
                }

                if (durationHours > facility.MaximumReservationHours)
                {
                    ModelState.AddModelError("", $"Maximum reservation duration is {facility.MaximumReservationHours} hours.");
                    return View(viewModel);
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
                    ModelState.AddModelError("", "The selected time slot conflicts with an existing reservation.");
                    return View(viewModel);
                }

                // Calculate total cost
                decimal totalCost = facility.HourlyRate * (decimal)durationHours;

                // Create the reservation
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                var reservation = new Reservation
                {
                    FacilityId = viewModel.FacilityId,
                    UserId = userId,
                    ReservationDate = viewModel.Date,
                    StartTime = startTime,
                    EndTime = endTime,
                    TotalAmount = totalCost,
                    Status = ReservationStatus.Pending,
                    Purpose = viewModel.Purpose,
                    AttendeeCount = viewModel.AttendeeCount,
                    SpecialRequests = viewModel.Notes,
                    CreatedDate = DateTime.Now
                };

                _context.Add(reservation);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your reservation has been submitted and is pending approval.";
                return RedirectToAction(nameof(MyReservations));
            }

            return View(viewModel);
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