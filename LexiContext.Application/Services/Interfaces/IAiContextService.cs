using LexiContext.Application.Models;
using LexiContext.Domain.Enums;

namespace LexiContext.Application.Services.Interfaces
{
    public interface IAiContextService
    {
        Task<AiContextResult> GetAiContextAsync(
            string word,
            LearningLanguage learningLanguage,
            LearningLanguage nativeLanguage,
            ProficiencyLevel level,
            string deckContext,
            AiTone tone);

        Task<string> TranslateWordAsync(string word, 
            LearningLanguage learningLanguage, 
            LearningLanguage nativeLanguage);

        Task<AiContextResult> SimplifyContextAsync(
            string word,
            string originalContext,
            LearningLanguage learningLanguage,
            LearningLanguage nativeLanguage,
            ProficiencyLevel simplerLevel);
    }
}
