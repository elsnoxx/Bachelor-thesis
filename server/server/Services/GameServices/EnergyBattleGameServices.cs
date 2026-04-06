using Serilog;
using server.Models.DTO;
using server.Models.Games;
using server.Repositories.Interfaces;
using server.Services.Utils;
using System.Collections.Concurrent;

namespace server.Services.GameServices
{
    public class EnergyBattleGameServices : IGameService
    {
        private readonly ConcurrentDictionary<string, EnergyBattleGame> _activeGames = new();
        private readonly Random _random = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DbWriteQueue _dbQueue;

        public EnergyBattleGameServices(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue)
        {
            _scopeFactory = scopeFactory;
            _dbQueue = dbQueue;
        }

        private EnergyBattleGame GetOrCreateGame(string roomId)
        {
            return _activeGames.GetOrAdd(roomId, id => new EnergyBattleGame { RoomId = id });
        }

        // Metoda zde byla 2x, jednu jsem smazal
        private double GenerateNewTarget() => _random.NextDouble() * (100.0 - 20.0) + 20.0;

        public object? ProcessInput(string roomId, string playerEmail, double value)
        {
            var game = GetOrCreateGame(roomId);
            double toleranceRange = 10.0;

            SaveBioFeedbackAsync(playerEmail, roomId, value);

            // 1. Registrace hráče (zůstává stejná)
            if (!game.Players.ContainsKey(playerEmail))
            {
                game.Players.TryAdd(playerEmail, new EnergyBattleGame.EnergyBattlePlayer
                {
                    Email = playerEmail,
                    TargetBioValue = GenerateNewTarget(),
                    LastTargetChange = DateTime.UtcNow
                });
                Log.Information("[ENERGY BATTLE] Player {Email} joined room {RoomId}", playerEmail, roomId);
            }

            // 2. Kontrola spuštění hry (pokud se připojí druhý, nastavíme StartTime)
            if (game.StartTime == null && game.Players.Count >= 2)
            {
                game.StartTime = DateTime.UtcNow;
                // Resetujeme časy změn cíle pro oba hráče na moment startu, aby měli čistých 30s
                foreach (var p in game.Players.Values) p.LastTargetChange = DateTime.UtcNow;

                _ = UpdateRoomStatusToInProgress(roomId);
                Log.Information("[ENERGY BATTLE] Game started in room {RoomId}", roomId);
            }

            if (!game.Players.TryGetValue(playerEmail, out var player)) return null;

            // --- KLÍČOVÁ ZMĚNA: Pokud hra ještě nezačala, nepočítej energii ani cíle ---
            if (game.StartTime != null && !game.IsFinished)
            {
                // 3. Logika změny cíle (jen když se hraje)
                if ((DateTime.UtcNow - player.LastTargetChange).TotalSeconds >= 30)
                {
                    player.TargetBioValue = GenerateNewTarget();
                    player.LastTargetChange = DateTime.UtcNow;
                }

                // 4. Logika nabíjení energie
                double difference = Math.Abs(player.TargetBioValue - value);
                double chargeGain = 0;

                if (difference <= toleranceRange)
                {
                    chargeGain = Math.Max(0.5, 5 - (difference / 2));
                }
                else
                {
                    chargeGain = 0.1;
                }

                player.Energy = Math.Min(100, player.Energy + chargeGain);
            }

            // 5. Vrácení stavu (posíláme data i když se ještě "neakumuluje" energie, aby UI vidělo čekání)
            return new
            {
                roomId = game.RoomId,
                // Přidáme informaci, zda hra běží, pro UI
                isStarted = game.StartTime != null,
                players = game.Players.Values.Select(p => new {
                    email = p.Email,
                    energy = p.Energy,
                    health = p.Health,
                    target = p.TargetBioValue,
                    targetMin = p.TargetBioValue - toleranceRange,
                    targetMax = p.TargetBioValue + toleranceRange,
                    isReady = p.IsReadyToFire,
                    // Pokud hra neběží, vracíme 30 (nebo 0)
                    nextChangeIn = game.StartTime != null
                        ? Math.Max(0, 30 - (int)(DateTime.UtcNow - p.LastTargetChange).TotalSeconds)
                        : 30
                }).ToList(),
                isFinished = game.IsFinished,
                winner = game.WinnerEmail,
                endMessage = game.IsFinished
                ? (game.WinnerEmail == playerEmail ? "Vítězství! Zničil jsi soupeře." : "Porážka! Tvoje základna byla zničena.")
                : null
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

                    // --- NOVÉ: Logika ukončení ---
                    SaveFinalGameStats(game); // Uloží výsledek zápasu do fronty
                    Log.Information("[ENERGY BATTLE] Game Finished in room {RoomId}. Winner: {Winner}", roomId, shooterEmail);
                }
            }

            return new
            {
                roomId = game.RoomId,
                isFinished = game.IsFinished,
                winner = game.WinnerEmail
            };
        }

        private async Task UpdateRoomStatusToInProgress(string roomId)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var roomRepo = scope.ServiceProvider.GetRequiredService<IGameRoomRepository>();

                if (Guid.TryParse(roomId, out Guid roomGuid))
                {
                    var gameRoom = await roomRepo.GameRoomById(roomGuid);
                    if (gameRoom != null && gameRoom.Status != "InProgress")
                    {
                        gameRoom.Status = "InProgress";
                        await roomRepo.UpdateGameRoomAsync(gameRoom);
                        Log.Information("[DB UPDATE] Room {RoomId} switched to InProgress", roomId);
                    }
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[DB UPDATE ERROR] Failed to set InProgress for room {RoomId}", roomId);
            }
        }

        public Dictionary<string, double> GetScores()
        {
            return _activeGames.Values
                .SelectMany(g => g.Players)
                .ToDictionary(p => p.Key, p => p.Value.Energy);
        }

        private void SaveBioFeedbackAsync(string email, string roomId, double value)
        {
            // Používáme TryParse, aby aplikace nespadla, pokud roomId není validní GUID
            if (Guid.TryParse(roomId, out Guid roomGuid))
            {
                // Jen hodíme do fronty - o zbytek se stará BackgroundService (Worker)
                _dbQueue.QueueBioFeedbackAsync(new BioFeedbackMessage(email, roomGuid, (float)value));
            }
        }

        private void SaveFinalGameStats(EnergyBattleGame game)
        {
            Log.Information("[GAME END] Queueing stats for room {RoomId}", game.RoomId);

            var context = new GameResultContext(
                game.RoomId,
                game.LeftPlayerId,
                game.RightPlayerId,
                "energybattle"
            );

            _dbQueue.QueueGameResultAsync(context);
        }
    }
}