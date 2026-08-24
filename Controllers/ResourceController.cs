using Microsoft.AspNetCore.Mvc;
using TableTies.Models;
using TableTies.Data;
using System.Collections.Generic;
using System.Linq;

namespace SpotApp.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ResourceController : ControllerBase
    {
        private readonly ApplicationDbContext _context;

        public ResourceController(ApplicationDbContext context)
        {
            _context = context;
        }

        [HttpGet("GetResources/{organizationType}")]
        public IActionResult GetResources(string organizationType)
        {
            if (organizationType == "Restaurant")
            {
                var resources = _context.RestaurantTables.ToList();
                return Ok(resources);
            }
            return NotFound(new { Message = "Organization type not found." });
        }
    }
}
