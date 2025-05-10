using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using CasaHeights.Data;
using CasaHeights.Models;
using System;
using System.Linq;
using System.Threading.Tasks;
using System.Security.Claims;

namespace CasaHeights.Controllers
{
    [Authorize(Roles = "Admin")]
    public class ReservationController : Controller
    {
        private readonly AppDbContext _context;

        public ReservationController(AppDbContext context)
        {
            _context = context;
        }

        // Display all reservations with filtering options
        public async Task<IActionResult> Index(string status = null)
        {
            System.Diagnostics.Debug.WriteLine("Admin - Loading all reservations");
            
            try
            {
                // Start with a base query
                IQueryable<Reservation> query = _context.Reservations
                    .Include(r => r.Facility)
                    .Include(r => r.User)
                    .Include(r => r.ProcessedBy);

                // Apply status filter if provided
                if (!string.IsNullOrEmpty(status) && Enum.TryParse<ReservationStatus>(status, out var statusEnum))
                {
                    query = query.Where(r => r.Status == statusEnum);
                    System.Diagnostics.Debug.WriteLine($"Filtering by status: {status}");
                }

                // Apply ordering at the end
                var orderedQuery = query.OrderByDescending(r => r.CreatedDate);
                
                var reservations = await orderedQuery.ToListAsync();
                
                // Add debug info for the view
                ViewBag.StatusFilter = status;
                ViewBag.TotalReservations = await _context.Reservations.CountAsync();
                ViewBag.FilteredCount = reservations.Count;
                
                System.Diagnostics.Debug.WriteLine($"Found {reservations.Count} reservations for admin view");
                foreach (var res in reservations.Take(5)) // Just log the first 5 for brevity
                {
                    System.Diagnostics.Debug.WriteLine($"Reservation ID: {res.Id}, Facility: {res.Facility?.Name}, User: {res.User?.Email}, Status: {res.Status}");
                }
                
                return View(reservations);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"ERROR loading reservations for admin: {ex.Message}");
                TempData["ErrorMessage"] = $"Error loading reservations: {ex.Message}";
                return View(new List<Reservation>());
            }
        }

        // GET: Reservation/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var reservation = await _context.Reservations
                .Include(r => r.Facility)
                .Include(r => r.User)
                .Include(r => r.ProcessedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (reservation == null)
            {
                return NotFound();
            }

            return View(reservation);
        }

        // POST: Reservation/Approve/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Approve(int id, string comments)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = ReservationStatus.Approved;
            reservation.ProcessedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            reservation.ProcessedDate = DateTime.Now;
            reservation.ApprovalDate = DateTime.Now;
            reservation.AdminComments = comments;

            _context.Update(reservation);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Reservation has been approved successfully.";
            
            return RedirectToAction(nameof(Details), new { id = reservation.Id });
        }

        // POST: Reservation/Reject/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Reject(int id, string rejectionReason)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            reservation.Status = ReservationStatus.Rejected;
            reservation.ProcessedById = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            reservation.ProcessedDate = DateTime.Now;
            reservation.RejectionDate = DateTime.Now;
            reservation.RejectionReason = rejectionReason;

            _context.Update(reservation);
            await _context.SaveChangesAsync();
            
            TempData["SuccessMessage"] = "Reservation has been rejected.";
            
            return RedirectToAction(nameof(Details), new { id = reservation.Id });
        }

        // POST: Reservation/RecordPayment/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> RecordPayment(int id, string paymentMethod, string paymentReference, decimal paymentAmount)
        {
            var reservation = await _context.Reservations.FindAsync(id);
            if (reservation == null)
            {
                return NotFound();
            }

            if (reservation.Status != ReservationStatus.Approved)
            {
                TempData["ErrorMessage"] = "Only approved reservations can be marked as paid.";
                return RedirectToAction(nameof(Details), new { id = reservation.Id });
            }

            reservation.IsPaid = true;
            reservation.PaymentDate = DateTime.Now;
            reservation.PaymentMethod = paymentMethod;
            reservation.PaymentReference = paymentReference;

            // Update the amount if it differs from the original
            if (paymentAmount != reservation.TotalAmount)
            {
                reservation.TotalAmount = paymentAmount;
            }

            _context.Update(reservation);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Payment has been recorded successfully.";
            return RedirectToAction(nameof(Details), new { id = reservation.Id });
        }

        // GET: Reservation/Calendar
        public IActionResult Calendar()
        {
            return View();
        }

        // GET JSON data for calendar
        [HttpGet]
        public async Task<IActionResult> GetCalendarEvents()
        {
            var reservations = await _context.Reservations
                .Include(r => r.Facility)
                .Include(r => r.User)
                .Where(r => r.Status != ReservationStatus.Rejected && r.Status != ReservationStatus.Cancelled)
                .Select(r => new
                {
                    id = r.Id,
                    title = r.Facility.Name + " - " + r.User.FullName,
                    start = r.StartTime.ToString("o"),
                    end = r.EndTime.ToString("o"),
                    status = r.Status.ToString(),
                    color = r.Status == ReservationStatus.Approved ? "#28a745" : 
                            r.Status == ReservationStatus.Pending ? "#ffc107" : "#6c757d"
                })
                .ToListAsync();

            return Json(reservations);
        }
    }
}