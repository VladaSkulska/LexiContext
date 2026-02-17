using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces
{
    public interface ICardRepository
    {
        Task<Guid> CreateAsync(Card card);
        Task<Card?> GetByIdAsync(Guid id);
        Task<List<Card>> GetByDeckIdAsync(Guid deckId);
        Task UpdateAsync(Card card);
        Task DeleteAsync(Card card);
    }
}
