using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces.Repos
{
    public interface IUserSettingsRepository
    {
        Task<UserSettings?> GetByUserIdAsync(Guid userId);
        Task<Guid> CreateAsync(UserSettings settings);
        Task UpdateAsync(UserSettings settings);
    }
}
