using LexiContext.Application.Interfaces.Repos;
using LexiContext.Domain.Entities;
using LexiContext.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexiContext.Infrastructure.Repositories
{
    public class UserSettingsRepository : IUserSettingsRepository
    {
        private readonly AppDbContext _context;

        public UserSettingsRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<UserSettings?> GetByUserIdAsync(Guid userId)
        {
            return await _context.UserSettings
                .FirstOrDefaultAsync(s => s.UserId == userId);
        }

        public async Task<Guid> CreateAsync(UserSettings settings)
        {
            await _context.UserSettings.AddAsync(settings);
            await _context.SaveChangesAsync();
            return settings.Id;
        }

        public async Task UpdateAsync(UserSettings settings)
        {
            _context.UserSettings.Update(settings);
            await _context.SaveChangesAsync();
        }
    }
}
