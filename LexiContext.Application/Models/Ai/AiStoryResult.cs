using LexiContext.Domain.Entities;
namespace LexiContext.Application.Models.Ai
{
    public class AiStoryResult
    {
        public string Title { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
        public List<AiStoryPhrase> Vocabulary { get; set; } = new();
    }
}
