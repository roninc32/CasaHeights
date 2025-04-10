using System;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace CasaHeights.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<NotificationService> _logger;

        public NotificationService(IConfiguration configuration, ILogger<NotificationService> logger)
        {
            _configuration = configuration;
            _logger = logger;
        }

        public async Task SendEmailNotificationAsync(string recipientEmail, string subject, string message)
        {
            try
            {
                // For development/testing, log the notification instead of sending it
                _logger.LogInformation($"EMAIL NOTIFICATION - To: {recipientEmail}, Subject: {subject}, Message: {message}");

                // Uncomment and configure this section when ready to send real emails
                /*
                var fromEmail = _configuration["EmailSettings:FromEmail"];
                var fromName = _configuration["EmailSettings:FromName"];
                var smtpServer = _configuration["EmailSettings:SmtpServer"];
                var smtpPort = int.Parse(_configuration["EmailSettings:SmtpPort"]);
                var smtpUsername = _configuration["EmailSettings:Username"];
                var smtpPassword = _configuration["EmailSettings:Password"];
                var useSsl = bool.Parse(_configuration["EmailSettings:UseSsl"]);

                var mailMessage = new MailMessage
                {
                    From = new MailAddress(fromEmail, fromName),
                    Subject = subject,
                    Body = message,
                    IsBodyHtml = true
                };
                mailMessage.To.Add(recipientEmail);

                using (var client = new SmtpClient(smtpServer, smtpPort))
                {
                    client.EnableSsl = useSsl;
                    client.Credentials = new NetworkCredential(smtpUsername, smtpPassword);
                    await client.SendMailAsync(mailMessage);
                }
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending email notification to {recipientEmail}");
                // In a production environment, you might want to throw or handle the exception differently
            }
        }

        public async Task SendSmsNotificationAsync(string phoneNumber, string message)
        {
            try
            {
                // For development/testing, log the notification instead of sending it
                _logger.LogInformation($"SMS NOTIFICATION - To: {phoneNumber}, Message: {message}");

                // Uncomment and implement when ready to send real SMS
                /*
                // You would typically use a third-party SMS service like Twilio, MessageBird, etc.
                // Example for Twilio (you would need to add the Twilio NuGet package):

                var accountSid = _configuration["TwilioSettings:AccountSid"];
                var authToken = _configuration["TwilioSettings:AuthToken"];
                var fromNumber = _configuration["TwilioSettings:FromNumber"];

                TwilioClient.Init(accountSid, authToken);

                var smsMessage = await MessageResource.CreateAsync(
                    body: message,
                    from: new Twilio.Types.PhoneNumber(fromNumber),
                    to: new Twilio.Types.PhoneNumber(phoneNumber)
                );
                */
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error sending SMS notification to {phoneNumber}");
                // In a production environment, you might want to throw or handle the exception differently
            }
        }
    }
}