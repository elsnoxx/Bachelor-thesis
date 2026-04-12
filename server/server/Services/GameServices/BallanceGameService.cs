using Microsoft.Extensions.DependencyInjection;
using Serilog;
using server.Models.DTO;
using server.Models.Games;
using server.Services.Utils;

namespace server.Services.GameServices
{
    public class BallanceGameService : BaseGameService<BalanceGame>
    {
        public BallanceGameService(IServiceScopeFactory scopeFactory, DbWriteQueue dbQueue)
            : base(scopeFactory, dbQueue) { }

        protected override BalanceGame CreateGame(string roomId) => new() { RoomId = roomId };

        public override object? ProcessInput(string roomId, string playerId, double value)
        {
            var game = GetOrCreateGame(roomId);

            if (string.IsNullOrEmpty(game.LeftPlayerId))
                game.LeftPlayerId = playerId;
            else if (string.IsNullOrEmpty(game.RightPlayerId) && game.LeftPlayerId != playerId)
                game.RightPlayerId = playerId;

            if (game.StartTime == null
                && !string.IsNullOrEmpty(game.LeftPlayerId)
                && !string.IsNullOrEmpty(game.RightPlayerId))
            {
                game.StartTime = DateTime.UtcNow;
                game.GenerateLimits();
                Log.Information("[GAME START] Room: {RoomId}, Players: {P1} vs {P2}",
                    roomId, game.LeftPlayerId, game.RightPlayerId);
                NotifyRoomStatus(roomId, RoomStatus.Start);           // <-- base method
            }

            if (game.StartTime != null)
            {
                game.AddValue(playerId, value);
                SaveBioFeedback(playerId, roomId, value);              // <-- base method
            }

            int remaining = game.GetRemainingTime();

            if (game.IsGameOver)
            {
                if (!game.WasSaved)
                {
                    game.WasSaved = true;
                    SaveGameResult(game.RoomId, game.LeftPlayerId,     // <-- base method
                        game.RightPlayerId, "ballance", playerId);
                }

                double ballPos = game.GetBallPosition();
                bool isWin;
                string reason;

                if (ballPos <= game.TargetMin)
                    { isWin = false; reason = "Loss: the ball fell off the left side."; }
                else if (ballPos >= game.TargetMax)
                    { isWin = false; reason = "Loss: the ball fell off the right side."; }
                else if (remaining <= 0)
                    { isWin = true; reason = "Victory! You kept balance until the end."; }
                else
                    { isWin = false; reason = "Game ended."; }

                return new
                {
                    roomId = game.RoomId, ballPosition = ballPos,
                    leftValue = game.LeftValue, rightValue = game.RightValue,
                    leftPlayerId = game.LeftPlayerId, rightPlayerId = game.RightPlayerId,
                    targetMin = game.TargetMin, targetMax = game.TargetMax,
                    targetMinPlayer = game.TargetMinPlayer, targetMaxPlayer = game.TargetMaxPlayer,
                    isGameOver = true, remainingTime = 0, isWin, endReason = reason
                };
            }

            return new
            {
                roomId = game.RoomId, ballPosition = game.GetBallPosition(),
                targetMin = game.TargetMin, targetMax = game.TargetMax,
                targetMinPlayer = game.TargetMinPlayer, targetMaxPlayer = game.TargetMaxPlayer,
                leftValue = game.LeftValue, rightValue = game.RightValue,
                leftPlayerId = game.LeftPlayerId, rightPlayerId = game.RightPlayerId,
                isGameOver = game.IsGameOver, remainingTime = remaining,
            };
        }

        public void RemoveRoom(string roomId) => RemoveGame(roomId);  // <-- base method

        public Dictionary<string, double> GetScores() => new();
    }
}