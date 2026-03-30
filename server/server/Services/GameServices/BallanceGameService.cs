using Serilog; // Ujisti se, že máš tento using
using server.Models.DB;
using server.Models.Games;
using server.Services.DbServices.Interfaces;
using System.Collections.Concurrent;
using Microsoft.Extensions.DependencyInjection;

namespace server.Services.GameServices
{
    public class BallanceGameService : IGameService
    {
        private readonly ConcurrentDictionary<string, BalanceGame> _activeGames = new();
        private readonly IServiceScopeFactory _scopeFactory;

        public BallanceGameService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        private BalanceGame GetOrCreateGame(string roomId)
        {
            return _activeGames.GetOrAdd(roomId, id => new BalanceGame { RoomId = id });
        }

        public object ProcessInput(string roomId, string playerId, double value)
        {
            var game = GetOrCreateGame(roomId);

            // Dynamické přiřazení stran
            if (string.IsNullOrEmpty(game.LeftPlayerId))
                game.LeftPlayerId = playerId;
            else if (string.IsNullOrEmpty(game.RightPlayerId) && game.LeftPlayerId != playerId)
                game.RightPlayerId = playerId;

            // Logování STARTU hry
            if (game.StartTime == null && !string.IsNullOrEmpty(game.LeftPlayerId) && !string.IsNullOrEmpty(game.RightPlayerId))
            {
                game.StartTime = DateTime.UtcNow;
                Log.Information("[GAME START] Room: {RoomId}, Players: {P1} vs {P2}, Time: {Time}",
                    roomId, game.LeftPlayerId, game.RightPlayerId, game.StartTime);
            }

            if (game.StartTime != null)
            {
                game.AddValue(playerId, value);
                _ = SaveBioFeedbackAsync(playerId, roomId, value);
            }

            int remaining = game.GetRemainingTime();

            // Logování blížícího se KONCE (volitelné - např. každých 30s)
            if (remaining > 0 && remaining % 30 == 0 && value % 10 < 1) // omezíme četnost logu
            {

                Log.Debug("[GAME TICK] Room: {RoomId}, Remaining: {Time}s", roomId, remaining);
            }

            // Kontrola KONCE hry
            if (game.IsGameOver)
            {
                if (!game.WasSaved)
                {
                    game.WasSaved = true; // Okamžitě zamkneme, aby další vlákno neukládalo

                    // Spustíme uložení na pozadí (nechceme blokovat websocket)
                    _ = SaveFinalGameStats(game);
                }

                return new
                {
                    roomId = game.RoomId,
                    ballPosition = game.GetBallPosition(),
                    leftValue = game.LeftValue,
                    rightValue = game.RightValue,
                    leftPlayerId = game.LeftPlayerId,
                    rightPlayerId = game.RightPlayerId,
                    isGameOver = true,
                    remainingTime = 0
                };
            }

            return new
            {
                roomId = game.RoomId,
                ballPosition = game.GetBallPosition(),
                leftValue = game.LeftValue,
                rightValue = game.RightValue,
                leftPlayerId = game.LeftPlayerId,
                rightPlayerId = game.RightPlayerId,
                isGameOver = game.IsGameOver,
                remainingTime = remaining,
            };
        }

        private async Task SaveFinalGameStats(BalanceGame game)
        {
            try
            {
                Log.Information("[DB SAVE] Saving stats for room {RoomId}", game.RoomId);

                var playersToSave = new[] {
                new { Email = game.LeftPlayerId, Side = "Left" },
                new { Email = game.RightPlayerId, Side = "Right" }
            };

                using var scope = _scopeFactory.CreateScope();
                var statisticService = scope.ServiceProvider.GetRequiredService<IStatisticServices>();

                foreach (var p in playersToSave)
                {
                    if (string.IsNullOrEmpty(p.Email)) continue;

                    var stats = new Statistic
                    {
                        GameType = "ballance",
                        LastPlayed = DateTime.UtcNow,
                        TotalSessions = 1
                    };

                    await statisticService.AddStatisticByEmailAsync(p.Email, stats);
                    Log.Debug("[DB SAVE] Stats saved for {Email}", p.Email);
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[DB SAVE ERROR] Failed to save game stats for room {RoomId}", game.RoomId);
            }
        }

        public void RemoveRoom(string roomId)
        {
            if (_activeGames.TryRemove(roomId, out var game))
            {
                Log.Information("[ROOM REMOVED] Room: {RoomId}", roomId);
            }
        }

        private async Task SaveBioFeedbackAsync(string email, string roomId, double value)
        {
            try
            {
                // Musíme vytvořit scope, protože běžíme v singletonu na pozadí
                using var scope = _scopeFactory.CreateScope();
                var statisticService = scope.ServiceProvider.GetRequiredService<IStatisticServices>();

                if (Guid.TryParse(roomId, out Guid roomGuid))
                {
                    // Voláme tvou metodu v servise, která si dohledá Guid uživatele podle emailu
                    await statisticService.AddBioFeedbackByEmailAsync(email, roomGuid, (float)value);
                }
            }
            catch (Exception ex)
            {
                // Logujeme jen občas nebo vůbec, aby se nezahltil log při chybě v každém ticku
                Log.Error(ex, "[GSR SAVE ERROR] Failed for user {Email}", email);
            }
        }

        public Dictionary<string, double> GetScores() => new Dictionary<string, double>();
    }
}