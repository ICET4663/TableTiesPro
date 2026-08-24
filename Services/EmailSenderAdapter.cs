using Microsoft.AspNetCore.Identity.UI.Services;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using TableTies.Services;

namespace TableTies.Services
{
    public class EmailSenderAdapter : IEmailSender
    {
        private readonly IEmailService _emailService;
        private readonly ILogger<EmailSenderAdapter> _logger;

        public EmailSenderAdapter(IEmailService emailService, ILogger<EmailSenderAdapter> logger)
        {
            _emailService = emailService;
            _logger = logger;
        }

        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _logger.LogInformation("Attempting to send email via adapter to {Email} with subject {Subject}.", email, subject);

            await _emailService.SendEmailAsync(email, subject, htmlMessage);
        }
    }
}
