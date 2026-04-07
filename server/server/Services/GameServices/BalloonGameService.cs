using server.Models.DTO;
using server.Models.Games;
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
            var game = _activeGames.GetOrAdd(roomId, id => new BalloonControlGame { RoomId = id });

            // 1. Registrace hráče
            if (!game.Players.ContainsKey(playerEmail))
            {
                game.Players.TryAdd(playerEmail, new BalloonControlGame.BalloonPlayer { Email = playerEmail });
            }

            // 2. Start hry při 2 hráčích
            if (game.StartTime == null && game.Players.Count >= 2)
            {
                game.StartTime = DateTime.UtcNow;
            }

            if (!game.Players.TryGetValue(playerEmail, out var player) || game.IsFinished)
                return GetState(game);

            // 3. Logika pohybu
            // GSR hodnota přímo určuje výšku (Altitude)
            player.Altitude = value;
            player.LastValue = value;

            // Simulace dopředného pohybu (v reálné hře by se volalo v loopu, 
            // zde posouváme s každým tickem dat od senzoru)
            if (game.StartTime != null)
            {
                player.DistanceTraveled += 2; // Konstantní rychlost vpřed

                // Kontrola vítězství
                if (player.DistanceTraveled >= game.FinishLineDistance)
                {
                    game.IsFinished = true;
                    game.WinnerEmail = player.Email;
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
            players = game.Players.Values.Select(p => new
            {
                email = p.Email,
                altitude = p.Altitude, // Frontend vykreslí Y osu
                distance = p.DistanceTraveled, // Frontend vykreslí X osu
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
