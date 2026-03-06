using LexiContext.Domain.Enums;

namespace LexiContext.Application.DTOs.Stories
{
    public class GenerateStoryDto
    {
        public Guid DeckId { get; set; }
        public StoryGenre Genre { get; set; } = StoryGenre.EverydayLife;
    }
}