using Microsoft.IdentityModel.Tokens;
using server.Models.DB;
using server.Services.Utils.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace server.Services.Utils
{
    /// <summary>
    /// Utility class for handling JSON Web Token (JWT) generation and secure random token creation.
    /// Provides methods for user authentication and session refresh mechanisms.
    /// </summary>
    public class JwtUtils : IJwtUtils
    {
        private readonly IConfiguration _config;

        public JwtUtils(IConfiguration config)
        {
            _config = config;
        }

        /// <summary>
        /// Generates a signed JWT access token for a specific user containing identity claims.
        /// </summary>
        /// <param name="user">The user entity for whom the token is being generated.</param>
        /// <returns>An encoded JWT string valid for 10 minutes.</returns>
        public string GenerateJwtToken(User user)
        {
            // Retrieve security settings from configuration
            var jwtKey = _config["Jwt:Key"];
            var jwtIssuer = _config["Jwt:Issuer"];

            // Define user identity claims to be embedded in the token payload
            var claims = new[]
            {
                new Claim(ClaimTypes.Name, user.Username),
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Role, user.Role)
            };

            // Set up cryptographic signing key and credentials (HMAC SHA256)
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            // Construct the JWT object with expiration and signing details
            var token = new JwtSecurityToken(
                issuer: jwtIssuer,
                audience: null,
                claims: claims,
                expires: DateTime.Now.AddMinutes(10), // Short lifespan for security
                signingCredentials: creds);

            // Serialize the token to a string format
            var handler = new JwtSecurityTokenHandler();
            return handler.WriteToken(token);
        }

        /// <summary>
        /// Generates a cryptographically secure random string to be used as a refresh token.
        /// </summary>
        /// <returns>A Base64-encoded string representing 64 random bytes.</returns>
        public string GenerateRefreshToken()
        {
            var randomNumber = new byte[64];
            // Use cryptographically strong random number generator (RNG)
            using (var rng = RandomNumberGenerator.Create())
            {
                rng.GetBytes(randomNumber);
            }
            return Convert.ToBase64String(randomNumber);
        }
    }
}