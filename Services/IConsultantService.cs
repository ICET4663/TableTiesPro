using Microsoft.AspNetCore.Mvc.Rendering;
using TableTies.Models;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace TableTies.Services
{
    public interface IConsultantService
    {
        Task<List<Consultant>> GetConsultantsAsync();

        Task<List<SelectListItem>> GetConsultantsListAsync();

        Task<ConsultantBooking?> GetConsultantBookingAsync(int bookingId);

        Task<List<ConsultantBooking>> GetUserConsultantBookingsAsync(Guid userId);

        Task<ConsultantBooking?> CreateConsultantBookingAsync(Guid userId, Guid consultantId, DateTime bookingDateTime, TimeSpan duration, string? details);

        Task<bool> CancelConsultantBookingAsync(int bookingId, Guid userId);

        Task<bool> UpdateConsultantBookingAsync(ConsultantBooking updatedBooking, Guid userId);
    }
}
