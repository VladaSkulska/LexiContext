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
        
        // AI Personalization
        public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Beginner;
        public AiTone Tone { get; set; } = AiTone.Neutral;
    }
}
