using LexiContext.Domain.Entities.Common;

namespace LexiContext.Domain.Entities
{
    public class UserCardProgress : BaseEntity
    {
        // who studies?
        public Guid UserId { get; set; }
        public User? User { get; set; }

        // which card?
        public Guid CardId { get; set; }
        public Card? Card { get; set; }

        // SM-2
        public int Box {  get; set; }
        public double EaseFactor { get; set; } = 2.5;
        public int IntervalDays { get; set; } 

        public DateTime NextReviewAt { get; set; }
    }
}
