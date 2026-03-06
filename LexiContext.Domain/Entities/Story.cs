using LexiContext.Domain.Entities.Common;
using LexiContext.Domain.Enums;

namespace LexiContext.Domain.Entities
{
    public class Story : BaseEntity
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;

        public StoryGenre Genre { get; set; } = StoryGenre.EverydayLife;

        public Guid DeckId { get; set; }
        public Deck? Deck { get; set; }
        public Guid CreatedId { get; set; }
        public User? Creater { get; set; }
        public ICollection<StoryPhrase> Phrases { get; set; } = new List<StoryPhrase>();
    }
}