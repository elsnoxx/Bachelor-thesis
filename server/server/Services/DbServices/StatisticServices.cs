using Microsoft.AspNetCore.Mvc;
using server.Models.DB;
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
    }
}
