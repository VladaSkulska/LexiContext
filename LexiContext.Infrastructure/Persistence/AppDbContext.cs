using LexiContext.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace LexiContext.Infrastructure.Persistence
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {

        }

        public DbSet<User> Users { get; set; }
        public DbSet<Deck> Decks { get; set; }
        public DbSet<Card> Cards { get; set; }
        public DbSet<UserSettings> UserSettings { get; set; }
        public DbSet<UserCardProgress> UserCardProgresses { get; set; }
    }
}
