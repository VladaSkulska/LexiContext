using LexiContext.Application.DTOs.Classrooms;
using LexiContext.Application.DTOs.Decks;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Domain.Entities.Classes;
using LexiContext.Domain.Exceptions;

namespace LexiContext.Infrastructure.Repositories
{
    public class ClassroomService : IClassroomService
    {
        private readonly IClassroomRepository _classroomRepository;
        private readonly IDeckRepository _deckRepository;

        public ClassroomService(IClassroomRepository classroomRepository, IDeckRepository deckRepository)
        {
            _classroomRepository = classroomRepository;
            _deckRepository = deckRepository;
        }

        public async Task<ClassroomDto> CreateClassroomAsync(CreateClassroomDto dto, Guid teacherId)
        {
            string joinCode;
            do
            {
                joinCode = $"CLASS-{Guid.NewGuid().ToString().Substring(0, 6).ToUpper()}";
            }
            while (!await _classroomRepository.IsCodeUniqueAsync(joinCode));

            var classroom = new Classroom
            {
                Name = dto.Name,
                Description = dto.Description,
                JoinCode = joinCode,
                TeacherId = teacherId
            };

            var createdClassroom = await _classroomRepository.CreateAsync(classroom);
            return MapToDto(createdClassroom);
        }

        public async Task<ClassroomDto> JoinClassroomAsync(string joinCode, Guid studentId)
        {
            var classroom = await _classroomRepository.GetByCodeWithDetailsAsync(joinCode.Trim().ToUpper());

            if (classroom == null)
                throw new NotFoundException("Classroom", joinCode);

            if (classroom.TeacherId == studentId)
                throw new InvalidOperationException("A teacher cannot be a student in their own class.");

            bool alreadyJoined = classroom.Students.Any(s => s.StudentId == studentId);
            if (alreadyJoined)
                throw new InvalidOperationException("You are already enrolled in this class.");

            var classroomStudent = new ClassroomStudent
            {
                ClassroomId = classroom.Id,
                StudentId = studentId
            };

            await _classroomRepository.AddStudentAsync(classroomStudent);

            classroom.Students.Add(classroomStudent);
            return MapToDto(classroom);
        }

        public async Task AddDeckToClassroomAsync(Guid classroomId, Guid deckId, Guid teacherId)
        {
            var classroom = await _classroomRepository.GetByIdAsync(classroomId);
            if (classroom == null || classroom.TeacherId != teacherId)
                throw new UnauthorizedAccessException("You do not have access to this class.");

            var deck = await _deckRepository.GetByIdAsync(deckId);
            if (deck == null || deck.CreatedId != teacherId)
                throw new UnauthorizedAccessException("The deck was not found or you are not its owner.");

            bool alreadyAdded = await _classroomRepository.IsDeckInClassroomAsync(classroomId, deckId);
            if (alreadyAdded)
                throw new InvalidOperationException("This deck has already been added to the class.");

            var classroomDeck = new ClassroomDeck
            {
                ClassroomId = classroomId,
                DeckId = deckId
            };

            await _classroomRepository.AddDeckAsync(classroomDeck);
        }

        public async Task<List<ClassroomDto>> GetTeacherClassroomsAsync(Guid teacherId)
        {
            var classrooms = await _classroomRepository.GetTeacherClassroomsAsync(teacherId);
            return classrooms.Select(MapToDto).ToList();
        }

        public async Task<List<ClassroomDto>> GetStudentClassroomsAsync(Guid studentId)
        {
            var classrooms = await _classroomRepository.GetStudentClassroomsAsync(studentId);
            return classrooms.Select(MapToDto).ToList();
        }

        private static ClassroomDto MapToDto(Classroom classroom)
        {
            return new ClassroomDto
            {
                Id = classroom.Id,
                Name = classroom.Name,
                Description = classroom.Description,
                JoinCode = classroom.JoinCode,
                StudentsCount = classroom.Students?.Count ?? 0,
                DecksCount = classroom.Decks?.Count ?? 0
            };
        }

        public async Task<List<DeckDto>> GetClassroomDecksAsync(Guid classroomId, Guid userId)
        {
            var classroom = await _classroomRepository.GetByIdWithDecksAsync(classroomId);

            if (classroom == null)
                throw new NotFoundException("Classroom", classroomId.ToString());

            bool isTeacher = classroom.TeacherId == userId;
            bool isStudent = classroom.Students != null && classroom.Students.Any(s => s.StudentId == userId);

            if (!isTeacher && !isStudent)
                throw new UnauthorizedAccessException("You do not have access to this classroom.");

            var sharedDeckIds = classroom.Decks?.Select(cd => cd.DeckId).ToList() ?? new List<Guid>();

            var sharedDecks = await _deckRepository.GetDecksByIdsAsync(sharedDeckIds);

            var ownedByClassDecks = await _deckRepository.GetDecksByOwnerClassroomIdAsync(classroomId);

            var resultDecks = sharedDecks
                .UnionBy(ownedByClassDecks, d => d.Id)
                .Select(MapToDeckDto)
                .ToList();

            return resultDecks;
        }

