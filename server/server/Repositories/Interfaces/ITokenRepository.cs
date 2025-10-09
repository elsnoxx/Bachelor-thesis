using server.Models.DB;

namespace server.Repositories.Interfaces
{
    public interface ITokenRepository
    {
        Task AddAsync(RefreshToken refreshToken);
        Task<RefreshToken?> GetByRefreshTokenAsync(string refreshToken);
        Task TokenRevocation(Guid id, string ip, string token);
    }
}
