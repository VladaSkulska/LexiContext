using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces.Repos
{
    public interface IUserActivityRepository
    {
        Task<UserActivity?> GetByDateAsync(Guid userId, DateTime date);
        Task CreateAsync(UserActivity activity);
        Task UpdateAsync(UserActivity activity);
    }
}
