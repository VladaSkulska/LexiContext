using LexiContext.Application.DTOs.Stories;
namespace LexiContext.Application.Services.Interfaces
{
    public interface IStoryService
    {
        Task<StoryDto> GenerateStoryAsync(GenerateStoryDto dto, Guid userId);
        Task<List<StoryDto>> GetUserStoriesAsync(Guid userId);
        Task<StoryDto> GetStoryByIdAsync(Guid id, Guid userId);
        Task DeleteStoryAsync(Guid id, Guid userId);
    }
}
