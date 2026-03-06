using LexiContext.Domain.Entities;
namespace LexiContext.Application.Interfaces.Repos
{
    public interface IStoryRepository
    {
        Task<Guid> CreateAsync(Story story);
        Task<Story?> GetByIdAsync(Guid id);
        Task<List<Story>> GetByUserIdAsync(Guid userId);
        Task<int> CountStoriesInLastWeekAsync(Guid userId);
        Task DeleteAsync(Story story);
    }
}
