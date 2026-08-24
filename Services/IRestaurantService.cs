using System.Collections.Generic;
using System.Threading.Tasks;
using TableTies.Models; 
using System; // Needed for DateTime, Guid, TimeSpan // Added TimeSpan here just in case, though not strictly needed for this interface definition

namespace TableTies.Services 
{
    
    public interface IRestaurantService
    {
        
        Task<List<Organization>> GetAllOrganizationsAsync();

        
        
        Task<List<Restaurant>> GetRestaurantsByOrganizationAsync(Guid organizationId); 

        
        Task<List<RestaurantTable>> GetAvailableTablesAsync(Guid restaurantId, DateTime date, DateTime startTime, DateTime endTime); // Confirmed DateTime for time parameters

       
        Task<Organization?> GetOrganizationByIdAsync(Guid id); 
        Task<Restaurant?> GetRestaurantByIdAsync(Guid id); 
        Task<RestaurantTable?> GetRestaurantTableByIdAsync(Guid id); 

    }
}
