using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using TableTies.Models;
using System;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.Identity.UI.Services;

namespace TableTies.Services
{
    public class EmailServiceOptions
    {
        public const string EmailService = "EmailService";

        public string? SmtpServer { get; set; }
        public int SmtpPort { get; set; }
        public string? SmtpUsername { get; set; }
        public string? SmtpPassword { get; set; }
        public bool EnableSsl { get; set; }
        public string? FromEmail { get; set; }
        public string? FromName { get; set; }
    }

    public class EmailService : IEmailService
    {
        private readonly EmailServiceOptions _options;
        private readonly ILogger<EmailService> _logger;

        public EmailService(IOptions<EmailServiceOptions> options, ILogger<EmailService> logger)
        {
            _options = options.Value;
            _logger = logger;
        }

        public async Task SendConfirmationEmailAsync(string userEmail, string confirmationLink)
        {
             if (string.IsNullOrEmpty(_options.SmtpServer) || _options.SmtpPort <= 0 ||
                 string.IsNullOrEmpty(_options.SmtpUsername) || string.IsNullOrEmpty(_options.SmtpPassword) ||
                 string.IsNullOrEmpty(_options.FromEmail))
             {
                 _logger.LogWarning("EmailService is not fully configured. Skipping confirmation email to {UserEmail}.", userEmail);
                 return;
             }

             var mailMessage = new MailMessage
             {
                 From = new MailAddress(_options.FromEmail, _options.FromName ?? "TableTies"),
                 Subject = "Email Confirmation",
                 Body = $"Please confirm your email by clicking on the following link: <a href='{confirmationLink}'>Confirm Email</a>",
                 IsBodyHtml = true
             };
             mailMessage.To.Add(userEmail);

             await SendEmailInternalAsync(mailMessage);
        }

        public async Task SendBookingConfirmationEmailAsync(string userEmail, Booking booking)
        {
            if (string.IsNullOrEmpty(_options.SmtpServer) || _options.SmtpPort <= 0 ||
                string.IsNullOrEmpty(_options.SmtpUsername) || string.IsNullOrEmpty(_options.SmtpPassword) ||
                string.IsNullOrEmpty(_options.FromEmail))
            {
                _logger.LogWarning("EmailService is not fully configured. Skipping booking confirmation email to {UserEmail}.", userEmail);
                return;
            }

            var mailMessage = new MailMessage
            {
                From = new MailAddress(_options.FromEmail, _options.FromName ?? "TableTies"),
                Subject = "Booking Confirmation",
                Body = $"Your booking for Restaurant: {booking.Restaurant?.Name} at Table: {booking.Table?.Id} on {booking.BookingDateTime.ToString("MM/dd/yyyy HH:mm")} has been confirmed.",
                IsBodyHtml = true
            };
            mailMessage.To.Add(userEmail);

            await SendEmailInternalAsync(mailMessage);
        }

        public async Task SendPasswordResetEmailAsync(string userEmail, string resetLink)
        {
             if (string.IsNullOrEmpty(_options.SmtpServer) || _options.SmtpPort <= 0 ||
                 string.IsNullOrEmpty(_options.SmtpUsername) || string.IsNullOrEmpty(_options.SmtpPassword) ||
                 string.IsNullOrEmpty(_options.FromEmail))
             {
                 _logger.LogWarning("EmailService is not fully configured. Skipping password reset email to {UserEmail}.", userEmail);
                 return;
             }

             var mailMessage = new MailMessage
             {
                 From = new MailAddress(_options.FromEmail, _options.FromName ?? "TableTies"),
                 Subject = "Password Reset",
                 Body = $"To reset your password, click on the following link: <a href='{resetLink}'>Reset Password</a>",
                 IsBodyHtml = true
             };
             mailMessage.To.Add(userEmail);

             await SendEmailInternalAsync(mailMessage);
        }

        public async Task SendEmailAsync(string toEmail, string subject, string htmlMessage)
        {
             if (string.IsNullOrEmpty(_options.SmtpServer) || _options.SmtpPort <= 0 ||
                 string.IsNullOrEmpty(_options.SmtpUsername) || string.IsNullOrEmpty(_options.SmtpPassword) ||
                 string.IsNullOrEmpty(_options.FromEmail))
             {
                 _logger.LogWarning("EmailService is not fully configured. Skipping general email to {ToEmail}.", toEmail);
                 return;
             }

             var mailMessage = new MailMessage
             {
                 From = new MailAddress(_options.FromEmail, _options.FromName ?? "TableTies"),
                 Subject = subject,
                 Body = htmlMessage,
                 IsBodyHtml = true
             };
             mailMessage.To.Add(toEmail);

             await SendEmailInternalAsync(mailMessage);
        }

        private async Task SendEmailInternalAsync(MailMessage mailMessage)
        {
            using (var smtpClient = new SmtpClient(_options.SmtpServer, _options.SmtpPort))
            {
                smtpClient.Credentials = new NetworkCredential(_options.SmtpUsername, _options.SmtpPassword);
                smtpClient.EnableSsl = _options.EnableSsl;

                try
                {
                    await smtpClient.SendMailAsync(mailMessage);
                    _logger.LogInformation("Email sent successfully to {ToEmail}", mailMessage.To.ToString());
                }
                catch (SmtpException ex)
                {
                    _logger.LogError(ex, "SMTP Error sending email to {ToEmail}: {StatusCode}, {ErrorMessage}", mailMessage.To.ToString(), ex.StatusCode, ex.Message);
                    
                    // Don't throw the exception - log it and continue
                    // This prevents the registration/login process from failing
                    _logger.LogWarning("Email sending failed but application will continue. Check SMTP configuration.");
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "General Email Sending Error to {ToEmail}: {ErrorMessage}", mailMessage.To.ToString(), ex.Message);
                    
                    // Don't throw the exception - log it and continue
                    _logger.LogWarning("Email sending failed but application will continue. Check email configuration.");
                }
            }
        }
    }
}
