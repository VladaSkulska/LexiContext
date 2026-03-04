using LexiContext.Domain.Entities;
namespace LexiContext.Application.Interfaces.Repos
{
    public interface IUserCardProgressRepository
    {
        Task<List<UserCardProgress>> GetByDeckIdAsync(Guid userId, Guid deckId);
        Task<UserCardProgress?> GetByCardIdAsync(Guid userId, Guid cardId);
        Task CreateAsync(UserCardProgress progress);
        Task UpdateAsync(UserCardProgress progress);
    }
}
