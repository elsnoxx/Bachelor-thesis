using Serilog;
using server.Models.DTO;
using server.Models.Games;
using server.Models.Games.EnergyBattle;
using server.Services.Utils;


namespace server.Services.GameServices
{
    public class EnergyBattleGameServices : BaseGameService<EnergyBattleGame>
    {
        private readonly Random _random = new();

        public EnergyBattleGameServices(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue)
            : base(scopeFactory, dbQueue) { }

        protected override EnergyBattleGame CreateGame(string roomId) => new() { RoomId = roomId };

        public override object? ProcessInput(string roomId, string playerId, double value)
        {
            var game = GetOrCreateGame(roomId);

            // Oprava inicializace hráče (ve vašem kódu byl drobný bug s TryAdd)
            if (!game.Players.TryGetValue(playerId, out var player))
            {
                player = new EnergyBattlePlayer
                {
                    Email = playerId,
                    CalibrationData = new List<double>()
                };
                game.Players.TryAdd(playerId, player);
            }

            double calibrationRemaining = 0;
            bool isCalibrating = false;

            if (game.StartTime == null && game.Players.Count >= 2)
            {
                game.StartTime = DateTime.UtcNow;
                NotifyRoomStatus(roomId, RoomStatus.Start);
            }

            if (game.StartTime != null && !game.IsFinished)
            {
                var elapsed = (DateTime.UtcNow - game.StartTime.Value).TotalSeconds;

                if (elapsed <= 10)
                {
                    isCalibrating = true;
                    calibrationRemaining = Math.Max(0, 10 - elapsed); // Výpočet zbývajících sekund

                    player.CalibrationData.Add(value);
                    if (elapsed >= 9.5 && !player.IsCalibrated)
                    {
                        player.Baseline = player.CalibrationData.Any() ? player.CalibrationData.Average() : value;
                        player.IsCalibrated = true;
                    }
                }
                else
                {
                    // Bojová fáze (vaše stávající logika)
                    if (value <= player.Baseline)
                    {
                        player.Energy = Math.Min(100, player.Energy + 1.5);
                    }

                    if (player.LastValue > 0 && (value - player.LastValue) > 100)
                    {
                        player.Energy = Math.Min(100, player.Energy + 15);
                        Log.Information("[BONUS] Player {Player} adrenaline rush!", playerId);
                    }
                }
            }

            player.LastValue = value;
            SaveBioFeedback(playerId, roomId, value);

            return new
            {
                roomId = game.RoomId,
                isStarted = game.StartTime != null,
                isCalibrating = isCalibrating,
                players = game.Players.Values.Select(p => new {
                    email = p.Email,
                    energy = p.Energy,
                    health = p.Health,
                    isReady = p.IsReadyToFire,
                    target = p.Baseline,
                    targetMin = 0,
                    targetMax = p.Baseline,
                    // Pokud probíhá kalibrace, pošleme zbývající čas do UI
                    nextChangeIn = isCalibrating ? (int)Math.Ceiling(calibrationRemaining) : 0
                }).ToList(),
                isFinished = game.IsFinished,
                winner = game.WinnerEmail
            };
        }

        public object? Fire(string roomId, string shooterEmail)
        {
            if (!_activeGames.TryGetValue(roomId, out var game) || game.IsFinished) return null;
            if (!game.Players.TryGetValue(shooterEmail, out var shooter)) return null;

            var opponent = game.Players.Values.FirstOrDefault(p => p.Email != shooterEmail);

            // Validace výstřelu na straně serveru: energie musí být >= 100
            if (shooter.IsReadyToFire && opponent != null)
            {
                shooter.Energy = 0; // Po úspěšném výstřelu se energie vynuluje

                opponent.Health -= 20; // Snížení HP soupeře

                if (opponent.Health <= 0)
                {
                    opponent.Health = 0;
                    game.IsFinished = true;
                    game.WinnerEmail = shooterEmail;

                    // Uložení výsledku do DB
                    SaveGameResult(
                        game.RoomId,
                        game.LeftPlayerId,
                        game.RightPlayerId,
                        "energybattle",
                        shooterEmail
                    );
                }
            }

            return new
            {
                roomId = game.RoomId,
                isFinished = game.IsFinished,
                winner = game.WinnerEmail
            };
        }

        public Dictionary<string, double> GetScores() =>
            _activeGames.Values.SelectMany(g => g.Players)
                .ToDictionary(p => p.Key, p => p.Value.Energy);
    }
}