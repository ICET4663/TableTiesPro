using Microsoft.AspNetCore.Identity.UI.Services;
using System.Diagnostics;
using System.Threading.Tasks;

namespace TableTies.Services
{
    public class DummyEmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            Debug.WriteLine($"-- Email To: {email}");
            Debug.WriteLine($"-- Subject: {subject}");
            Debug.WriteLine($"-- Message: {htmlMessage}");

            return Task.CompletedTask;
        }
    }
}
