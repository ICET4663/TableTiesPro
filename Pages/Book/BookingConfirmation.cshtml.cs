using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Threading.Tasks; // Needed for Task
using TableTies.Models; // Ensure this namespace matches your Booking model
using TableTies.Services; // Ensure this namespace matches your IBookingService
using Microsoft.Extensions.Logging; // Needed for ILogger
using System; // Needed for Guid

namespace TableTies.Pages.Book // Ensure this namespace matches your file location
{
    public class BookingConfirmationModel : PageModel
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingConfirmationModel>? _logger; // Example logger

        // Property to hold the booking details for display
        public Booking? ConfirmedBooking { get; set; } // Nullable Booking property

        // Inject services
        public BookingConfirmationModel(IBookingService bookingService, ILogger<BookingConfirmationModel>? logger = null)
        {
            _bookingService = bookingService;
            _logger = logger; // Assign logger if provided
        }

        // Handler for GET requests (when redirected from the booking page)
        // The bookingId is passed in the route
        /// <summary>
        /// Handles GET requests for the booking confirmation page.
        /// Retrieves booking details based on the provided booking ID.
        /// </summary>
        /// <param name="bookingId">The ID (Guid) of the booking to confirm.</param>
        /// <returns>An IActionResult representing the page or a redirect.</returns>
        public async Task<IActionResult> OnGetAsync(Guid bookingId)
        {
            // Check for an invalid booking ID (Guid.Empty is the default value for Guid)
            if (bookingId == Guid.Empty)
            {
                _logger?.LogWarning("Booking confirmation page accessed with invalid booking ID: {BookingId}", bookingId);
                // Handle invalid booking ID (e.g., redirect to an error page or home)
                return RedirectToPage("/Index"); // Adjust the redirect path as needed
            }

            _logger?.LogInformation("Attempting to retrieve booking with ID {BookingId} for confirmation.", bookingId);

            // Retrieve the booking details using the booking ID (which is now Guid)
            // Corrected method name from GetBookingByIdAsync to GetBookingAsync
            ConfirmedBooking = await _bookingService.GetBookingAsync(bookingId);

            if (ConfirmedBooking == null)
            {
                _logger?.LogWarning("Booking with ID {BookingId} not found for confirmation.", bookingId);
                // Handle booking not found
                return RedirectToPage("/Index"); // Redirect or show an error message
            }

            _logger?.LogInformation("Booking confirmation page displayed for booking ID: {BookingId}", bookingId);

            // Return the current page to display the confirmation details
            return Page();
        }

        // You might add other handlers here if needed (e.g., to cancel from confirmation)
    }
}
