using LexiContext.Domain.Entities.Common;
namespace LexiContext.Domain.Entities.Classes
{
    public class StudentHomework : BaseEntity
    {
        public Guid ClassroomId { get; set; }
        public Guid StudentId { get; set; }
        public Guid GroupTaskId { get; set; } 
        public string TaskText { get; set; } = string.Empty;
        public bool IsCompleted { get; set; } = false;
    }
}
