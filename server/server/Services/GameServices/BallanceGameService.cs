using Microsoft.Extensions.DependencyInjection;
using Serilog; // Ujisti se, že máš tento using
using server.Models.DB;
using server.Models.DTO;
using server.Models.Games;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;
using server.Services.Utils;
using System.Collections.Concurrent;

namespace server.Services.GameServices
{
    public class BallanceGameService : IGameService
    {
        private readonly ConcurrentDictionary<string, BalanceGame> _activeGames = new();
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly DbWriteQueue _dbQueue;

        public BallanceGameService(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue)
        {
            _scopeFactory = scopeFactory;
            _dbQueue = dbQueue;
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
                _ = UpdateRoomStatusToInProgress(roomId);
            }

            if (game.StartTime != null)
            {
                game.AddValue(playerId, value);
                SaveBioFeedbackAsync(playerId, roomId, value);
            }

            int remaining = game.GetRemainingTime();

            // Logování blížícího se KONCE (volitelné - např. každých 30s)
            if (remaining > 0 && remaining % 30 == 0 && value % 10 < 1) // omezíme četnost logu
            {

                Log.Debug("[GAME TICK] Room: {RoomId}, Remaining: {Time}s", roomId, remaining);
            }

            // Kontrola KONCE hry
            // Kontrola KONCE hry v rámci metody ProcessInput
            if (game.IsGameOver)
            {
                if (!game.WasSaved)
                {
                    game.WasSaved = true;
                    SaveFinalGameStats(game);
                }

                bool isWin = false;
                string reason = "";
                double ballPos = game.GetBallPosition();

                if (ballPos <= 0)
                {
                    isWin = false;
                    reason = "Prohra: Kulička vypadla na levé straně.";
                }
                else if (ballPos >= 100)
                {
                    isWin = false;
                    reason = "Prohra: Kulička vypadla na pravé straně.";
                }
                else if (remaining <= 0)
                {
                    isWin = true;
                    reason = "Vítězství! Dokázali jste spolupracovat a udržet balanc až do konce.";
                }
                else
                {
                    isWin = false;
                    reason = "Hra byla ukončena.";
                }

                return new
                {
                    roomId = game.RoomId,
                    ballPosition = ballPos,
                    leftValue = game.LeftValue,
                    rightValue = game.RightValue,
                    leftPlayerId = game.LeftPlayerId,
                    rightPlayerId = game.RightPlayerId,
                    isGameOver = true,
                    remainingTime = 0,
                    isWin = isWin,
                    endReason = reason
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

        private void SaveFinalGameStats(BalanceGame game)
        {
            Log.Information("[GAME END] Queueing stats for room {RoomId}", game.RoomId);

            // Žádný await, žádný try-catch (ten je ve workeru), žádný IServiceScope
            _dbQueue.QueueGameResultAsync(new GameResultContext(
                game.RoomId,
                game.LeftPlayerId,
                game.RightPlayerId,
                "ballance"
            ));
        }

        public void RemoveRoom(string roomId)
        {
            if (_activeGames.TryRemove(roomId, out var game))
            {
                Log.Information("[ROOM REMOVED] Room: {RoomId}", roomId);
            }
        }

        private void SaveBioFeedbackAsync(string email, string roomId, double value)
        {
            if (Guid.TryParse(roomId, out Guid roomGuid))
            {
                // Žádný await, žádný scope. Jen hodíme do fronty.
                _dbQueue.QueueBioFeedbackAsync(new BioFeedbackMessage(email, roomGuid, (float)value));
            }
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

        public Dictionary<string, double> GetScores() => new Dictionary<string, double>();
    }
}