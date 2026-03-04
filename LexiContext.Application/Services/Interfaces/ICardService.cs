using LexiContext.Application.DTOs.Cards;

namespace LexiContext.Application.Services.Interfaces
{
    public interface ICardService
    {
        public Task<CardDto> CreateCardAsync(CreateCardDto dto, Guid userId);
        public Task<CardDto> GetCardByIdAsync(Guid id, Guid userId);
        public Task<List<CardDto>> GetCardsByDeckIdAsync(Guid deckId, Guid userId);
        public Task<CardDto> UpdateCardAsync(Guid id, UpdateCardDto dto, Guid userId);
        public Task DeleteCardAsync(Guid id, Guid userId);
        public Task<CardDto> SimplifyCardAsync(Guid id, Guid userId);
    }
}