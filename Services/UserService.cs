// Services/UserService.cs
using CasaHeights.Services.Interfaces;
using CasaHeights.Data;
using CasaHeights.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System;
using System.Security.Claims;
using System.Threading.Tasks;

namespace CasaHeights.Services
{
    public class UserService : IUserService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<Users> _userManager;
        private readonly AppDbContext _context;

        public UserService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<Users> userManager,
            AppDbContext context)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
            _context = context;
        }

        public string GetCurrentUserId()
        {
            var userId = _httpContextAccessor.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId))
                throw new InvalidOperationException("User is not authenticated");
            return userId;
        }

        public async Task<string> GetCurrentUserNameAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext?.User);
            if (user == null)
                throw new InvalidOperationException("User is not authenticated");
            return user.UserName ?? "Unknown";
        }

        public async Task<bool> IsUserAuthorizedForRequestAsync(int requestId)
        {
            var userId = GetCurrentUserId();
            var request = await _context.ServiceRequests
                .FirstOrDefaultAsync(r => r.Id == requestId);
            return request?.ResidentId == userId;
        }
    }
}
