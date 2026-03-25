using server.Models.DB;

namespace server.Repositories.Interfaces
{
    public interface IBiofeedbackRepository
    {
        Task<IEnumerable<BioFeedback>> GetStatistic(Guid userId);
    }
}
