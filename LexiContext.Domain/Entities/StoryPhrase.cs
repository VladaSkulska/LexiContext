using LexiContext.Domain.Entities.Common;
namespace LexiContext.Domain.Entities
{
    public class StoryPhrase : BaseEntity
    {
        public Guid StoryId { get; set; }
        public Story? Story { get; set; }

        public string Phrase { get; set; } = string.Empty;
        public string Translation { get; set; } = string.Empty;
        public string Reading { get; set; } = string.Empty;
    }
}
