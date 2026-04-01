using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Serilog;
using server.Models.DB;
using server.Models.DTO;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;

namespace server.Services.DbServices
{

    public class StatisticServices : IStatisticServices
    {
        private readonly IStatisticRepository _statisticRepo;
        private readonly IUserRepository _userRepository;
        private readonly IBiofeedbackRepository _bioRepo;
        private readonly ISesionRepository _sesionRepo;
        private readonly ILogger<StatisticServices> _logger;

        public StatisticServices(IStatisticRepository statisticRepo, IBiofeedbackRepository bioRepo, IUserRepository userRepository, ILogger<StatisticServices> logger, ISesionRepository sesionRepository)
        {
            _statisticRepo = statisticRepo;
            _bioRepo = bioRepo;
            _userRepository = userRepository;
            _logger = logger;
            _sesionRepo = sesionRepository;
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

            // 1. Získáme data z repozitáře (tady se provedou a stáhnou do RAM)
            var records = await _bioRepo.GetBioFeedbackBySesionAndUSer(user.Id, roomId);

            // 2. Kontrola, jestli máme vůbec nějaká data (aby Average nehodil chybu)
            if (records == null || !records.Any())
                return null;

            // 3. Výpočet v paměti serveru
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
            return await _statisticRepo.GetStatistic(user.Id);
        }

        public async Task<IEnumerable<BioFeedback>> GetUserBiofeedbackAsync(string userEmail)
        {
            var user = await _userRepository.GetByEmailAsync(userEmail);
            return await _bioRepo.GetStatistic(user.Id);
        }

        public async Task<DetailBioFeedbackData> GetBioSummaryAsync(string userEmail, string sessionIdString)
        {
            // 1. Validace vstupu
            if (!Guid.TryParse(sessionIdString, out Guid sessionGuid))
                return new DetailBioFeedbackData();

            var user = await _userRepository.GetByEmailAsync(userEmail);
            if (user == null) return new DetailBioFeedbackData();

            // 2. Najdeme session – Zásadní krok!
            // Musíme vědět, kdy hráč hrál (StartTime) a v jaké místnosti (GameRoomId)
            var stats = await _statisticRepo.GetStatisticById(sessionGuid);
            
            var session = await _sesionRepo.GetSesionByIdAsync(stats.SessionId);
            if (session == null) return new DetailBioFeedbackData();

            // 3. Načteme VŠECHNA biofeedback data uživatele
            var allUserBioData = await _bioRepo.GetStatistic(user.Id);

            // 4. FILTRACE - Tady se to láme:
            var rawData = allUserBioData
                .Where(d => d.GameRoomId == session.GameRoomId) // Musí odpovídat místnosti '9ab44...'
                .Where(d => d.Timestamp >= session.StartTime)   // Musí to být od začátku session
                .OrderBy(d => d.Timestamp)
                .ToList();

            if (!rawData.Any())
            {
                // LOG pro debug:
                _logger.LogWarning("Žádná data pro User: {UserId} v Room: {RoomId} od {Start}",
                    user.Id, session.GameRoomId, session.StartTime);
                return new DetailBioFeedbackData();
            }

            // 1. Základní statistiky
            var values = rawData.Select(d => d.GsrValue).ToList();
            var avg = values.Average();
            var threshold = avg * 1.2; // Definujeme stres jako 20% nad průměrem

            // 2. Klouzavý průměr (Smoothing) a Peak detection
            var chartData = new List<BioPoint>();
            int windowSize = 5; // Průměr z 5 vzorků
            int peaks = 0;

            for (int i = 0; i < rawData.Count; i++)
            {
                // Výpočet klouzavého průměru pro daný bod
                var window = values.Skip(Math.Max(0, i - windowSize + 1)).Take(windowSize);
                double smoothedValue = window.Average();

                // Jednoduchá detekce peaku (lokální maximum + nad thresholdem)
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
                AverageGsr = (double)stats.AverageGsr,
                MaxGsr = values.Max(),
                MinGsr = values.Min(),
                PeakCount = peaks,
                Baseline = Math.Round(values.Take(10).Average(), 2), // Prvních 10 záznamů jako baseline
                TimeAboveThreshold = rawData.Count(d => d.GsrValue > threshold), // Pokud 1 záznam = 1 sekunda
                ChartData = chartData
            };
        }
    }
}
