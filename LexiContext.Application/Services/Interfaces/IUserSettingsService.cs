using LexiContext.Application.DTOs.UserSettings;

namespace LexiContext.Application.Services.Interfaces
{
    public interface IUserSettingsService
    {
        Task<UserSettingsDto> GetSettingsAsync(Guid userId);
        Task<UserSettingsDto> UpdateSettingsAsync(Guid userId, UpdateUserSettingsDto dto);
    }
}