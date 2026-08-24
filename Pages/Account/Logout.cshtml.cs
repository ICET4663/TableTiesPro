using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace TableTies.Pages.Account
{
    public class LogoutModel : PageModel
    {
        private readonly ILogger<LogoutModel>? _logger;

        public LogoutModel(ILogger<LogoutModel>? logger = null)
        {
            _logger = logger;
        }

        // OnGet is typically used for a simple confirmation page before logging out
        // Or you can skip the confirmation and just handle POST
        public void OnGet()
        {
             // Optional: Display a "Are you sure you want to log out?" page
        }

        // OnPost is the preferred way to handle logout as it's an action that changes state
        public IActionResult OnPost(string? returnUrl = null)
        {
            // Since authentication is disabled, just redirect to home page
            _logger?.LogInformation("Logout requested (authentication disabled).");

            // Redirect to a specified returnUrl, or the home page
            if (returnUrl != null && Url.IsLocalUrl(returnUrl))
            {
                return LocalRedirect(returnUrl);
            }
            else
            {
                // Default redirect after logout - home page
                return RedirectToPage("/Index");
            }
        }
    }
}