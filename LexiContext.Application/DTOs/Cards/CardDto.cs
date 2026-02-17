namespace LexiContext.Application.DTOs.Cards
{
    public class CardDto
    {
        public Guid Id { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; } 

        public Guid DeckId { get; set; }

        public string Front { get; set; } = string.Empty;
        public string Back { get; set; } = string.Empty;

        public string? GeneratedContext { get; set; }
        public string? ContextTranslation { get; set; }
        public string? ImageURL { get; set; }
        public string? AdditionalMetadata { get; set; }
    }
}
