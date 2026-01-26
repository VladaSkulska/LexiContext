using LexiContext.Domain.Entities;

namespace LexiContext.Application.Interfaces
{
    public interface IDeckRepository
    {
        Task<Guid> CreateAsync(Deck deck);
    }
}
