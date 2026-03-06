using LexiContext.Domain.Enums;
namespace LexiContext.Application.DTOs.Stories
{
    public class StoryDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public StoryGenre Genre { get; set; }
        public Guid? DeckId { get; set; }
        public DateTime CreatedAt { get; set; }

        public List<StoryPhraseDto> Phrases { get; set; } = new();
    }
}
