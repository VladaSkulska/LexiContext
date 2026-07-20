using LexiContext.Domain.Entities.Common;

namespace LexiContext.Domain.Entities.Classes
{
    public class ClassroomDeck : BaseEntity
    {
        public Guid ClassroomId { get; set; }
        public Classroom? Classroom { get; set; }

        public Guid DeckId { get; set; }
        public Deck? Deck { get; set; }
    }
}
