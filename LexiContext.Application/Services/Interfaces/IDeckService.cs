using LexiContext.Application.DTOs.Decks;

namespace LexiContext.Application.Services.Interfaces
{
    public interface IDeckService
    {
        Task<DeckDto> CreateDeckAsync(CreateDeckDto dto, Guid userId);
        Task<DeckDto> GetDeckByIdAsync(Guid id, Guid userId);
        Task<List<DeckDto>> GetAllDecksAsync(Guid userId);
        Task<DeckDto> UpdateDeckAsync(Guid id, UpdateDeckDto dto, Guid userId);
        Task DeleteDeckAsync(Guid id, Guid userId);
    }
}