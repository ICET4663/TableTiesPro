using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering; // Needed for SelectListItem
using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using System; // Needed for Guid, DateTime, TimeSpan
using System.Collections.Generic; // Needed for ICollection, List<T>
using TableTies.Services; // Assuming your service namespace
using TableTies.Models; // Assuming your model namespace (Organization, Restaurant, RestaurantTable, TableBooking, ApplicationUser, BookingDto)
using Microsoft.Extensions.Logging; // Needed for ILogger
using Microsoft.AspNetCore.Authorization; // Needed for [Authorize]
using System.Security.Claims; // Needed for ClaimsPrincipal
using System.Linq; // Needed for LINQ methods like Select, ToList

// FIX: Add the namespace declaration
namespace TableTies.Pages.Book
{

    // Apply [Authorize] to the entire Razor Page model.
    // This ensures that only authenticated users can access this page and its handlers (OnGet, OnPost, OnGetRestaurantsByOrganizationAsync, OnGetAvailableTablesAsync).
    // If an unauthenticated user attempts to access this page, they will be automatically redirected to the login page.
    public class RestaurantModel : PageModel
    {
        // Inject necessary services and logger
        private readonly IRestaurantService _restaurantService;
        private readonly IBookingService _bookingService; // Assuming you have a booking service
        private readonly ILogger<RestaurantModel> _logger;

        public RestaurantModel(IRestaurantService restaurantService, IBookingService bookingService, ILogger<RestaurantModel> logger)
        {
            _restaurantService = restaurantService;
            _bookingService = bookingService;
            _logger = logger;
        }

        // --- Properties to hold data for dropdowns ---
        // These lists will be populated in OnGetAsync and OnPostAsync (if validation fails)
        // Initialized with empty lists to prevent null reference exceptions in the view
        public List<SelectListItem> OrganizationsList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> RestaurantsList { get; set; } = new List<SelectListItem>();
        public List<SelectListItem> AvailableTablesList { get; set; } = new List<SelectListItem>();
        // --- End Properties for dropdowns ---


        // --- Properties to bind form data submitted via POST ---
        // These properties are directly on the PageModel and bound using [BindProperty]
        [BindProperty]
        [Required(ErrorMessage = "Please select an organization.")]
        public Guid SelectedOrganizationId { get; set; } // Uses Guid directly

        [BindProperty]
        [Required(ErrorMessage = "Please select a restaurant.")]
        public Guid SelectedRestaurantId { get; set; } // Uses Guid directly

        [BindProperty]
        [Required(ErrorMessage = "Please select a table.")]
        public Guid SelectedTableId { get; set; } // Uses Guid directly

        [BindProperty]
        [Required(ErrorMessage = "Please select a date.")]
        [DataType(DataType.Date)]
        public DateTime BookingDate { get; set; } = DateTime.Today; // Default to today

        [BindProperty]
        [Required(ErrorMessage = "Please select a start time.")]
        [DataType(DataType.Time)]
        public TimeSpan StartTime { get; set; } = TimeSpan.FromHours(14); // Default time (2 PM)

        [BindProperty]
        [Required(ErrorMessage = "Please select an end time.")]
        [DataType(DataType.Time)]
        public TimeSpan EndTime { get; set; } = TimeSpan.FromHours(15); // Default time (3 PM) - 1 hour duration

        [BindProperty]
        [Required(ErrorMessage = "Please enter the number of guests.")]
        [Range(1, 20, ErrorMessage = "Number of guests must be between {1} and {2}.")] // Example range
        public int NumberOfGuests { get; set; }

        [BindProperty]
        public string? SpecialRequests { get; set; } // Optional field

        // Use TempData to show success/error messages after a redirect (e.g., after successful booking)
        [TempData]
        public string? StatusMessage { get; set; }
        // --- End Properties for form data ---


