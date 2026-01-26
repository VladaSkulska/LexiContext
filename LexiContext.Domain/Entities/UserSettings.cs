using LexiContext.Domain.Entities.Common;
using LexiContext.Domain.Enums;

namespace LexiContext.Domain.Entities
{
    public class UserSettings : BaseEntity
    {
        public LanguageCode InterfaceLanguage { get; set; } = LanguageCode.English;
        public AppTheme Theme { get; set; } = AppTheme.Light;
        public double FontScale { get; set; } = 1.0;
        public bool EnableSound { get; set; } = true;

        //connections
        public Guid UserId { get; set; }
        public User? User { get; set; }
    }
}
