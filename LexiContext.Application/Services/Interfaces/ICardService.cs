using LexiContext.Application.DTOs.Cards;

namespace LexiContext.Application.Services.Interfaces
{
    public interface ICardService
    {
        public Task<CardDto> CreateCardAsync(CreateCardDto dto);
        public Task<CardDto> GetCardByIdAsync(Guid id);
        public Task<List<CardDto>> GetCardsByDeckIdAsync(Guid deckId);
        public Task<CardDto> UpdateCardAsync(Guid id, UpdateCardDto dto);
        public Task DeleteCardAsync(Guid id);
    }
}
