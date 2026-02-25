using LexiContext.Domain.Entities.Common;
using LexiContext.Domain.Enums;

namespace LexiContext.Domain.Entities
{
    public class Deck : BaseEntity
    {
        public string Title { get; set; } = string.Empty; // Використовуємо як Theme/Context для ШІ
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
        public int DailyNewCardsLimit { get; set; } = 20;
        public int DailyReviewLimit { get; set; } = 50;

        public LearningLanguage TargetLanguage { get; set; } = LearningLanguage.English;
        public LearningLanguage NativeLanguage { get; set; } = LearningLanguage.Ukrainian;

        // AI Settings
        public ProficiencyLevel Level { get; set; } = ProficiencyLevel.Beginner;
        public AiTone Tone { get; set; } = AiTone.Neutral;

        
        public Guid CreatedId { get; set; }
        public User? Creater { get; set; }

        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
