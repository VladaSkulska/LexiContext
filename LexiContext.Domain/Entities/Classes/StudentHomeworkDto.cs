namespace LexiContext.Domain.Entities.Classes
{
    public class StudentHomeworkDto
    {
        public Guid Id { get; set; }
        public Guid GroupTaskId { get; set; }
        public string TaskText { get; set; }
        public bool IsCompleted { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