        // OnGetAsync runs when the page is initially loaded via a GET request.
        // It populates the initial dropdowns.
        public async Task<IActionResult> OnGetAsync()
        {
            _logger.LogInformation("Restaurant booking page OnGetAsync executed.");

            // Initialize default values for bound properties on GET
            SelectedOrganizationId = Guid.Empty;
            SelectedRestaurantId = Guid.Empty;
            SelectedTableId = Guid.Empty;
            BookingDate = DateTime.Today;
            StartTime = TimeSpan.FromHours(14); // 2 PM
            EndTime = TimeSpan.FromHours(15); // 3 PM
            NumberOfGuests = 1;
            SpecialRequests = null;

            try
            {
                // Populate the Organizations dropdown on page load
                await PopulateOrganizationsDropdownAsync();

                // If the page is loaded after a failed POST, the bound properties (SelectedOrganizationId, etc.)
                // will retain their values. We need to re-populate dependent dropdowns based on these values.
                // If it's a fresh GET, these will be default Guids (Guid.Empty).

                // Populate Restaurants dropdown if an organization is already selected
                if (SelectedOrganizationId != Guid.Empty)
                {
                    await PopulateRestaurantsDropdownAsync(SelectedOrganizationId);

                    // Populate Tables dropdown if a restaurant is also selected and date/times are set
                    if (SelectedRestaurantId != Guid.Empty)
                    {
                         // Corrected: Convert StartTime and EndTime (TimeSpan) to DateTime
                         // by combining with BookingDate before passing to the helper.
                         await PopulateAvailableTablesDropdownAsync(SelectedRestaurantId, BookingDate, BookingDate.Date.Add(StartTime), BookingDate.Date.Add(EndTime));
                    }
                }
            }
            catch (Exception ex)
            {
                // Catch any exceptions during initial data loading and log them
                _logger.LogError(ex, "An error occurred while loading data for the restaurant booking page.");
                // Set a user-friendly error message to display on the page
                StatusMessage = "Error loading booking options. Please try again.";
                // Ensure lists are empty in case of error to prevent null reference in view
                OrganizationsList = new List<SelectListItem>();
                RestaurantsList = new List<SelectListItem>();
                AvailableTablesList = new List<SelectListItem>();
            }

            // Explicitly ensure lists are not null before returning the page for rendering
            OrganizationsList ??= new List<SelectListItem>();
            RestaurantsList ??= new List<SelectListItem>();
            AvailableTablesList ??= new List<SelectListItem>();


            return Page(); // Return the Razor Page
        }

