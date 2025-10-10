using server.Models;
using server.Models.DB;
using server.Models.DTO;

namespace server.Services.DbServices.Interfaces
{
    public interface IGameRoomService
    {
        Task<Result<IEnumerable<GameRoomDTO>>> GetGameRoomsListAsync(string gameType);
        Task<Result<IEnumerable<UserDTO>>> GetUsersGameRoomAsync(Guid gameRoomId);
        Task<Result<bool>> CreateGameRoomAsync(GameRoomCreationDTO gameRoomDTO);

        Task<Result<bool>> JoinGameRoomAsync(Guid gameRoomId, JoinRoomRequest request);
        Task<Result<bool>> LeaveGameRoomAsync(Guid gameRoomId, LeaveRoomRequest request);
    }
}

