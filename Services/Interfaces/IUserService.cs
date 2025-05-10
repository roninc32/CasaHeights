// Services/Interfaces/IUserService.cs
using System.Threading.Tasks;

namespace CasaHeights.Services.Interfaces
{
    public interface IUserService
    {
        string GetCurrentUserId();
        Task<string> GetCurrentUserNameAsync();
        Task<bool> IsUserAuthorizedForRequestAsync(int requestId);
    }
}
