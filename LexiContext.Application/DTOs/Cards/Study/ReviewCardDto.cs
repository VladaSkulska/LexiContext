using LexiContext.Domain.Enums;

namespace LexiContext.Application.DTOs.Cards.Study
{
    public class ReviewCardDto
    {
        public Guid CardId { get; set; }
        public RecallQuality Quality { get; set; }
    }
}
