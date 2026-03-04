using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace LexiContext.API.Controllers
{
    [ApiController]
    [Authorize]
    [Route("api/[controller]")]
    public class DecksController : ControllerBase
    {
        private readonly IDeckService _deckService;

        public DecksController(IDeckService deckService)
        {
            _deckService = deckService;
        }

        private Guid GetUserId()
        {
            var userIdString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdString))
            {
                throw new UnauthorizedAccessException("User ID не знайдено в токені.");
            }
            return Guid.Parse(userIdString);
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeck(CreateDeckDto requestDto)
        {
            var userId = GetUserId();
            var createdDeck = await _deckService.CreateDeckAsync(requestDto, userId);
            return CreatedAtAction(nameof(GetDeckById), new { id = createdDeck.Id }, createdDeck);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeckById(Guid id)
        {
            var userId = GetUserId();
            var result = await _deckService.GetDeckByIdAsync(id, userId);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDecks()
        {
            var userId = GetUserId();
            var result = await _deckService.GetAllDecksAsync(userId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeck(Guid id, UpdateDeckDto deckDto)
        {
            var userId = GetUserId();
            var result = await _deckService.UpdateDeckAsync(id, deckDto, userId);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeck(Guid id)
        {
            var userId = GetUserId();
            await _deckService.DeleteDeckAsync(id, userId);
            return NoContent();
        }
    }
}