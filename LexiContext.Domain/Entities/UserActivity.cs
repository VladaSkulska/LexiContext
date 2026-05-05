using LexiContext.Domain.Entities.Common;
namespace LexiContext.Domain.Entities
{
    public class UserActivity : BaseEntity
    {
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public int CardsStudied { get; set; }
        public User? User { get; set; }
    }
}
