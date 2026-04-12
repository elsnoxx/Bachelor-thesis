using BCrypt.Net;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.VisualBasic;
using Newtonsoft.Json.Linq;
using Serilog;
using server.Data;
using server.Helpers;
using server.Models;
using server.Models.Auth;
using server.Models.DB;
using server.Models.DTO;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;
using server.Services.Utils.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Net.Http.Headers;
using System.Runtime.InteropServices;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;

namespace server.Services.DbServices
{
    public class AuthService : IAuthService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtUtils _jwtUtils;
        private readonly ITokenRepository _tokenRepository;
        private readonly AppDbContext _context;
        private readonly IConfiguration _configuration;
        public AuthService(IUserRepository userRepository, IJwtUtils jwtUtils, ITokenRepository tokenRepository, AppDbContext appDbContext, IConfiguration configuration)
        {
            _userRepository = userRepository;
            _jwtUtils = jwtUtils;
            _tokenRepository = tokenRepository;
            _context = appDbContext;
            _configuration = configuration;
        }

        public async Task<Result<Tokens>> LoginAsync(UserLogin userLogin, HttpContext httpContext)
        {
            var user = await _userRepository.GetByEmailAsync(userLogin.Email);
            
            if (user == null)
            {
                return Result<Tokens>.Fail("Invalid email or password.");
            }

            if(!user.IsEmailConfirmed)
            {
                return Result<Tokens>.Fail("Email not confirmed. Please check your inbox.");
            }

            if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.PasswordHash))
            {
                return Result<Tokens>.Fail("Invalid email or password.");
            }

            var tokens = GenerateTokens(user);

            // Create refresh token and save it to the database
            var newRefreshToken = CreateRefreshToken(user.Id, HttpHelper.GetClientIp(httpContext), tokens.RefreshToken);

            // Register the login and save the refresh token in a transaction
            await RegisterLogin(newRefreshToken);

            return Result<Tokens>.Ok(tokens);
        }

        public async Task<Result<bool>> RegisterAsync(UserRegister userRegister)
        {
            var existingUsername = await _userRepository.GetByUsernameAsync(userRegister.Username);
            var existingEmail = await _userRepository.GetByEmailAsync(userRegister.Email);

            if (existingUsername != null || existingEmail != null) 
            {
                return Result<bool>.Fail("Username or email already exists.");
            }

            var newUser = CreateUser(userRegister);
            

            try
            {
                await _userRepository.AddAsync(newUser);

                await SendConfirmationEmail(newUser.Email);

                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                Log.Error("Error registering user: {Error}", ex.Message);
                return Result<bool>.Fail("Internal server error");
            }
        }

        public async Task<Result<bool>> ConfirmEmailAsync(string token)
        {
            if (!Guid.TryParse(token, out Guid tokenGuid))
                return Result<bool>.Fail("Invalid token format.");

            var user = await _userRepository.GetByEmailConfirmationTokenAsync(tokenGuid);


            if (user == null || user.EmailConfirmationToken != tokenGuid)
            {
                return Result<bool>.Fail("Invalid token or email.");
            }
            user.IsEmailConfirmed = true;
            user.EmailConfirmationToken = Guid.Empty; 
            try
            {
                await _userRepository.UpdateAsync(user);
                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                Log.Error("Error confirming email: {Error}", ex.Message);
                return Result<bool>.Fail("Internal server error");
            }
        }

        public async Task<Result<Tokens>> RefreshTokenAsync(string refreshToken, HttpContext httpContext)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Result<Tokens>.Fail("No refresh token provided.");
            }

            var tokenInfo = await _tokenRepository.GetByTokenAsync(refreshToken);

            if (tokenInfo == null || tokenInfo.Expires < DateTime.UtcNow)
            {
                return Result<Tokens>.Fail("Invalid or expired refresh token.");
            }

            var user = await _userRepository.GetByIdAsync(tokenInfo.UserId);

            var tokens = GenerateTokens(user);

            // Refresh token rotation: create a new refresh token and revoke the old one
            var newRefreshToken = CreateRefreshToken(user.Id, HttpHelper.GetClientIp(httpContext), tokens.RefreshToken);

            await NewRefreshToken(newRefreshToken, tokenInfo );

            return Result<Tokens>.Ok(tokens);

        }

        public async Task RegisterLogin(RefreshToken refreshToken)
        {

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var user = await _userRepository.GetByIdAsync(refreshToken.UserId);
                // Update last login time for the user
                await _userRepository.UpdateLastLoginAsync(user);

                await _tokenRepository.AddAsync(refreshToken);

                // Confirm the entire transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // revert all changes in case of an error
                await transaction.RollbackAsync();
                Log.Error(ex, "Error during RegisterLoginAsync");
                throw; // Propagate the error further
            }
        }

        public async Task NewRefreshToken(RefreshToken newRefreshToken, RefreshToken tokenInfo)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _tokenRepository.RevokeTokenAsync(tokenInfo.Id, newRefreshToken.CreatedByIp, newRefreshToken.Token);

                await _tokenRepository.AddAsync(newRefreshToken);

                // Confirm the entire transaction
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Revert all changes in case of an error
                await transaction.RollbackAsync();
                Log.Error(ex, "Error during NewRefreshToken");
                throw; // Propagate the error further
            }

        }

        private async Task SendConfirmationEmail(string email)
        {
            // Read SMTP configuration from appsettings.json or environment variables
            var smtpHost = _configuration["SMTP_HOST"];
            var smtpPort = int.Parse(_configuration["SMTP_PORT"] ?? "587");
            var smtpUser = _configuration["SMTP_USER"];
            var smtpPass = _configuration["SMTP_PASS"];
            var serverUrl = _configuration["SERVER_URL"] ?? "https://localhost:7202";

            if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
            {
                throw new Exception("SMTP konfigurace nebyla nalezena v appsettings.json ani v ENV.");
            }

            var user = await _userRepository.GetByEmailAsync(email);

            var confirmationLink = $"{serverUrl}/api/confirm-email?token={user.EmailConfirmationToken.ToString()}";

            using var client = new System.Net.Mail.SmtpClient(smtpHost, smtpPort)
            {
                Credentials = new System.Net.NetworkCredential(smtpUser, smtpPass),
                EnableSsl = true
            };

            var mailMessage = new System.Net.Mail.MailMessage
            {
                From = new System.Net.Mail.MailAddress(smtpUser),
                Subject = "Ověření e-mailu - Biofeedback App",
                Body = $"<h1>Vítejte!</h1><p>Pro dokončení registrace klikněte na odkaz níže:</p><a href='{confirmationLink}'>Potvrdit účet</a>",
                IsBodyHtml = true
            };
            mailMessage.To.Add(email);

            await client.SendMailAsync(mailMessage);
        }

        public async Task<bool> ResetPasswordAsync(UserResetPassword model)
        {
            // 1. Search for the user by email
            var user = await _context.Users.FirstOrDefaultAsync(u => u.Email == model.Email);

            if (user == null)
            {
                return false;
            }

            // 2. Hash the new password and update the user's password hash
            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(model.NewPassword);

            // 3. Save the changes to the database
            _context.Users.Update(user);
            await _context.SaveChangesAsync();

            return true;
        }

        private Tokens GenerateTokens(User user)
        {
            return new Tokens
            {
                TokenJWT = _jwtUtils.GenerateJwtToken(user),
                RefreshToken = _jwtUtils.GenerateRefreshToken()
            };
        }

        private RefreshToken CreateRefreshToken(Guid userId, string ip, string token)
        {
            return new RefreshToken
            {
                UserId = userId,
                Token = token,
                CreatedByIp = ip
            };
        }

        private User CreateUser(UserRegister userRegister)
        {
            return new User
            {
                Id = Guid.NewGuid(),
                Username = userRegister.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegister.Password),
                Email = userRegister.Email,
                Role = "User"
            };
        }

    }
}
