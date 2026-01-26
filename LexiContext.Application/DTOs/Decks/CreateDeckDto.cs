using LexiContext.Domain.Enums;

namespace LexiContext.Application.DTOs.Decks
{
    public class CreateDeckDto
    {
        public string Title { get; set; } = string.Empty;   
        public string? Description { get; set; }
        public bool IsPublic { get; set; }

        // localization
        public LearningLanguage TargetLanguage { get; set; } = LearningLanguage.English;
        public LearningLanguage NativeLanguage { get; set; } = LearningLanguage.Ukrainian;
    }
}
