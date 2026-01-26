using LexiContext.Domain.Entities.Common;
using LexiContext.Domain.Enums;

namespace LexiContext.Domain.Entities
{
    public class Deck : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }

        // deck limis
        public int DailyNewCardsLimit { get; set; } = 20;
        public int DailyReviewLimit { get; set; } = 50;

        // localization
        public LearningLanguage TargetLanguage { get; set; } = LearningLanguage.English;
        public LearningLanguage NativeLanguage { get; set; } = LearningLanguage.Ukrainian;
        
        // connections
        public Guid CreatedId { get; set; }
        public User? Creater { get; set; }

        public ICollection<Card> Cards { get; set; } = new List<Card>();
    }
}
