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
            var id = await _deckService.CreateDeckAsync(requestDto);
            return Ok(id);
        }

        [HttpGet("{id}")]
        public async Task<IActionResult> GetDeckById(Guid id)
        {
            var result = await _deckService.GetDeckByIdAsync(id);

            if(result == null)
            {
                return NotFound($"Колоду з ID {result} не знайдено");
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<IActionResult> GetAllDecks()
        {
            var result = await _deckService.GetAllDecksAsync();
            return Ok(result);
        }
    }
}
