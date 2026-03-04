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
        }
        public DbSet<User> Users => Set<User>();
        public DbSet<Deck> Decks => Set<Deck>();
        public DbSet<Card> Cards => Set<Card>();
        public DbSet<UserSettings> UserSettings => Set<UserSettings>();
        public DbSet<UserCardProgress> UserCardProgresses => Set<UserCardProgress>();
    }
}
