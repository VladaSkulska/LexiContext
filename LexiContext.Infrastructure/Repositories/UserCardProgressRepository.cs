using LexiContext.Application.Interfaces.Repos;
using LexiContext.Domain.Entities;
using LexiContext.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexiContext.Infrastructure.Repositories
{
    public class UserCardProgressRepository : IUserCardProgressRepository
    {
        private readonly AppDbContext _context;

        public UserCardProgressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<List<UserCardProgress>> GetByDeckIdAsync(Guid userId, Guid deckId)
        {
                return await _context.UserCardProgresses
                .Include(p => p.Card)
                .Where(p => p.UserId == userId && p.Card != null && p.Card.DeckId == deckId)
                .ToListAsync();
        }

        public async Task<UserCardProgress?> GetByCardIdAsync(Guid userId, Guid cardId)
        {
            return await _context.UserCardProgresses
                .FirstOrDefaultAsync(p => p.UserId == userId && p.CardId == cardId);
        }

        public async Task CreateAsync(UserCardProgress progress)
        {
            await _context.UserCardProgresses.AddAsync(progress);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(UserCardProgress progress)
        {
            _context.UserCardProgresses.Update(progress);
            await _context.SaveChangesAsync();
        }
    }
}
