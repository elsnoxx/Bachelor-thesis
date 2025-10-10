using server.Models.DB;

namespace server.Repositories.Interfaces
{
    public interface IGameRoomRepository
    {
        Task<IEnumerable<GameRoom>> AllGameRoomsAsync(string gameType);
        Task CreateGameRoomAsync(GameRoom gameRoom);
        Task<GameRoom> GameRoomById(Guid roomId);
        Task<bool> GameRoomExistsAsync(Guid roomId);
        Task DeleteGameRoomAsync(GameRoom gameRoom);
    }
}
