using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Serilog;
using server.Models.Auth;
using server.Services.DbServices.Interfaces;
using System.Diagnostics.CodeAnalysis;
using System.Threading.Tasks;

namespace server.Controllers
{
    [ApiController]
    [Route("api/")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthDbService _authDbService;
        public AuthController(IAuthDbService authDbService)
        {
            _authDbService = authDbService;
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] UserLogin userLogin)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid user data");
            }

            try
            {
                var result = await _authDbService.LoginAsync(userLogin, HttpContext);
                if (result.Success)
                {
                    return Ok(new { Token = result.Data.tokenJWT, RefreshToken = result.Data.refreshToken  });
                }
                return Unauthorized(result.Data);
                
            }
            catch (Exception ex)
            {
                Log.Error($"Internal server error: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("refresh")]
        public async Task<IActionResult> Logout()
        {
            if(!Request.Cookies.TryGetValue("refresh_token", out var refreshTokenValue))
            {
                return BadRequest("No refresh token provided");
            }
            try
            {
                var result = await _authDbService.RefreshTokenAsync(refreshTokenValue, HttpContext);
                if (result.Success)
                {
                    return Ok(new { Token = result.Data.tokenJWT, RefreshToken = result.Data.refreshToken });
                }
                return Unauthorized(result.Data);
            }
            catch (Exception ex)
            {
                Log.Error($"Internal server error: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] UserRegister user)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest("Invalid user data");
            }

            try
            {
                var result = await _authDbService.RegisterAsync(user);
                if (result.Success)
                {
                    return Ok("User registered successfully");
                }
                return BadRequest(result.Error);
            }
            catch (Exception ex)
            {
                Log.Error($"Internal server error: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}
// user@example.com
//   "username": "elsnoxx",
//   "password": "Test12345!",