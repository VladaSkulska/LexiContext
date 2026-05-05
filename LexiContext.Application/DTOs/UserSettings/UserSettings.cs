using LexiContext.Domain.Enums;

namespace LexiContext.Application.DTOs.UserSettings
{
    public class UserSettingsDto
    {
        public LanguageCode InterfaceLanguage { get; set; }
        public AppTheme Theme { get; set; }
        public double FontScale { get; set; }
        public bool EnableSound { get; set; }
    }
}