using server.Models.DB;

namespace server.Repositories.Interfaces
{
    public interface IStatisticRepository
    {
        Task<IEnumerable<Statistic>> GetStatistic(Guid UserId);
    }
}
