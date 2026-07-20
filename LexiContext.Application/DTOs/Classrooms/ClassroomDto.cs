namespace LexiContext.Application.DTOs.Classrooms
{
    public class ClassroomDto
    {
        public Guid Id { get; set; }
        public string Name { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string JoinCode { get; set; } = string.Empty;
        public int StudentsCount { get; set; }
        public int DecksCount { get; set; }
    }
}
