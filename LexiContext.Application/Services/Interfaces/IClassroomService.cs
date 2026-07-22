using LexiContext.Application.DTOs.Classrooms;
using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Domain.Entities.Classes;
namespace LexiContext.Application.Services.Interfaces
{
    public interface IClassroomService
    {
        Task<ClassroomDto> CreateClassroomAsync(CreateClassroomDto dto, Guid teacherId);
        Task<ClassroomDto> JoinClassroomAsync(string joinCode, Guid studentId);
        Task AddDeckToClassroomAsync(Guid classroomId, Guid deckId, Guid teacherId);
        Task<List<ClassroomDto>> GetTeacherClassroomsAsync(Guid teacherId);
        Task<List<ClassroomDto>> GetStudentClassroomsAsync(Guid studentId);
        Task<List<DeckDto>> GetClassroomDecksAsync(Guid classroomId, Guid userId);
        Task LeaveClassroomAsync(Guid classroomId, Guid studentId);
        Task RemoveDeckFromClassroomAsync(Guid classroomId, Guid deckId, Guid teacherId);
        Task DeleteClassroomAsync(Guid classroomId, Guid teacherId);
        Task CreateHomeworkAsync(Guid classroomId, string text, Guid teacherId);
        Task<List<HomeworkSummaryDto>> GetHomeworkForTeacherAsync(Guid classroomId, Guid teacherId);
        Task<List<StudentHomeworkDto>> GetHomeworkForStudentAsync(Guid classroomId, Guid studentId);
        Task DeleteHomeworkAsync(Guid groupTaskId, Guid teacherId);
        Task ToggleHomeworkAsync(Guid groupTaskId, Guid studentId);
    }
}
