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

        [HttpGet("login")]
        public IActionResult Login()
        {
            Log.Information("Login called");
            return Ok("Login successful");
        }
    }
}