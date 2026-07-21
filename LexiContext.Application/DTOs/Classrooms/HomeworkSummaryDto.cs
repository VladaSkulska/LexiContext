namespace LexiContext.Application.DTOs.Classrooms
{
    public record HomeworkSummaryDto(
        Guid GroupTaskId,
        string TaskText,
        DateTime CreatedAt,
        int TotalStudents,
        int CompletedCount,
        bool IsCompleted
    );
}
