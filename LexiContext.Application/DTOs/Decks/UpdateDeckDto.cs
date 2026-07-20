using LexiContext.Domain.Enums;

namespace LexiContext.Application.DTOs.Decks
{
    public class UpdateDeckDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;

        // ДОДАНО: Ліміти
        public int DailyNewCardsLimit { get; set; }
        public int DailyReviewLimit { get; set; }

        public ProficiencyLevel ProficiencyLevel { get; set; }
        public AiTone Tone { get; set; }
    }
}