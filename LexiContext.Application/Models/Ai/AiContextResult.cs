namespace LexiContext.Application.Models.Ai
{
    public record AiContextResult
    (
        string GeneratedContext,
        string ContextTranslation,
        string ContextReading,
        string WordTranslation,
        string CorrectedWord
    );
}
