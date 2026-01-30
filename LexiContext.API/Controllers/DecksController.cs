using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LexiContext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DecksController : ControllerBase
    {
        private readonly IDeckService _deckService;
        public DecksController(IDeckService deckService)
        {
            _deckService = deckService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeck(CreateDeckDto requestDto)
        {
            var createdDeck = await _deckService.CreateDeckAsync(requestDto);
            return CreatedAtAction(nameof(GetDeckById), new { id = createdDeck.Id }, createdDeck);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeckById(Guid id)
        {
            var result = await _deckService.GetDeckByIdAsync(id);
            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDecks()
        {
            var result = await _deckService.GetAllDecksAsync();
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateDeck(Guid id, UpdateDeckDto deckDto)
        {
            var result = await _deckService.UpdateDeckAsync(id, deckDto);
            return Ok(result);
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeck(Guid id)
        {
            await _deckService.DeleteDeckAsync(id);
            return NoContent();
        }
    }
}
