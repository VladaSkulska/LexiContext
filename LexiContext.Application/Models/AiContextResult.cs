namespace LexiContext.Application.Models
{
    public record AiContextResult
    (
        string GeneratedContext,
        string ContextTranslation,
        string ContextReading,
        string WordTranslation
    );
}
