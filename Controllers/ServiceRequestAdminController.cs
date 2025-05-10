// Controllers/ServiceRequestAdminController.cs
using CasaHeights.Data;
using CasaHeights.Models;
using CasaHeights.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using CasaHeights.Services.Interfaces;
using CasaHeights.Data;
using System.ComponentModel.DataAnnotations;


namespace CasaHeights.Controllers
{
    [Authorize(Roles = "Admin,Staff")]
    public class ServiceRequestAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly IUserService _userService;

        public ServiceRequestAdminController(AppDbContext context, IUserService userService)
        {
            _context = context;
            _userService = userService;
        }

        public async Task<IActionResult> Index(string status = null, string type = null)
        {
            var query = _context.ServiceRequests
                .Include(r => r.Resident)
                .Include(r => r.AssignedStaff)
                .AsQueryable();

            if (!string.IsNullOrEmpty(status))
            {
                var statusEnum = Enum.Parse<ServiceRequestStatus>(status);
                query = query.Where(r => r.Status == statusEnum);
            }

            if (!string.IsNullOrEmpty(type))
            {
                var typeEnum = Enum.Parse<ServiceRequestType>(type);
                query = query.Where(r => r.RequestType == typeEnum);
            }

            var requests = await query
                .OrderByDescending(r => r.Priority)
                .ThenByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View(requests);
        }

        public async Task<IActionResult> Details(int id)
        {
            var request = await _context.ServiceRequests
                .Include(r => r.Resident)
                .Include(r => r.AssignedStaff)
                .FirstOrDefaultAsync(r => r.Id == id);

            if (request == null)
                return NotFound();

            return View(request);
        }

        public async Task<IActionResult> Dashboard()
        {
            var today = DateTime.Today;
            var model = new ServiceRequestDashboardViewModel
            {
                NewRequestsCount = await _context.ServiceRequests
                    .CountAsync(r => r.Status == ServiceRequestStatus.New),
                    
                InProgressCount = await _context.ServiceRequests
                    .CountAsync(r => r.Status == ServiceRequestStatus.InProgress),
                    
                CompletedTodayCount = await _context.ServiceRequests
                    .CountAsync(r => r.Status == ServiceRequestStatus.Completed 
                        && r.ProcessedDate >= today),
                        
                EmergencyCount = await _context.ServiceRequests
                    .CountAsync(r => r.Priority == PriorityLevel.Emergency 
                        && r.Status != ServiceRequestStatus.Completed 
                        && r.Status != ServiceRequestStatus.Cancelled),
                        
                RecentRequests = await _context.ServiceRequests
                    .OrderByDescending(r => r.CreatedDate)
                    .Take(10)
                    .ToListAsync(),
                    
                RequestsByType = await _context.ServiceRequests
                    .GroupBy(r => r.RequestType)
                    .Select(g => new RequestTypeStats
                    {
                        Type = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToListAsync(),
                    
                RequestsByStatus = await _context.ServiceRequests
                    .GroupBy(r => r.Status)
                    .Select(g => new RequestStatusStats
                    {
                        Status = g.Key.ToString(),
                        Count = g.Count()
                    })
                    .ToListAsync()
            };

            return View(model);
        }


        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int id, ServiceRequestStatus status, string staffNotes)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.Status = status;
            request.StaffNotes = staffNotes;
            request.StaffId = _userService.GetCurrentUserId();
            request.ProcessedDate = DateTime.Now;

            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignStaff(int id, string staffId)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null)
                return NotFound();

            request.StaffId = staffId;
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Details), new { id });
        }
    }
}
