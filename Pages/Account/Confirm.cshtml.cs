using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Identity;
using TableTies.Models;
using System;

namespace TableTies.Pages.Account
{
    public class ConfirmEmailModel : PageModel
    {
        private readonly UserManager<ApplicationUser> _userManager;

        public ConfirmEmailModel(UserManager<ApplicationUser> userManager)
        {
            _userManager = userManager;
        }

        [TempData]
        public string? StatusMessage { get; set; }

        public async Task<IActionResult> OnGetAsync(string? userId, string? code)
        {
            if (userId == null || code == null)
            {
                StatusMessage = "Error: Invalid email confirmation link.";
                return RedirectToPage("./Index");
            }

            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                 StatusMessage = "Error: Invalid email confirmation link.";
                return RedirectToPage("./Index");
            }

            var result = await _userManager.ConfirmEmailAsync(user, code);

            if (result.Succeeded)
            {
                StatusMessage = "Thank you for confirming your email. You can now log in.";
            }
            else
            {
                StatusMessage = "Error confirming your email.";
            }

            return Page();
        }
    }
}
