using server.Models.DTO;
using server.Models.Games;
using server.Repositories.Interfaces;
using server.Services.Utils;
using System.Collections.Concurrent;

namespace server.Services.GameServices
{
    public class BalloonGameService : IGameService
    {
        private readonly ConcurrentDictionary<string, BalloonControlGame> _activeGames = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DbWriteQueue _dbQueue;

        public BalloonGameService(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue)
        {
            _scopeFactory = scopeFactory;
            _dbQueue = dbQueue;
        }

        public object? ProcessInput(string roomId, string playerEmail, double value)
        {
            // 1. Získání nebo vytvoření hry se synchronním zámkem pro inicializaci z DB
            var game = _activeGames.GetOrAdd(roomId, id => {
                var newGame = new BalloonControlGame { RoomId = id };

                // Načtení MaxPlayers z DB
                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IGameRoomRepository>();
                    if (Guid.TryParse(id, out Guid roomGuid))
                    {
                        var room = repo.GetByIdAsync(roomGuid).Result; // .Result v tomto kontextu
                        if (room != null) newGame.MaxPlayers = room.MaxPlayers;
                    }
                }
                return newGame;
            });

            // 2. Registrace hráče
            if (!game.Players.ContainsKey(playerEmail))
            {
                game.Players.TryAdd(playerEmail, new BalloonControlGame.BalloonPlayer { Email = playerEmail });
            }

            // 3. Start hry při dosažení počtu hráčů z DB
            if (game.StartTime == null && game.Players.Count >= game.MaxPlayers)
            {
                game.StartTime = DateTime.UtcNow;
            }

            if (!game.Players.TryGetValue(playerEmail, out var player) || game.IsFinished)
                return GetState(game);

            // 4. Logika pohybu
            player.Altitude = value;
            player.LastValue = value;

            if (game.StartTime != null)
            {
                player.DistanceTraveled += 2;

                if (player.DistanceTraveled >= game.FinishLineDistance)
                {
                    game.IsFinished = true;
                    game.WinnerEmail = player.Email;
                    // Tady nastavíme ty hlášky
                    game.EndReason = $"Hráč {player.Email.Split('@')[0]} doletěl do cíle jako první!";
                    SaveFinalStats(game);
                }
            }

            SaveBioFeedback(playerEmail, roomId, value);
            return GetState(game);
        }

        private object GetState(BalloonControlGame game) => new
        {
            roomId = game.RoomId,
            isStarted = game.StartTime != null,
            isFinished = game.IsFinished,
            winner = game.WinnerEmail,
            endReason = game.EndReason, // Nové pole pro UI
            players = game.Players.Values.Select(p => new
            {
                email = p.Email,
                altitude = p.Altitude,
                distance = p.DistanceTraveled,
                progress = (p.DistanceTraveled / game.FinishLineDistance) * 100
            }).ToList()
        };

        private void SaveBioFeedback(string email, string roomId, double value)
        {
            if (Guid.TryParse(roomId, out Guid roomGuid))
                _dbQueue.QueueBioFeedbackAsync(new BioFeedbackMessage(email, roomGuid, (float)value));
        }

        private void SaveFinalStats(BalloonControlGame game)
        {
            var pIds = game.Players.Keys.ToList();
            var context = new GameResultContext(game.RoomId, pIds.FirstOrDefault(), pIds.Skip(1).FirstOrDefault(), "balloon");
            _dbQueue.QueueGameResultAsync(context);
        }
    }
}
