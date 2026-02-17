using LexiContext.Application.Interfaces;
using LexiContext.Domain.Entities;
using LexiContext.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace LexiContext.Infrastructure.Repositories
{
    public class CardRepository : ICardRepository
    {
        private readonly AppDbContext _context;

        public CardRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Guid> CreateAsync(Card card)
        {
            await _context.Cards.AddAsync(card);

            await _context.SaveChangesAsync();

            return card.Id;
        }

        public async Task<List<Card>> GetByDeckIdAsync(Guid deckId)
        {
            return await _context.Cards.Where(c => c.DeckId == deckId)
                .AsNoTracking().ToListAsync();
        }

        public async Task<Card?> GetByIdAsync(Guid id)
        {
            return await _context.Cards.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task UpdateAsync(Card card)
        {
            _context.Cards.Update(card);

            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Card card)
        {
            _context.Cards.Remove(card);

            await _context.SaveChangesAsync();
        }
    }
}
