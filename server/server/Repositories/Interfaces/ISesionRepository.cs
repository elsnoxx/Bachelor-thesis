using server.Models.DB;

namespace server.Repositories.Interfaces
{
    public interface ISesionRepository
    {
        Task<bool> AddUserToSesion(Session session);
        Task<Session?> GetSesionByIdAsync(Guid sessionId);
        Task DeleteSesionAsync(Session session);
        Task<bool> SesionExistsAsync(Guid sessionId);
        Task<IEnumerable<Guid>> GetUsersInGameRoomAsync(Guid gameRoomId);
        Task<bool> RemoveUserFromSesion(Guid userId, Guid gameRoomId);
    }
}
