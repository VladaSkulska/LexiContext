using LexiContext.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LexiContext.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            var testUserId = Guid.Parse("00000000-0000-0000-0000-000000000001");

            modelBuilder.Entity<User>().HasData(new User
            {
                Id = testUserId,
                Username = "DevStudent_KPI",
                Email = "student@fpm.kpi.ua",
                AuthProvider = "System",
                ExternalProviderId = "sys-001",
                CurrentStreak = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<User>()
                .HasOne(u => u.Settings)
                .WithOne(s => s.User)
                .HasForeignKey<UserSettings>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCardProgress>()
                .HasOne(up => up.User)
                .WithMany(u => u.Progress)
                .HasForeignKey(up => up.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCardProgress>()
                .HasOne(up => up.Card)
                .WithMany()
                .HasForeignKey(up => up.CardId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserActivity>()
                .HasOne(ua => ua.User)
                .WithMany()
                .HasForeignKey(ua => ua.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Deck>()
                .HasOne(d => d.Creater)
                .WithMany(u => u.CreatedDecks)
                .HasForeignKey(d => d.CreatedId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Card>()
                .HasOne(c => c.Deck)
                .WithMany(d => d.Cards)
                .HasForeignKey(c => c.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Story>()
                .HasOne(s => s.Deck)
                .WithMany()
                .HasForeignKey(s => s.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoryPhrase>()
                .HasOne(p => p.Story)
                .WithMany(s => s.Phrases)
                .HasForeignKey(p => p.StoryId)
                .OnDelete(DeleteBehavior.Cascade);
        }

        public DbSet<User> Users => Set<User>();
        public DbSet<Deck> Decks => Set<Deck>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<UserSettings> UserSettings => Set<UserSettings>();
        public DbSet<UserCardProgress> UserCardProgresses => Set<UserCardProgress>();
        public DbSet<Story> Stories => Set<Story>();
        public DbSet<StoryPhrase> StoryPhrases => Set<StoryPhrase>();
        public DbSet<UserActivity> UserActivities => Set<UserActivity>();
    }
}