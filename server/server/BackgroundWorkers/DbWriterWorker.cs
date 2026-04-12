using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Serilog;
using server.Models.DB;
using server.Models.DTO;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;
using server.Services.Utils;

namespace server.BackgroundWorkers
{
    /// <summary>
    /// Background service responsible for consuming queued database write operations.
    /// It processes EDA (GSR) telemetry data and final game statistics asynchronously 
    /// to ensure minimal latency for the SignalR real-time communication.
    /// </summary>
    public class DbWriterWorker : BackgroundService
    {
        private readonly DbWriteQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;

        public DbWriterWorker(DbWriteQueue queue, IServiceScopeFactory scopeFactory)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
        }

        /// <summary>
        /// Starts the background processing of biofeedback and game result queues.
        /// </summary>
        /// <param name="stoppingToken">Token to signal service shutdown.</param>
        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("DbWriterWorker is starting.");

            // Running both consumers in parallel to handle telemetry and final results independently
            var bioTask = ProcessBioQueue(stoppingToken);
            var resultsTask = ProcessResultsQueue(stoppingToken);
            var roomStatusTask = ProcessRoomStatusQueue(stoppingToken);


            await Task.WhenAll(bioTask, resultsTask, roomStatusTask);
        }

        /// <summary>
        /// Processes the stream of EDA/GSR data points from the queue.
        /// Each data point is saved to the database for future session analysis.
        /// </summary>
        private async Task ProcessBioQueue(CancellationToken ct)
        {
            // Čteme z fronty pro BioFeedback (GSR data)
            await foreach (var message in _queue.ReadBioAllAsync(ct))
            {
                try
                {
                    // Using a scope because IStatisticServices is a scoped service
                    using var scope = _scopeFactory.CreateScope();
                    var statisticService = scope.ServiceProvider.GetRequiredService<IStatisticService>();

                    await statisticService.AddBioFeedbackByEmailAsync(message.Email, message.RoomId, message.Value);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error writing biofeedback to DB for user {Email}", message.Email);
                }
            }
        }

        /// <summary>
        /// Processes game completion events. Calculates final session summaries (average/max GSR)
        /// and updates player statistics once the game ends.
        /// </summary>
        private async Task ProcessResultsQueue(CancellationToken ct)
        {
            // Čteme z fronty pro finální výsledky her
            await foreach (var game in _queue.ReadResultsAllAsync(ct))
            {
                try
                {
                    // Delay ensuring that all physiological data points from ProcessBioQueue 
                    // have been persisted before calculating the final average.
                    await Task.Delay(500, ct);

                    using var scope = _scopeFactory.CreateScope();
                    var statisticService = scope.ServiceProvider.GetRequiredService<IStatisticService>();
                    var gameroomService = scope.ServiceProvider.GetRequiredService<IGameRoomService>();
                    var sesionRepo = scope.ServiceProvider.GetRequiredService<ISessionRepository>();
                    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    if (!Guid.TryParse(game.RoomId, out Guid roomGuid)) continue;

                    var players = new[] { game.LeftPlayerId, game.RightPlayerId };

                    foreach (var email in players.Where(e => !string.IsNullOrEmpty(e)))
                    {
                        var user = await userRepo.GetByEmailAsync(email);
                        var summary = await statisticService.GetSessionSummaryAsync(email, roomGuid);
                        var sesionId = await sesionRepo.GetSessionIdByUserAndRoomAsync(user.Id, roomGuid);

                        var stats = new Statistic
                        {
                            GameType = game.GameType,
                            LastPlayed = DateTime.UtcNow,
                            TotalSessions = 1,
                            AverageGsr = summary?.Avg ?? 0,
                            BestScore = summary?.Max ?? 0,
                            SessionId = sesionId,
                            Result = game.WinnerPlayerEmail == email ? "Win" : "Lose"
                        };

                        await statisticService.AddStatisticByEmailAsync(email, stats);
                        Log.Information("[DB WORKER] Final stats saved for {Email} in room {RoomId}. Avg: {Avg}", email, game.RoomId, stats.AverageGsr);
                    }

                    await gameroomService.FinishGameRoomAsync(roomGuid);
                }
                catch (OperationCanceledException) { /* Standard shutdown behavior */ }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error processing final stats for room {RoomId}", game.RoomId);
                }
            }
        }

        private async Task ProcessRoomStatusQueue(CancellationToken ct)
        {
            await foreach (var message in _queue.ReadRoomStatusAllAsync(ct))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var roomService = scope.ServiceProvider.GetRequiredService<IGameRoomService>();

                    // Tady používáme message.Status a message.RoomId
                    if (message.Status  == RoomStatus.Start)
                    {
                        await roomService.StartGameRoomAsync(message.RoomId);
                        Log.Information("[DB WORKER] Room {RoomId} set to InProgress", message.RoomId);
                    }
                    else if (message.Status == RoomStatus.Finish)
                    {
                        await roomService.FinishGameRoomAsync(message.RoomId);
                        Log.Information("[DB WORKER] Room {RoomId} set to Finished", message.RoomId);
                    }
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error updating status for room {RoomId}", message.RoomId);
                }
            }
        }
    }
}