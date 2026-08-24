using TableTies.Models;
using System.Threading.Tasks;

namespace TableTies.Services
{
    public interface IEmailService
    {
        Task SendConfirmationEmailAsync(string userEmail, string confirmationLink);

        Task SendBookingConfirmationEmailAsync(string userEmail, Booking booking);

        Task SendPasswordResetEmailAsync(string userEmail, string resetLink);

        Task SendEmailAsync(string toEmail, string subject, string htmlMessage);
    }
}
