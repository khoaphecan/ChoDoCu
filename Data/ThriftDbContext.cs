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
        public DbSet<Order> Orders => Set<Order>();
        public DbSet<OrderItem> OrderItems => Set<OrderItem>();
        public DbSet<Favorite> Favorites => Set<Favorite>();
        public DbSet<ChatMessage> ChatMessages => Set<ChatMessage>();
        public DbSet<Conversation> Conversations => Set<Conversation>();

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // ==========================================
            // 1. CẤU HÌNH ENTITY CATEGORY
            // ==========================================
            modelBuilder.Entity<Category>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Name).IsRequired().HasMaxLength(100);
                entity.Property(c => c.Icon).IsRequired().HasMaxLength(50);
                entity.Property(c => c.Description).HasMaxLength(255);
            });

            // ==========================================
            // 2. CẤU HÌNH ENTITY USER
            // ==========================================
            modelBuilder.Entity<User>(entity =>
            {
                entity.HasKey(u => u.Id);
                entity.Property(u => u.FullName).IsRequired().HasMaxLength(100);
                entity.Property(u => u.Email).IsRequired().HasMaxLength(150);
                entity.HasIndex(u => u.Email).IsUnique();
                entity.Property(u => u.PasswordHash).IsRequired();
                entity.Property(u => u.Phone).HasMaxLength(20);
                entity.Property(u => u.Address).HasMaxLength(255);
                entity.Property(u => u.Avatar);
                entity.Property(u => u.Bio).HasMaxLength(500);
                entity.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("customer");
                entity.Property(u => u.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
            });

            // ==========================================
            // 3. CẤU HÌNH ENTITY PRODUCT
            // ==========================================
            modelBuilder.Entity<Product>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
                entity.Property(p => p.CategoryName).HasMaxLength(100);
                entity.Property(p => p.SellerName).HasMaxLength(100);
                entity.Property(p => p.SellerPhone).HasMaxLength(20);
                entity.Property(p => p.SellerEmail).HasMaxLength(150);
                entity.Property(p => p.SellerAddress).HasMaxLength(255);
                entity.Property(p => p.SellerAvatar).HasMaxLength(255);
                entity.Property(p => p.Status).HasMaxLength(30).HasDefaultValue("approved");
                entity.Property(p => p.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                // Cấu hình kiểu dữ liệu decimal(18,2) cho giá tiền
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)")
                      .HasPrecision(18, 2);

                entity.Property(p => p.OriginalPrice)
                      .HasColumnType("decimal(18,2)")
                      .HasPrecision(18, 2);

                // Khóa ngoại CategoryId với DeleteBehavior.Restrict để tránh cascade delete ngoài ý muốn
                entity.HasOne(p => p.Category)
                      .WithMany(c => c.Products)
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Khóa ngoại SellerId với User
                entity.HasOne(p => p.Seller)
                      .WithMany(u => u.Products)
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Global Query Filter: Tự động loại trừ các sản phẩm đã xóa mềm
                entity.HasQueryFilter(p => !p.Deleted);
            });

            // ==========================================
            // 4. CẤU HÌNH ENTITY PENDINGPOST
            // ==========================================
            modelBuilder.Entity<PendingPost>(entity =>
            {
                entity.HasKey(p => p.Id);
                entity.Property(p => p.Name).IsRequired().HasMaxLength(150);
                entity.Property(p => p.CategoryName).HasMaxLength(100);
                entity.Property(p => p.SellerName).IsRequired().HasMaxLength(100);
                entity.Property(p => p.SellerPhone).IsRequired().HasMaxLength(20);
                entity.Property(p => p.SellerEmail).HasMaxLength(150);
                entity.Property(p => p.SellerAddress).IsRequired().HasMaxLength(255);
                entity.Property(p => p.SellerAvatar).HasMaxLength(255);
                entity.Property(p => p.Status).HasMaxLength(30).HasDefaultValue("pending");
                entity.Property(p => p.RejectReason).HasMaxLength(255);
                entity.Property(p => p.Deleted).HasDefaultValue(false);
                entity.Property(p => p.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(p => p.UpdatedAt).HasColumnType("datetime2");

                // Cấu hình kiểu dữ liệu decimal(18,2) cho giá tiền
                entity.Property(p => p.Price)
                      .HasColumnType("decimal(18,2)")
                      .HasPrecision(18, 2);

                entity.Property(p => p.OriginalPrice)
                      .HasColumnType("decimal(18,2)")
                      .HasPrecision(18, 2);

                // Khóa ngoại CategoryId với DeleteBehavior.Restrict
                entity.HasOne(p => p.Category)
                      .WithMany()
                      .HasForeignKey(p => p.CategoryId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Khóa ngoại SellerId với User
                entity.HasOne(p => p.Seller)
                      .WithMany()
                      .HasForeignKey(p => p.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);

                // Global Query Filter: Tự động loại trừ các bài đăng đã xóa mềm
                entity.HasQueryFilter(p => p.Status != "deleted");
            });

            // ==========================================
            // 5. CẤU HÌNH ENTITY ORDER & ORDERITEM
            // ==========================================
            modelBuilder.Entity<Order>(entity =>
            {
                entity.HasKey(o => o.Id);
                entity.Property(o => o.CustName).IsRequired().HasMaxLength(100);
                entity.Property(o => o.CustPhone).IsRequired().HasMaxLength(20);
                entity.Property(o => o.CustEmail).HasMaxLength(150);
                entity.Property(o => o.CustAddress).IsRequired().HasMaxLength(255);
                entity.Property(o => o.PaymentMethod).HasMaxLength(100).HasDefaultValue("COD - Thanh toán khi nhận hàng");
                entity.Property(o => o.Notes).HasMaxLength(500);
                entity.Property(o => o.Status).HasMaxLength(30).HasDefaultValue("pending");
                entity.Property(o => o.OrderDate).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(o => o.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                // Cấu hình kiểu dữ liệu decimal(18,2) cho TotalAmount
                entity.Property(o => o.TotalAmount)
                      .HasColumnType("decimal(18,2)")
                      .HasPrecision(18, 2);

                // Khóa ngoại UserId (nullable) liên kết với User
                entity.HasOne(o => o.User)
                      .WithMany(u => u.Orders)
                      .HasForeignKey(o => o.UserId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                // Quan hệ 1 - N với OrderItem: Xóa đơn hàng xóa luôn các OrderItem con
                entity.HasMany(o => o.OrderItems)
                      .WithOne(oi => oi.Order)
                      .HasForeignKey(oi => oi.OrderId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<OrderItem>(entity =>
            {
                entity.HasKey(oi => oi.Id);
                entity.Property(oi => oi.ProductName).IsRequired().HasMaxLength(150);
                entity.Property(oi => oi.Image).HasMaxLength(255);

                // Cấu hình kiểu dữ liệu decimal(18,2) cho Price & Subtotal
                entity.Property(oi => oi.Price)
                      .HasColumnType("decimal(18,2)")
                      .HasPrecision(18, 2);

                entity.Property(oi => oi.Subtotal)
                      .HasColumnType("decimal(18,2)")
                      .HasPrecision(18, 2);

                // Khóa ngoại ProductId
                entity.HasOne(oi => oi.Product)
                      .WithMany()
                      .HasForeignKey(oi => oi.ProductId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            // ==========================================
            // 6. CẤU HÌNH FAVORITE & CHATMESSAGE
            // ==========================================
            modelBuilder.Entity<Favorite>(entity =>
            {
                entity.HasKey(f => f.Id);
                entity.Property(f => f.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(f => f.Product)
                      .WithMany()
                      .HasForeignKey(f => f.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.User)
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            // ==========================================
            // 7. CẤU HÌNH CONVERSATION & CHATMESSAGE
            // ==========================================
            modelBuilder.Entity<Conversation>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.BuyerClearedAt).HasColumnType("datetime2");
                entity.Property(c => c.SellerClearedAt).HasColumnType("datetime2");
                entity.Property(c => c.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.UpdatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");

                entity.HasOne(c => c.Product)
                      .WithMany()
                      .HasForeignKey(c => c.ProductId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);

                entity.HasOne(c => c.Buyer)
                      .WithMany()
                      .HasForeignKey(c => c.BuyerId)
                      .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(c => c.Seller)
                      .WithMany()
                      .HasForeignKey(c => c.SellerId)
                      .OnDelete(DeleteBehavior.Restrict);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.SentAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.CreatedAt).HasColumnType("datetime2").HasDefaultValueSql("GETDATE()");
                entity.Property(c => c.Message).IsRequired(false).HasColumnType("nvarchar(max)");
                entity.Property(c => c.ImageUrl).IsRequired(false).HasColumnType("nvarchar(max)");
                entity.Property(c => c.SenderAvatar).IsRequired(false).HasColumnType("nvarchar(max)");
                entity.Property(c => c.ReceiverAvatar).IsRequired(false).HasColumnType("nvarchar(max)");

                entity.HasOne(c => c.Conversation)
                      .WithMany(cv => cv.Messages)
                      .HasForeignKey(c => c.ConversationId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(c => c.Product)
                      .WithMany()
                      .HasForeignKey(c => c.ProductId)
                      .IsRequired(false)
                      .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
