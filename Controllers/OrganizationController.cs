using Microsoft.AspNetCore.Mvc;
using TableTies.Models;
using TableTies.Data;
namespace TableTies.Controllers
{
    [Route("api/[controller]")]
    [ApiController]

    public class OrganizationController : ControllerBase
    {
        [HttpGet("GetOrganizationTypes")]
        public IActionResult GetOrganizationTypes()
        {
            var organizationTypes = new[] { "Restaurant", "Hotel" };
            return Ok(organizationTypes);
        }

        [HttpGet("GetOrganizationConfig/{organizationType}")]
        public IActionResult GetOrganizationConfig(string organizationType)
        {
            var config = new { Name = organizationType, Services = new[] { "Booking", "Reservations" } };
            return Ok(config);
        }
    }
}
