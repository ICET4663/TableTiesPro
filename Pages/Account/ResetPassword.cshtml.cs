using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.WebUtilities;
using System.ComponentModel.DataAnnotations;
using System.Text;
using System.Threading.Tasks;
using TableTies.Models; // Ensure this matches your ApplicationUser namespace

namespace TableTies.Pages.Account
{
    public class ResetPasswordModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;
         private readonly ILogger<ResetPasswordModel> _logger;


        public ResetPasswordModel(
            UserManager<ApplicationUser> userManager,
             ILogger<ResetPasswordModel> logger)
        {
            _userManager = userManager;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            [Required]
            [EmailAddress]
            public required string Email { get; set; }

            [Required]
            [StringLength(100, ErrorMessage = "The {0} must be at least {2} and at most {1} characters long.", MinimumLength = 8)] // Match your Identity options
            [DataType(DataType.Password)]
            public required string Password { get; set; }

            [DataType(DataType.Password)]
            [Compare("Password", ErrorMessage = "The password and confirmation password do not match.")]
            public string? ConfirmPassword { get; set; }

            [Required]
            public required string Code { get; set; } // The password reset token
        }

        public IActionResult OnGet(string? code = null)
        {
            if (code == null)
            {
                 _logger.LogWarning("Reset password page accessed without a code.");
                return RedirectToPage("./ForgotPassword"); // No code means they haven't clicked the link
            }
            else
            {
                Input = new InputModel
                {
                    Code = Encoding.UTF8.GetString(WebEncoders.Base64UrlDecode(code)), // Decode the code from the URL
                    Email = string.Empty, // Email will be entered by the user or potentially pre-filled if you change the link
                    Password = string.Empty,
                    ConfirmPassword = string.Empty
                };
                 _logger.LogInformation("Reset password page loaded with code.");
                return Page();
            }
        }

        public async Task<IActionResult> OnPostAsync()
        {
            if (!ModelState.IsValid)
            {
                 _logger.LogWarning("Reset password form submitted with invalid model state.");
                return Page();
            }

            var user = await _userManager.FindByEmailAsync(Input.Email);
            if (user == null)
            {
                 _logger.LogWarning("Password reset attempt for non-existent email: {Email}", Input.Email);
                // Don't reveal that the user does not exist
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            // Use the user manager to reset the password with the provided token and new password
            var result = await _userManager.ResetPasswordAsync(user, Input.Code, Input.Password);
            if (result.Succeeded)
            {
                _logger.LogInformation("User password reset successfully for email: {Email}", Input.Email);
                // Redirect to a confirmation page or the login page
                return RedirectToPage("./ResetPasswordConfirmation");
            }

            // If reset failed, add errors to model state
            foreach (var error in result.Errors)
            {
                _logger.LogWarning("Password reset failed for email {Email}: {Error}", Input.Email, error.Description);
                ModelState.AddModelError(string.Empty, error.Description);
            }
             _logger.LogWarning("Password reset attempt failed for email: {Email}", Input.Email);
            return Page(); // Return the page with error messages
        }
    }
}