        private static DeckDto MapToDeckDto(Deck deck)
        {
            return new DeckDto
            {
                Id = deck.Id,
                Title = deck.Title,
                Description = deck.Description ?? string.Empty,
                ShareCode = deck.ShareCode,
                CreatedAt = deck.CreatedAt,
                TargetLanguage = deck.TargetLanguage,
                NativeLanguage = deck.NativeLanguage,
                ProficiencyLevel = deck.ProficiencyLevel,
                Tone = deck.Tone,
                DailyNewCardsLimit = deck.DailyNewCardsLimit,
                DailyReviewLimit = deck.DailyReviewLimit
            };
        }

        public async Task LeaveClassroomAsync(Guid classroomId, Guid studentId)
        {
            var studentClassrooms = await _classroomRepository.GetStudentClassroomsAsync(studentId);
            if (!studentClassrooms.Any(c => c.Id == classroomId))
                throw new InvalidOperationException("You are not enrolled in this class.");

            await _classroomRepository.RemoveStudentAsync(classroomId, studentId);
        }

        public async Task RemoveDeckFromClassroomAsync(Guid classroomId, Guid deckId, Guid teacherId)
        {
            var classroom = await _classroomRepository.GetByIdAsync(classroomId);
            if (classroom == null || classroom.TeacherId != teacherId)
                throw new UnauthorizedAccessException("You do not have access to this class.");

            bool isDeckAttached = await _classroomRepository.IsDeckInClassroomAsync(classroomId, deckId);
            if (!isDeckAttached)
                throw new InvalidOperationException("This deck is not attached to the class.");

            await _classroomRepository.RemoveDeckWithProgressAsync(classroomId, deckId);
        }

        public async Task DeleteClassroomAsync(Guid classroomId, Guid teacherId)
        {
            var classroom = await _classroomRepository.GetByIdWithDecksAsync(classroomId);
            if (classroom == null || classroom.TeacherId != teacherId)
                throw new UnauthorizedAccessException("You do not have access to delete this class.");

            await _classroomRepository.DeleteAsync(classroom);
        }
        public async Task CreateHomeworkAsync(Guid classroomId, string text, Guid teacherId)
        {
            var classroom = await _classroomRepository.GetByIdAsync(classroomId);
            if (classroom == null || classroom.TeacherId != teacherId)
                throw new UnauthorizedAccessException("You are not the teacher of this class.");

            var studentIds = await _classroomRepository.GetStudentIdsByClassroomAsync(classroomId);

            if (!studentIds.Any()) return;

            var groupTaskId = Guid.NewGuid();
            var homeworkRecords = studentIds.Select(studentId => new StudentHomework
            {
                ClassroomId = classroomId,
                StudentId = studentId,
                GroupTaskId = groupTaskId,
                TaskText = text,
                IsCompleted = false
            }).ToList();

            await _classroomRepository.AddStudentHomeworksAsync(homeworkRecords);
        }

        public async Task<List<HomeworkSummaryDto>> GetHomeworkForTeacherAsync(Guid classroomId, Guid teacherId)
        {
            var classroom = await _classroomRepository.GetByIdAsync(classroomId);
            if (classroom == null || classroom.TeacherId != teacherId)
                throw new UnauthorizedAccessException("You are not the teacher of this class.");

            return (await _classroomRepository.GetHomeworkSummaryForTeacherAsync(classroomId)).ToList();
        }

        public async Task<List<StudentHomeworkDto>> GetHomeworkForStudentAsync(Guid classroomId, Guid studentId)
        {
            var homeworks = await _classroomRepository.GetStudentHomeworksAsync(classroomId, studentId);

            return homeworks.Select(h => new StudentHomeworkDto
            {
                Id = h.Id,
                GroupTaskId = h.GroupTaskId,
                TaskText = h.TaskText,
                IsCompleted = h.IsCompleted,
                CreatedAt = h.CreatedAt
            }).ToList();
        }

        public async Task DeleteHomeworkAsync(Guid groupTaskId, Guid teacherId)
        {
            await _classroomRepository.DeleteHomeworkByGroupTaskIdAsync(groupTaskId);
        }

        public async Task ToggleHomeworkAsync(Guid groupTaskId, Guid studentId)
        {
            var homework = await _classroomRepository.GetHomeworkByGroupAndStudentAsync(groupTaskId, studentId);
            if (homework == null)
                throw new InvalidOperationException("Homework not found.");

            homework.IsCompleted = !homework.IsCompleted;
            await _classroomRepository.UpdateHomeworkAsync(homework);
        }
    }
}