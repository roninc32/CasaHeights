// Controllers/ServiceRequestController.cs
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
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;

namespace CasaHeights.Controllers
{
    [Authorize]
    public class ServiceRequestController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ServiceRequestController(
            AppDbContext context, 
            UserManager<Users> userManager,
            IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _userManager = userManager;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET: ServiceRequest/MyRequests
        public async Task<IActionResult> MyRequests()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var requests = await _context.ServiceRequests
                .Include(r => r.AssignedStaff)
                .Where(r => r.ResidentId == userId)
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();

            return View("MyRequests", requests);  // Changed from "MyRequest" to "MyRequests"
        }

        // GET: ServiceRequest/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.ServiceRequests
                .Include(r => r.AssignedStaff)
                .FirstOrDefaultAsync(m => m.Id == id && m.ResidentId == userId);

            if (request == null)
            {
                return NotFound();
            }

            return View(request);
        }

        // GET: ServiceRequest/Create
        public IActionResult Create()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var viewModel = new ServiceRequestViewModel
            {
                UserId = userId,
                CreatedDate = DateTime.Now,
                Status = ServiceRequestStatus.New
            };
            return View(viewModel);
        }

        // POST: ServiceRequest/Create
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create(ServiceRequestViewModel viewModel, IFormFile? attachment)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values
                        .SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage)
                        .ToList();
                    
                    TempData["ErrorMessage"] = string.Join("<br>", errors);
                    return View(viewModel);
                }

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                {
                    TempData["ErrorMessage"] = "User not authenticated. Please log in again.";
                    return View(viewModel);
                }

                // Set both UserId and ResidentId
                viewModel.UserId = userId;
                viewModel.ResidentId = userId;

                var request = new ServiceRequest
                {
                    Title = viewModel.Title.Trim(),
                    Description = viewModel.Description.Trim(),
                    RequestType = viewModel.RequestType,
                    Priority = viewModel.Priority,
                    Location = viewModel.Location?.Trim(),
                    CreatedDate = DateTime.Now,
                    Status = ServiceRequestStatus.New,
                    ResidentId = userId  // Use userId directly here
                };

                if (attachment != null)
                {
                    try
                    {
                        request.AttachmentUrl = await UploadAttachment(attachment);
                    }
                    catch (Exception ex)
                    {
                        TempData["ErrorMessage"] = $"File upload failed: {ex.Message}";
                        return View(viewModel);
                    }
                }

                _context.ServiceRequests.Add(request);
                await _context.SaveChangesAsync();

                TempData["SuccessMessage"] = "Your service request has been submitted successfully.";
                return RedirectToAction(nameof(MyRequests));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "An error occurred while submitting your request. Please try again.";
                return View(viewModel);
            }
        }

        // GET: ServiceRequest/Cancel/5
        public async Task<IActionResult> Cancel(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.ServiceRequests
                .Include(r => r.AssignedStaff)
                .Include(r => r.Resident)
                .FirstOrDefaultAsync(m => m.Id == id && m.ResidentId == userId);

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != ServiceRequestStatus.New && request.Status != ServiceRequestStatus.OnHold)
            {
                TempData["ErrorMessage"] = "Only new or on-hold requests can be cancelled.";
                return RedirectToAction(nameof(MyRequests));
            }

            return View(request);
        }

        // POST: ServiceRequest/Cancel/5
        [HttpPost, ActionName("Cancel")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CancelConfirmed(int id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(m => m.Id == id && m.ResidentId == userId);

            if (request == null)
            {
                return NotFound();
            }

            if (request.Status != ServiceRequestStatus.New && request.Status != ServiceRequestStatus.OnHold)
            {
                TempData["ErrorMessage"] = "Only new or on-hold requests can be cancelled.";
                return RedirectToAction(nameof(MyRequests));
            }

            request.Status = ServiceRequestStatus.Cancelled;
            _context.Update(request);
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Your service request has been cancelled successfully.";
            return RedirectToAction(nameof(MyRequests));
        }

        private async Task<string> UploadAttachment(IFormFile file)
        {
            if (file.Length > 5 * 1024 * 1024) // 5MB limit
                throw new InvalidOperationException("File size exceeds 5MB limit");

            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "service-requests");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString() + "_" + file.FileName;
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/service-requests/{uniqueFileName}";
        }
    }
}
