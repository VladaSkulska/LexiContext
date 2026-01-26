using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Interfaces;
using LexiContext.Domain.Entities;
using Microsoft.AspNetCore.Mvc;

namespace LexiContext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class DecksController : ControllerBase
    {
        private readonly IDeckRepository _deckRepository;
        public DecksController(IDeckRepository deckRepository)
        {
            _deckRepository = deckRepository;
        }

        [HttpPost]
        public async Task<IActionResult> CreateDeck(CreateDeckDto dto)
        {
            var deckEntity = new Deck
            {
                Title = dto.Title,
                Description = dto.Description ?? string.Empty,
                IsPublic = dto.IsPublic,
                TargetLanguage = dto.TargetLanguage,
                NativeLanguage = dto.NativeLanguage
            };

            var id = await _deckRepository.CreateAsync(deckEntity);
            return Ok(id);
        }
    }
}
