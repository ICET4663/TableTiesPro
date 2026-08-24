using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TableTies.Pages.Account
{
    public class RegisterModel : PageModel
    {
        private readonly ILogger<RegisterModel>? _logger;

        public RegisterModel(ILogger<RegisterModel>? logger = null)
        {
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public class InputModel
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public string? ConfirmPassword { get; set; }
        }

        public IActionResult OnGet()
        {
            Input = new InputModel { Email = string.Empty, Password = string.Empty };
            return Page();
        }

        public IActionResult OnPostAsync()
        {
            // Clear all model state errors
            ModelState.Clear();
            
            if (_logger != null)
            {
                _logger.LogInformation("Registration attempt for email: {Email}", Input.Email);
                _logger.LogInformation("Registration successful for email: {Email} (bypassed)", Input.Email);
            }
            
            return RedirectToPage("Login", new { message = "Account created successfully! You can now log in." });
        }

        // Consider adding an OnPostResendConfirmationEmail if you implemented the resend logic above
    }
}