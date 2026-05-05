namespace LexiContext.Application.DTOs.Statistics
{
    public class MasteryLevelDto
    {
        public int NewCards { get; set; }
        public int LearningCards { get; set; }
        public int MasteredCards { get; set; }

        public int TotalCards => NewCards + LearningCards + MasteredCards;
    }
}
