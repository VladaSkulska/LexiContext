using LexiContext.Domain.Entities.Classes;

namespace LexiContext.Application.Interfaces.Repos
{
    public interface IClassroomRepository
    {
        Task<Classroom> CreateAsync(Classroom classroom);
        Task<bool> IsCodeUniqueAsync(string joinCode);
        Task<Classroom?> GetByCodeWithDetailsAsync(string joinCode);
        Task<Classroom?> GetByIdAsync(Guid id);
        Task AddStudentAsync(ClassroomStudent classroomStudent);
        Task AddDeckAsync(ClassroomDeck classroomDeck);
        Task<bool> IsDeckInClassroomAsync(Guid classroomId, Guid deckId);
        Task<List<Classroom>> GetTeacherClassroomsAsync(Guid teacherId);
        Task<List<Classroom>> GetStudentClassroomsAsync(Guid studentId);
        Task<Classroom?> GetByIdWithDecksAsync(Guid id);
        Task RemoveStudentAsync(Guid classroomId, Guid studentId);
        Task RemoveDeckAsync(Guid classroomId, Guid deckId);
        Task DeleteAsync(Classroom classroom);
        Task<bool> HasStudentAccessToDeckAsync(Guid deckId, Guid studentId);
        Task RemoveDeckWithProgressAsync(Guid classroomId, Guid deckId);
        Task<List<Guid>> GetStudentIdsByClassroomAsync(Guid classroomId);
        Task AddStudentHomeworksAsync(IEnumerable<StudentHomework> homeworks);
        Task<bool> IsDeckSharedWithAnyClassroomAsync(Guid deckId);
        Task<List<StudentHomework>> GetHomeworksByClassroomAsync(Guid classroomId);
        Task<List<StudentHomework>> GetStudentHomeworksAsync(Guid classroomId, Guid studentId);
        Task<StudentHomework?> GetHomeworkByGroupAndStudentAsync(Guid groupTaskId, Guid studentId);
        Task DeleteHomeworkByGroupTaskIdAsync(Guid groupTaskId);
        Task UpdateHomeworkAsync(StudentHomework homework);
        Task<List<dynamic>> GetHomeworkSummaryForTeacherAsync(Guid classroomId);
    }
}
