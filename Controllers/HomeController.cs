using System.Diagnostics;
using CasaHeights.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CasaHeights.Data;
using Microsoft.EntityFrameworkCore;

namespace CasaHeights.Controllers
{
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly AppDbContext _context;

        public HomeController(ILogger<HomeController> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public IActionResult Index()
        {
            return View();
        }

        [Authorize]
        public IActionResult Privacy()
        {
            return View();
        }

        [Authorize(Roles = "Admin")]
        public IActionResult Admin()
        {
            return View();
        }

        [Authorize(Roles = "Staff")]
        public IActionResult Staff()
        {
            return View();
        }

        [Authorize(Roles = "User")]
        public async Task<IActionResult> User()
        {
            // Get top facilities for the dashboard
            ViewBag.Facilities = await _context.Facilities
                .Where(f => f.IsActive)
                .OrderBy(f => f.Name)
                .Take(3)
                .ToListAsync();

            // Get recent announcements
            ViewBag.RecentAnnouncements = await _context.Announcements
                .Where(a => a.ExpiryDate >= DateTime.Now)
                .OrderByDescending(a => a.IsUrgent)
                .ThenByDescending(a => a.PostedDate)
                .Take(3)
                .ToListAsync();

            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}