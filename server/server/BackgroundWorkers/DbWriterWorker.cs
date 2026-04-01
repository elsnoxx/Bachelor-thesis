using Serilog;
using server.Models.DB;
using server.Services.DbServices.Interfaces;
using server.Services.Utils;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using server.Repositories.Interfaces;

namespace server.BackgroundWorkers
{
    public class DbWriterWorker : BackgroundService
    {
        private readonly DbWriteQueue _queue;
        private readonly IServiceScopeFactory _scopeFactory;

        public DbWriterWorker(DbWriteQueue queue, IServiceScopeFactory scopeFactory)
        {
            _queue = queue;
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            Log.Information("DbWriterWorker is starting.");

            // Spustíme oba konzumenty paralelně
            var bioTask = ProcessBioQueue(stoppingToken);
            var resultsTask = ProcessResultsQueue(stoppingToken);

            // Běžíme, dokud oba úkoly neskončí (což bude při vypnutí aplikace)
            await Task.WhenAll(bioTask, resultsTask);
        }

        private async Task ProcessBioQueue(CancellationToken ct)
        {
            // Čteme z fronty pro BioFeedback (GSR data)
            await foreach (var message in _queue.ReadBioAllAsync(ct))
            {
                try
                {
                    using var scope = _scopeFactory.CreateScope();
                    var statisticService = scope.ServiceProvider.GetRequiredService<IStatisticServices>();

                    // Zápis jednotlivého bodu biofeedbacku
                    await statisticService.AddBioFeedbackByEmailAsync(message.email, message.roomId, message.value);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error writing biofeedback to DB for user {Email}", message.email);
                }
            }
        }

        private async Task ProcessResultsQueue(CancellationToken ct)
        {
            // Čteme z fronty pro finální výsledky her
            await foreach (var game in _queue.ReadResultsAllAsync(ct))
            {
                try
                {
                    // Malé zpoždění, aby se stihla zapsat poslední GSR data z druhé fronty
                    await Task.Delay(500, ct);

                    using var scope = _scopeFactory.CreateScope();
                    var statisticService = scope.ServiceProvider.GetRequiredService<IStatisticServices>();
                    var gameroomService = scope.ServiceProvider.GetRequiredService<IGameRoomService>();
                    var sesionRepo = scope.ServiceProvider.GetRequiredService<ISesionRepository>();
                    var userRepo = scope.ServiceProvider.GetRequiredService<IUserRepository>();

                    if (!Guid.TryParse(game.roomId, out Guid roomGuid)) continue;

                    var players = new[] { game.LeftPlayerId, game.RightPlayerId };

                    foreach (var email in players.Where(e => !string.IsNullOrEmpty(e)))
                    {
                        // 1. Výpočet průměru a max z dat, která už jsou v DB
                        var user = await userRepo.GetByEmailAsync(email);
                        var summary = await statisticService.GetSessionSummaryAsync(email, roomGuid);
                        var sesionId = await sesionRepo.GetSesionIdByEmailAndRoomAsync(user.Id, roomGuid);

                        // 2. Vytvoření statistického záznamu
                        var stats = new Statistic
                        {
                            GameType = game.GameType,
                            LastPlayed = DateTime.UtcNow,
                            TotalSessions = 1,
                            AverageGsr = summary?.Avg ?? 0,
                            BestScore = summary?.Max ?? 0,
                            SessionId = sesionId
                        };

                        await statisticService.AddStatisticByEmailAsync(email, stats);
                        Log.Information("[DB WORKER] Final stats saved for {Email} in room {RoomId}. Avg: {Avg}", email, game.roomId, stats.AverageGsr);
                    }

                    await gameroomService.FinishGameRoom(roomGuid);
                }
                catch (OperationCanceledException) { /* Ignorovat při vypínání */ }
                catch (Exception ex)
                {
                    Log.Error(ex, "Error processing final stats for room {RoomId}", game.roomId);
                }
            }
        }
    }
}