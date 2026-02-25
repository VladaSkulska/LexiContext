using LexiContext.Application.Models;
using LexiContext.Domain.Enums;

namespace LexiContext.Application.Services.Interfaces
{
    public interface ISpacedRepetitionService
    {
        SpacedRepetitionResult CalculateNextReview(int currentRepetitions, 
            int currentIntervalDays, double currentEaseFactor, RecallQuality recallQuality);
    }
}
