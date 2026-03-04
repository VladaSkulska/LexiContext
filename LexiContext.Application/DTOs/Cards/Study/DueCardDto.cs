namespace LexiContext.Application.DTOs.Cards.Study
{
    public class DueCardDto
    {
        public Guid CardId { get; set; }
        public string Front { get; set; } = string.Empty;
        public string Back { get; set; } = string.Empty;

        public string GeneratedContext { get; set; } = string.Empty;
        public string ContextTranslation { get; set; } = string.Empty;
        public string ContextReading { get; set; } = string.Empty;

        public string ImageURL { get; set; } = string.Empty;

        public bool IsNew { get; set; }
    }
}
