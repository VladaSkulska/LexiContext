using LexiContext.Domain.Entities.Common;
namespace LexiContext.Domain.Entities.Classes
{
    public class Classroom : BaseEntity
    {
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }

        public string JoinCode { get; set; } = string.Empty;

        public Guid TeacherId { get; set; }
        public User? Teacher { get; set; }

        // Зв'язки
        public ICollection<ClassroomStudent> Students { get; set; } = new List<ClassroomStudent>();
        public ICollection<ClassroomDeck> Decks { get; set; } = new List<ClassroomDeck>();
    }
}
