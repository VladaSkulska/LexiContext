using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces.Repos
{
    public interface ICardRepository
    {
        Task<Guid> CreateAsync(Card card);
        Task<Card?> GetByIdAsync(Guid id);
        Task<List<Card>> GetByDeckIdAsync(Guid deckId);
        Task UpdateAsync(Card card);
        Task DeleteAsync(Card card);
        Task<List<Card>> GetRandomCardsForStoryAsync(Guid deckId, int count);
    }
}
