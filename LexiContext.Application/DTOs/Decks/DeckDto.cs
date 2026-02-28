using LexiContext.Domain.Enums;

namespace LexiContext.Application.DTOs.Decks
{
    public class DeckDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // localization
        public LearningLanguage TargetLanguage { get; set; }
        public LearningLanguage NativeLanguage { get; set; }
        
        // AI Personalization
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public AiTone Tone { get; set; }
    }
}
