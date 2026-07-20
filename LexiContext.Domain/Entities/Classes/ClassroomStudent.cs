using LexiContext.Domain.Entities.Common;

namespace LexiContext.Domain.Entities.Classes
{
    public class ClassroomStudent : BaseEntity
    {
        public Guid ClassroomId { get; set; }
        public Classroom? Classroom { get; set; }

        public Guid StudentId { get; set; }
        public User? Student { get; set; }
    }
}
