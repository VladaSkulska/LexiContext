namespace LexiContext.Application.DTOs.Statistics
{
    public class DailyActivity
    {
        public Guid Id { get; set; }
        public Guid UserId { get; set; }
        public DateTime Date { get; set; }
        public int CardsStudied { get; set; }
    }
}
