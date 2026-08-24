using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging; 

namespace TableTies.Controllers 
{
    [Route("api/[controller]")] 
    [ApiController] 
    public class AccountController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly ILogger<AccountController>? _logger; 

        public AccountController(
            IConfiguration configuration,
            ILogger<AccountController>? logger = null) 
        {
            _configuration = configuration;
            _logger = logger;
        }

        [HttpPost("Register")] 
        public IActionResult Register([FromBody] RegisterModel model)
        {
            _logger?.LogInformation("Register: Registration attempt for {Email} (authentication disabled)", model.Email);
            
            // Since authentication is disabled, just return success
            return Ok(new { Message = "Registration successful (authentication disabled)", Email = model.Email });
        }

        [HttpPost("Login")] 
        public IActionResult Login([FromBody] LoginModel model)
        {
            _logger?.LogInformation("Login: Login attempt for {Email} (authentication disabled)", model.Email);
            
            // Since authentication is disabled, just return success with a dummy token
            return Ok(new { 
                Message = "Login successful (authentication disabled)", 
                Email = model.Email,
                Token = "dummy-token-authentication-disabled"
            });
        }
    }

    public class RegisterModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }

    public class LoginModel
    {
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
    }
}
