using LexiContext.Application.DTOs.Decks;

namespace LexiContext.Application.Services.Interfaces
{
    public interface IDeckService
    {
        public Task<DeckDto> CreateDeckAsync(CreateDeckDto dto);
        public Task<DeckDto> GetDeckByIdAsync(Guid id);
        public Task<List<DeckDto>> GetAllDecksAsync();
    }
}
