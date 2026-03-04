using LexiContext.Application.DTOs.Cards.Study;
using LexiContext.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LexiContext.Api.Controllers
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class StudyController : ControllerBase
    {
        private readonly IStudyService _studyService;

        public StudyController(IStudyService studyService)
        {
            _studyService = studyService;
        }

        [HttpGet("due/{deckId}")]
        public async Task<IActionResult> GetDueCards(Guid deckId)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var userId = Guid.Parse(userIdString);
            var dueCards = await _studyService.GetDueCardsAsync(deckId, userId);

            if (!dueCards.Any())
            {
                return Ok(new { message = "You have studied all cards for today!", cards = dueCards });
            }

            return Ok(dueCards);
        }

        [HttpPost("review")]
        public async Task<IActionResult> ReviewCard([FromBody] ReviewCardDto dto)
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);

            if (string.IsNullOrEmpty(userIdString))
            {
                return Unauthorized(new { message = "User ID not found in token." });
            }

            var userId = Guid.Parse(userIdString);
            await _studyService.ProcessReviewAsync(userId, dto);

            return Ok(new { message = "Card progress updated successfully." });
        }
    }
}