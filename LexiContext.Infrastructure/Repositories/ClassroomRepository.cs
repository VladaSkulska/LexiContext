using LexiContext.Application.DTOs.Classrooms;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Entities.Classes;
using LexiContext.Domain.Exceptions;
using LexiContext.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexiContext.Infrastructure.Repositories
{
    public class ClassroomRepository : IClassroomRepository
    {
        private readonly AppDbContext _context;

        public ClassroomRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Classroom> CreateAsync(Classroom classroom)
        {
            await _context.Classrooms.AddAsync(classroom);
            await _context.SaveChangesAsync();
            return classroom;
        }

        public async Task<bool> IsCodeUniqueAsync(string joinCode)
        {
            return !await _context.Classrooms.AnyAsync(c => c.JoinCode == joinCode);
        }

        public async Task<Classroom?> GetByCodeWithDetailsAsync(string joinCode)
        {
            return await _context.Classrooms
                .Include(c => c.Students)
                .Include(c => c.Decks)
                .FirstOrDefaultAsync(c => c.JoinCode == joinCode);
        }

        public async Task<Classroom?> GetByIdAsync(Guid id)
        {
            return await _context.Classrooms.FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task AddStudentAsync(ClassroomStudent classroomStudent)
        {
            await _context.ClassroomStudents.AddAsync(classroomStudent);
            await _context.SaveChangesAsync();
        }

        public async Task AddDeckAsync(ClassroomDeck classroomDeck)
        {
            await _context.ClassroomDecks.AddAsync(classroomDeck);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> IsDeckInClassroomAsync(Guid classroomId, Guid deckId)
        {
            return await _context.ClassroomDecks.AnyAsync(cd => cd.ClassroomId == classroomId && cd.DeckId == deckId);
        }

        public async Task<List<Classroom>> GetTeacherClassroomsAsync(Guid teacherId)
        {
            return await _context.Classrooms
                .Include(c => c.Students)
                .Include(c => c.Decks)
                .Where(c => c.TeacherId == teacherId)
                .ToListAsync();
        }

        public async Task<List<Classroom>> GetStudentClassroomsAsync(Guid studentId)
        {
            return await _context.Classrooms
                .Include(c => c.Students)
                .Include(c => c.Decks)
                .Where(c => c.Students.Any(s => s.StudentId == studentId))
                .ToListAsync();
        }

        public async Task<Classroom?> GetByIdWithDecksAsync(Guid id)
        {
            return await _context.Classrooms
                .Include(c => c.Students)
                .Include(c => c.Decks)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task RemoveStudentAsync(Guid classroomId, Guid studentId)
        {
            var entity = await _context.ClassroomStudents
                .FirstOrDefaultAsync(cs => cs.ClassroomId == classroomId && cs.StudentId == studentId);
            if (entity != null)
            {
                _context.ClassroomStudents.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task RemoveDeckAsync(Guid classroomId, Guid deckId)
        {
            var entity = await _context.ClassroomDecks
                .FirstOrDefaultAsync(cd => cd.ClassroomId == classroomId && cd.DeckId == deckId);
            if (entity != null)
            {
                _context.ClassroomDecks.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(Classroom classroom)
        {
            if (classroom.Students != null) _context.ClassroomStudents.RemoveRange(classroom.Students);
            if (classroom.Decks != null) _context.ClassroomDecks.RemoveRange(classroom.Decks);

            _context.Classrooms.Remove(classroom);
            await _context.SaveChangesAsync();
        }

        public async Task<bool> HasStudentAccessToDeckAsync(Guid deckId, Guid studentId)
        {
            return await _context.ClassroomDecks
                .Where(cd => cd.DeckId == deckId)
                .AnyAsync(cd => cd.Classroom!.Students.Any(s => s.StudentId == studentId));
        }
        public async Task RemoveDeckWithProgressAsync(Guid classroomId, Guid deckId)
        {
            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                var studentIds = await _context.ClassroomStudents
                    .Where(cs => cs.ClassroomId == classroomId)
                    .Select(cs => cs.StudentId)
                    .ToListAsync();

                var cardIds = await _context.Cards
                    .Where(c => c.DeckId == deckId)
                    .Select(c => c.Id)
                    .ToListAsync();

                await _context.UserCardProgresses
                    .Where(p => studentIds.Contains(p.UserId) && cardIds.Contains(p.CardId))
                    .ExecuteDeleteAsync();

                await _context.ClassroomDecks
                    .Where(cd => cd.ClassroomId == classroomId && cd.DeckId == deckId)
                    .ExecuteDeleteAsync();

                await transaction.CommitAsync();
            }
            catch
            {
                await transaction.RollbackAsync();
                throw;
            }
        }
        public async Task<List<Guid>> GetStudentIdsByClassroomAsync(Guid classroomId)
        {
            return await _context.ClassroomStudents
                .Where(cs => cs.ClassroomId == classroomId)
                .Select(cs => cs.StudentId)
                .ToListAsync();
        }
        public async Task AddStudentHomeworksAsync(IEnumerable<StudentHomework> homeworks)
        {
            await _context.StudentHomeworks.AddRangeAsync(homeworks);
            await _context.SaveChangesAsync();
        }
        public async Task<bool> IsDeckSharedWithAnyClassroomAsync(Guid deckId)
        {
            return await _context.ClassroomDecks.AnyAsync(cd => cd.DeckId == deckId);
        }

        public async Task<List<StudentHomework>> GetHomeworksByClassroomAsync(Guid classroomId)
        {
            return await _context.StudentHomeworks
                .Where(h => h.ClassroomId == classroomId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }
        public async Task<List<HomeworkSummaryDto>> GetHomeworkSummaryForTeacherAsync(Guid classroomId)
        {
            return await _context.StudentHomeworks
                .Where(h => h.ClassroomId == classroomId)
                .GroupBy(h => new { h.GroupTaskId, h.TaskText, h.CreatedAt })
                .Select(g => new HomeworkSummaryDto(
                    g.Key.GroupTaskId,
                    g.Key.TaskText,
                    g.Key.CreatedAt,
                    g.Count(),
                    g.Count(x => x.IsCompleted),
                    g.All(x => x.IsCompleted)
                ))
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync();
        }

        public async Task<List<StudentHomework>> GetStudentHomeworksAsync(Guid classroomId, Guid studentId)
        {
            return await _context.StudentHomeworks
                .Where(h => h.ClassroomId == classroomId && h.StudentId == studentId)
                .OrderByDescending(h => h.CreatedAt)
                .ToListAsync();
        }

        public async Task<StudentHomework?> GetHomeworkByGroupAndStudentAsync(Guid groupTaskId, Guid studentId)
        {
            return await _context.StudentHomeworks
                .FirstOrDefaultAsync(h => h.GroupTaskId == groupTaskId && h.StudentId == studentId);
        }

        public async Task DeleteHomeworkByGroupTaskIdAsync(Guid groupTaskId)
        {
            var records = await _context.StudentHomeworks
                .Where(h => h.GroupTaskId == groupTaskId)
                .ToListAsync();

            if (records.Any())
            {
                _context.StudentHomeworks.RemoveRange(records);
                await _context.SaveChangesAsync();
            }
        }

        public async Task UpdateHomeworkAsync(StudentHomework homework)
        {
            _context.StudentHomeworks.Update(homework);
            await _context.SaveChangesAsync();
        }
    }
}
