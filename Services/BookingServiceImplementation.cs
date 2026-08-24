using TableTies.Models;
using TableTies.Data;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;
using Microsoft.Extensions.Logging;

namespace TableTies.Services
{
    public class BookingServiceImplementation : IBookingService
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<BookingServiceImplementation>? _logger;

        public BookingServiceImplementation(ApplicationDbContext context, ILogger<BookingServiceImplementation>? logger = null)
        {
            _context = context;
            _logger = logger;
        }

        public async Task<Booking?> CreateBookingAsync(BookingDto bookingDto)
        {
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == bookingDto.UserEmail);

            if (user == null)
            {
                _logger?.LogWarning("User with email {UserEmail} not found during booking creation.", bookingDto.UserEmail);
                return null;
            }

            var restaurantTable = await _context.RestaurantTables.FindAsync(bookingDto.RestaurantTableId);
             if (restaurantTable == null)
             {
                 _logger?.LogWarning("Restaurant Table with Id {RestaurantTableId} not found during booking creation.", bookingDto.RestaurantTableId);
                 return null;
             }

             var restaurant = await _context.Restaurants.FindAsync(bookingDto.RestaurantId);
             if (restaurant == null)
             {
                 _logger?.LogWarning("Restaurant with Id {RestaurantId} not found during booking creation.", bookingDto.RestaurantId);
                 return null;
             }

            var booking = new Booking
            {
                UserId = user.Id,
                RestaurantId = bookingDto.RestaurantId,
                TableId = bookingDto.RestaurantTableId,

                BookingDateTime = bookingDto.BookingDate.Date.Add(bookingDto.StartTime),

                NumberOfGuests = bookingDto.NumberOfGuests,
                SpecialRequests = bookingDto.SpecialRequests,

                 Duration = bookingDto.EndTime - bookingDto.StartTime,
                 CreatedAt = DateTime.UtcNow,
                 BookingType = "RestaurantTable"
            };

             _context.Bookings.Add(booking);
             await _context.SaveChangesAsync();

            _logger?.LogInformation("Booking created successfully with ID {BookingId} for user {UserId}.", booking.Id, booking.UserId);

