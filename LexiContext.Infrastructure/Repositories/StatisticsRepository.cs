using LexiContext.Application.DTOs.Statistics;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexiContext.Infrastructure.Repositories
{
    public class StatisticsRepository : IStatisticsRepository
    {
        private readonly AppDbContext _context;

        public StatisticsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<MasteryLevelDto> GetMasteryLevelAsync(Guid userId, Guid? deckId = null)
        {
            var cardsQuery = _context.Cards.AsQueryable();

            if (deckId.HasValue)
            {
                // ДОДАНО ЗНАК ОКЛИКУ: c.Deck!
                cardsQuery = cardsQuery.Where(c => c.DeckId == deckId.Value && c.Deck!.CreatedId == userId);
            }
            else
            {
                // ДОДАНО ЗНАК ОКЛИКУ: c.Deck!
                cardsQuery = cardsQuery.Where(c => c.Deck!.CreatedId == userId);
            }

            var cardStats = await cardsQuery
                .Select(c => new
                {
                    CardId = c.Id,
                    ProgressInterval = _context.UserCardProgresses
                        .Where(p => p.CardId == c.Id && p.UserId == userId)
                        .Select(p => (int?)p.IntervalDays)
                        .FirstOrDefault()
                })
                .ToListAsync();

            return new MasteryLevelDto
            {
                NewCards = cardStats.Count(c => c.ProgressInterval == null),
                LearningCards = cardStats.Count(c => c.ProgressInterval != null && c.ProgressInterval < 21),
                MasteredCards = cardStats.Count(c => c.ProgressInterval != null && c.ProgressInterval >= 21)
            };
        }
        public async Task<List<ForecastDto>> GetFutureForecastAsync(Guid userId, int days = 7)
        {
            var today = DateTime.UtcNow.Date;
            var endDate = today.AddDays(days);

            var forecast = await _context.UserCardProgresses
                .Where(p => p.UserId == userId && p.NextReviewAt.Date >= today && p.NextReviewAt.Date <= endDate)
                .GroupBy(p => p.NextReviewAt.Date)
                .Select(g => new ForecastDto
                {
                    Date = g.Key,
                    Count = g.Count()
                })
                .OrderBy(g => g.Date)
                .ToListAsync();

            return forecast;
        }

        public async Task<List<ActivityDto>> GetActivityHistoryAsync(Guid userId)
        {
            var oneYearAgo = DateTime.UtcNow.Date.AddYears(-1);

            return await _context.UserActivities
                .Where(a => a.UserId == userId && a.Date >= oneYearAgo)
                .Select(a => new ActivityDto
                {
                    Date = a.Date,
                    Count = a.CardsStudied
                })
                .OrderBy(a => a.Date)
                .ToListAsync();
        }

        public async Task<int> GetClassroomAverageProgressAsync(Guid classroomId)
        {
            var classroom = await _context.Classrooms
                .Include(c => c.Students)
                .Include(c => c.Decks)
                .FirstOrDefaultAsync(c => c.Id == classroomId);

            if (classroom == null || !classroom.Students.Any() || !classroom.Decks.Any())
                return 0;

            var studentIds = classroom.Students.Select(s => s.StudentId).ToList();
            var deckIds = classroom.Decks.Select(d => d.DeckId).ToList();

            var totalCards = await _context.Cards.CountAsync(c => deckIds.Contains(c.DeckId));
            if (totalCards == 0) return 0;

            var totalPossibleProgress = totalCards * studentIds.Count;

            var studiedCardsCount = await _context.UserCardProgresses
                .Where(p => studentIds.Contains(p.UserId) && p.IntervalDays > 0)
                .Where(p => _context.Cards.Any(c => c.Id == p.CardId && deckIds.Contains(c.DeckId)))
                .CountAsync();

            var percentage = (double)studiedCardsCount / totalPossibleProgress * 100;
            return (int)Math.Round(percentage);
        }
    }
}