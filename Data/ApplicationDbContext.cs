using EventSystemAPI.Models;
using Microsoft.EntityFrameworkCore;

namespace EventSystemAPI.Data
{
    public class ApplicationDbContext : DbContext
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

        // Khai báo các bảng sẽ tạo trong SQL Server
        public DbSet<User> Users { get; set; }
        public DbSet<Event> Events { get; set; }
        public DbSet<Ticket> Tickets { get; set; }
        public DbSet<Registration> Registrations { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // [RÀNG BUỘC] Email là duy nhất, không ai được trùng
            modelBuilder.Entity<User>().HasIndex(u => u.Email).IsUnique();

            // [LOGIC NGHIỆP VỤ] Chống Spam: 
            // Tạo khóa phức hợp (UserId + EventId).
            // Nghĩa là 1 User chỉ được xuất hiện 1 lần trong danh sách đăng ký của 1 Event.
            modelBuilder.Entity<Registration>()
                .HasIndex(r => new { r.UserId, r.EventId }).IsUnique();

            // [TỰ ĐỘNG] Nếu xóa Event -> Xóa luôn các Ticket của Event đó
            modelBuilder.Entity<Event>()
                .HasMany(e => e.Tickets)
                .WithOne(t => t.Event)
                .HasForeignKey(t => t.EventId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}