        // Handler for AJAX call to get restaurants based on selected organization ID.
        // Returns a JSON result for client-side JavaScript to populate the dropdown.
        public async Task<JsonResult> OnGetRestaurantsByOrganizationAsync(Guid organizationId) // Parameter type is Guid
        {
            _logger.LogInformation("Fetching restaurants for organization ID {OrganizationId}.", organizationId);
            try
            {
                 // Fetch restaurants using the injected service
                 var restaurants = await _restaurantService.GetRestaurantsByOrganizationAsync(organizationId);
                 // Map Restaurant entities to SelectListItem objects for the dropdown
                 // The client-side JS expects { value: "...", text: "..." }
                 var restaurantItems = restaurants.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name }).ToList();
                 // Return the list of SelectListItem objects as JSON
                 return new JsonResult(restaurantItems);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error fetching restaurants for organization ID {OrganizationId}.", organizationId);
                 // Return an empty list or an error indicator to the client-side JS
                 return new JsonResult(new List<SelectListItem>()); // Return empty list on error
                 // Or return StatusCode(500, "Error fetching restaurants."); // Return an HTTP error status
            }
        }

        // Handler for AJAX call to get available tables based on selected restaurant ID, date, and time.
        // Returns a JSON result for client-side JavaScript to populate the dropdown.
        // Note: This handler receives TimeSpan parameters from the client-side JS.
        public async Task<JsonResult> OnGetAvailableTablesAsync(Guid restaurantId, DateTime bookingDate, TimeSpan startTime, TimeSpan endTime) // Parameter types match form inputs
        {
            _logger.LogInformation("Fetching available tables for restaurant ID {RestaurantId} on {BookingDate:d} from {StartTime:hh\\:mm} to {EndTime:hh\\:mm}.",
                restaurantId, bookingDate, startTime, endTime); // Log with specific date/time formatters

            try
            {
                 // Fetch available tables using the injected service
                 // FIX: Convert the incoming TimeSpan parameters to DateTime by adding them to the bookingDate.Date
                 // before passing them to the service method, as the service method expects DateTime for time parameters.
                 var availableTables = await _restaurantService.GetAvailableTablesAsync(restaurantId, bookingDate, bookingDate.Date.Add(startTime), bookingDate.Date.Add(endTime));

                 // Map RestaurantTable entities to SelectListItem objects for the dropdown
                 // The client-side JS expects { value: "...", text: "..." }
                 var tableItems = availableTables.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.TableName} ({t.Capacity} seats)" }).ToList();
                 // Return the list of SelectListItem objects as JSON
                 return new JsonResult(tableItems);
            }
            catch (Exception ex)
            {
                 _logger.LogError(ex, "Error fetching available tables for restaurant ID {RestaurantId}.", restaurantId);
                 // Return an empty list or an error indicator to the client-side JS
                 return new JsonResult(new List<SelectListItem>()); // Return empty list on error
                 // Or return StatusCode(500, "Error fetching available tables."); // Return an HTTP error status
            }
        }


        // OnPostAsync runs when the form is submitted via a POST request.
        // This method handles the booking creation logic.
        public async Task<IActionResult> OnPostAsync()
        {
            _logger.LogInformation("Restaurant booking form submitted.");

            // --- DATA RE-FETCHING FOR DROPDOWNS ON POST (Crucial if validation fails) ---
            // Always re-populate dropdowns at the beginning of OnPostAsync.
            // If ModelState is invalid, the page will be re-rendered, and these lists
            // will be used by the asp-items tag helper to show the dropdowns with data,
            // preserving the user's previous selections via the [BindProperty] values.

            // Wrap re-population in try-catch as well
            try
            {
                await PopulateOrganizationsDropdownAsync();
                // Only attempt to populate restaurants if an organization was selected
                if (SelectedOrganizationId != Guid.Empty)
                {
                    await PopulateRestaurantsDropdownAsync(SelectedOrganizationId);
                    // Only attempt to populate tables if a restaurant is also selected and date/times are set
                    if (SelectedRestaurantId != Guid.Empty)
                    {
                         // Corrected: Convert StartTime and EndTime (TimeSpan) to DateTime
                         // by combining with BookingDate before passing to the helper.
                         await PopulateAvailableTablesDropdownAsync(SelectedRestaurantId, BookingDate, BookingDate.Date.Add(StartTime), BookingDate.Date.Add(EndTime));
                    }
                }
            }
             catch (Exception ex)
            {
                 _logger.LogError(ex, "An error occurred while re-populating dropdowns on POST.");
                 // Set a status message, but still proceed with validation as other fields might be valid
                 StatusMessage = "Error re-loading booking options. Please try again.";
                 // Ensure lists are empty in case of error to prevent null reference in view
                 OrganizationsList = new List<SelectListItem>();
                 RestaurantsList = new List<SelectListItem>();
                 AvailableTablesList = new List<SelectListItem>();
            }
            // --- END DATA RE-FETCHING ---


            // Check if the model state is valid based on data annotations applied to properties
            if (!ModelState.IsValid)
            {
                _logger.LogWarning("Restaurant booking form submitted with invalid model state.");
                // If invalid, return the current page. The dropdowns are already populated above.
                return Page(); // Return the page to show validation errors
            }

            // --- Authentication Check ---
            // The [Authorize] attribute handles most authentication checks at the middleware level.
            // This explicit check is good practice, but with [Authorize] on the class,
            // an unauthenticated user should not reach this point.
            if (!User.Identity?.IsAuthenticated ?? true)
            {
                 _logger.LogWarning("Unauthenticated user attempted to create a restaurant booking.");
                 // If not authenticated, redirect to login page with ReturnUrl
                 return RedirectToPage("/Account/Login", new { ReturnUrl = "/Book/Restaurant" });
            }
            // --- End Authentication Check ---

            // Get the current user's email from the authenticated user's claims
            var userEmail = User.FindFirstValue(ClaimTypes.Email);

            // FIX: Check if userEmail is null or empty before creating BookingDto
            if (string.IsNullOrEmpty(userEmail))
            {
                 _logger.LogError("Authenticated user does not have an email claim.");
                 StatusMessage = "Error: Could not retrieve user email. Please try logging in again.";
                 // Re-populate dropdowns before returning the page on error
                 await PopulateOrganizationsDropdownAsync();
                 if (SelectedOrganizationId != Guid.Empty)
                 {
                     await PopulateRestaurantsDropdownAsync(SelectedOrganizationId);
                     if (SelectedRestaurantId != Guid.Empty)
                     {
                         await PopulateAvailableTablesDropdownAsync(SelectedRestaurantId, BookingDate, BookingDate.Date.Add(StartTime), BookingDate.Date.Add(EndTime));
                     }
                 }
                 return Page();
            }


            // Create a BookingDto from the bound properties to pass to the service
            // This aligns with the IBookingService interface definition
            var bookingDto = new BookingDto // Assuming BookingDto is in TableTies.Models
            {
                // Map the bound properties to the BookingDto properties
                RestaurantId = SelectedRestaurantId,
                RestaurantTableId = SelectedTableId, // Assuming BookingDto uses RestaurantTableId
                BookingDate = BookingDate,
                // FIX: Ensure BookingDto StartTime and EndTime are TimeSpan, and assign the TimeSpan properties directly.
                StartTime = StartTime, // Pass TimeSpan directly
                EndTime = EndTime,     // Pass TimeSpan directly
                NumberOfGuests = NumberOfGuests,
                // FIX: Use null-coalescing operator to potentially resolve CS8601
                SpecialRequests = SpecialRequests ?? null, // Explicitly handle nullability
                // Assign the non-null userEmail
                UserEmail = userEmail // Assign the checked non-null email
            };

            // Add validation for StartTime < EndTime if not already handled by data annotations
            if (bookingDto.StartTime >= bookingDto.EndTime)
            {
                 ModelState.AddModelError(string.Empty, "End time must be after start time.");
                  _logger.LogWarning("Booking submission failed: End time is not after start time.");
                  // Re-populate dropdowns before returning the page on validation error
                  await PopulateOrganizationsDropdownAsync();
                  if (SelectedOrganizationId != Guid.Empty)
                  {
                      await PopulateRestaurantsDropdownAsync(SelectedOrganizationId);
                      if (SelectedRestaurantId != Guid.Empty)
                      {
                          await PopulateAvailableTablesDropdownAsync(SelectedRestaurantId, BookingDate, BookingDate.Date.Add(StartTime), BookingDate.Date.Add(EndTime));
                      }
                  }
                 return Page();
            }


            try
            {
                // Attempt to create the booking using the injected booking service
                // Corrected: Call CreateBookingAsync with the BookingDto
                // The service method returns Task<Booking>
                var booking = await _bookingService.CreateBookingAsync(bookingDto);

                // Check if the booking was successfully created (service returns the created Booking object)
                if (booking != null) // Assuming service returns null or throws on failure
                {
                    _logger.LogInformation("Restaurant booking created successfully with ID {BookingId} for user {UserId}.", booking.Id, booking.UserId);
                    StatusMessage = "Booking successful!"; // Set success message in TempData
                    // Redirect to a confirmation page or the user's bookings page after successful creation
                    // Replace "/Book/BookingConfirmation" with the actual path to your confirmation page
                    return RedirectToPage("/Book/BookingConfirmation", new { bookingId = booking.Id });
                }
                else
                {
                    // If the service method returns null (indicating failure)
                    _logger.LogError("Failed to create restaurant booking for user {UserEmail} (Service returned null).", userEmail);
                    StatusMessage = "Error: Failed to create booking. Please try again."; // Set error message in TempData
                    // Re-populate dropdowns before returning the page on service failure
                    await PopulateOrganizationsDropdownAsync();
                    if (SelectedOrganizationId != Guid.Empty)
                    {
                        await PopulateRestaurantsDropdownAsync(SelectedOrganizationId);
                        if (SelectedRestaurantId != Guid.Empty)
                        {
                            await PopulateAvailableTablesDropdownAsync(SelectedRestaurantId, BookingDate, BookingDate.Date.Add(StartTime), BookingDate.Date.Add(EndTime));
                        }
                    }
                    return Page();
                }
            }
            catch (ArgumentException ex) // Handle specific business logic errors thrown by the service (e.g., user or table not found, table not available)
            {
                _logger.LogWarning(ex, "Restaurant booking creation failed due to invalid arguments: {Message}", ex.Message); // Log the specific exception
                ModelState.AddModelError(string.Empty, ex.Message); // Add the exception message as a general model error
                // Re-populate dropdowns before returning the page on ArgumentException
                await PopulateOrganizationsDropdownAsync();
                if (SelectedOrganizationId != Guid.Empty)
                {
                    await PopulateRestaurantsDropdownAsync(SelectedOrganizationId);
                    if (SelectedRestaurantId != Guid.Empty)
                    {
                        await PopulateAvailableTablesDropdownAsync(SelectedRestaurantId, BookingDate, BookingDate.Date.Add(StartTime), BookingDate.Date.Add(EndTime));
                    }
                }
                return Page();
            }
            catch (Exception ex) // Handle any other unexpected errors during booking creation
            {
                _logger.LogError(ex, "An unexpected error occurred while creating the restaurant booking."); // Log the exception details
                ModelState.AddModelError(string.Empty, "An internal server error occurred."); // Provide a generic error message to the user
                // Re-populate dropdowns before returning the page on unexpected error
                await PopulateOrganizationsDropdownAsync();
                if (SelectedOrganizationId != Guid.Empty)
                {
                    await PopulateRestaurantsDropdownAsync(SelectedOrganizationId);
                    if (SelectedRestaurantId != Guid.Empty)
                    {
                        await PopulateAvailableTablesDropdownAsync(SelectedRestaurantId, BookingDate, BookingDate.Date.Add(StartTime), BookingDate.Date.Add(EndTime));
                    }
                }
                return Page();
            }
        }


        // Helper method to populate the Organizations dropdown list
        private async Task PopulateOrganizationsDropdownAsync()
        {
            _logger.LogDebug("Populating organizations dropdown.");
            var organizations = await _restaurantService.GetAllOrganizationsAsync();
            // Map entities to SelectListItem, using Id (Guid) as Value and Name (string) as Text
            OrganizationsList = organizations.Select(o => new SelectListItem { Value = o.Id.ToString(), Text = o.Name }).ToList();
            // Add a default "Select Organization" option at the beginning
            OrganizationsList.Insert(0, new SelectListItem { Value = "", Text = "--- Select Organization ---", Disabled = true, Selected = true });
        }

        // Helper method to populate the Restaurants dropdown list based on the selected organization ID
        private async Task PopulateRestaurantsDropdownAsync(Guid organizationId)
        {
             _logger.LogDebug("Populating restaurants dropdown for organization ID {OrganizationId}.", organizationId);
            var restaurants = await _restaurantService.GetRestaurantsByOrganizationAsync(organizationId);
            // Map entities to SelectListItem, using Id (Guid) as Value and Name (string) as Text
            RestaurantsList = restaurants.Select(r => new SelectListItem { Value = r.Id.ToString(), Text = r.Name }).ToList();
             // Add a default "Select Restaurant" option at the beginning
            RestaurantsList.Insert(0, new SelectListItem { Value = "", Text = "--- Select Restaurant ---", Disabled = true, Selected = true });
        }

        // Helper method to populate the Available Tables dropdown list based on the selected restaurant ID, date, and time
        // Note: This helper's signature expects DateTime for time parameters as per the error message.
        private async Task PopulateAvailableTablesDropdownAsync(Guid restaurantId, DateTime bookingDate, DateTime startTime, DateTime endTime)
        {
             _logger.LogDebug("Populating available tables dropdown for restaurant ID {RestaurantId} on {BookingDate:d} from {StartTime:hh\\:mm} to {EndTime:hh\\:mm}.",
                restaurantId, bookingDate, startTime, endTime); // Log with specific date/time formatters
            // Fetch available tables using the injected service
            // FIX: Pass the DateTime startTime and endTime directly to the service method
            // as the error indicates it expects DateTime, not TimeSpan.
            var availableTables = await _restaurantService.GetAvailableTablesAsync(restaurantId, bookingDate, startTime, endTime); // Pass DateTime directly

            // Map RestaurantTable entities to SelectListItem, using Id (Guid) as Value and a formatted string as Text
            AvailableTablesList = availableTables.Select(t => new SelectListItem { Value = t.Id.ToString(), Text = $"{t.TableName} ({t.Capacity} seats)" }).ToList();
             // Add a default "Select Table" option at the beginning
            AvailableTablesList.Insert(0, new SelectListItem { Value = "", Text = "--- Select Table ---", Disabled = true, Selected = true });
        }
    }
}
