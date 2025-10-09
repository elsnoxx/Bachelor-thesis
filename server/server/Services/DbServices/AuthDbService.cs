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
    public class AuthDbService : IAuthDbService
    {
        private readonly IUserRepository _userRepository;
        private readonly IJwtUtils _jwtUtils;
        private readonly ITokenRepository _tokenRepository;
        private readonly AppDbContext _context;
        public AuthDbService(IUserRepository userRepository, IJwtUtils jwtUtils, ITokenRepository tokenRepository, AppDbContext appDbContext)
        {
            _userRepository = userRepository;
            _jwtUtils = jwtUtils;
            _tokenRepository = tokenRepository;
            _context = appDbContext;
        }

        public async Task<Result<Tokens>> LoginAsync(UserLogin userLogin, HttpContext httpContext)
        {
            var user = await _userRepository.GetByEmailAsync(userLogin.Email);
            
            if (user == null)
            {
                return Result<Tokens>.Fail("Invalid email or password.");
            }

            if (!BCrypt.Net.BCrypt.Verify(userLogin.Password, user.PasswordHash))
            {
                return Result<Tokens>.Fail("Invalid email or password.");
            }

            var tokens = GenerateTokens(user);

            // Vytvoření refresh tokenu
            var refreshToken = new RefreshToken
            {
                UserId = user.Id,
                Token = tokens.refreshToken,
                CreatedByIp = httpContext.Connection.RemoteIpAddress?.ToString()
            };


            await RegisterLogin(refreshToken);

            return Result<Tokens>.Ok(tokens);
        }

        public async Task<Result<bool>> RegisterAsync(UserRegister userRegister)
        {
            var existingUsername = await _userRepository.GetByUsernameAsync(userRegister.Username);
            var existingEmail = await _userRepository.GetByEmailAsync(userRegister.Email);

            if (existingUsername != null && existingEmail != null) 
            {
                return Result<bool>.Fail("Username or email already exists.");
            }

            var newUser = new User
            {
                Id = Guid.NewGuid(),
                Username = userRegister.Username,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(userRegister.Password),
                Email = userRegister.Email,
                Role = "User"
            };
            

            try
            {
                await _userRepository.AddAsync(newUser);
                return Result<bool>.Ok(true);
            }
            catch (Exception ex)
            {
                Log.Error("Error registering user: {Error}", ex.Message);
                return Result<bool>.Fail("Internal server error");
            }
        }

        public async Task<Result<Tokens>> RefreshTokenAsync(string refreshToken, HttpContext httpContext)
        {
            if (string.IsNullOrEmpty(refreshToken))
            {
                return Result<Tokens>.Fail("No refresh token provided.");
            }

            var tokenInfo = await _tokenRepository.GetByRefreshTokenAsync(refreshToken);

            if (tokenInfo == null || tokenInfo.Expires < DateTime.UtcNow)
            {
                return Result<Tokens>.Fail("Invalid or expired refresh token.");
            }

            var user = await _userRepository.GetByIdAsync(tokenInfo.UserId);

            var tokens = GenerateTokens(user);

            // Vytvoření refresh tokenu
            var newRefreshToken = CreateRefreshToken(user.Id, HttpHelper.GetClientIp(httpContext), tokens.refreshToken);

            await NewRefreshToken(newRefreshToken, tokenInfo );

            return Result<Tokens>.Ok(tokens);

        }

        public async Task RegisterLogin(RefreshToken refreshToken)
        {

            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Aktualizace dat uživatele
                await _userRepository.LoginUser(refreshToken.UserId);

                await _tokenRepository.AddAsync(refreshToken);

                // Potvrzení celé transakce
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Vrácení všech změn v případě chyby
                await transaction.RollbackAsync();
                Log.Error(ex, "Error during RegisterLoginAsync");
                throw; // Propagace chyby dál
            }
        }

        public async Task NewRefreshToken(RefreshToken newRefreshToken, RefreshToken tokenInfo)
        {
            await using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                await _tokenRepository.TokenRevocation(tokenInfo.Id, newRefreshToken.CreatedByIp, newRefreshToken.Token);

                await _tokenRepository.AddAsync(newRefreshToken);

                // Potvrzení celé transakce
                await transaction.CommitAsync();
            }
            catch (Exception ex)
            {
                // Vrácení všech změn v případě chyby
                await transaction.RollbackAsync();
                Log.Error(ex, "Error during NewRefreshToken");
                throw; // Propagace chyby dál
            }

        }
        private Tokens GenerateTokens(User user)
        {
            return new Tokens
            {
                tokenJWT = _jwtUtils.GenerateJwtToken(user),
                refreshToken = _jwtUtils.GenerateRefreshToken()
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

    }
}
