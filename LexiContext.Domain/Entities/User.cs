using LexiContext.Domain.Entities.Common;

namespace LexiContext.Domain.Entities
{
    public class User : BaseEntity
    {
        public string Username { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string PasswordHash {  get; set; } = string.Empty;

        // gamification
        public int CurrentStreak { get; set; } 
        public DateTime? LastStudyDate {  get; set; }
        
        // connections
        // settings 1 to 1
        public UserSettings? Settings { get; set; }
        // decks 1 to many
        public ICollection<Deck> CreatedDecks { get; set; } = new List<Deck>();
        // progress 1 to many
        public ICollection<UserCardProgress> Progress { get; set; } = new List<UserCardProgress>();
    }
}
