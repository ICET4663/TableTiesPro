using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TableTies.Pages.Account
{
    public class LoginModel : PageModel
    {
        private readonly ILogger<LoginModel>? _logger;

        public LoginModel(ILogger<LoginModel>? logger = null)
        {
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        public string? ReturnUrl { get; set; }
        
        [TempData]
        public string? Message { get; set; }

        public class InputModel
        {
            public string Email { get; set; } = string.Empty;
            public string Password { get; set; } = string.Empty;
            public bool RememberMe { get; set; }
        }

        public IActionResult OnGet(string? returnUrl = null, string? message = null)
        {
            Input = new InputModel { Email = string.Empty, Password = string.Empty };
            ReturnUrl = returnUrl;
            Message = message;

            _logger?.LogInformation("Login page loaded. ReturnUrl: {ReturnUrl}, Message: {Message}", ReturnUrl ?? "None", Message ?? "None");
            return Page();
        }

        public IActionResult OnPostAsync(string? returnUrl = null)
        {
            // Clear all model state errors to ensure clean login
            ModelState.Clear();
            
            // Log the received returnUrl before processing
            _logger?.LogInformation("Login form submitted for email {Email}. Received ReturnUrl: {ReturnUrl}", Input.Email, returnUrl ?? "None");

            // Skip all validation - allow any email/password combination
            _logger?.LogInformation("Login successful for email {Email} (authentication bypassed)", Input.Email);

            // --- CORRECTED REDIRECT LOGIC ---
            // If returnUrl is null or empty, redirect to /Book/Restaurant.
            // Otherwise, redirect to the provided returnUrl.
            string redirectUrl = string.IsNullOrEmpty(returnUrl) ? "/Book/Restaurant" : returnUrl;

            _logger?.LogInformation("Redirecting to {RedirectUrl} after successful login.", redirectUrl);
            return LocalRedirect(redirectUrl);
            // --- END CORRECTED REDIRECT LOGIC ---
        }
    }
}
