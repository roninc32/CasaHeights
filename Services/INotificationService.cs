using System;
using System.Threading.Tasks;

namespace CasaHeights.Services
{
    public interface INotificationService
    {
        Task SendEmailNotificationAsync(string recipientEmail, string subject, string message);
        Task SendSmsNotificationAsync(string phoneNumber, string message);
    }
}