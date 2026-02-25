using LexiContext.Application.Models;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Enums;

namespace LexiContext.Application.Services
{
    public class SpacedRepetitionService : ISpacedRepetitionService
    {
        private const int MaxIntervalDays = 3650;
        private const double MinEaseFactor = 1.3;
        public SpacedRepetitionResult CalculateNextReview(int currentRepetitions, 
            int currentIntervalDays, 
            double currentEaseFactor, 
            RecallQuality recallQuality)
        {
            int qualityScore = (int)recallQuality;

            double newEaseFactor = currentEaseFactor + (0.1 - (5 - qualityScore) * (0.08 + (5 - qualityScore) * 0.02));

            newEaseFactor = Math.Max(newEaseFactor, MinEaseFactor);

            int newRepetitions;
            int newIntervalDays;

            if(recallQuality == RecallQuality.Fail)
            {
                newRepetitions = 0;
                newIntervalDays = 1;
            }
            else
            {
                newRepetitions = currentRepetitions + 1;
                if(newRepetitions == 1)
                {
                    newIntervalDays = 1;
                }
                else if(newRepetitions == 2)
                {
                    newIntervalDays = 3;
                }
                else
                {
                    newIntervalDays = (int)(currentIntervalDays * newEaseFactor);
                    newIntervalDays = Math.Min(newIntervalDays, MaxIntervalDays);
                }
            }

            newIntervalDays = Math.Min(newIntervalDays, MaxIntervalDays);

            DateTime newReviewDate = DateTime.UtcNow.AddDays(newIntervalDays);

            return new SpacedRepetitionResult
            (
                newIntervalDays,
                newEaseFactor,
                newRepetitions,
                newReviewDate
            );
        }
    }
}
