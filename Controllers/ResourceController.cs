// Controllers/ResourcesController.cs
using CasaHeights.Data;
using CasaHeights.Models;
using CasaHeights.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CasaHeights.Controllers
{
    public class ResourcesController : Controller
    {
        private readonly AppDbContext _context;
        private readonly UserManager<Users> _userManager;

        public ResourcesController(AppDbContext context, UserManager<Users> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // GET: Resources/Contacts
        [Authorize]
        public async Task<IActionResult> Contacts()
        {
            // Get all users
            var users = await _userManager.Users.ToListAsync();
            
            // Create dictionary to store users by role
            var usersByRole = new Dictionary<string, List<UserContactViewModel>>();
            
            // Process each user
            foreach (var user in users)
            {
                // Get user roles
                var roles = await _userManager.GetRolesAsync(user);
                var roleName = roles.FirstOrDefault() ?? "User"; // Default to User if no role found
                
                // Create contact view model
                var contactInfo = new UserContactViewModel
                {
                    Id = user.Id,
                    Name = user.FullName,
                    Email = user.Email,
                    PhoneNumber = user.PhoneNumber,
                    HouseNumber = user.HouseNumber,
                    ProfilePicture = user.ProfilePicture,
                    Role = roleName
                };
                
                // Add to appropriate category
                if (!usersByRole.ContainsKey(roleName))
                {
                    usersByRole[roleName] = new List<UserContactViewModel>();
                }
                
                usersByRole[roleName].Add(contactInfo);
            }
            
            // Sort each role group
            foreach (var role in usersByRole.Keys.ToList())
            {
                usersByRole[role] = usersByRole[role]
                    .OrderBy(u => role == "User" ? u.HouseNumber : u.Name) // Sort homeowners by house number, others by name
                    .ToList();
            }
            
            // Create view model
            var viewModel = new ContactDirectoryViewModel
            {
                AdminContacts = usersByRole.ContainsKey("Admin") ? usersByRole["Admin"] : new List<UserContactViewModel>(),
                StaffContacts = usersByRole.ContainsKey("Staff") ? usersByRole["Staff"] : new List<UserContactViewModel>(),
                HomeownerContacts = usersByRole.ContainsKey("User") ? usersByRole["User"] : new List<UserContactViewModel>()
            };

            return View(viewModel);
        }

        // GET: Resources/Guide
        [Authorize]
        public IActionResult Guide()
        {
            // This would return the homeowner's guide document
            return View();
        }

        // GET: Resources/Rules
        [Authorize]
        public IActionResult Rules()
        {
            // This would return the community rules document
            return View();
        }

        // GET: Resources/Calendar
        [Authorize]
        public IActionResult Calendar()
        {
            // This would return the event calendar
            return View();
        }
    }
}
