using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using TableTies.Models; // Ensure this matches your ApplicationUser namespace

namespace TableTies.Pages.Account
{
    public class ForgotPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IEmailSender _emailSender;
        private readonly ILogger<ForgotPasswordModel> _logger;

        public ForgotPasswordModel(
            UserManager<ApplicationUser> userManager,
            IEmailSender emailSender,
            ILogger<ForgotPasswordModel> logger)
        {
            _userManager = userManager;
            _emailSender = emailSender;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public required string Email { get; set; }
        }

        // No specific logic needed for OnGet usually, just display the form
        public void OnGet()
        {
            Input = new InputModel { Email = string.Empty };
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            // Don't reveal that the user does not exist or is not confirmed
            // This is a security measure to prevent email enumeration
            if (user == null || !(await _userManager.IsEmailConfirmedAsync(user)))
            {
                _logger.LogWarning("Forgot password attempt for non-existent or unconfirmed email: {Email}", Input.Email);
                // Redirect to a confirmation page that *doesn't* indicate success/failure based on email existence
                return RedirectToPage("./ForgotPasswordConfirmation");
            }

            // Generate a password reset token for the user
            var code = await _userManager.GeneratePasswordResetTokenAsync(user);
            // UrlEncode the token so it can be safely included in the URL
            code = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(code));

            // Generate the callback URL for the reset password page
            var callbackUrl = Url.PageLink(
                pageName: "/Account/ResetPassword", // The target page
                pageHandler: null,
                values: new { area = "Identity", code }, // Pass the code as a query parameter
                protocol: Request.Scheme); // Use the current scheme (http or https)

            if (callbackUrl == null)
            {
                 _logger.LogError("Failed to generate password reset callback URL for user: {Email}", Input.Email);
                 // Add a generic error, but still redirect to confirmation page for consistency
                 ModelState.AddModelError(string.Empty, "An error occurred while generating the reset link.");
                 return Page(); // Or redirect to an error page if preferred
            }

            // Send the password reset email using the injected IEmailSender
            await _emailSender.SendEmailAsync(
                Input.Email,
                "Reset your password",
                $"Please reset your password by <a href='{HtmlEncoder.Default.Encode(callbackUrl)}'>clicking here</a>.");

             _logger.LogInformation("Password reset email sent to {Email}", Input.Email);


            // Redirect to a confirmation page
            return RedirectToPage("./ForgotPasswordConfirmation");
        }
    }
}