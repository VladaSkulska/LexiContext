using LexiContext.Domain.Entities;
using LexiContext.Application.Interfaces;
using LexiContext.Infrastructure.Persistence;

namespace LexiContext.Infrastructure.Repositories
{
    public class DeckRepository : IDeckRepository
    {
        private readonly AppDbContext _context;
        public DeckRepository(AppDbContext context)
        {
            _context = context;
        }
        public async Task<Guid> CreateAsync(Deck deck)
        {
            await _context.Decks.AddAsync(deck);

            await _context.SaveChangesAsync();

            return deck.Id;
        }
    }
}
