using CasaHeights.Data;
using CasaHeights.Models;
using CasaHeights.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaHeights.Controllers
{
    public class AnnouncementsController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly INotificationService _notificationService;

        public AnnouncementsController(
            AppDbContext context, 
            UserManager<Users> userManager,
            INotificationService notificationService)
        {
            _context = context;
            _userManager = userManager;
            _notificationService = notificationService;
        }

        // GET: Announcements - visible to all users
        public async Task<IActionResult> Index()
        {
            // Get current active announcements
            var announcements = await _context.Announcements
                .Where(a => a.ExpiryDate >= DateTime.Now)
                .OrderByDescending(a => a.IsUrgent)
                .ThenByDescending(a => a.PostedDate)
                .Include(a => a.CreatedBy)
                .ToListAsync();

            return View(announcements);
        }

        // GET: Announcements/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);

            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        // Admin actions below
        [Authorize(Roles = "Admin")]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Create([Bind("Title,Content,ExpiryDate,IsUrgent,SendEmail,SendSMS")] Announcement announcement)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Create action initiated for: {announcement.Title}");
                
                // Important: Always validate manually since we're excluding CreatedById from binding
                if (!ModelState.IsValid)
                {
                    // Log validation errors
                    System.Diagnostics.Debug.WriteLine("MODEL VALIDATION FAILED");
                    foreach (var modelState in ModelState.Values)
                    {
                        foreach (var error in modelState.Errors)
                        {
                            System.Diagnostics.Debug.WriteLine($"Validation error: {error.ErrorMessage}");
                        }
                    }
                    return View(announcement);
                }
                
                // Set required fields that aren't in the form
                announcement.PostedDate = DateTime.Now;
                
                // Get current user ID
                try
                {
                    var userId = _userManager.GetUserId(User);
                    System.Diagnostics.Debug.WriteLine($"Current user ID: {userId}");
                    
                    // Only set if we have a valid user ID
                    if (!string.IsNullOrEmpty(userId))
                    {
                        announcement.CreatedById = userId;
                    }
                }
                catch (Exception userEx)
                {
                    // Log the exception but continue with null CreatedById
                    System.Diagnostics.Debug.WriteLine($"Error getting user: {userEx.Message}");
                }
                
                try
                {
                    _context.Announcements.Add(announcement);
                    await _context.SaveChangesAsync();
                    System.Diagnostics.Debug.WriteLine("SaveChanges completed successfully");
                    
                    TempData["SuccessMessage"] = "Announcement was created successfully!";
                    
                    // Handle notifications if configured
                    if (announcement.SendEmail || announcement.SendSMS)
                    {
                        await SendNotifications(announcement);
                    }
                    
                    return RedirectToAction(nameof(Index));
                }
                catch (Exception dbEx)
                {
                    // Log the database exception
                    System.Diagnostics.Debug.WriteLine($"DATABASE ERROR: {dbEx.Message}");
                    System.Diagnostics.Debug.WriteLine($"Inner exception: {dbEx.InnerException?.Message}");
                    
                    ModelState.AddModelError(string.Empty, "Database error occurred while saving the announcement.");
                    TempData["ErrorMessage"] = $"Error: {dbEx.Message}";
                    return View(announcement);
                }
            }
            catch (Exception ex)
            {
                // Log any other exceptions
                System.Diagnostics.Debug.WriteLine($"ERROR CREATING ANNOUNCEMENT: {ex.Message}");
                
                ModelState.AddModelError(string.Empty, "An error occurred while creating the announcement.");
                TempData["ErrorMessage"] = $"Error: {ex.Message}";
                return View(announcement);
            }
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }
            return View(announcement);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Content,ExpiryDate,IsUrgent,SendEmail,SendSMS")] Announcement announcement)
        {
            if (id != announcement.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existingAnnouncement = await _context.Announcements.FindAsync(id);
                    existingAnnouncement.Title = announcement.Title;
                    existingAnnouncement.Content = announcement.Content;
                    existingAnnouncement.ExpiryDate = announcement.ExpiryDate;
                    existingAnnouncement.IsUrgent = announcement.IsUrgent;
                    
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!AnnouncementExists(announcement.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            return View(announcement);
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements
                .Include(a => a.CreatedBy)
                .FirstOrDefaultAsync(m => m.Id == id);
                
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            _context.Announcements.Remove(announcement);
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> SendNotification(int id)
        {
            var announcement = await _context.Announcements.FindAsync(id);
            if (announcement == null)
            {
                return NotFound();
            }

            await SendNotifications(announcement);
            
            TempData["Message"] = "Notifications sent successfully";
            return RedirectToAction(nameof(Details), new { id = id });
        }

        private async Task SendNotifications(Announcement announcement)
        {
            var users = await _userManager.Users.ToListAsync();
            
            foreach (var user in users)
            {
                // Send email notification
                if (announcement.SendEmail && !string.IsNullOrEmpty(user.Email))
                {
                    await _notificationService.SendEmailNotificationAsync(
                        user.Email,
                        $"Casa Heights: {(announcement.IsUrgent ? "URGENT - " : "")}{announcement.Title}",
                        $@"<h2>{announcement.Title}</h2>
                          <p>{announcement.Content}</p>
                          <p>Posted: {announcement.PostedDate}</p>"
                    );
                }
                
                // Send SMS notification
                if (announcement.SendSMS && !string.IsNullOrEmpty(user.PhoneNumber))
                {
                    string smsContent = $"{(announcement.IsUrgent ? "URGENT: " : "")}Casa Heights: {announcement.Title} - {announcement.Content.Substring(0, Math.Min(announcement.Content.Length, 100))}";
                    if (announcement.Content.Length > 100)
                        smsContent += "...";
                        
                    await _notificationService.SendSmsNotificationAsync(user.PhoneNumber, smsContent);
                }
            }
        }

        // Add this action to your AnnouncementsController
        [Authorize] // This allows any authenticated user to access
        public async Task<IActionResult> UserAnnouncements()
        {
            // Get current active announcements
            var announcements = await _context.Announcements
                .Where(a => a.ExpiryDate >= DateTime.Now)
                .OrderByDescending(a => a.IsUrgent)
                .ThenByDescending(a => a.PostedDate)
                .ToListAsync();

            return View(announcements);
        }

        // Also add a details view for users
        [Authorize]
        public async Task<IActionResult> UserAnnouncementDetails(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var announcement = await _context.Announcements
                .FirstOrDefaultAsync(m => m.Id == id);
            if (announcement == null)
            {
                return NotFound();
            }

            return View(announcement);
        }

        private bool AnnouncementExists(int id)
        {
            return _context.Announcements.Any(e => e.Id == id);
        }
    }
}