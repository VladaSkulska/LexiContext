using LexiContext.Domain.Enums;

namespace LexiContext.Application.DTOs.Decks
{
    public class CreateDeckDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }

        public int DailyNewCardsLimit { get; set; } = 20;
        public int DailyReviewLimit { get; set; } = 50;

        public LearningLanguage TargetLanguage { get; set; } = LearningLanguage.English;
        public LearningLanguage NativeLanguage { get; set; } = LearningLanguage.Ukrainian;

        public ProficiencyLevel ProficiencyLevel { get; set; } = ProficiencyLevel.Beginner;
        public AiTone Tone { get; set; } = AiTone.Neutral;

        public Guid? ClassroomId { get; set; }
    }
}