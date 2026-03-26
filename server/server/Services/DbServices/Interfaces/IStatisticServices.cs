using server.Models.DB;

namespace server.Services.DbServices.Interfaces
{
    public interface IStatisticServices
    {
        Task<IEnumerable<Statistic>> GetUserStatsAsync(string userId);
        Task<IEnumerable<BioFeedback>> GetUserBiofeedbackAsync(string userId);
    }
}
