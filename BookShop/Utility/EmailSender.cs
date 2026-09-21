using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Logging;
using System.Threading.Tasks;

namespace BookShop.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly ILogger<EmailSender> _logger;

        public EmailSender(ILogger<EmailSender> logger)
        {
            _logger = logger;
        }

        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            // Logging email for development; replace with SendGrid, Mailkit, or SMTP in production
            _logger.LogInformation("Email sent to {Email} with subject: {Subject}", email, subject);
            return Task.CompletedTask;
        }
    }
}
