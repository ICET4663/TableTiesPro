using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using TableTies.Models; // Your models
using TableTies.Services; // Your services
using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.AspNetCore.Authorization; // Needed for [Authorize]
using Microsoft.Extensions.Logging; // Needed for ILogger

namespace TableTies.Pages.Book
{
    // [Authorize] attribute removed to prevent authorization middleware requirement
    public class ConsultantModel : PageModel
    {
        private readonly IConsultantService _consultantService;
        private readonly ILogger<ConsultantModel> _logger;

        public ConsultantModel(
            IConsultantService consultantService,
            ILogger<ConsultantModel> logger)
        {
            _consultantService = consultantService;
            _logger = logger;
        }

        [BindProperty]
        public InputModel Input { get; set; } = default!;

        // For dropdown population
        public List<SelectListItem> ConsultantsList { get; set; } = new List<SelectListItem>();

        // Property to hold the list of consultants fetched from the database for display
        // This was in the second code block, adding it here for potential display purposes
        public IList<Consultant> Consultants { get; set; } = new List<Consultant>();


        [TempData] // Used to display messages after redirects
        public string? StatusMessage { get; set; }

        public class InputModel
        {
            [Required(ErrorMessage = "Please select a Consultant.")]
            [Display(Name = "Consultant")]
            public Guid SelectedConsultantId { get; set; }

            [Required(ErrorMessage = "Please select a Date.")]
            [DataType(DataType.Date)]
            [Display(Name = "Booking Date")]
            public DateTime BookingDate { get; set; }

            [Required(ErrorMessage = "Please select a Start Time.")]
            [DataType(DataType.Time)]
            [Display(Name = "Start Time")]
            public TimeSpan StartTime { get; set; }

            [Required(ErrorMessage = "Please select a Duration.")]
            [Display(Name = "Duration (minutes)")] // Or hours, adjust input type/validation accordingly
            [Range(15, 240, ErrorMessage = "Duration must be between 15 and 240 minutes.")] // Example range
            public int DurationMinutes { get; set; } // Using minutes and converting to TimeSpan

            [StringLength(500, ErrorMessage = "Details cannot exceed 500 characters.")]
            [Display(Name = "Details / Questions")]
            public string? Details { get; set; }
        }

        public async Task<IActionResult> OnGetAsync()
        {
            // Populate the consultants dropdown for the booking form
            ConsultantsList = await _consultantService.GetConsultantsListAsync();

            // Fetch all consultants for potential display in a list on the page (from the second block)
            Consultants = await _consultantService.GetConsultantsAsync();


            // Set default date/time for better UX
            Input = new InputModel
            {
                BookingDate = DateTime.Today,
                StartTime = DateTime.Now.TimeOfDay.Add(TimeSpan.FromMinutes(30 - (DateTime.Now.Minute % 30))) // Suggest next half hour
            };


            return Page();
        }

        public async Task<IActionResult> OnPostAsync()
        {
            // Repopulate dropdown in case of validation errors
            ConsultantsList = await _consultantService.GetConsultantsListAsync();
            // Re-fetch all consultants for potential display in a list on the page (from the second block)
            Consultants = await _consultantService.GetConsultantsAsync();

            if (!ModelState.IsValid)
            {
                 _logger.LogWarning("Consultant booking form submitted with invalid model state.");
                 StatusMessage = "Error: Please fix the errors in the form."; // Provide general error message
                 return Page(); // Return the page with validation errors
            }

            // Since authentication is disabled, use a dummy user ID for demonstration
            var dummyUserId = Guid.NewGuid(); // In real app, this would come from authenticated user

            // Combine date and time
            var bookingDateTime = Input.BookingDate.Date + Input.StartTime;

            // Convert duration minutes to TimeSpan
            var duration = TimeSpan.FromMinutes(Input.DurationMinutes);

            // Call the service to create the booking
            var newBooking = await _consultantService.CreateConsultantBookingAsync(
                dummyUserId,
                Input.SelectedConsultantId,
                bookingDateTime,
                duration,
                Input.Details
            );

            if (newBooking != null)
            {
                 _logger.LogInformation("Consultant booking created successfully for user {UserId} with consultant {ConsultantId}.", dummyUserId, Input.SelectedConsultantId);
                 StatusMessage = "Consultant booking created successfully!";
                // Redirect to a confirmation page or the user's bookings list
                return RedirectToPage("./MyBookings"); // Redirect to MyBookings page
            }
            else
            {
                 _logger.LogError("Failed to create consultant booking for user {UserId} with consultant {ConsultantId}. Consultant may not exist or other error.", dummyUserId, Input.SelectedConsultantId);
                // Add a model state error if creation failed (e.g., consultant ID was invalid)
                 ModelState.AddModelError(string.Empty, "Error creating booking. Please check your selections and try again.");
                return Page(); // Return the page to show the error
            }
        }

        // You could add Handlers here for AJAX calls if needed, e.g., checking availability
        // public JsonResult OnGetAvailableSlotsJson(Guid consultantId, DateTime date) { ... }
    }
}
