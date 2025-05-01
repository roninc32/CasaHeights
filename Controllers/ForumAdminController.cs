// Controllers/Admin/ForumAdminController.cs
using CasaHeights.Data;
using CasaHeights.Models;
using CasaHeights.Models.Forum;
using CasaHeights.Models.Forum.Enums;
using CasaHeights.ViewModels.Forum.Admin;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CasaHeights.Controllers.Admin
{
    [Authorize(Roles = "Admin")]
    [Route("Admin/Forum")]
    public class ForumAdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;
        private readonly ILogger<ForumAdminController> _logger;

        public ForumAdminController(
            AppDbContext context,
            UserManager<Users> userManager,
            ILogger<ForumAdminController> logger)
        {
            _context = context;
            _userManager = userManager;
            _logger = logger;
        }

        // Update the route attribute
        [HttpGet]
        [Route("")] // This makes it the default action for /Admin/Forum
        public async Task<IActionResult> Index(string filter)
        {
            try
            {
                var query = _context.ForumPosts
                    .Include(p => p.Author)
                    .Include(p => p.Categories)
                    .Include(p => p.Comments)
                    .AsQueryable();

                // Apply filters
                query = filter switch
                {
                    "reported" => query.Where(p => p.Status == PostStatus.Reported),
                    "deleted" => query.Where(p => p.IsDeleted),
                    "pinned" => query.Where(p => p.IsPinned),
                    _ => query.Where(p => !p.IsDeleted)
                };

                var posts = await query
                    .OrderByDescending(p => p.CreatedAt)
                    .ToListAsync();

                var viewModel = new ForumAdminIndexViewModel
                {
                    Posts = posts,
                    Filter = filter,
                    Statistics = new ForumStatistics
                    {
                        TotalPosts = await _context.ForumPosts.CountAsync(),
                        TotalComments = await _context.ForumComments.CountAsync(),
                        ReportedPosts = await _context.ForumPosts.CountAsync(p => p.Status == PostStatus.Reported),
                        ActiveUsers = await _context.ForumPosts
                            .Select(p => p.AuthorId)
                            .Distinct()
                            .CountAsync()
                    }
                };

                return View("~/Views/Admin/Forum/Index.cshtml", viewModel);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error in forum admin index");
                TempData["ErrorMessage"] = "An error occurred while loading the forum administration page.";
                return View("~/Views/Admin/Forum/Index.cshtml", new ForumAdminIndexViewModel());
            }
        }

        // POST: Admin/Forum/DeletePost/5
        [HttpPost("DeletePost/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeletePost(int id, string reason)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            var admin = await _userManager.GetUserAsync(User);
            
            post.IsDeleted = true;
            post.DeletedBy = admin.Id;
            post.DeletedAt = DateTime.UtcNow;
            post.DeletionReason = reason;
            post.Status = PostStatus.Deleted;

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Post has been deleted successfully.";
            return RedirectToAction(nameof(Index));
        }

        // POST: Admin/Forum/PinPost/5
        [HttpPost("PinPost/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> PinPost(int id)
        {
            var post = await _context.ForumPosts.FindAsync(id);
            if (post == null)
            {
                return NotFound();
            }

            post.IsPinned = !post.IsPinned;
            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = post.IsPinned ? 
                "Post has been pinned successfully." : 
                "Post has been unpinned successfully.";
                
            return RedirectToAction(nameof(Index));
        }

        // GET: Admin/Forum/Reports
        [HttpGet("Reports")]
        public async Task<IActionResult> Reports()
        {
            var reports = await _context.ForumReports
                .Include(r => r.Reporter)
                .Include(r => r.Post)
                .Include(r => r.Comment)
                .Where(r => r.Status == ReportStatus.Pending)
                .OrderByDescending(r => r.CreatedAt)
                .ToListAsync();

            return View(reports);
        }

        // POST: Admin/Forum/HandleReport/5
        [HttpPost("HandleReport/{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> HandleReport(int id, ReportStatus status, string notes)
        {
            var report = await _context.ForumReports.FindAsync(id);
            if (report == null)
            {
                return NotFound();
            }

            var admin = await _userManager.GetUserAsync(User);
            
            report.Status = status;
            report.ModeratorId = admin.Id;
            report.ReviewedAt = DateTime.UtcNow;
            report.ModeratorNotes = notes;

            if (status == ReportStatus.Resolved)
            {
                if (report.PostId.HasValue)
                {
                    var post = await _context.ForumPosts.FindAsync(report.PostId);
                    if (post != null)
                    {
                        post.Status = PostStatus.Hidden;
                    }
                }
                else if (report.CommentId.HasValue)
                {
                    var comment = await _context.ForumComments.FindAsync(report.CommentId);
                    if (comment != null)
                    {
                        comment.IsDeleted = true;
                        comment.DeletedBy = admin.Id;
                        comment.DeletedAt = DateTime.UtcNow;
                        comment.DeletionReason = notes;
                    }
                }
            }

            await _context.SaveChangesAsync();

            TempData["SuccessMessage"] = "Report has been handled successfully.";
            return RedirectToAction(nameof(Reports));
        }

        // GET: Admin/Forum/Categories
        [HttpGet("Categories")]
        public async Task<IActionResult> Categories()
        {
            var categories = await _context.ForumCategories
                .OrderBy(c => c.DisplayOrder)
                .ToListAsync();

            return View(categories);
        }

        // POST: Admin/Forum/AddCategory
        [HttpPost("AddCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(ForumCategory category)
        {
            if (ModelState.IsValid)
            {
                _context.ForumCategories.Add(category);
                await _context.SaveChangesAsync();
                TempData["SuccessMessage"] = "Category added successfully.";
            }

            return RedirectToAction(nameof(Categories));
        }
    }
}