            return booking;
        }

        public async Task<Booking?> GetBookingAsync(Guid id)
        {
             var booking = await _context.Bookings
                 .Include(b => b.Restaurant)
                 .Include(b => b.Table)
                 .Include(b => b.User)
                 .FirstOrDefaultAsync(b => b.Id == id);

             if (booking == null)
             {
                 _logger?.LogInformation("Booking with ID {BookingId} not found.", id);
             }

             return booking;
        }

        public async Task<List<Booking>> GetUserBookingsAsync(Guid userId)
        {
             var bookings = await _context.Bookings
                 .Where(b => b.UserId == userId)
                 .Include(b => b.Restaurant)
                 .Include(b => b.Table)
                 .Include(b => b.User)
                 .OrderBy(b => b.BookingDateTime)
                 .ToListAsync();

            _logger?.LogInformation("Retrieved {BookingCount} bookings for user {UserId}.", bookings.Count, userId);

            return bookings;
        }

        /// <summary>
        /// Cancels a booking by its ID, verifying user ownership.
        /// Provides more specific feedback if the booking is not found, not owned, or already cancelled.
        /// </summary>
        /// <param name="bookingId">The ID (Guid) of the booking to cancel.</param>
        /// <param name="userId">The ID (Guid) of the user attempting to cancel.</param>
        /// <returns>True if the booking was cancelled successfully, otherwise false.</returns>
        public async Task<bool> CancelBookingAsync(Guid bookingId, Guid userId)
        {
            _logger?.LogInformation("Attempting to cancel booking with ID {BookingId} for user {UserId}.", bookingId, userId);

            // Fetch the booking first without checking the CancelledAt status
            var booking = await _context.Bookings.FirstOrDefaultAsync(b => b.Id == bookingId);

            if (booking == null)
            {
                _logger?.LogWarning("CancelBookingAsync: Booking with ID {BookingId} not found.", bookingId);
                return false; // Booking not found
            }

            if (booking.UserId != userId)
            {
                _logger?.LogWarning("CancelBookingAsync: Booking with ID {BookingId} does not belong to user {UserId}. Ownership check failed.", bookingId, userId);
                return false; // Booking does not belong to the user
            }

            if (booking.CancelledAt != null)
            {
                _logger?.LogWarning("CancelBookingAsync: Booking with ID {BookingId} is already cancelled.", bookingId);
                return false; // Booking is already cancelled
            }

            // Optional: Add logic here to prevent cancelling bookings too close to the time
            // For example: if (booking.BookingDateTime.Add(booking.Duration) < DateTime.UtcNow.AddHours(24)) { return false; }

            booking.CancelledAt = DateTime.UtcNow; // Mark as cancelled with current UTC time
            await _context.SaveChangesAsync(); // Save changes to the database

            _logger?.LogInformation("Booking with ID {BookingId} cancelled successfully by user {UserId}.", bookingId, userId);

            return true; // Indicate successful cancellation
        }

        public async Task<Booking?> UpdateBookingAsync(Guid bookingId, BookingDto bookingDto)
        {
             var booking = await _context.Bookings.FindAsync(bookingId);
             if (booking == null)
             {
                 _logger?.LogWarning("Attempted to update non-existent booking with ID {BookingId} using DTO.", bookingId);
                 return null;
             }

             try
             {
                 _context.Bookings.Update(booking);
                 await _context.SaveChangesAsync();
                 _logger?.LogInformation("Booking with ID {BookingId} updated successfully using DTO.", bookingId);
                 return booking;
             }
             catch (Exception ex)
             {
                  _logger?.LogError(ex, "Error updating booking with ID {BookingId} using DTO.", bookingId);
                  return null;
             }
        }

        public async Task<bool> UpdateBookingAsync(Booking booking, Guid userId)
        {
            _logger?.LogInformation("Attempting to update booking with ID {BookingId} for user {UserId}.", booking.Id, userId);
            try
            {
                var existingBooking = await _context.Bookings
                    .Where(b => b.Id == booking.Id && b.UserId == userId && b.CancelledAt == null)
                    .FirstOrDefaultAsync();

                if (existingBooking == null)
                {
                    _logger?.LogWarning("Booking with ID {BookingId} not found, does not belong to user {UserId}, or is already cancelled during update attempt.", booking.Id, userId);
                    return false;
                }

                // --- Add Availability Check Here ---
                // A real-world scenario would require checking if the table is available
                // at the *new* requested time slot (BookingDateTime + Duration)
                // before allowing the update. This would query existing non-cancelled bookings
                // for the same table to see if the new time range overlaps.
                // For simplicity, this check is omitted here, but it's crucial for a robust system.
                // Example check signature:
                // bool isAvailable = await IsTableAvailable(existingBooking.TableId.Value, updatedBooking.BookingDateTime, updatedBooking.Duration, existingBooking.Id); // Pass current booking ID to exclude itself from checks
                // if (!isAvailable) { /* handle conflict */ return false; }
                // --- End Availability Check ---

                // Update the properties that are editable from the passed-in booking entity
                // Assuming the 'booking' parameter contains the updated values from the PageModel's BindProperty
                existingBooking.BookingDateTime = booking.BookingDateTime;
                existingBooking.NumberOfGuests = booking.NumberOfGuests;
                existingBooking.SpecialRequests = booking.SpecialRequests;
                existingBooking.Duration = booking.Duration; // Assuming Duration is now part of the Booking model

                // Save the changes
                // _context.Entry(existingBooking).State = EntityState.Modified; // Often not needed if fetched by the same context
                var rowsAffected = await _context.SaveChangesAsync();

                if (rowsAffected > 0)
                {
                    _logger?.LogInformation("Booking with ID {BookingId} updated successfully by user {UserId} (rows affected: {RowsAffected}).", booking.Id, userId, rowsAffected);
                    return true;
                }
                else
                {
                    _logger?.LogWarning("Booking with ID {BookingId} update failed (no rows affected) for user {UserId}.", booking.Id, userId);
                    // No rows were affected, possibly no changes were made or a concurrency issue occurred.
                    return false;
                }
            }
            catch (DbUpdateConcurrencyException ex)
            {
                 _logger?.LogError(ex, "Concurrency conflict occurred while updating booking with ID {BookingId} for user {UserId}.", booking.Id, userId);
                 // Handle concurrency conflicts (e.g., another user updated the same booking)
                 return false; // Indicate failure
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred while updating booking with ID {BookingId} for user {UserId}.", booking.Id, userId);
                // Throw or return false depending on desired error handling
                // throw; // Re-throw the exception to be handled by the caller
                return false; // Indicate failure
            }
        }

        public async Task<bool> DeleteBookingAsync(Guid bookingId, Guid userId)
        {
            _logger?.LogInformation("Attempting to delete booking with ID {BookingId} for user {UserId}.", bookingId, userId);
            try
            {
                var bookingToDelete = await _context.Bookings
                    .Where(b => b.Id == bookingId && b.UserId == userId)
                    .FirstOrDefaultAsync();

                if (bookingToDelete == null)
                {
                    _logger?.LogWarning("Booking with ID {BookingId} not found or does not belong to user {UserId} for deletion.", bookingId, userId);
                    return false;
                }

                _context.Bookings.Remove(bookingToDelete);

                var rowsAffected = await _context.SaveChangesAsync();

                if (rowsAffected > 0)
                {
                    _logger?.LogInformation("Booking with ID {BookingId} deleted successfully for user {UserId}.", bookingId, userId);
                    return true;
                }
                else
                {
                    _logger?.LogWarning("Booking with ID {BookingId} deletion failed (no rows affected) for user {UserId}.", bookingId, userId);
                    // This case is less likely if bookingToDelete was found, but possible in concurrency scenarios.
                    return false;
                }
            }
             catch (Exception ex)
             {
                 _logger?.LogError(ex, "An error occurred while deleting booking with ID {BookingId} for user {UserId}.", bookingId, userId);

                 return false;
             }
        }
    }
}
