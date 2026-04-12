using Serilog;
using server.Models.DTO;
using server.Services.Utils;
using System.Collections.Concurrent;

namespace server.Services.GameServices
{
    public abstract class BaseGameService<TGame> : IGameService
        where TGame : class, new()
    {
        protected readonly ConcurrentDictionary<string, TGame> _activeGames = new();
        protected readonly DbWriteQueue _dbQueue;
        protected readonly IServiceScopeFactory _scopeFactory;

        protected BaseGameService(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue)
        {
            _scopeFactory = scopeFactory;
            _dbQueue = dbQueue;
        }

        // --- Shared: get or create a game instance ---
        protected TGame GetOrCreateGame(string roomId)
        {
            return _activeGames.GetOrAdd(roomId, CreateGame);
        }

        // Override this in services that need custom initialization (e.g. DB lookup)
        protected virtual TGame CreateGame(string roomId) => new();

        // --- Shared: remove a finished game from memory ---
        protected bool RemoveGame(string roomId)
        {
            bool removed = _activeGames.TryRemove(roomId, out _);
            if (removed) Log.Information("[ROOM REMOVED] Room: {RoomId}", roomId);
            return removed;
        }

        // --- Shared: save a single bio-feedback sample ---
        protected void SaveBioFeedback(string email, string roomId, double value)
        {
            if (Guid.TryParse(roomId, out Guid roomGuid))
                _dbQueue.QueueBioFeedbackAsync(new BioFeedbackMessage(email, roomGuid, (float)value));
        }

        // --- Shared: notify DB about room status change (Start / Finish) ---
        protected void NotifyRoomStatus(string roomId, RoomStatus status)
        {
            if (Guid.TryParse(roomId, out Guid roomGuid))
                _dbQueue.QueueRoomStatus(new RoomStatusMessage(roomGuid, status));
        }

        // --- Shared: queue final game result for DB writer ---
        protected void SaveGameResult(string roomId, string? player1, string? player2, string gameType, string? winnerPlayerEmail)
        {
            Log.Information("[GAME END] Queueing stats for room {RoomId}", roomId);
            _dbQueue.QueueGameResultAsync(new GameResultContext(roomId, player1, player2, gameType, winnerPlayerEmail));
        }

        public abstract object? ProcessInput(string roomId, string playerId, double value);
    }
}
