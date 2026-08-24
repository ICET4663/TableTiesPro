using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.EntityFrameworkCore; // Needed for Include()
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations; // Needed for InputModel attributes
using System.Linq;
using System.Threading.Tasks;
using TableTies.Models; // Your models (Booking, ConsultantBooking, ApplicationUser, etc.)
using TableTies.Services; // Your services (IBookingService, IConsultantService)
using Microsoft.AspNetCore.Authorization; // Needed for [Authorize]
using Microsoft.Extensions.Logging; // Needed for ILogger
using System.Globalization; // Needed for CultureInfo

namespace TableTies.Pages.Book
{
    // [Authorize] attribute removed to prevent authorization middleware requirement
    public class MyBookingsModel : PageModel
    {
        private readonly IBookingService _bookingService; // Service for Restaurant Bookings
        private readonly IConsultantService _consultantService; // Service for Consultant Bookings
        private readonly ILogger<MyBookingsModel>? _logger; // Optional: for logging

        public MyBookingsModel(
            IBookingService bookingService,
            IConsultantService consultantService,
            ILogger<MyBookingsModel>? logger = null) // Optional: Inject logger
        {
            _bookingService = bookingService;
            _consultantService = consultantService;
            _logger = logger; // Optional: Assign logger
        }

        // Lists to hold the current user's bookings
        public IList<Booking> RestaurantBookings { get; set; } = new List<Booking>();
        public IList<ConsultantBooking> ConsultantBookings { get; set; } = new List<ConsultantBooking>();

        // Used to pass status messages (success/error) after redirects or POSTs
        [TempData]
        public string? StatusMessage { get; set; }

        // --- Handlers ---

        // Handles the initial GET request to display the list of bookings
        public IActionResult OnGetAsync()
        {
            // Since authentication is disabled, show empty booking lists for demonstration
            RestaurantBookings = new List<Booking>();
            ConsultantBookings = new List<ConsultantBooking>();
            
            StatusMessage = "Booking functionality is available but requires user authentication to view personal bookings.";

            return Page(); // Return the page to display the lists
        }
    }
}
