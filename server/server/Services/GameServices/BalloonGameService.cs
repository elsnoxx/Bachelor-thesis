using Serilog;
using server.Models.DTO;
using server.Models.Games;
using server.Repositories.Interfaces;
using server.Services.Utils;

namespace server.Services.GameServices
{
    public class BalloonGameService : BaseGameService<BalloonControlGame>
    {
        public BalloonGameService(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue)
            : base(scopeFactory, dbQueue) { }

        // Custom factory: read MaxPlayers from DB on first access
        protected override BalloonControlGame CreateGame(string roomId)
        {
            var game = new BalloonControlGame { RoomId = roomId };
            using var scope = _scopeFactory.CreateScope();
            var repo = scope.ServiceProvider.GetRequiredService<IGameRoomRepository>();
            if (Guid.TryParse(roomId, out Guid guid))
            {
                var room = repo.GetByIdAsync(guid).GetAwaiter().GetResult();
                if (room != null) game.MaxPlayers = room.MaxPlayers;
            }
            return game;
        }

        public override object? ProcessInput(string roomId, string playerId, double value)
        {
            var game = GetOrCreateGame(roomId);

            if (!game.Players.ContainsKey(playerId) && game.Players.Count < game.MaxPlayers)
                game.Players.TryAdd(playerId, new BalloonControlGame.BalloonPlayer { Email = playerId });

            if (game.StartTime == null && game.Players.Count >= game.MaxPlayers)
            {
                game.StartTime = DateTime.UtcNow;
                NotifyRoomStatus(roomId, RoomStatus.Start);
            }

            if (game.StartTime == null)
                return GetWaitingState(game);

            if (!game.Players.TryGetValue(playerId, out var player) || game.IsFinished)
                return GetState(game);

            player.Altitude = value;
            player.LastValue = value;
            player.DistanceTraveled += 2;

            if (player.DistanceTraveled >= game.FinishLineDistance)
            {
                game.IsFinished = true;
                game.WinnerEmail = player.Email;
                game.EndReason = $"Player {player.Email.Split('@')[0]} reached the finish line first!";

                var ids = game.Players.Keys.ToList();
                SaveGameResult(game.RoomId, ids.FirstOrDefault(), ids.Skip(1).FirstOrDefault(), "balloon", playerId);
            }

            SaveBioFeedback(playerId, roomId, value);
            return GetState(game);
        }

        private object GetWaitingState(BalloonControlGame game) => new
        {
            roomId = game.RoomId, isStarted = false, isFinished = false,
            connectedPlayers = game.Players.Count, requiredPlayers = game.MaxPlayers,
            message = $"Waiting for players ({game.Players.Count}/{game.MaxPlayers})",
            players = game.Players.Values.Select(p => new { email = p.Email }).ToList()
        };

        private object GetState(BalloonControlGame game) => new
        {
            roomId = game.RoomId, isStarted = game.StartTime != null,
            isFinished = game.IsFinished, winner = game.WinnerEmail, endReason = game.EndReason,
            players = game.Players.Values.Select(p => new
            {
                email = p.Email, altitude = p.Altitude, distance = p.DistanceTraveled,
                progress = (p.DistanceTraveled / game.FinishLineDistance) * 100
            }).ToList()
        };
    }
}
