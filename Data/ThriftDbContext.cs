using Microsoft.EntityFrameworkCore;
using ThriftShop.Models;

namespace ThriftShop.Data
{
    public class ThriftDbContext : DbContext
    {
        public ThriftDbContext(DbContextOptions<ThriftDbContext> options) : base(options)
        {
        }

        public DbSet<Category> Categories => Set<Category>();
        public DbSet<Product> Products => Set<Product>();
        public DbSet<PendingPost> PendingPosts => Set<PendingPost>();
        public DbSet<User> Users => Set<User>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Cấu hình quan hệ và kiểu dữ liệu
            modelBuilder.Entity<Product>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Product>()
                .Property(p => p.OriginalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PendingPost>()
                .Property(p => p.Price)
                .HasPrecision(18, 2);

            modelBuilder.Entity<PendingPost>()
                .Property(p => p.OriginalPrice)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.Subtotal)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.ShippingFee)
                .HasPrecision(18, 2);

            modelBuilder.Entity<Order>()
                .Property(o => o.TotalAmount)
                .HasPrecision(18, 2);

            modelBuilder.Entity<OrderItem>()
                .Property(oi => oi.Price)
                .HasPrecision(18, 2);
        }
    }
}
