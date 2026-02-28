using LexiContext.Application.DTOs.Cards;
using LexiContext.Application.Services.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace LexiContext.API.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CardsController : ControllerBase
    {
        private readonly ICardService _cardService;
        public CardsController(ICardService cardService)
        {
            _cardService = cardService;
        }

        [HttpPost]
        public async Task<ActionResult<CardDto>> CreateCard([FromBody] CreateCardDto requestDto) 
        {
            var createdCard = await _cardService.CreateCardAsync(requestDto);
            
            // Важливо: nameof(GetCardById) має вказувати на метод НИЖЧЕ 👇
            return CreatedAtAction(nameof(GetCardById), new { id = createdCard.Id }, createdCard);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CardDto>> GetCardById(Guid id)
        {
            var result = await _cardService.GetCardByIdAsync(id);
            return Ok(result);
        }

        [HttpGet("deck/{deckId}")]
        public async Task<ActionResult<List<CardDto>>> GetCardsByDeckId(Guid deckId)
        {
            var result = await _cardService.GetCardsByDeckIdAsync(deckId);
            return Ok(result);
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<CardDto>> UpdateCard(Guid id, [FromBody] UpdateCardDto cardDto)
        {
            var result = await _cardService.UpdateCardAsync(id, cardDto);
            return Ok(result);
        }

        [HttpPatch("{id}/simplify")]
        public async Task<ActionResult<CardDto>> SimplifyCard(Guid id)
        {
            var result = await _cardService.SimplifyCardAsync(id);
            return Ok(result);
        }   

        [HttpDelete("{id}")]    
        public async Task<IActionResult> DeleteCard(Guid id)
        {
            await _cardService.DeleteCardAsync(id);
            return NoContent();
        }
    }
}
