using LexiContext.Domain.Entities;
using LexiContext.Domain.Entities.Classes;
using LexiContext.Domain.Enums;
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
                Role = UserRole.Teacher,
                CurrentStreak = 0,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });

            modelBuilder.Entity<User>()
                .HasOne(u => u.Settings).WithOne(s => s.User)
                .HasForeignKey<UserSettings>(s => s.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCardProgress>()
                .HasOne(up => up.User).WithMany(u => u.Progress)
                .HasForeignKey(up => up.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserCardProgress>()
                .HasOne(up => up.Card).WithMany()
                .HasForeignKey(up => up.CardId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<UserActivity>()
                .HasOne(ua => ua.User).WithMany()
                .HasForeignKey(ua => ua.UserId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Deck>()
                .HasOne(d => d.Creater).WithMany(u => u.CreatedDecks)
                .HasForeignKey(d => d.CreatedId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Card>()
                .HasOne(c => c.Deck).WithMany(d => d.Cards)
                .HasForeignKey(c => c.DeckId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Story>()
                .HasOne(s => s.Deck).WithMany()
                .HasForeignKey(s => s.DeckId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<StoryPhrase>()
                .HasOne(p => p.Story).WithMany(s => s.Phrases)
                .HasForeignKey(p => p.StoryId).OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Classroom>()
                .HasOne(c => c.Teacher)
                .WithMany() 
                .HasForeignKey(c => c.TeacherId)
                .OnDelete(DeleteBehavior.Restrict);

            modelBuilder.Entity<ClassroomStudent>()
                .HasOne(cs => cs.Classroom)
                .WithMany(c => c.Students)
                .HasForeignKey(cs => cs.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassroomStudent>()
                .HasOne(cs => cs.Student)
                .WithMany()
                .HasForeignKey(cs => cs.StudentId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassroomDeck>()
                .HasOne(cd => cd.Classroom)
                .WithMany(c => c.Decks)
                .HasForeignKey(cd => cd.ClassroomId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<ClassroomDeck>()
                .HasOne(cd => cd.Deck)
                .WithMany()
                .HasForeignKey(cd => cd.DeckId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Deck>()
                .HasOne<Classroom>()
                .WithMany()
                .HasForeignKey(d => d.OwnerClassroomId)
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
        public DbSet<Classroom> Classrooms => Set<Classroom>();
        public DbSet<ClassroomStudent> ClassroomStudents => Set<ClassroomStudent>();
        public DbSet<ClassroomDeck> ClassroomDecks => Set<ClassroomDeck>();
        public DbSet<StudentHomework> StudentHomeworks { get; set; }
    }
}