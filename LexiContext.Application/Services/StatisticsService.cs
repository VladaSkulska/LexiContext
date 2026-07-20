using LexiContext.Application.DTOs.Statistics;
using LexiContext.Application.Interfaces.Repos;
using LexiContext.Application.Services.Interfaces;

namespace LexiContext.Application.Services
{
    public class StatisticsService : IStatisticsService
    {
        private readonly IStatisticsRepository _statisticsRepo;

        public StatisticsService(IStatisticsRepository statisticsRepo)
        {
            _statisticsRepo = statisticsRepo;
        }

        public async Task<MasteryLevelDto> GetMasteryLevelAsync(Guid userId, Guid? deckId = null)
        {
            return await _statisticsRepo.GetMasteryLevelAsync(userId, deckId);
        }
        public async Task<List<ForecastDto>> GetFutureForecastAsync(Guid userId)
        {
            return await _statisticsRepo.GetFutureForecastAsync(userId);
        }
        public async Task<List<ActivityDto>> GetActivityHistoryAsync(Guid userId)
        {
            return await _statisticsRepo.GetActivityHistoryAsync(userId);
        }
        public async Task<int> GetClassroomAverageProgressAsync(Guid classroomId)
        {
            return await _statisticsRepo.GetClassroomAverageProgressAsync(classroomId);
        }
    }
}