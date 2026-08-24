using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using TableTies.Models;
using TableTies.Services;
using Microsoft.Extensions.Logging;
using System.Linq;

namespace TableTies.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class BookingController : ControllerBase
    {
        private readonly IBookingService _bookingService;
        private readonly ILogger<BookingController>? _logger;

        public BookingController(IBookingService bookingService, ILogger<BookingController>? logger = null)
        {
            _bookingService = bookingService;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> CreateBooking([FromBody] BookingDto bookingDto)
        {
            if (!ModelState.IsValid)
            {
                _logger?.LogWarning("Invalid ModelState for booking creation.");
                return BadRequest(ModelState);
            }

            _logger?.LogInformation("Attempting to create booking for user email: {UserEmail}", bookingDto.UserEmail);

            try
            {
                var createdBooking = await _bookingService.CreateBookingAsync(bookingDto);

                if (createdBooking == null)
                {
                    _logger?.LogWarning("Booking creation failed for user email: {UserEmail}. Service returned null.", bookingDto.UserEmail);
                    return BadRequest("Could not create booking. Please check details.");
                }

                _logger?.LogInformation("Booking created successfully with ID {BookingId} for user {UserId}.", createdBooking.Id, createdBooking.UserId);

                return CreatedAtAction(nameof(GetBooking), new { id = createdBooking.Id }, createdBooking);
            }
            catch (ArgumentException ex)
            {
                 _logger?.LogError(ex, "Argument error during booking creation for user email: {UserEmail}.", bookingDto.UserEmail);
                 return BadRequest(ex.Message);
            }
            catch (Exception ex)
            {
                _logger?.LogError(ex, "An error occurred during booking creation for user email: {UserEmail}.", bookingDto.UserEmail);
                return StatusCode(500, "An internal error occurred while creating the booking.");
            }
        }

        [HttpGet("{id:guid}")]
        public async Task<ActionResult<Booking>> GetBooking(Guid id)
        {
            _logger?.LogInformation("Attempting to retrieve booking with ID: {BookingId}", id);

            var booking = await _bookingService.GetBookingAsync(id);
            if (booking == null)
            {
                _logger?.LogInformation("Booking with ID {BookingId} not found.", id);
                return NotFound();
            }

            _logger?.LogInformation("Booking with ID {BookingId} retrieved successfully.", id);

            return Ok(booking);
        }

        [HttpGet("user")]
        public ActionResult<IEnumerable<Booking>> GetUserBookings()
        {
            // Since authentication is disabled, return empty list for demonstration
            _logger?.LogInformation("GetUserBookings called but authentication is disabled.");
            return Ok(new List<Booking>());
        }

        [HttpDelete("{id:guid}")]
        public async Task<IActionResult> CancelBooking(Guid id)
        {
            // Since authentication is disabled, allow cancellation for demonstration
            _logger?.LogInformation("CancelBooking called for ID {BookingId} but authentication is disabled.", id);
            
            try
            {
                var success = await _bookingService.CancelBookingAsync(id, Guid.Empty);
                if (success)
                {
                    _logger?.LogInformation("Booking ID {BookingId} cancelled successfully.", id);
                    return NoContent();
                }
                else
                {
                    _logger?.LogWarning("Booking ID {BookingId} cancellation failed.", id);
                    return BadRequest("Could not cancel booking. It may not exist or already be cancelled.");
                }
            }
            catch (Exception ex)
            {
                 _logger?.LogError(ex, "An error occurred while cancelling booking ID: {BookingId}.", id);
                 return StatusCode(500, "An internal error occurred while cancelling the booking.");
            }
        }

        [HttpPut("{id:guid}")]
        public async Task<IActionResult> UpdateBooking(Guid id, [FromBody] Booking updatedBooking)
        {
            if (id != updatedBooking.Id)
            {
                _logger?.LogWarning("Booking ID mismatch in PUT request. Route ID: {RouteId}, Body ID: {BodyId}.", id, updatedBooking.Id);
                return BadRequest("Booking ID mismatch.");
            }

            if (!ModelState.IsValid)
            {
                _logger?.LogWarning("Invalid ModelState for booking update of ID: {BookingId}.", id);
                return BadRequest(ModelState);
            }

            _logger?.LogInformation("Attempting to update booking ID: {BookingId}.", id);

            try
            {
                var success = await _bookingService.UpdateBookingAsync(updatedBooking, Guid.Empty);
                if (success)
                {
                    _logger?.LogInformation("Booking ID {BookingId} updated successfully.", id);
                    return NoContent();
                }
                else
                {
                    _logger?.LogWarning("Booking ID {BookingId} update failed.", id);
                    return BadRequest("Could not update booking. It may not exist or there is a conflict.");
                }
            }
             catch (Exception ex)
            {
                 _logger?.LogError(ex, "An error occurred while updating booking ID: {BookingId}.", id);
                 return StatusCode(500, "An internal error occurred while updating the booking.");
            }
        }

        [HttpDelete("delete/{id:guid}")]
        public async Task<IActionResult> DeleteBooking(Guid id)
        {
            _logger?.LogInformation("Attempting to delete booking ID: {BookingId}.", id);

            try
            {
                 var success = await _bookingService.DeleteBookingAsync(id, Guid.Empty);
                 if (success)
                 {
                     _logger?.LogInformation("Booking ID {BookingId} deleted successfully.", id);
                     return NoContent();
                 }
                 else
                 {
                     _logger?.LogWarning("Booking ID {BookingId} deletion failed.", id);
                     return BadRequest("Could not delete booking. It may not exist.");
                 }
             }
             catch (Exception ex)
            {
                  _logger?.LogError(ex, "An error occurred while deleting booking ID: {BookingId}.", id);
                  return StatusCode(500, "An internal error occurred while deleting the booking.");
             }
        }
    }
}
