using System.Collections.Generic;
using System.Threading.Tasks;
using TableTies.Models;
using System;

namespace TableTies.Services
{
    public interface IBookingService
    {
        Task<Booking?> CreateBookingAsync(BookingDto bookingDto);

        Task<Booking?> GetBookingAsync(Guid id);

        Task<List<Booking>> GetUserBookingsAsync(Guid userId);

        Task<bool> CancelBookingAsync(Guid bookingId, Guid userId);

        Task<bool> UpdateBookingAsync(Booking booking, Guid userId);

        Task<Booking?> UpdateBookingAsync(Guid bookingId, BookingDto bookingDto);

        Task<bool> DeleteBookingAsync(Guid bookingId, Guid userId);
    }
}
