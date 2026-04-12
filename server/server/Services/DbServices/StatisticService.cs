using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using server.Models.DB;
using server.Models.DTO;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;

namespace server.Services.DbServices
{

    public class StatisticService : IStatisticService
    {
        private readonly IStatisticRepository _statisticRepo;
        private readonly IUserRepository _userRepository;
        private readonly IBiofeedbackRepository _bioRepo;
        private readonly ISessionRepository _sessionRepo;
        private readonly ILogger<StatisticService> _logger;

        public StatisticService(IStatisticRepository statisticRepo, IBiofeedbackRepository bioRepo, IUserRepository userRepository, ILogger<StatisticService> logger, ISessionRepository sesionRepository)
        {
            _statisticRepo = statisticRepo;
            _bioRepo = bioRepo;
            _userRepository = userRepository;
            _logger = logger;
            _sessionRepo = sesionRepository;
        }

        public async Task AddStatisticByEmailAsync(string email, Statistic statistic)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            _logger.LogInformation("Adding statistic for user {Email} with game type {GameType}, user id is {userId}", email, statistic.GameType, user.Id);
            if (user != null)
            {
                var stats = new Statistic
                {
                    UserId = user.Id,
                    GameType = statistic.GameType,
                    AverageGsr = statistic.AverageGsr,
                    BestScore = statistic.BestScore,
                    TotalSessions = statistic.TotalSessions,
                    LastPlayed = statistic.LastPlayed,
                    SessionId = statistic.SessionId
                };
                await _statisticRepo.AddAsync(stats);
            }
        }

        public async Task<BioSummary?> GetSessionSummaryAsync(string email, Guid roomId)
        {
            var user = await _userRepository.GetByEmailAsync(email);
            if (user == null) return null;

            var records = await _bioRepo.GetBySessionAsync(user.Id, roomId);

            if (records == null || !records.Any())
                return null;

            return new BioSummary
            {
                Avg = (float)records.Average(b => b.GsrValue),
                Min = (float)records.Min(b => b.GsrValue),
                Max = (float)records.Max(b => b.GsrValue)
            };
        }

        public async Task AddBioFeedbackByEmailAsync(string email, Guid roomGuid, float value)
        {
            var user = await _userRepository.GetByEmailAsync(email);

            if (user != null)
            {
                var bio = new BioFeedback
                {
                    UserId = user.Id,
                    GameRoomId = roomGuid,
                    GsrValue = value,
                    Timestamp = DateTime.UtcNow
                };
                await _bioRepo.AddAsync(bio);
            }
        }

        public async Task<IEnumerable<Statistic>> GetUserStatsAsync(string userEmail)
        {
            var user = await _userRepository.GetByEmailAsync(userEmail);
            return await _statisticRepo.GetByUserIdAsync(user.Id);
        }

        public async Task<IEnumerable<BioFeedback>> GetUserBiofeedbackAsync(string userEmail)
        {
            var user = await _userRepository.GetByEmailAsync(userEmail);
            return await _bioRepo.GetUserStatisticsAsync(user.Id);
        }

        public async Task<DetailBioFeedbackData> GetBioSummaryAsync(string userEmail, string sessionIdString)
        {
            if (!Guid.TryParse(sessionIdString, out Guid sessionGuid))
                return new DetailBioFeedbackData();

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null) return new DetailBioFeedbackData();

            var stats = await _statisticRepo.GetGameRoomIdBySessionIdAsync(sessionGuid);
            
            var session = await _sessionRepo.GetByIdAsync(stats);
            if (session == null) return new DetailBioFeedbackData();


            var allUserBioData = await _bioRepo.GetUserStatisticsAsync(user.Id);


            var rawData = allUserBioData
                .Where(d => d.GameRoomId == session.GameRoomId)
                .Where(d => d.Timestamp >= session.StartTime)
                .OrderBy(d => d.Timestamp)
                .ToList();

            if (!rawData.Any())
            {
                _logger.LogWarning("Žádná data pro User: {UserId} v Room: {RoomId} od {Start}",
                    user.Id, session.GameRoomId, session.StartTime);
                return new DetailBioFeedbackData();
            }


            var values = rawData.Select(d => d.GsrValue).ToList();
            var avg = values.Average();
            var threshold = avg * 1.2;

            var chartData = new List<BioPoint>();
            int windowSize = 5;
            int peaks = 0;

            for (int i = 0; i < rawData.Count; i++)
            {
                var window = values.Skip(Math.Max(0, i - windowSize + 1)).Take(windowSize);
                double smoothedValue = window.Average();

                bool isPeak = false;
                if (i > 0 && i < rawData.Count - 1)
                {
                    if (values[i] > values[i - 1] && values[i] > values[i + 1] && values[i] > threshold)
                    {
                        isPeak = true;
                        peaks++;
                    }
                }

                chartData.Add(new BioPoint
                {
                    Timestamp = rawData[i].Timestamp,
                    Value = Math.Round(smoothedValue, 2),
                    IsPeak = isPeak
                });
            }

            return new DetailBioFeedbackData
            {
                AverageGsr = Math.Round(values.Average(), 2),
                MaxGsr = values.Max(),
                MinGsr = values.Min(),
                PeakCount = peaks,
                Baseline = Math.Round(values.Take(10).Average(), 2),
                TimeAboveThreshold = rawData.Count(d => d.GsrValue > threshold),
                ChartData = chartData
            };
        }
    }
}
