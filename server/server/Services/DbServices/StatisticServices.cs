using Microsoft.AspNetCore.Mvc;
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

        public StatisticServices(IStatisticRepository statisticRepo, IBiofeedbackRepository bioRepo, IUserRepository userRepository)
        {
            _statisticRepo = statisticRepo;
            _bioRepo = bioRepo;
            _userRepository = userRepository;
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

        public async Task<DetailBioFeedbackData> GetBioSummaryAsync(string userEmail, string sesionId)
        {
            var user = await _userRepository.GetByEmailAsync(userEmail);

            // 1. Převedeme string na Guid
            if (!Guid.TryParse(sesionId, out Guid sessionGuid))
            {
                // Pokud string není validní Guid, vrátíme prázdná data (nebo vyhodíme chybu)
                return new DetailBioFeedbackData();
            }

            var allData = await _bioRepo.GetStatistic(user.Id);

            // 2. Filtrujeme pomocí Guidu
            var rawData = allData
                            .Where(d => d.GameRoomId == sessionGuid)
                            .OrderBy(d => d.Timestamp)
                            .ToList();

            if (!rawData.Any()) return new DetailBioFeedbackData();

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
                AverageGsr = Math.Round(avg, 2),
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
