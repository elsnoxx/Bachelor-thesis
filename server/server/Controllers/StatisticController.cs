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

        [HttpGet("user/{userEmail}")]
        public async Task<IActionResult> GetUserStatistics(string userEmail)
        {
            var stats = await _statisticServices.GetUserStatsAsync(userEmail);
            return Ok(stats);
        }

        [HttpGet("biofeedback/{userEmail}")]
        public async Task<IActionResult> GetUserBiofeedback(string userEmail)
        {
            var data = await _statisticServices.GetUserBiofeedbackAsync(userEmail);
            return Ok(data);
        }

        [HttpGet("biofeedback/{userEmail}/summary/{sesionId}")]
        public async Task<IActionResult> GetBioSummary(string userEmail, string sesionId)
        {
            var summary = await _statisticServices.GetBioSummaryAsync(userEmail, sesionId);
            return Ok(summary);
        }
    }
}
