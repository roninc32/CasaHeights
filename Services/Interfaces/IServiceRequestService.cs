// Services/Interfaces/IServiceRequestService.cs
using CasaHeights.Models;
using CasaHeights.ViewModels;
using Microsoft.AspNetCore.Http;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CasaHeights.Services.Interfaces
{
    public interface IServiceRequestService
    {
        Task<IEnumerable<ServiceRequestViewModel>> GetUserRequestsAsync(string userId);
        Task<ServiceRequestViewModel?> GetRequestByIdAsync(int id);
        Task<int> CreateRequestAsync(ServiceRequestViewModel model);
        Task<string> UploadAttachmentAsync(IFormFile file);
        Task UpdateRequestStatusAsync(int id, ServiceRequestStatus newStatus);
    }
}
