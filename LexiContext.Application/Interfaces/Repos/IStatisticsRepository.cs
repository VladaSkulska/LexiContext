using LexiContext.Application.DTOs.Statistics;
namespace LexiContext.Application.Interfaces.Repos
{
    public interface IStatisticsRepository
    {
        Task<MasteryLevelDto> GetMasteryLevelAsync(Guid userId, Guid? deckId = null);
        Task<List<ForecastDto>> GetFutureForecastAsync(Guid userId, int days = 7);
        Task<List<ActivityDto>> GetActivityHistoryAsync(Guid userId);
    }
}
