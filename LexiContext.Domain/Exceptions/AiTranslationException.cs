namespace LexiContext.Domain.Exceptions
{
    public class AiTranslationException : Exception
    {
        public AiTranslationException () : base() { }
        public AiTranslationException (string message) : base(message) { }
        public AiTranslationException (string message, Exception innerException) : base(message, innerException) { }
    }
}
