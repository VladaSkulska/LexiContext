using LexiContext.Domain.Enums;

namespace LexiContext.Application.DTOs.Decks
{
    public class DeckDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string? ShareCode { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // ДОДАНО: Ліміти
        public int DailyNewCardsLimit { get; set; }
        public int DailyReviewLimit { get; set; }

        public Guid CreatedId { get; set; }

        // localization
        public LearningLanguage TargetLanguage { get; set; }
        public LearningLanguage NativeLanguage { get; set; }

        // AI Personalization
        public ProficiencyLevel ProficiencyLevel { get; set; }
        public AiTone Tone { get; set; }

        public int NewCards { get; set; }
        public int LearningCards { get; set; }
        public int ToReview { get; set; }
    }
}