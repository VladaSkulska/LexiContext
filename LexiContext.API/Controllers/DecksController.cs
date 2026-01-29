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
            try
            {
                var createdDeck = await _deckService.CreateDeckAsync(requestDto);

                return CreatedAtAction(nameof(GetDeckById), new { id = createdDeck.Id }, createdDeck);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeckById(Guid id)
        {
            var result = await _deckService.GetDeckByIdAsync(id);

            if (result == null)
            {
                return NotFound($"Deck with ID {id} is not found");
            }

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
            try
            {
                var result = await _deckService.UpdateDeckAsync(id, deckDto);

                if (result == null)
                {
                    return NotFound($"Deck with ID {id} is not found");
                }

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteDeck(Guid id)
        {
            var isDeleted = await _deckService.DeleteDeckAsync(id);

            if (!isDeleted)
            {
                return NotFound($"Deck with ID {id} was not found to be deleted");
            }

            return NoContent();
        }
    }
}
