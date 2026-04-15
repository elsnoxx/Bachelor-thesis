using Serilog;
using server.Models.DTO;
using server.Models.Games;
using server.Services.Utils;

namespace server.Services.GameServices
{
    public class EnergyBattleGameServices : BaseGameService<EnergyBattleGame>
    {
        private readonly Random _random = new();

        public EnergyBattleGameServices(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue)
            : base(scopeFactory, dbQueue) { }

        protected override EnergyBattleGame CreateGame(string roomId) => new() { RoomId = roomId };

        private double GenerateNewTarget() => _random.NextDouble() * 80.0 + 20.0;

        public override object? ProcessInput(string roomId, string playerId, double value)
        {
            var game = GetOrCreateGame(roomId);
            double toleranceRange = 10.0;

            SaveBioFeedback(playerId, roomId, value);

            // 1. Zajištění existence hráče
            if (!game.Players.ContainsKey(playerId))
            {
                game.Players.TryAdd(playerId, new EnergyBattleGame.EnergyBattlePlayer
                {
                    Email = playerId,
                    TargetBioValue = GenerateNewTarget(),
                    LastTargetChange = DateTime.UtcNow
                });
            }

            // 2. Start hry při dostatku hráčů
            if (game.StartTime == null && game.Players.Count >= 2)
            {
                game.StartTime = DateTime.UtcNow;
                foreach (var p in game.Players.Values) p.LastTargetChange = DateTime.UtcNow;
                NotifyRoomStatus(roomId, RoomStatus.Start);
            }

            if (!game.Players.TryGetValue(playerId, out var player)) return null;

            // 3. Logika hry během běhu
            if (game.StartTime != null && !game.IsFinished)
            {
                if ((DateTime.UtcNow - player.LastTargetChange).TotalSeconds >= 30)
                {
                    player.TargetBioValue = GenerateNewTarget();
                    player.LastTargetChange = DateTime.UtcNow;
                }

                double difference = Math.Abs(player.TargetBioValue - value);
                player.Energy = Math.Min(100, player.Energy +
                    (difference <= toleranceRange ? Math.Max(0.5, 5 - (difference / 2)) : 0.1));
            }

            // 4. Vyřešení zpráv konce hry (oprava S3358)
            string? endMessage = null;
            if (game.IsFinished)
            {
                endMessage = (game.WinnerEmail == playerId)
                    ? "Victory! You destroyed the opponent."
                    : "Defeat! Your base was destroyed.";
            }

            return new
            {
                roomId = game.RoomId,
                isStarted = game.StartTime != null,
                players = game.Players.Values.Select(p => new {
                    email = p.Email, energy = p.Energy, health = p.Health,
                    target = p.TargetBioValue,
                    targetMin = p.TargetBioValue - toleranceRange,
                    targetMax = p.TargetBioValue + toleranceRange,
                    isReady = p.IsReadyToFire,
                    nextChangeIn = game.StartTime != null
                        ? Math.Max(0, 30 - (int)(DateTime.UtcNow - p.LastTargetChange).TotalSeconds) : 30
                }).ToList(),
                isFinished = game.IsFinished, winner = game.WinnerEmail,
                endMessage = endMessage,
            };
        }

        public object? Fire(string roomId, string shooterEmail)
        {
            if (!_activeGames.TryGetValue(roomId, out var game) || game.IsFinished) return null;
            if (!game.Players.TryGetValue(shooterEmail, out var shooter)) return null;

            var opponent = game.Players.Values.FirstOrDefault(p => p.Email != shooterEmail);
            if (shooter.IsReadyToFire && opponent != null)
            {
                shooter.Energy = 0;
                shooter.TargetBioValue = GenerateNewTarget();
                opponent.Health -= 20;

                if (opponent.Health <= 0)
                {
                    opponent.Health = 0;
                    game.IsFinished = true;
                    game.WinnerEmail = shooterEmail;
                    SaveGameResult(game.RoomId, game.LeftPlayerId,       // <-- base
                        game.RightPlayerId, "energybattle", game.LeftPlayerId);
                }
            }

            return new { roomId = game.RoomId, isFinished = game.IsFinished, winner = game.WinnerEmail };
        }

        public Dictionary<string, double> GetScores() =>
            _activeGames.Values.SelectMany(g => g.Players)
                .ToDictionary(p => p.Key, p => p.Value.Energy);
    }
}