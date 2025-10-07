using Microsoft.AspNetCore.Mvc;
using Serilog;

namespace server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        public AuthController()
        {
        }

        [HttpPost("login")]
        public IActionResult Login()
        {
            Log.Information("Login called");
            return Ok("Login successful");
        }

        [HttpGet("logout")]
        public IActionResult Logout()
        {
            Log.Information("Logout called");
            return Ok("Logout successful");
        }

        [HttpGet("register")]
        public IActionResult Register()
        {
            Log.Information("Register called");
            return Ok("Register successful");
        }
    }
}