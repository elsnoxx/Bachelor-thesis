using server.Models.DB;

namespace server.Services.Utils.Interfaces
{
    public interface IJwtUtils
    {
        string GenerateJwtToken(User user);
        string GenerateRefreshToken();
    }
}
