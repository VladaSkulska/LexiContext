namespace LexiContext.Application.DTOs.Stories
{
    public class StoryPhraseDto
    {
        public Guid Id { get; set; }
        public string Phrase { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string Reading { get; set; } = string.Empty;
    }
}
