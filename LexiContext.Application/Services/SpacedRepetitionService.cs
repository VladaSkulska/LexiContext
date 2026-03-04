using LexiContext.Application.Models;
using LexiContext.Application.Services.Interfaces;
using LexiContext.Domain.Enums;

namespace LexiContext.Application.Services
{
    public class SpacedRepetitionService : ISpacedRepetitionService
    {
        private const int MaxIntervalDays = 3650;
        private const double MinEaseFactor = 1.3;
        private const double MaxEaseFactor = 3.0;

        public SpacedRepetitionResult CalculateNextReview(
            int currentRepetitions,
            int currentIntervalDays,
            double currentEaseFactor,
            RecallQuality recallQuality)
        {
            int qualityScore = MapToSm2QualityScore(recallQuality);

            double newEaseFactor = CalculateEaseFactor(currentEaseFactor, qualityScore);

            var (newRepetitions, newIntervalDays) = CalculateRepetitionsAndInterval(
                currentRepetitions, currentIntervalDays, newEaseFactor, recallQuality);

            DateTime newReviewDate = DateTime.UtcNow.AddDays(newIntervalDays);

            return new SpacedRepetitionResult(
                newIntervalDays,
                newEaseFactor,
                newRepetitions,
                newReviewDate
            );
        }
        private static int MapToSm2QualityScore(RecallQuality quality)
        {
            return quality switch
            {
                RecallQuality.Fail => 1,
                RecallQuality.Hard => 3,
                RecallQuality.Easy => 5,
                _ => 3
            };
        }

        private static double CalculateEaseFactor(double currentEaseFactor, int qualityScore)
        {
            double newEaseFactor = currentEaseFactor + (0.1 - (5 - qualityScore) * (0.08 + (5 - qualityScore) * 0.02));

            return Math.Clamp(newEaseFactor, MinEaseFactor, MaxEaseFactor);
        }

        private static (int Repetitions, int IntervalDays) CalculateRepetitionsAndInterval(
            int currentRepetitions,
            int currentIntervalDays,
            double newEaseFactor,
            RecallQuality quality)
        {
            if (quality == RecallQuality.Fail)
            {
                return (0, 1);
            }

            int newRepetitions = currentRepetitions + 1;
            int newIntervalDays;

            if (newRepetitions == 1)
            {
                newIntervalDays = 1;
            }
            else if (newRepetitions == 2)
            {
                newIntervalDays = 3;
            }
            else
            {
                if (quality == RecallQuality.Hard)
                {
                    newIntervalDays = (int)Math.Round(currentIntervalDays * 1.2);
                }
                else
                {
                    newIntervalDays = (int)Math.Round(currentIntervalDays * newEaseFactor);
                }
            }

            if (newIntervalDays <= currentIntervalDays)
            {
                newIntervalDays = currentIntervalDays + 1;
            }

            newIntervalDays = Math.Min(newIntervalDays, MaxIntervalDays);

            return (newRepetitions, newIntervalDays);
        }
    }
}