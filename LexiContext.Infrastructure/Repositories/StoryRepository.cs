using LexiContext.Application.Interfaces.Repos;
using LexiContext.Domain.Entities;
using LexiContext.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace LexiContext.Infrastructure.Repositories
{
    public class StoryRepository : IStoryRepository
    {
        private readonly AppDbContext _context;

        public StoryRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Story story)
        {
            await _context.Stories.AddAsync(story);
            await _context.SaveChangesAsync();
            return story.Id;
        }

        public async Task<Story?> GetByIdAsync(Guid id)
        {
            return await _context.Stories
                .Include(s => s.Phrases)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<List<Story>> GetByUserIdAsync(Guid userId)
        {
            return await _context.Stories
                .AsNoTracking()
                .Where(s => s.CreatedId == userId)
                .OrderByDescending(s => s.CreatedAt)
                .ToListAsync();
        }

        public async Task<int> CountStoriesInLastWeekAsync(Guid userId)
        {
            var lastWeek = DateTime.UtcNow.AddDays(-7);
            return await _context.Stories
                .CountAsync(s => s.CreatedId == userId && s.CreatedAt >= lastWeek);
        }

        public async Task DeleteAsync(Story story)
        {
            _context.Stories.Remove(story);
            await _context.SaveChangesAsync();
        }
    }
}
