using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Services.DbServices.Interfaces;

namespace server.Controllers
{
    [ApiController]
    [Route("api/stats")]
    [Authorize]
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
            var stats = await _statisticServices.GetUserStatsAsync(userId);
            return Ok(stats);
        }

        [HttpGet("biofeedback/{userId}")]
        public async Task<IActionResult> GetUserBiofeedback(Guid userId)
        {
            var data = await _statisticServices.GetUserBiofeedbackAsync(userId);
            return Ok(data);
        }
    }
}
