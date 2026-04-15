using server.Models.DTO;

namespace server.Services.DbServices.Interfaces
{
    /// <summary>
    /// Service for managing game lobby lifecycle, player matchmaking, and room status.
    /// Acts as a bridge between game room repositories and real-time hubs.
    /// </summary>
    public interface IGameRoomService
    {
        /// <summary>
        /// Retrieves a list of available rooms for a specific game type.
        /// </summary>
        Task<Result<IEnumerable<GameRoomDTO>>> GetGameRoomsListAsync(string gameType);

        /// <summary>
        /// Gets profiles of all users currently waiting or playing in a specific room.
        /// </summary>
        Task<Result<IEnumerable<UserDto>>> GetUsersInGameRoomAsync(Guid gameRoomId);

        /// <summary>
        /// Initializes a new game room with security settings (password) and capacity limits.
        /// </summary>
        Task<Result<bool>> CreateGameRoomAsync(GameRoomCreationDto gameRoomDTO);

        /// <summary>
        /// Validates entry requirements (password, capacity) and connects a user to a room.
        /// </summary>
        Task<Result<bool>> JoinGameRoomAsync(Guid gameRoomId, JoinRoomRequest request);

        /// <summary>
        /// Disconnects a user from a room and cleans up their session.
        /// </summary>
        Task<Result<bool>> LeaveGameRoomAsync(Guid gameRoomId, LeaveRoomRequest request);

        /// <summary>
        /// Changes room status to 'Finished', stopping data collection.
        /// </summary>
        Task FinishGameRoomAsync(Guid gameRoomId);

        /// <summary>
        /// Changes room status to 'InProgress', starting the game logic.
        /// </summary>
        Task StartGameRoomAsync(Guid gameRoomId);
    }
}