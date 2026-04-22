using Serilog;
using server.Models.DTO;
using server.Models.Games;
using server.Models.Games.BalloonGame;
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
                game.Players.TryAdd(playerId, new BalloonPlayer { Email = playerId });

            if (game.StartTime == null && game.Players.Count >= game.MaxPlayers)
            {
                game.StartTime = DateTime.UtcNow;
                NotifyRoomStatus(roomId, RoomStatus.Start);
            }

            if (game.StartTime == null) return GetWaitingState(game);
            if (!game.Players.TryGetValue(playerId, out var player) || game.IsFinished) return GetState(game);

            var elapsed = (DateTime.UtcNow - game.StartTime.Value).TotalSeconds;

            // 1. Fáze kalibrace (10 s) dle textu
            if (elapsed <= 10)
            {
                player.CalibrationData.Add(value);
                if (elapsed >= 9.5 && !player.IsCalibrated)
                {
                    player.Baseline = player.CalibrationData.Any() ? player.CalibrationData.Average() : value;
                    player.IsCalibrated = true;
                }
            }
            // 2. Závodní fáze
            else
            {
                // Pohon klidem (Tonická složka): +5 jen pokud value <= baseline
                if (value <= player.Baseline)
                {
                    player.DistanceTraveled += 5;
                }

                // Akcelerační bonus (SCR): +20 při skoku
                // Upravte práh (750 v textu může být moc, pokud normalizujete 0-1000)
                if (player.LastValue > 0 && (value - player.LastValue) > 150)
                {
                    player.DistanceTraveled += 20;
                    Log.Information("[SCR BONUS] Player {Player} jumped forward!", playerId);
                }
            }

            player.Altitude = value; // y-osa odpovídá v_norm
            player.LastValue = value;

            // Kontrola cíle (1000 jednotek)
            if (player.DistanceTraveled >= game.FinishLineDistance)
            {
                game.IsFinished = true;
                game.WinnerEmail = player.Email;

                var ids = game.Players.Keys.ToList();
                // Uložení: pro multiplayer (2-4) posíláme vítěze a první dva v seznamu
                SaveGameResult(game.RoomId, ids[0], ids.Count > 1 ? ids[1] : null, "balloon", playerId);
                NotifyRoomStatus(roomId, RoomStatus.Finish);
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
