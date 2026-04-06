using server.Models;
using server.Models.Auth;

namespace server.Services.DbServices.Interfaces
{
    public interface IAuthDbService
    {
        Task<Result<Tokens>> LoginAsync(UserLogin userLogin, HttpContext httpContext);
        Task<Result<bool>> RegisterAsync(UserRegister userRegister);
        Task<Result<Tokens>> RefreshTokenAsync(string refreshToken, HttpContext httpContext);
        Task<bool> ResetPasswordAsync(UserResetPassword model);
        Task<Result<bool>> ConfirmEmailAsync(string token);
    }
}
