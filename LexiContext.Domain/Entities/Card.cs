using LexiContext.Domain.Entities.Common;

namespace LexiContext.Domain.Entities
{
    public class Card : BaseEntity
    {
        public string Front { get; set; } = string.Empty;
        public string Back { get; set; } = string.Empty;

        //AI
        public string? GeneratedContext { get; set; }
        public string? ContextTranslation { get; set; }
        public string? ContextReading { get; set; }
        public string? ImageURL { get; set; }
        public string? AdditionalMetadata { get; set; }
        public bool IsSimplified { get; set; } = false;

        public Guid DeckId { get; set; }
        public Deck? Deck { get; set; }
    }
}
