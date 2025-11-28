using EventSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSystemAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            modelBuilder.Entity<Registration>()
                .HasIndex(r => new { r.UserId, r.EventId }).IsUnique();

            modelBuilder.Entity<Event>()
                .HasMany(e => e.Tickets)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.Ticket)
                .WithMany()
                .HasForeignKey(r => r.TicketId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<Registration>()
                .HasOne(r => r.User)
                .WithMany(u => u.Registrations)
                .HasForeignKey(r => r.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            modelBuilder.Entity<User>().HasData(
                new User
                {
                    Id = 1,
                    FullName = "System Administrator",
                    Email = "admin@system.com",
                    // Mật khẩu là "M77830311" đã được mã hóa
                    PasswordHash = BCrypt.Net.BCrypt.HashPassword("M77830311"),
                    Role = "Admin"
                }
            );
        }
    }
}