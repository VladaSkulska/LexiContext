using LexiContext.Application.DTOs.Cards;
using LexiContext.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LexiContext.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;
        public CardsController(ICardService cardService)
        {
            _cardService = cardService;
        }
        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
                throw new UnauthorizedAccessException("User ID не знайдено в токені.");
            return Guid.Parse(userIdString);
        }

        [HttpPost]
        public async Task<ActionResult<CardDto>> CreateCard([FromBody] CreateCardDto requestDto)
        {
            var userId = GetUserId();
            var createdCard = await _cardService.CreateCardAsync(requestDto, userId);
            return CreatedAtAction(nameof(GetCardById), new { id = createdCard.Id }, createdCard);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CardDto>> GetCardById(Guid id)
        {
            var userId = GetUserId();
            var result = await _cardService.GetCardByIdAsync(id, userId);
            return Ok(result);
        }

        [HttpGet("deck/{deckId}")]
        public async Task<ActionResult<List<CardDto>>> GetCardsByDeckId(Guid deckId)
        {
            var userId = GetUserId();
            var result = await _cardService.GetCardsByDeckIdAsync(deckId, userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CardDto>> UpdateCard(Guid id, [FromBody] UpdateCardDto cardDto)
        {
            var userId = GetUserId();
            var result = await _cardService.UpdateCardAsync(id, cardDto, userId);
            return Ok(result);
        }

        [HttpPatch("{id}/simplify")]
        public async Task<ActionResult<CardDto>> SimplifyCard(Guid id)
        {
            var userId = GetUserId();
            var result = await _cardService.SimplifyCardAsync(id, userId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCard(Guid id)
        {
            var userId = GetUserId();
            await _cardService.DeleteCardAsync(id, userId);
            return NoContent();
        }
    }
}