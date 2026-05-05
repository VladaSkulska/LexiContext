using LexiContext.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LexiContext.API.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StatisticsController : ControllerBase
    {
        private readonly IStatisticsService _statisticsService;

        public StatisticsController(IStatisticsService statisticsService)
        {
            _statisticsService = statisticsService;
        }

        [HttpGet("mastery-level")]
        public async Task<IActionResult> GetMasteryLevel([FromQuery] Guid? deckId)
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId))
            {
                return Unauthorized(new { message = "Invalid user token." });
            }

            var result = await _statisticsService.GetMasteryLevelAsync(userId, deckId);

            return Ok(result);
        }

        [HttpGet("forecast")]
        public async Task<IActionResult> GetForecast()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var result = await _statisticsService.GetFutureForecastAsync(userId);
            return Ok(result);
        }

        [HttpGet("activity")]
        public async Task<IActionResult> GetActivity()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdClaim, out var userId)) return Unauthorized();

            var result = await _statisticsService.GetActivityHistoryAsync(userId);
            return Ok(result);
        }
    }
}
