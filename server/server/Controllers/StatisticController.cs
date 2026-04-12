using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using server.Models.DB;
using server.Services.DbServices.Interfaces;

namespace server.Controllers
{
    /// <summary>
    /// Controller providing access to historical game statistics and recorded 
    /// physiological data (EDA/GSR) for users.
    /// </summary>
    [ApiController]
    [Route("api/stats")]
    [Authorize] // Access to personal health/game data is restricted to authenticated users
    public class StatisticController : ControllerBase
    {
        private readonly IStatisticService _statisticServices;

        public StatisticController(IStatisticService statisticServices)
        {
            _statisticServices = statisticServices;
        }

        /// <summary>
        /// Retrieves overall game statistics for a specific user.
        /// Includes total games played, average GSR values, and high scores.
        /// </summary>
        /// <param name="userEmail">Email address of the user.</param>
        /// <returns>A collection of statistical records.</returns>
        [HttpGet("user/{userEmail}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserStatistics(string userEmail)
        {
            var stats = await _statisticServices.GetUserStatsAsync(userEmail);
            return Ok(stats);
        }

        /// <summary>
        /// Fetches raw biofeedback data points (GSR) for a specific user.
        /// Used for rendering historical stress-level charts.
        /// </summary>
        /// <param name="userEmail">Email address of the user.</param>
        /// <returns>A list of raw EDA data points with timestamps.</returns>
        [HttpGet("biofeedback/{userEmail}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        public async Task<IActionResult> GetUserBiofeedback(string userEmail)
        {
            var data = await _statisticServices.GetUserBiofeedbackAsync(userEmail);
            return Ok(data);
        }

        /// <summary>
        /// Provides a summary of physiological activity for a specific game session.
        /// Calculates baseline vs. peak electrodermal activity.
        /// </summary>
        /// <param name="userEmail">Email address of the user.</param>
        /// <param name="sessionId">Unique identifier of the game session.</param>
        /// <returns>Aggregated session summary (average, min, max GSR values).</returns>
        [HttpGet("biofeedback/{userEmail}/summary/{sessionId}")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetBioSummary(string userEmail, string sesionId)
        {
            var summary = await _statisticServices.GetBioSummaryAsync(userEmail, sesionId);

            if (summary == null)
            {
                return NotFound("Session statistics not found.");
            }

            return Ok(summary);
        }
    }
}
