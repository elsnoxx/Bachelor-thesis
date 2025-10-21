using Microsoft.AspNetCore.Mvc;
using server.Services.DbServices.Interfaces;

namespace server.Controllers
{
    [ApiController]
    [Route("api/stats/")]
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticServices _statisticServices;
        public StatisticController(IStatisticServices statisticServices)
        {
            _statisticServices = statisticServices;
        }

        [HttpGet("user/{userId}")]
        public async Task<IActionResult> GetUserStatistics(Guid userId)
        {
            // Logic to retrieve user statistics from the database
            return Ok($"Statistics for user {userId}");
        }

        [HttpGet("biofeedback/{userId}")]
        public async Task<IActionResult> GetUserBiofeedback(Guid userId)
        {
            // Logic to retrieve user biofeedback data from the database
            return Ok($"Biofeedback data for user {userId}");
        }
    }
}
