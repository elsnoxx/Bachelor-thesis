using server.Models.DB;

namespace server.Repositories.Interfaces
{
    public interface IBiofeedbackRepository
    {
        Task<IEnumerable<BioFeedback>> GetStatistic(Guid userId);
        Task<IEnumerable<BioFeedback>> GetBySessionAsync(Guid userId, Guid sessionId);
        Task AddAsync(BioFeedback bioFeedback);
        Task<IEnumerable<BioFeedback>> GetBioFeedbackBySesionAndUSer(Guid userId, Guid sessionId);
    }
}
