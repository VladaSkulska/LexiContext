using LexiContext.Application.DTOs.Decks;
using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces
{
    public interface IDeckRepository
    {
        Task<Guid> CreateAsync(Deck deck);
        Task<Deck?> GetByIdAsync(Guid id);
        Task<List<Deck>> GetAllAsync();
        Task UpdateAsync(Deck deck);
        Task DeleteAsync(Deck deck);
    }
}
