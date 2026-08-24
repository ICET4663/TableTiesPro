using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using TableTies.Data; 
using TableTies.Models; 
using System; 
using System.Collections.Generic; 
using System.Linq; 
using System.Threading.Tasks; 
using System.Globalization; 
namespace TableTies.Services 
{
    public class ConsultantService : IConsultantService
    {
        private readonly ApplicationDbContext _context; 

                public ConsultantService(ApplicationDbContext context)
        {
            _context = context;
        }

      
        public async Task<List<Consultant>> GetConsultantsAsync()
        {
            return await _context.Consultants.ToListAsync();
        }

        public async Task<List<SelectListItem>> GetConsultantsListAsync()
        {
            var consultants = await GetConsultantsAsync();
            return consultants.Select(c => new SelectListItem
            {
               
                Value = c.Id.ToString(), 
                Text = c.Name + (string.IsNullOrEmpty(c.Specialty) ? "" : $" ({c.Specialty})") 
            }).ToList();
        }

             public async Task<ConsultantBooking?> GetConsultantBookingAsync(int bookingId)
        {
            
            return await _context.ConsultantBookings
                .Include(cb => cb.User)       
                .FirstOrDefaultAsync(cb => cb.Id == bookingId); 
        }

       
        public async Task<List<ConsultantBooking>> GetUserConsultantBookingsAsync(Guid userId)
        {
           
            var bookings = await _context.ConsultantBookings
                .Where(cb => cb.UserId == userId && cb.CancelledDateTime == null)
                .Include(cb => cb.Consultant) 
                .ToListAsync(); 

           
            var filteredAndOrderedBookings = bookings
                .AsEnumerable() 
                .Where(cb => cb.BookingDateTime.Add(cb.Duration) > DateTime.UtcNow) 
                .ToList(); 

            return filteredAndOrderedBookings;
        }

       
        
        public async Task<ConsultantBooking?> CreateConsultantBookingAsync(Guid userId, Guid consultantId, DateTime bookingDateTime, TimeSpan duration, string? details)
        {
            var consultant = await _context.Consultants.FindAsync(consultantId);
            if (consultant == null)
            {
                return null; 
            }

           

            var newBooking = new ConsultantBooking
            {
                UserId = userId, // Assign Guid userId
                ConsultantId = consultantId,
                BookingDateTime = bookingDateTime,
                Duration = duration,
                Details = details
            };

            _context.ConsultantBookings.Add(newBooking); // Add the new booking to the context
            await _context.SaveChangesAsync(); // Save changes to the database

            return newBooking; // Return the created booking
        }

        /// <summary>
        /// Cancels an existing consultant booking.
        /// Verifies user ownership and that the booking is not already cancelled.
        /// </summary>
        /// <param name="bookingId">The ID (int) of the booking to cancel.</param>
        /// <param name="userId">The ID (Guid) of the user attempting to cancel (for ownership verification).</param>
        /// <returns>True if cancellation was successful, false otherwise.</returns>
        // Changed userId parameter type to Guid
        public async Task<bool> CancelConsultantBookingAsync(int bookingId, Guid userId)
        {
            var booking = await _context.ConsultantBookings
                
                .Where(cb => cb.Id == bookingId && cb.UserId == userId && cb.CancelledDateTime == null)
                .FirstOrDefaultAsync();

            if (booking == null)
            {
                return false; 
            }

         

            booking.CancelledDateTime = DateTime.UtcNow; 
            await _context.SaveChangesAsync();

            return true; 
        }

       
        public async Task<bool> UpdateConsultantBookingAsync(ConsultantBooking updatedBooking, Guid userId)
        {
          
            var existingBooking = await _context.ConsultantBookings
                .Where(cb => cb.Id == updatedBooking.Id && cb.UserId == userId && cb.CancelledDateTime == null)
                .FirstOrDefaultAsync();

            if (existingBooking == null)
            {
                return false;
            }

           


            existingBooking.BookingDateTime = updatedBooking.BookingDateTime; 
            existingBooking.Duration = updatedBooking.Duration;          
            existingBooking.Details = updatedBooking.Details;            

           
            await _context.SaveChangesAsync(); 

            return true; // Update successful
        }

      
    }
}
