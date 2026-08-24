using TableTies.Data;
using TableTies.Models;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Linq;
using System;

namespace TableTies.Services
{
    public class RestaurantService : IRestaurantService
    {
        private readonly ApplicationDbContext _context;

        public RestaurantService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<List<Organization>> GetAllOrganizationsAsync()
        {
            return await _context.Organizations.ToListAsync();
        }

        public async Task<List<Restaurant>> GetRestaurantsByOrganizationAsync(Guid organizationId)
        {
            return await _context.Restaurants
                .Where(r => r.OrganizationId == organizationId)
                .ToListAsync();
        }

        public async Task<List<RestaurantTable>> GetAvailableTablesAsync(Guid restaurantId, DateTime date, DateTime startTime, DateTime endTime)
        {
            DateTime bookingStartDateTime = date.Date + startTime.TimeOfDay;
            DateTime bookingEndDateTime = date.Date + endTime.TimeOfDay;

             var bookedTableIds = await _context.TableBookings
                 .Where(tb => tb.RestaurantId == restaurantId &&
                              tb.BookingDateTime < bookingEndDateTime &&
                              tb.BookingDateTime.AddMinutes(90) > bookingStartDateTime)
                 .Select(tb => tb.TableId)
                 .Distinct()
                 .ToListAsync();

             var availableTables = await _context.RestaurantTables
                 .Where(t => t.RestaurantId == restaurantId &&
                             !bookedTableIds.Contains(t.Id))
                 .ToListAsync();

            return availableTables;
        }

         public async Task<Organization?> GetOrganizationByIdAsync(Guid id)
         {
             return await _context.Organizations.FirstOrDefaultAsync(o => o.Id == id);
         }

         public async Task<Restaurant?> GetRestaurantByIdAsync(Guid id)
         {
             return await _context.Restaurants.FirstOrDefaultAsync(r => r.Id == id);
         }

         public async Task<RestaurantTable?> GetRestaurantTableByIdAsync(Guid id)
         {
             return await _context.RestaurantTables.FirstOrDefaultAsync(t => t.Id == id);
         }
    }
}
