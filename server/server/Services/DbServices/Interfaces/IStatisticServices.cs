using server.Models.DB;

namespace server.Services.DbServices.Interfaces
{
    public interface IStatisticServices
    {
        Task<IEnumerable<Statistic>> GetUserStatsAsync(Guid userId);
        Task<IEnumerable<BioFeedback>> GetUserBiofeedbackAsync(Guid userId);
    }
}
