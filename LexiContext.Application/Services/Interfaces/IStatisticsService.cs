using LexiContext.Application.DTOs.Statistics;
namespace LexiContext.Application.Services.Interfaces
{
    public interface IStatisticsService
    {
        Task<MasteryLevelDto> GetMasteryLevelAsync(Guid userId, Guid? deckId = null);
        Task<List<ForecastDto>> GetFutureForecastAsync(Guid userId);
        Task<List<ActivityDto>> GetActivityHistoryAsync(Guid userId);
        Task<int> GetClassroomAverageProgressAsync(Guid classroomId);
    }
}
