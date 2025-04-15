// Services/ServiceRequestService.cs
using CasaHeights.Services.Interfaces;
using CasaHeights.Models;
using CasaHeights.ViewModels;
using CasaHeights.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace CasaHeights.Services
{
    public class ServiceRequestService : IServiceRequestService
    {
        private readonly AppDbContext _context;
        private readonly IWebHostEnvironment _webHostEnvironment;

        public ServiceRequestService(AppDbContext context, IWebHostEnvironment webHostEnvironment)
        {
            _context = context;
            _webHostEnvironment = webHostEnvironment;
        }

        public async Task<IEnumerable<ServiceRequestViewModel>> GetUserRequestsAsync(string userId)
        {
            return await _context.ServiceRequests
                .Where(r => r.ResidentId == userId)
                .Select(r => new ServiceRequestViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    RequestType = r.RequestType,
                    Priority = r.Priority,
                    Status = r.Status,
                    CreatedDate = r.CreatedDate,
                    Location = r.Location,
                    AttachmentUrl = r.AttachmentUrl
                })
                .OrderByDescending(r => r.CreatedDate)
                .ToListAsync();
        }

        public async Task<ServiceRequestViewModel?> GetRequestByIdAsync(int id)
        {
            return await _context.ServiceRequests
                .Where(r => r.Id == id)
                .Select(r => new ServiceRequestViewModel
                {
                    Id = r.Id,
                    Title = r.Title,
                    Description = r.Description,
                    RequestType = r.RequestType,
                    Priority = r.Priority,
                    Status = r.Status,
                    CreatedDate = r.CreatedDate,
                    Location = r.Location,
                    AttachmentUrl = r.AttachmentUrl
                })
                .FirstOrDefaultAsync();
        }

        public async Task<int> CreateRequestAsync(ServiceRequestViewModel model)
        {
            if (string.IsNullOrEmpty(model.UserId))
            {
                throw new ArgumentException("User ID is required");
            }

            var request = new ServiceRequest
            {
                Title = model.Title,
                Description = model.Description,
                RequestType = model.RequestType,
                Priority = model.Priority,
                Status = ServiceRequestStatus.New,
                CreatedDate = DateTime.Now,
                Location = model.Location,
                AttachmentUrl = model.AttachmentUrl,
                ResidentId = model.UserId
            };

            _context.ServiceRequests.Add(request);
            await _context.SaveChangesAsync();
            return request.Id;
        }

        public async Task<string> UploadAttachmentAsync(IFormFile file)
        {
            if (file == null || file.Length == 0)
                throw new ArgumentException("No file provided");

            // Validate file size (5MB max)
            if (file.Length > 5 * 1024 * 1024)
                throw new ArgumentException("File size exceeds 5MB limit");

            // Validate file type
            var allowedTypes = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".doc", ".docx" };
            var fileExt = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!allowedTypes.Contains(fileExt))
                throw new ArgumentException("Invalid file type");

            // Generate unique filename
            var fileName = $"{Guid.NewGuid()}{fileExt}";
            var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "uploads", "service-requests");

            // Ensure directory exists
            Directory.CreateDirectory(uploadsFolder);

            var filePath = Path.Combine(uploadsFolder, fileName);

            // Save file
            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return $"/uploads/service-requests/{fileName}";
        }

        public async Task UpdateRequestStatusAsync(int id, ServiceRequestStatus newStatus)
        {
            var request = await _context.ServiceRequests.FindAsync(id);
            if (request == null)
                throw new ArgumentException("Request not found");

            request.Status = newStatus;
            await _context.SaveChangesAsync();
        }
    }
}
