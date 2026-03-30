using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using server.Models.Auth;
using server.Models.DB;
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
                // HttpContext je zde dostupný automaticky, předáváme ho do servisy
                var result = await _authDbService.LoginAsync(userLogin, HttpContext);

                if (result.Success)
                {
                    // Nastavení cookie přímo přes vlastnost Response
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true,
                        Secure = true, // Pokud testuješ na čistém HTTP (ne HTTPS), dej false
                        SameSite = SameSiteMode.None,
                        Expires = DateTime.UtcNow.AddDays(7)
                    };

                    // Tady to je - používáš vlastnost Response, která patří k ControllerBase
                    Response.Cookies.Append("refresh_token", result.Data.refreshToken, cookieOptions);

                    return Ok(new
                    {
                        Token = result.Data.tokenJWT,
                        RefreshToken = result.Data.refreshToken
                    });
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
        public async Task<IActionResult> Refresh()
        {
            // Zkontroluj, zda se cookie jmenuje "refresh_token" nebo "refreshToken"
            if (!Request.Cookies.TryGetValue("refresh_token", out var refreshTokenValue))
            {
                return Unauthorized("Missing refresh token cookie");
            }

            var result = await _authDbService.RefreshTokenAsync(refreshTokenValue, HttpContext);

            if (result.Success)
            {
                // Vracíme JSON s novým JWT, Refresh token zůstává v cookie (nastaveno v servise)
                return Ok(new { Token = result.Data.tokenJWT });
            }
            return Unauthorized();
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