using Microsoft.AspNetCore.Mvc;
using server.Models.DB;
using server.Repositories.Interfaces;
using server.Services.DbServices.Interfaces;

namespace server.Services.DbServices
{

    public class StatisticServices : IStatisticServices
    {
        private readonly IStatisticRepository _statisticRepo;
        private readonly IBiofeedbackRepository _bioRepo;

        public StatisticServices(IStatisticRepository statisticRepo, IBiofeedbackRepository bioRepo)
        {
            _statisticRepo = statisticRepo;
            _bioRepo = bioRepo;
        }

        public async Task<IEnumerable<Statistic>> GetUserStatsAsync(Guid userId)
        {
            return await _statisticRepo.GetStatistic(userId);
        }

        public async Task<IEnumerable<BioFeedback>> GetUserBiofeedbackAsync(Guid userId)
        {
            return await _bioRepo.GetStatistic(userId);
        }
    }
}
