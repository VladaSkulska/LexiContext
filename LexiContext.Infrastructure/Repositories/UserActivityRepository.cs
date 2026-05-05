using LexiContext.Application.Interfaces.Repos;
using LexiContext.Domain.Entities;
using LexiContext.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
namespace LexiContext.Infrastructure.Repositories
{
    public class UserActivityRepository : IUserActivityRepository
    {
        private readonly AppDbContext _context;

        public UserActivityRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserActivity?> GetByDateAsync(Guid userId, DateTime date)
        {
            return await _context.UserActivities
                .FirstOrDefaultAsync(a => a.UserId == userId && a.Date == date);
        }

        public async Task CreateAsync(UserActivity activity)
        {
            await _context.UserActivities.AddAsync(activity);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserActivity activity)
        {
            _context.UserActivities.Update(activity);
            await _context.SaveChangesAsync();
        }
    }
}
