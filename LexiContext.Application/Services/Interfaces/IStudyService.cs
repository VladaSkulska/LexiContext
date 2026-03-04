using LexiContext.Application.DTOs.Cards.Study;

namespace LexiContext.Application.Services.Interfaces
{
    public interface IStudyService
    {
        Task<List<DueCardDto>> GetDueCardsAsync(Guid deckId, Guid userId);
        Task ProcessReviewAsync(Guid userId, ReviewCardDto dto);
    }
}
