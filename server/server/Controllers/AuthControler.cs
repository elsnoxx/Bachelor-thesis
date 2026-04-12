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
    /// <summary>
    /// Controller handling user authentication, registration, and session management.
    /// Utilizes JWT tokens for authorization and secure cookies for refresh tokens.
    /// </summary>
    [ApiController]
    [Route("api/")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authDbService;
        public AuthController(IAuthService authDbService)
        {
            _authDbService = authDbService;
        }

        /// <summary>
        /// Authenticates a user and issues a JWT token along with a secure refresh token cookie.
        /// </summary>
        /// <param name="userLogin">User credentials (email and password).</param>
        /// <returns>JWT token and user details if successful.</returns>
        /// <response code="200">Returns the JWT token and sets the refresh_token cookie.</response>
        /// <response code="401">Invalid credentials.</response>
        [HttpPost("login")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
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
                    // Secure cookie configuration to prevent XSS and CSRF
                    var cookieOptions = new CookieOptions
                    {
                        HttpOnly = true, // Prevents JavaScript access to the cookie
                        Secure = true,   // Ensures the cookie is sent only over HTTPS
                        SameSite = SameSiteMode.None, // Required for cross-domain requests in modern browsers
                        Expires = DateTime.UtcNow.AddDays(7)
                    };

                    Response.Cookies.Append("refresh_token", result.Data.RefreshToken, cookieOptions);

                    return Ok(new
                    {
                        Token = result.Data.TokenJWT,
                        RefreshToken = result.Data.RefreshToken
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

        /// <summary>
        /// Refreshes an expired JWT token using the refresh token stored in secure cookies.
        /// </summary>
        /// <returns>A new JWT token.</returns>
        [HttpPost("refresh")]
        public async Task<IActionResult> Refresh()
        {
            if (!Request.Cookies.TryGetValue("refresh_token", out var refreshTokenValue))
            {
                return Unauthorized("Missing refresh token cookie");
            }

            var result = await _authDbService.RefreshTokenAsync(refreshTokenValue, HttpContext);

            if (result.Success)
            {
                return Ok(new { Token = result.Data.TokenJWT });
            }
            return Unauthorized();
        }

        /// <summary>
        /// Registers a new user into the system.
        /// </summary>
        /// <param name="user">Registration details including email, username and password.</param>
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

        /// <summary>
        /// Initiates or completes a password reset process for a user.
        /// </summary>
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] UserResetPassword model)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            try
            {
                // Předpokládáme, že v IAuthDbService bude metoda ResetPasswordAsync
                var result = await _authDbService.ResetPasswordAsync(model);

                if (result)
                {
                    return Ok("Heslo bylo úspěšně změněno.");
                }

                return BadRequest("Uživatel s tímto e-mailem neexistuje.");
            }
            catch (Exception ex)
            {
                Log.Error($"Error during password reset: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }

        /// <summary>
        /// Confirms user's email address using a token sent during registration.
        /// </summary>
        /// <param name="token">Unique confirmation token.</param>
        [HttpGet("confirm-email")]
        public async Task<IActionResult> ConfirmEmail(string token)
        {
            try
            {
                var user = await _authDbService.ConfirmEmailAsync(token);
                if (user.Success)
                {
                    return Ok("Email confirmed successfully.");
                }
                else
                {
                    return BadRequest(user.Error);
                }
            }
            catch (Exception ex)
            {
                Log.Error($"Internal server error: {ex}");
                return StatusCode(500, "Internal server error");
            }
        }
    }
}