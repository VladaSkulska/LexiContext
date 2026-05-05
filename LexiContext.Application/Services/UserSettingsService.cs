using LexiContext.Application.DTOs.UserSettings;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Enums;

namespace LexiContext.Application.Services
{
    public class UserSettingsService : IUserSettingsService
    {
        private readonly IUserSettingsRepository _settingsRepository;

        public UserSettingsService(IUserSettingsRepository settingsRepository)
        {
            _settingsRepository = settingsRepository;
        }

        public async Task<UserSettingsDto> GetSettingsAsync(Guid userId)
        {
            var settings = await _settingsRepository.GetByUserIdAsync(userId);

            if (settings == null)
            {
                settings = new UserSettings
                {
                    UserId = userId,
                    InterfaceLanguage = LanguageCode.English,
                    Theme = AppTheme.Light,
                    FontScale = 1.0,
                    EnableSound = true
                };
                await _settingsRepository.CreateAsync(settings);
            }

            return new UserSettingsDto
            {
                InterfaceLanguage = settings.InterfaceLanguage,
                Theme = settings.Theme,
                FontScale = settings.FontScale,
                EnableSound = settings.EnableSound
            };
        }

        public async Task<UserSettingsDto> UpdateSettingsAsync(Guid userId, UpdateUserSettingsDto dto)
        {
            var settings = await _settingsRepository.GetByUserIdAsync(userId);

            if (settings == null)
            {
                settings = new UserSettings { UserId = userId };
                await _settingsRepository.CreateAsync(settings);
            }

            settings.InterfaceLanguage = dto.InterfaceLanguage;
            settings.Theme = dto.Theme;
            settings.FontScale = dto.FontScale;
            settings.EnableSound = dto.EnableSound;

            await _settingsRepository.UpdateAsync(settings);

            return new UserSettingsDto
            {
                InterfaceLanguage = settings.InterfaceLanguage,
                Theme = settings.Theme,
                FontScale = settings.FontScale,
                EnableSound = settings.EnableSound
            };
        }
    }
}
