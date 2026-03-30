using Microsoft.AspNetCore.Mvc;
using server.Models.DB;
using server.Models.DTO;

namespace server.Services.DbServices.Interfaces
{
    public interface IStatisticServices
    {
        Task AddStatisticByEmailAsync(string email, Statistic statistic);
        Task AddBioFeedbackByEmailAsync(string email, Guid roomGuid, float value);
        Task<IEnumerable<Statistic>> GetUserStatsAsync(string userEmail);
        Task<IEnumerable<BioFeedback>> GetUserBiofeedbackAsync(string userEmail);
        Task<DetailBioFeedbackData> GetBioSummaryAsync(string userEmail, string sesionId);
    }
}
