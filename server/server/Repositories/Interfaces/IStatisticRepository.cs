using server.Models.DB;

namespace server.Repositories.Interfaces
{
    public interface IStatisticRepository
    {
        Task<IEnumerable<Statistic>> GetStatistic(Guid UserId);
        Task<Statistic> GetStatisticById(Guid id);

        Task AddAsync(Statistic statistic);
        Task<Guid> GetGameRoomBySessionId(Guid sessionId);
    }
}
