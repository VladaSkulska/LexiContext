using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces.Repos
{
    public interface IDeckRepository
    {
        Task<Guid> CreateAsync(Deck deck);
        Task<Deck?> GetByIdAsync(Guid id);
        Task<List<Deck>> GetAllAsync();
        Task<List<Deck>> GetAllByUserIdAsync(Guid userId);
        Task UpdateAsync(Deck deck);
        Task DeleteAsync(Deck deck);
    }
}