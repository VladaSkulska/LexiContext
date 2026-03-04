using LexiContext.Domain.Entities.Common;

namespace LexiContext.Domain.Entities
{
    public class UserCardProgress : BaseEntity
    {
        public Guid UserId { get; set; }
        public User? User { get; set; }

        public Guid CardId { get; set; }
        public Card? Card { get; set; }

        // SM-2
        public int Repetitions { get; set; }
        public double EaseFactor { get; set; } = 2.5;
        public int IntervalDays { get; set; } 

        public DateTime NextReviewAt { get; set; }
    }
}
