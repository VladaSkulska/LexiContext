namespace LexiContext.Application.DTOs.Decks
{
    public class UpdateDeckDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public bool IsPublic { get; set; }
    }
}
