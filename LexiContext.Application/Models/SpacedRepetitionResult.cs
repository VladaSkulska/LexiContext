namespace LexiContext.Application.Models
{
    public record SpacedRepetitionResult
    (
        int IntervalDays,
        double EaseFactor,
        int Repetitions,
        DateTime NextReviewAt
    );
}
