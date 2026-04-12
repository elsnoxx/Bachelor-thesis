using Serilog;
using server.Models.DTO;
using server.Models.Games;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;
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
            // 1. Získání nebo vytvoření hry (Lazy inicializace z DB)
            var game = _activeGames.GetOrAdd(roomId, id => {
                var newGame = new BalloonControlGame { RoomId = id };

                using (var scope = _scopeFactory.CreateScope())
                {
                    var repo = scope.ServiceProvider.GetRequiredService<IGameRoomRepository>();
                    if (Guid.TryParse(id, out Guid roomGuid))
                    {
                        // .GetAwaiter().GetResult() je v GetOrAdd bezpečnější než jen .Result
                        var room = repo.GetByIdAsync(roomGuid).GetAwaiter().GetResult();
                        if (room != null)
                        {
                            newGame.MaxPlayers = room.MaxPlayers;
                        }
                    }
                }
                return newGame;
            });

            // 2. Registrace hráče (pokud ještě není v seznamu)
            if (!game.Players.ContainsKey(playerEmail))
            {
                // Kontrola, aby se nepřipojilo víc lidí, než je MaxPlayers z DB
                if (game.Players.Count < game.MaxPlayers)
                {
                    game.Players.TryAdd(playerEmail, new BalloonControlGame.BalloonPlayer { Email = playerEmail });
                }
            }

            // 3. Start hry - nastane POUZE, když počet připojených odpovídá MaxPlayers
            if (game.StartTime == null && game.Players.Count >= game.MaxPlayers)
            {
                game.StartTime = DateTime.UtcNow;
                // Spustíme asynchronní update v DB (nečekáme na něj, aby hra běžela hned)
                _ = UpdateRoomStatusToInProgress(roomId);
            }

            // Pokud hra ještě nezačala, vracíme stav "Waiting", ale nepohneme se
            if (game.StartTime == null)
            {
                return GetWaitingState(game);
            }

            if (!game.Players.TryGetValue(playerEmail, out var player) || game.IsFinished)
                return GetState(game);

            // 4. Logika pohybu (běží jen po StartTime)
            player.Altitude = value;
            player.LastValue = value;
            player.DistanceTraveled += 2;

            if (player.DistanceTraveled >= game.FinishLineDistance)
            {
                game.IsFinished = true;
                game.WinnerEmail = player.Email;
                game.EndReason = $"Hráč {player.Email.Split('@')[0]} doletěl do cíle jako první!";
                SaveFinalStats(game);
            }

            SaveBioFeedback(playerEmail, roomId, value);
            return GetState(game);
        }

        // Pomocná metoda pro stav před startem
        private object GetWaitingState(BalloonControlGame game) => new
        {
            roomId = game.RoomId,
            isStarted = false,
            isFinished = false,
            connectedPlayers = game.Players.Count,
            requiredPlayers = game.MaxPlayers,
            message = $"Čekání na hráče ({game.Players.Count}/{game.MaxPlayers})",
            players = game.Players.Values.Select(p => new { email = p.Email }).ToList()
        };

        private async Task UpdateRoomStatusToInProgress(string roomId)
        {
            try
            {
                if (Guid.TryParse(roomId, out Guid roomGuid))
                {
                    _dbQueue.QueueRoomStatus(new RoomStatusMessage(roomGuid, RoomStatus.Start));
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[DB UPDATE ERROR] Failed to set InProgress for room {RoomId}", roomId);
            }
        }

        private object GetState(BalloonControlGame game) => new
        {
            roomId = game.RoomId,
            isStarted = game.StartTime != null,
            isFinished = game.IsFinished,
            winner = game.WinnerEmail,
            endReason = game.EndReason,
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
