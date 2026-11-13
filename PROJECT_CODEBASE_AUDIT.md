# PROJECT CODEBASE AUDIT - THRIFT SHOP C2C

> Tài liệu tổng hợp toàn bộ mã nguồn Backend C# (.NET 8 MVC) và Frontend (Razor Views, JavaScript) của dự án.
> Tổng số lượng file: **56** files.

---

## MỤC LỤC TÀI LIỆU

### Backend - Entry Point & Config
- [Program.cs](#programcs)

### Backend - Database & Context
- [Data/ThriftDbContext.cs](#datathriftdbcontextcs)
- [Data/DbInitializer.cs](#datadbinitializercs)

### Backend - Entities & DTOs (Models)
- [Models/User.cs](#modelsusercs)
- [Models/Category.cs](#modelscategorycs)
- [Models/Product.cs](#modelsproductcs)
- [Models/PendingPost.cs](#modelspendingpostcs)
- [Models/Order.cs](#modelsordercs)
- [Models/ChatMessage.cs](#modelschatmessagecs)
- [Models/Favorite.cs](#modelsfavoritecs)
- [Models/ViewModels.cs](#modelsviewmodelscs)
- [Models/DTOs/BaseApiResponse.cs](#modelsdtosbaseapiresponsecs)
- [Models/DTOs/AuthDtos.cs](#modelsdtosauthdtoscs)
- [Models/DTOs/ProductDtos.cs](#modelsdtosproductdtoscs)
- [Models/DTOs/OrderDtos.cs](#modelsdtosorderdtoscs)
- [Models/DTOs/AdminDtos.cs](#modelsdtosadmindtoscs)

### Backend - Controllers
- [Controllers/HomeController.cs](#controllershomecontrollercs)
- [Controllers/AccountController.cs](#controllersaccountcontrollercs)
- [Controllers/ProductsController.cs](#controllersproductscontrollercs)
- [Controllers/CartController.cs](#controllerscartcontrollercs)
- [Controllers/OrdersController.cs](#controllersorderscontrollercs)
- [Controllers/AdminController.cs](#controllersadmincontrollercs)

### Frontend - Razor Views
- [Views/_ViewImports.cshtml](#viewsviewimportscshtml)
- [Views/_ViewStart.cshtml](#viewsviewstartcshtml)
- [Views/Shared/_Layout.cshtml](#viewssharedlayoutcshtml)
- [Views/Shared/_AdminLayout.cshtml](#viewssharedadminlayoutcshtml)
- [Views/Shared/Error.cshtml](#viewssharederrorcshtml)
- [Views/Home/Index.cshtml](#viewshomeindexcshtml)
- [Views/Home/Barter.cshtml](#viewshomebartercshtml)
- [Views/Home/Repair.cshtml](#viewshomerepaircshtml)
- [Views/Home/Warranty.cshtml](#viewshomewarrantycshtml)
- [Views/Account/Login.cshtml](#viewsaccountlogincshtml)
- [Views/Account/Register.cshtml](#viewsaccountregistercshtml)
- [Views/Account/Profile.cshtml](#viewsaccountprofilecshtml)
- [Views/Account/SellerProfile.cshtml](#viewsaccountsellerprofilecshtml)
- [Views/Products/Index.cshtml](#viewsproductsindexcshtml)
- [Views/Products/Details.cshtml](#viewsproductsdetailscshtml)
- [Views/Products/PostItem.cshtml](#viewsproductspostitemcshtml)
- [Views/Products/MyListings.cshtml](#viewsproductsmylistingscshtml)
- [Views/Products/Favorites.cshtml](#viewsproductsfavoritescshtml)
- [Views/Cart/Index.cshtml](#viewscartindexcshtml)
- [Views/Cart/Checkout.cshtml](#viewscartcheckoutcshtml)
- [Views/Cart/OrderSuccess.cshtml](#viewscartordersuccesscshtml)
- [Views/Orders/Index.cshtml](#viewsordersindexcshtml)
- [Views/Admin/Index.cshtml](#viewsadminindexcshtml)
- [Views/Admin/ApproveProducts.cshtml](#viewsadminapproveproductscshtml)
- [Views/Admin/Products.cshtml](#viewsadminproductscshtml)
- [Views/Admin/Categories.cshtml](#viewsadmincategoriescshtml)
- [Views/Admin/Orders.cshtml](#viewsadminorderscshtml)
- [Views/Admin/Inventory.cshtml](#viewsadmininventorycshtml)
- [Views/Admin/Users.cshtml](#viewsadminuserscshtml)

### Frontend - Client Scripts (wwwroot/js)
- [wwwroot/js/auth.js](#wwwrootjsauthjs)
- [wwwroot/js/data.js](#wwwrootjsdatajs)
- [wwwroot/js/main.js](#wwwrootjsmainjs)
- [wwwroot/js/admin.js](#wwwrootjsadminjs)
- [wwwroot/js/page-transition.js](#wwwrootjspagetransitionjs)

---

# BACKEND - ENTRY POINT & CONFIG

## Program.cs

```csharp
using Microsoft.EntityFrameworkCore;
using ThriftShop.Data;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký HttpContextAccessor & Session (Quản lý đăng nhập & phân quyền Server-side theo giáo trình)
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký DbContext kết nối SQL Server LocalDB (Chuong 08 Entity Framework)
var sqlConnectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? "Server=(localdb)\\mssqllocaldb;Database=ThriftShopDB;Trusted_Connection=True;MultipleActiveResultSets=true;TrustServerCertificate=True";

builder.Services.AddDbContext<ThriftDbContext>(options =>
{
    options.UseSqlServer(sqlConnectionString, sqlOptions =>
    {
        sqlOptions.EnableRetryOnFailure(3, TimeSpan.FromSeconds(5), null);
    });
});

// Đăng ký dịch vụ MVC Controllers và Razor Views (theo giáo trình Chuong 05-07)
builder.Services.AddControllersWithViews()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
        options.JsonSerializerOptions.PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase;
    });

var app = builder.Build();

// Tự động khởi tạo & Seed dữ liệu ban đầu cho Cơ sở dữ liệu (5 Danh mục, Sản phẩm, User)
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ThriftDbContext>();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        Console.WriteLine($"[Lỗi khởi tạo CSDL SQL Server]: {ex.Message}");
    }
}

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/Home/Error");
    app.UseHsts();
}

app.UseStaticFiles();


app.UseRouting();

// Sử dụng Session middleware cho xác thực người dùng
app.UseSession();

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("==================================================================");
Console.WriteLine(" ThriftShop - Website Mua Bán Đồ Cũ Secondhand");
Console.WriteLine(" Backend ASP.NET MVC + Entity Framework + LINQ + SQL Server");
Console.WriteLine("==================================================================");

app.Run();
```

---

# BACKEND - DATABASE & CONTEXT

## Data/ThriftDbContext.cs

```csharp
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
                entity.Property(u => u.Avatar).HasMaxLength(255);
                entity.Property(u => u.Bio).HasMaxLength(500);
                entity.Property(u => u.Role).HasMaxLength(20).HasDefaultValue("customer");
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

                entity.HasOne(f => f.Product)
                      .WithMany()
                      .HasForeignKey(f => f.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);

                entity.HasOne(f => f.User)
                      .WithMany()
                      .HasForeignKey(f => f.UserId)
                      .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<ChatMessage>(entity =>
            {
                entity.HasKey(c => c.Id);
                entity.Property(c => c.Message).IsRequired().HasMaxLength(1000);

                entity.HasOne(c => c.Product)
                      .WithMany()
                      .HasForeignKey(c => c.ProductId)
                      .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
```

---

## Data/DbInitializer.cs

```csharp
using Microsoft.EntityFrameworkCore;
using ThriftShop.Models;

namespace ThriftShop.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ThriftDbContext context)
        {
            context.Database.EnsureCreated();

            // Đảm bảo cập nhật bảng Orders nếu CSDL đã tồn tại từ trước (Giải quyết Issue 5 & 8)
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Orders')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedAt')
                        BEGIN
                            ALTER TABLE Orders ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                        END
                    END
                ");
            }
            catch { }

            // 1. SEED CATEGORIES (5 Danh mục chuẩn theo yêu cầu)
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Đồ điện tử", Icon = "fas fa-laptop", Description = "Điện thoại, tai nghe, máy tính, máy ảnh cũ thanh lý" },
                    new() { Name = "Đồ gia dụng", Icon = "fas fa-couch", Description = "Bàn ghế, quạt, đèn, đồ dùng gia đình thanh lý" },
                    new() { Name = "Văn phòng phẩm", Icon = "fas fa-book", Description = "Sách cũ, giáo trình, truyện tranh, bút vở thanh lý" },
                    new() { Name = "Thời trang", Icon = "fas fa-tshirt", Description = "Quần áo vintage, áo khoác, giày dép, túi xách secondhand" },
                    new() { Name = "Khác", Icon = "fas fa-shapes", Description = "Nhạc cụ, đồ decor, phụ kiện, đồ sưu tầm độc lạ" }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            var catList = context.Categories.ToList();
            int catDienTu = catList.FirstOrDefault(c => c.Name == "Đồ điện tử")?.Id ?? 1;
            int catGiaDung = catList.FirstOrDefault(c => c.Name == "Đồ gia dụng")?.Id ?? 2;
            int catVPP = catList.FirstOrDefault(c => c.Name == "Văn phòng phẩm")?.Id ?? 3;
            int catThoiTrang = catList.FirstOrDefault(c => c.Name == "Thời trang")?.Id ?? 4;
            int catKhac = catList.FirstOrDefault(c => c.Name == "Khác")?.Id ?? 5;

            // 2. SEED USERS (Admin và Khách hàng)
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new()
                    {
                        Name = "Ban Quản Trị ThriftShop",
                        Email = "admin@thriftshop.vn",
                        Password = "admin",
                        Phone = "19006868",
                        Address = "TP. Hồ Chí Minh",
                        Avatar = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80",
                        Bio = "Ban Quản Trị & Kiểm Duyệt Sàn Đồ Cũ ThriftShop",
                        Role = "admin"
                    },
                    new()
                    {
                        Name = "Người Dùng Mẫu",
                        Email = "user@thriftshop.vn",
                        Password = "user",
                        Phone = "0901234567",
                        Address = "Quận 1, TP. Hồ Chí Minh",
                        Avatar = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                        Bio = "Thành viên tích cực mua bán & tái sử dụng đồ cũ.",
                        Role = "customer"
                    },
                    new()
                    {
                        Name = "Chung Tấn Thành",
                        Email = "thanh@thriftshop.vn",
                        Password = "user",
                        Phone = "0908765432",
                        Address = "Quận 1, TP. Hồ Chí Minh",
                        Avatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Bio = "Chuyên thanh lý đồ vintage, áo khoác, đồ denim sưu tầm.",
                        Role = "customer"
                    },
                    new()
                    {
                        Name = "Đặng Thuỳ Dương",
                        Email = "duong.dang@thriftshop.vn",
                        Password = "user",
                        Phone = "0918765432",
                        Address = "Quận Phú Nhuận, TP.HCM",
                        Avatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
                        Bio = "Thanh lý đồ công nghệ, đồ decor nhà cửa còn mới 95%.",
                        Role = "customer"
                    },
                    new()
                    {
                        Name = "Hoàng Minh Trí",
                        Email = "tri.hoang@thriftshop.vn",
                        Password = "user",
                        Phone = "0987654321",
                        Address = "Quận 3, TP.HCM",
                        Avatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Bio = "Đam mê sneaker secondhand và thời trang đường phố.",
                        Role = "customer"
                    }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            var userList = context.Users.ToList();
            int userThanh = userList.FirstOrDefault(u => u.Name == "Chung Tấn Thành")?.Id ?? 3;
            int userDuong = userList.FirstOrDefault(u => u.Name == "Đặng Thuỳ Dương")?.Id ?? 4;
            int userTri = userList.FirstOrDefault(u => u.Name == "Hoàng Minh Trí")?.Id ?? 5;

            // 3. SEED PRODUCTS (Sản phẩm đồ cũ mở bán)
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new()
                    {
                        Name = "Áo khoác Jean Levis Vintage 1990s lót lông",
                        CategoryId = catThoiTrang,
                        CategoryName = "Thời trang",
                        Price = 420000,
                        OriginalPrice = 1650000,
                        Description = "Áo jean Levis nguyên bản form boxy chuẩn vintage thập niên 90. Chất vải denim dày dặn, có bạc màu tự nhiên rất phong cách. Lớp lót lông cừu ấm áp không bị rách.  (Tình trạng: Độ mới: 92% - Tốt - Khuy áo nguyên bản, hơi sờn nhẹ gấu tay áo (chất vintage), không rách, đã giặt sấy thơm tho sạch sẽ.)",
                        Image = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80\",\"https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80\",\"https://images.unsplash.com/photo-1520639888713-7851133b1ed0?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userThanh,
                        SellerName = "Chung Tấn Thành",
                        SellerPhone = "0908765432",
                        SellerEmail = "thanh@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Featured = true,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("01/10/2026 10:30", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Tai nghe chống ồn Sony WH-1000XM4 màu Bạc",
                        CategoryId = catDienTu,
                        CategoryName = "Đồ điện tử",
                        Price = 3600000,
                        OriginalPrice = 6990000,
                        Description = "Tai nghe chống ồn chủ động đỉnh cao Sony WH-1000XM4. Mua chính hãng Sony Store Việt Nam, ít dùng do chuyển sang dùng in-ear. Pin còn nghe liên tục trên 28 tiếng chống ồn.  (Tình trạng: Độ mới: 95% - Rất mới - Đệm tai và headband còn nguyên vẹn không tróc da. Đầy đủ phụ kiện: hộp cứng bảo vệ, cáp Type-C, jack 3.5mm và adapter máy bay zin.)",
                        Image = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80\",\"https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&q=80\",\"https://images.unsplash.com/photo-1484704849700-f032a568e944?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userDuong,
                        SellerName = "Đặng Thuỳ Dương",
                        SellerPhone = "0918765432",
                        SellerEmail = "duong.dang@thriftshop.vn",
                        SellerAddress = "Quận Phú Nhuận, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
                        Featured = true,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("02/10/2026 14:15", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Giày Sneaker Nike Air Jordan 1 Retro High OG (Size 42)",
                        CategoryId = catThoiTrang,
                        CategoryName = "Thời trang",
                        Price = 1850000,
                        OriginalPrice = 4800000,
                        Description = "Đôi Jordan 1 Retro bản High OG phối màu Chicago kinh điển. Giày chính hãng mua tại cửa hàng Nike Landmark 81, có box thay thế sạch sẽ.  (Tình trạng: Độ mới: 90% - Tốt - Đế giày mòn nhẹ gót (đã dán sole bảo vệ lúc mới mua), da mũi giày có nếp nhăn tự nhiên, tặng kèm dây giày cam thay thế.)",
                        Image = "https://images.unsplash.com/photo-1552346154-21d32810aba3?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1552346154-21d32810aba3?w=600&q=80\",\"https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=600&q=80\",\"https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userTri,
                        SellerName = "Hoàng Minh Trí",
                        SellerPhone = "0987654321",
                        SellerEmail = "tri.hoang@thriftshop.vn",
                        SellerAddress = "Quận 3, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Featured = true,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("03/10/2026 09:00", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Máy đọc sách Kindle Paperwhite Gen 4 (10th) 8GB",
                        CategoryId = catDienTu,
                        CategoryName = "Đồ điện tử",
                        Price = 1450000,
                        OriginalPrice = 2890000,
                        Description = "Kindle Paperwhite Gen 4 chống nước IPX8, màn hình 300ppi sắc nét như trang sách thật. Máy đã cài đặt sẵn từ điển Anh-Việt, hỗ trợ đọc file PRC, MOBI, EPUB, PDF.  (Tình trạng: Độ mới: 94% - Rất mới - Màn hình E-ink không một vết xước, vỏ lưng cao su không chảy keo, pin dùng 3 tuần đọc mỗi ngày 1 tiếng.)",
                        Image = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80\",\"https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80\",\"https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userDuong,
                        SellerName = "Đặng Thuỳ Dương",
                        SellerPhone = "0918765432",
                        SellerEmail = "duong.dang@thriftshop.vn",
                        SellerAddress = "Quận Phú Nhuận, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
                        Featured = true,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("04/10/2026 11:20", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Nồi chiên không dầu Philips HD9252 dung tích 4.1L",
                        CategoryId = catGiaDung,
                        CategoryName = "Đồ gia dụng",
                        Price = 850000,
                        OriginalPrice = 2200000,
                        Description = "Nồi chiên không dầu Philips công nghệ Rapid Air giảm 90% lượng dầu mỡ. Dung tích 4.1L đủ nướng gà nguyên con hoặc làm bánh cho cả gia đình.  (Tình trạng: Độ mới: 90% - Tốt - Khay chống dính còn rất tốt không bong tróc, bảng điều khiển cảm ứng nhạy bén, dây nguồn zin.)",
                        Image = "https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=600&q=80\",\"https://images.unsplash.com/photo-1556911220-e15b29be8c8f?w=600&q=80\",\"https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userThanh,
                        SellerName = "Chung Tấn Thành",
                        SellerPhone = "0908765432",
                        SellerEmail = "thanh@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("05/10/2026 16:45", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Combo trọn bộ 12 tập Manga Naruto Boxset sưu tầm",
                        CategoryId = catVPP,
                        CategoryName = "Văn phòng phẩm",
                        Price = 290000,
                        OriginalPrice = 650000,
                        Description = "Trọn bộ truyện tranh Naruto chính hãng NXB Kim Đồng bản in chất lượng cao. Bộ sưu tập cá nhân ít đọc, giữ gìn kỹ càng trong tủ kính.  (Tình trạng: Độ mới: 96% - Như mới - Sách bọc màng co nilon bảo quản cẩn thận, giấy không ố vàng, không quăn mép, đầy đủ bookmark tặng kèm.)",
                        Image = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80\",\"https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80\",\"https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userTri,
                        SellerName = "Hoàng Minh Trí",
                        SellerPhone = "0987654321",
                        SellerEmail = "tri.hoang@thriftshop.vn",
                        SellerAddress = "Quận 3, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("06/10/2026 08:30", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Đèn bàn học chống cận Taotronics LED điều chỉnh 5 độ sáng",
                        CategoryId = catGiaDung,
                        CategoryName = "Đồ gia dụng",
                        Price = 320000,
                        OriginalPrice = 850000,
                        Description = "Đèn LED Taotronics bảo vệ thị lực, có 5 tông màu ánh sáng từ vàng ấm đến trắng sáng và 7 mức cường độ sáng cảm ứng.  (Tình trạng: Độ mới: 93% - Rất tốt - Mọi chế độ sáng và cổng sạc USB tích hợp hoạt động bình thường, chân đế nặng chắc chắn không lỏng lẻo.)",
                        Image = "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600&q=80\",\"https://images.unsplash.com/photo-1513506003901-1e6a229e2d15?w=600&q=80\",\"https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userThanh,
                        SellerName = "Chung Tấn Thành",
                        SellerPhone = "0908765432",
                        SellerEmail = "thanh@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("07/10/2026 13:10", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Bút máy Parker Jotter London vỏ thép không gỉ",
                        CategoryId = catVPP,
                        CategoryName = "Văn phòng phẩm",
                        Price = 280000,
                        OriginalPrice = 680000,
                        Description = "Bút máy cao cấp Parker Jotter sản xuất tại Pháp. Thiết kế mũi tên đặc trưng, vỏ kim loại sáng bóng thanh lịch, phù hợp ký hợp đồng hoặc làm quà tặng.  (Tình trạng: Độ mới: 95% - Rất mới - Ngòi bút F trơn mượt không cào giấy, bơm mực piston chính hãng đi kèm, vỏ bút không trầy xước.)",
                        Image = "https://images.unsplash.com/photo-1583485088034-697b5bc54ccd?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1583485088034-697b5bc54ccd?w=600&q=80\",\"https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80\",\"https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userDuong,
                        SellerName = "Đặng Thuỳ Dương",
                        SellerPhone = "0918765432",
                        SellerEmail = "duong.dang@thriftshop.vn",
                        SellerAddress = "Quận Phú Nhuận, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("08/10/2026 15:00", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Đàn Ukulele Concert Mahalo gỗ Mahogany tự nhiên",
                        CategoryId = catKhac,
                        CategoryName = "Khác",
                        Price = 390000,
                        OriginalPrice = 950000,
                        Description = "Cây ukulele size Concert 23 inch dễ bấm cho người mới tập hoặc mang đi du lịch dã ngoại. Âm thanh ấm áp, mộc mạc và tươi vui.  (Tình trạng: Độ mới: 94% - Rất tốt - Gỗ khô vang âm chuẩn, khóa đàn chắc chắn giữ dây tốt, dây Aquila chính hãng Ý, tặng bao da 3 lớp.)",
                        Image = "https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&q=80\",\"https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=600&q=80\",\"https://images.unsplash.com/photo-1465847899084-d164df4dedc6?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userTri,
                        SellerName = "Hoàng Minh Trí",
                        SellerPhone = "0987654321",
                        SellerEmail = "tri.hoang@thriftshop.vn",
                        SellerAddress = "Quận 3, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("09/10/2026 17:20", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Mô hình Figure One Piece Luffy Gear 4 chính hãng Bandai",
                        CategoryId = catKhac,
                        CategoryName = "Khác",
                        Price = 450000,
                        OriginalPrice = 1100000,
                        Description = "Mô hình nhân vật Luffy trạng thái Gear 4 Snakeman siêu ngầu. Chi tiết sơn sắc nét, cơ bắp sống động, hiệu ứng khói chân thực.  (Tình trạng: Độ mới: 98% - Like New - Mô hình trưng tủ kính không bụi bẩn, không trầy sơn, đủ hộp và đế chống zin của hãng.)",
                        Image = "https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?w=600&q=80\",\"https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&q=80\",\"https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userThanh,
                        SellerName = "Chung Tấn Thành",
                        SellerPhone = "0908765432",
                        SellerEmail = "thanh@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.ParseExact("10/10/2026 12:00", "dd/MM/yyyy HH:mm", null)
                    }
                };
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            // 4. SEED PENDING POSTS (Tin chờ Admin duyệt)
            if (!context.PendingPosts.Any())
            {
                var pendingPosts = new List<PendingPost>
                {
                    new()
                    {
                        Name = "Bàn phím cơ không dây Keychron K2 v2 nhôm RGB",
                        CategoryId = catDienTu,
                        CategoryName = "Đồ điện tử",
                        Price = 1150000,
                        OriginalPrice = 2100000,
                        Description = "Bàn phím cơ nhỏ gọn layout 75% cho cả Mac và Windows. Ít dùng do công ty cấp máy mới, còn nguyên tem bảo hành phongvu.  (Tình trạng: Độ mới: 95% - Rất tốt - Gateron Red Switch êm ái gõ văn phòng, pin kết nối bluetooth 3 thiết bị ngon lành, kèm cáp Type-C zin và keycap puller.)",
                        Image = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80\",\"https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80\",\"https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = 2,
                        SellerName = "Người Dùng Mẫu",
                        SellerPhone = "0901234567",
                        SellerEmail = "user@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                        Status = "pending",
                        CreatedAt = DateTime.ParseExact("12/10/2026 14:00", "dd/MM/yyyy HH:mm", null)
                    },
                    new()
                    {
                        Name = "Áo Sweater Polo Ralph Lauren Vintage thêu ngựa nhỏ",
                        CategoryId = catThoiTrang,
                        CategoryName = "Thời trang",
                        Price = 350000,
                        OriginalPrice = 1200000,
                        Description = "Sweater Ralph Lauren xách tay Nhật. Form oversize thoải mái unisex cả nam nữ đều mặc đẹp.  (Tình trạng: Độ mới: 90% - Tốt - Chất len dệt kim mỏng nhẹ mặc mùa thu mát mẻ, bo chun tay áo còn co giãn tốt, không thủng rách.)",
                        Image = "https://images.unsplash.com/photo-1556905055-8f358a7a47b2?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1556905055-8f358a7a47b2?w=600&q=80\",\"https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80\",\"https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userTri,
                        SellerName = "Hoàng Minh Trí",
                        SellerPhone = "0987654321",
                        SellerEmail = "tri.hoang@thriftshop.vn",
                        SellerAddress = "Quận 3, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Status = "pending",
                        CreatedAt = DateTime.ParseExact("12/10/2026 15:30", "dd/MM/yyyy HH:mm", null)
                    }
                };
                context.PendingPosts.AddRange(pendingPosts);
                context.SaveChanges();
            }

            // 5. SEED ORDERS
            if (!context.Orders.Any())
            {
                var sampleOrder = new Order
                {
                    CustName = "Nguyễn Anh Khoa",
                    CustPhone = "0901234567",
                    CustEmail = "user@thriftshop.vn",
                    CustAddress = "Quận 1, TP. Hồ Chí Minh",
                    PaymentMethod = "COD - Thanh toán khi nhận hàng",
                    Status = "pending",
                    OrderDate = DateTime.Now.AddDays(-1),
                    TotalAmount = 450000,
                    Notes = "Đồng kiểm ngoại hình khi nhận hàng",
                    UserId = userList.FirstOrDefault(u => u.Email == "user@thriftshop.vn")?.Id ?? 2,
                    OrderItems = new List<OrderItem>
                    {
                        new()
                        {
                            ProductId = 1,
                            ProductName = "Bàn phím cơ DareU EK87 Tenkeyless Blue Switch cũ",
                            Image = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80",
                            Price = 450000,
                            Quantity = 1,
                            Subtotal = 450000
                        }
                    }
                };
                context.Orders.Add(sampleOrder);
                context.SaveChanges();
            }
        }
    }
}
```

---

# BACKEND - ENTITIES & DTOS (MODELS)

## Models/User.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Người dùng (User / Customer / Admin)
    /// </summary>
    public class User
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [Display(Name = "Họ và Tên")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không đúng định dạng")]
        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
        public string? Phone { get; set; }

        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string? Address { get; set; }

        [StringLength(255, ErrorMessage = "Link ảnh đại diện tối đa 255 ký tự")]
        public string? Avatar { get; set; }

        [StringLength(500, ErrorMessage = "Tiểu sử tối đa 500 ký tự")]
        public string? Bio { get; set; }

        [StringLength(20)]
        public string Role { get; set; } = "customer"; // 'customer' | 'admin'

        // Thuộc tính Helper tương thích ngược với các View/Controller cũ
        [NotMapped]
        public string Name
        {
            get => FullName;
            set => FullName = value;
        }

        [NotMapped]
        public string Password
        {
            get => PasswordHash;
            set => PasswordHash = value;
        }

        // Navigation properties
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
        public virtual ICollection<Order> Orders { get; set; } = new List<Order>();
    }

    /// <summary>
    /// ViewModel Đăng Nhập cho Razor View (Account/Login.cshtml)
    /// </summary>
    public class LoginViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [Display(Name = "Địa chỉ Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [Display(Name = "Ghi nhớ đăng nhập")]
        public bool RememberMe { get; set; } = false;
    }

    /// <summary>
    /// ViewModel Đăng Ký cho Razor View (Account/Register.cshtml)
    /// </summary>
    public class RegisterViewModel
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [Display(Name = "Họ và Tên")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập email")]
        [EmailAddress(ErrorMessage = "Địa chỉ email không hợp lệ")]
        [Display(Name = "Email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ")]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6 ký tự trở lên")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; } = string.Empty;

        [DataType(DataType.Password)]
        [Display(Name = "Xác nhận mật khẩu")]
        [Compare("Password", ErrorMessage = "Mật khẩu xác nhận không khớp.")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    public class ErrorViewModel
    {
        public string? RequestId { get; set; }
        public bool ShowRequestId => !string.IsNullOrEmpty(RequestId);
    }
}
```

---

## Models/Category.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Danh mục món đồ cũ (Category)
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 100 ký tự")]
        [Display(Name = "Tên Danh Mục")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập icon Font Awesome")]
        [StringLength(50, ErrorMessage = "Icon tối đa 50 ký tự")]
        [Display(Name = "Biểu Tượng (Icon)")]
        public string Icon { get; set; } = "fas fa-tag";

        [StringLength(255, ErrorMessage = "Mô tả danh mục tối đa 255 ký tự")]
        [Display(Name = "Mô Tả Danh Mục")]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
```

---

## Models/Product.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Sản phẩm đồ cũ (Secondhand Product)
    /// ĐÃ BỎ HOÀN TOÀN các cột: Condition, ConditionPercent, ConditionDetails
    /// </summary>
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên món đồ thanh lý")]
        [StringLength(150, ErrorMessage = "Tên sản phẩm tối đa 150 ký tự")]
        [Display(Name = "Tên Món Đồ")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Mã Danh Mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Tên Danh Mục")]
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá thanh lý")]
        [Range(1000, 1000000000, ErrorMessage = "Giá thanh lý từ 1.000 ₫ trở lên")]
        [Display(Name = "Giá Thanh Lý (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá gốc")]
        [Range(0, 1000000000, ErrorMessage = "Giá gốc từ 0 ₫ trở lên")]
        [Display(Name = "Giá Mua Mới Ban Đầu (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Display(Name = "Mô Tả Sản Phẩm & Tình Trạng Khuyết Điểm")]
        public string? Description { get; set; }

        [Display(Name = "Hình Ảnh Đại Diện")]
        public string Image { get; set; } = string.Empty;

        /// <summary>
        /// Chuỗi JSON lưu danh sách ảnh phụ thực tế (nvarchar(max))
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ImagesJson { get; set; }

        /// <summary>
        /// Chuỗi JSON lưu danh sách video thực tế (nvarchar(max))
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? VideosJson { get; set; }

        /// <summary>
        /// Helper get/set mảng ảnh phụ thông qua serialize/deserialize ImagesJson
        /// </summary>
        [NotMapped]
        public string[] Images
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagesJson)) return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(ImagesJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                ImagesJson = value != null && value.Length > 0 ? JsonSerializer.Serialize(value) : null;
            }
        }

        /// <summary>
        /// Helper get/set mảng video thông qua serialize/deserialize VideosJson
        /// </summary>
        [NotMapped]
        public string[] Videos
        {
            get
            {
                if (string.IsNullOrWhiteSpace(VideosJson)) return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(VideosJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                VideosJson = value != null && value.Length > 0 ? JsonSerializer.Serialize(value) : null;
            }
        }

        [Range(0, 100, ErrorMessage = "Số lượng tồn từ 0 trở lên")]
        [Display(Name = "Số Lượng Tồn Kho")]
        public int Stock { get; set; } = 1;

        public int SoldCount { get; set; } = 0;

        [Required]
        [Display(Name = "Mã Người Bán")]
        public int SellerId { get; set; }

        [Display(Name = "Người Bán")]
        [StringLength(100)]
        public string SellerName { get; set; } = string.Empty;

        [Display(Name = "SĐT Người Bán")]
        [StringLength(20)]
        public string SellerPhone { get; set; } = string.Empty;

        [StringLength(150)]
        public string? SellerEmail { get; set; }

        [Display(Name = "Địa Chỉ Người Bán")]
        [StringLength(255)]
        public string SellerAddress { get; set; } = string.Empty;

        [StringLength(255)]
        public string? SellerAvatar { get; set; }

        public bool Featured { get; set; } = false;

        [StringLength(30)]
        public string Status { get; set; } = "approved"; // 'approved' | 'pending' | 'sold' | 'rejected'

        public bool IsSold { get; set; } = false;

        public bool Deleted { get; set; } = false;

        public int? PendingPostId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public string CreatedAtString => CreatedAt.ToString("dd/MM/yyyy HH:mm");

        // Navigation properties (EF Core)
        public virtual Category? Category { get; set; }
        public virtual User? Seller { get; set; }
    }
}
```

---

## Models/PendingPost.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Tin đăng bán đồ cũ người dùng gửi lên chờ Admin duyệt (Cốt lõi Đề tài 30)
    /// ĐÃ BỎ HOÀN TOÀN các cột: Condition, ConditionPercent, ConditionDetails
    /// </summary>
    public class PendingPost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên món đồ")]
        [StringLength(150, ErrorMessage = "Tên món đồ tối đa 150 ký tự")]
        [Display(Name = "Tên Món Đồ")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá bán mong muốn")]
        [Range(1000, 1000000000, ErrorMessage = "Giá thanh lý từ 1.000 ₫ trở lên")]
        [Display(Name = "Giá Thanh Lý (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá gốc")]
        [Display(Name = "Giá Mua Mới (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết, tình trạng và khuyết điểm")]
        [Display(Name = "Mô Tả Sản Phẩm & Tình Trạng")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Hình Ảnh Thật")]
        public string Image { get; set; } = string.Empty;

        /// <summary>
        /// Chuỗi JSON lưu danh sách ảnh phụ thực tế (nvarchar(max))
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ImagesJson { get; set; }

        /// <summary>
        /// Chuỗi JSON lưu danh sách video thực tế (nvarchar(max))
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? VideosJson { get; set; }

        /// <summary>
        /// Helper get/set mảng ảnh phụ thông qua serialize/deserialize ImagesJson
        /// </summary>
        [NotMapped]
        public string[] Images
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagesJson)) return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(ImagesJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                ImagesJson = value != null && value.Length > 0 ? JsonSerializer.Serialize(value) : null;
            }
        }

        /// <summary>
        /// Helper get/set mảng video thông qua serialize/deserialize VideosJson
        /// </summary>
        [NotMapped]
        public string[] Videos
        {
            get
            {
                if (string.IsNullOrWhiteSpace(VideosJson)) return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(VideosJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                VideosJson = value != null && value.Length > 0 ? JsonSerializer.Serialize(value) : null;
            }
        }

        public int Stock { get; set; } = 1;

        [Required]
        public int SellerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người bán")]
        [Display(Name = "Họ Tên Người Bán")]
        [StringLength(100)]
        public string SellerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số Điện Thoại")]
        [StringLength(20)]
        public string SellerPhone { get; set; } = string.Empty;

        [StringLength(150)]
        public string? SellerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khu vực")]
        [Display(Name = "Khu Vực")]
        [StringLength(255)]
        public string SellerAddress { get; set; } = string.Empty;

        [StringLength(255)]
        public string? SellerAvatar { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "pending"; // 'pending' | 'approved' | 'rejected' | 'sold'

        public bool IsSold { get; set; } = false;

        [StringLength(255)]
        public string? RejectReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public string CreatedAtString => CreatedAt.ToString("dd/MM/yyyy HH:mm");

        public string? UpdatedAt { get; set; }

        // Navigation properties (EF Core)
        public virtual Category? Category { get; set; }
        public virtual User? Seller { get; set; }
    }
}
```

---

## Models/Order.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Đơn hàng (Order)
    /// </summary>
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        public string CustName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
        public string CustPhone { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
        public string? CustEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string CustAddress { get; set; } = string.Empty;

        [StringLength(100)]
        public string PaymentMethod { get; set; } = "COD - Thanh toán khi nhận hàng";

        [StringLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "pending"; // 'pending' | 'processing' | 'shipping' | 'delivered' | 'cancelled'

        public DateTime OrderDate { get; set; } = DateTime.Now;

        public int? UserId { get; set; }

        // Navigation properties (EF Core)
        public virtual User? User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Thuộc tính Helper tương thích mã đơn 'DH000123' và các View cũ
        [NotMapped]
        public string OrderCode => $"DH{Id:D6}";

        [NotMapped]
        public string CustomerName
        {
            get => CustName;
            set => CustName = value;
        }

        [NotMapped]
        public string CustomerPhone
        {
            get => CustPhone;
            set => CustPhone = value;
        }

        [NotMapped]
        public string? CustomerEmail
        {
            get => CustEmail;
            set => CustEmail = value;
        }

        [NotMapped]
        public string ShippingAddress
        {
            get => CustAddress;
            set => CustAddress = value;
        }

        [NotMapped]
        public string OrderStatus
        {
            get => Status;
            set => Status = value;
        }

        [NotMapped]
        public string OrderDateString => OrderDate.ToString("dd/MM/yyyy HH:mm");

        [NotMapped]
        public List<OrderItem> Items
        {
            get => OrderItems.ToList();
            set => OrderItems = value;
        }
    }

    /// <summary>
    /// Entity Chi tiết món đồ trong đơn hàng (OrderItem)
    /// ĐÃ BỎ HOÀN TOÀN CỘT Condition
    /// </summary>
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [StringLength(255)]
        public string Image { get; set; } = string.Empty;

        // Navigation properties (EF Core)
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
    }
}
```

---

## Models/ChatMessage.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace ThriftShop.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverAvatar { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public virtual Product? Product { get; set; }
    }
}
```

---

## Models/Favorite.cs

```csharp
using System.ComponentModel.DataAnnotations;

namespace ThriftShop.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Product? Product { get; set; }
        public virtual User? User { get; set; }
    }
}
```

---

## Models/ViewModels.cs

```csharp
namespace ThriftShop.Models
{
    public class HomeViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? Search { get; set; }
        public int[]? CatFilter { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Sort { get; set; } = "newest";
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = new();
        public List<Product> RelatedProducts { get; set; } = new();
        public User? Seller { get; set; }
        public List<string> Images { get; set; } = new();
        public List<string> Videos { get; set; } = new();
        public bool IsFavorite { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int PendingCount { get; set; }
        public int CategoriesCount { get; set; }
        public int ProductsCount { get; set; }
        public int UsersCount { get; set; }
        public List<PendingPost> RecentPending { get; set; } = new();
        public List<Product> RecentProducts { get; set; } = new();
    }

    public class SellerProfileViewModel
    {
        public User User { get; set; } = new();
        public List<Product> ActiveProducts { get; set; } = new();
    }

    public class ChatThreadViewModel
    {
        public Product Product { get; set; } = new();
        public User OtherUser { get; set; } = new();
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
```

---

## Models/DTOs/BaseApiResponse.cs

```csharp
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Response chuẩn toàn hệ thống trả về kết quả cho Frontend
    /// </summary>
    public class BaseApiResponse
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public object? Data { get; set; }

        public BaseApiResponse() { }

        public BaseApiResponse(bool success, string message, object? data = null)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static BaseApiResponse Ok(string message = "Thao tác thành công", object? data = null)
        {
            return new BaseApiResponse(true, message, data);
        }

        public static BaseApiResponse Fail(string message = "Đã xảy ra lỗi", object? data = null)
        {
            return new BaseApiResponse(false, message, data);
        }
    }

    /// <summary>
    /// Response chuẩn có kiểu dữ liệu cụ thể (Generic)
    /// </summary>
    public class BaseApiResponse<T>
    {
        [JsonPropertyName("success")]
        public bool Success { get; set; }

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("data")]
        public T? Data { get; set; }

        public BaseApiResponse() { }

        public BaseApiResponse(bool success, string message, T? data = default)
        {
            Success = success;
            Message = message;
            Data = data;
        }

        public static BaseApiResponse<T> Ok(string message = "Thao tác thành công", T? data = default)
        {
            return new BaseApiResponse<T>(true, message, data);
        }

        public static BaseApiResponse<T> Fail(string message = "Đã xảy ra lỗi", T? data = default)
        {
            return new BaseApiResponse<T>(false, message, data);
        }
    }
}
```

---

## Models/DTOs/AuthDtos.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Payload đăng nhập gửi lên từ frontend (auth.js hoặc form)
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payload đăng ký tài khoản gửi lên từ frontend (auth.js)
    /// </summary>
    public class RegisterRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [JsonPropertyName("fullName")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ email")]
        [EmailAddress(ErrorMessage = "Email không đúng định dạng")]
        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ (10 số, bắt đầu bằng 0 hoặc +84)")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu")]
        [MinLength(6, ErrorMessage = "Mật khẩu phải có tối thiểu 6 ký tự")]
        [JsonPropertyName("password")]
        public string Password { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng xác nhận mật khẩu")]
        [Compare(nameof(Password), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }

    /// <summary>
    /// Payload cập nhật thông tin cá nhân (Profile.cshtml)
    /// </summary>
    public class UpdateProfileDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string? Address { get; set; }

        [JsonPropertyName("bio")]
        public string? Bio { get; set; }

        [JsonPropertyName("avatar")]
        public string? Avatar { get; set; }
    }

    /// <summary>
    /// Payload đổi mật khẩu người dùng (Profile.cshtml)
    /// </summary>
    public class ChangePasswordDto
    {
        [Required(ErrorMessage = "Vui lòng nhập mật khẩu hiện tại")]
        [JsonPropertyName("oldPassword")]
        public string OldPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mật khẩu mới")]
        [MinLength(6, ErrorMessage = "Mật khẩu mới phải có tối thiểu 6 ký tự")]
        [JsonPropertyName("newPassword")]
        public string NewPassword { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập lại mật khẩu mới")]
        [Compare(nameof(NewPassword), ErrorMessage = "Mật khẩu xác nhận không khớp")]
        [JsonPropertyName("confirmPassword")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
```

---

## Models/DTOs/ProductDtos.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Payload đăng bán món đồ cũ người dùng gửi lên từ PostItem.cshtml
    /// ĐÃ BỎ HOÀN TOÀN CÁC TRƯỜNG Condition, ConditionPercent, ConditionDetails
    /// (Toàn bộ mô tả tình trạng & khuyết điểm đã gộp vào Description)
    /// </summary>
    public class PostItemRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên món đồ cần thanh lý")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Tên món đồ từ 3 đến 150 ký tự")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục món đồ")]
        [Range(1, int.MaxValue, ErrorMessage = "Danh mục không hợp lệ")]
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá thanh lý mong muốn")]
        [Range(1000, 1000000000, ErrorMessage = "Giá thanh lý phải từ 1.000 ₫ đến 1.000.000.000 ₫")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá gốc không hợp lệ")]
        [JsonPropertyName("originalPrice")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khu vực giao dịch / địa chỉ xem đồ")]
        [StringLength(255, ErrorMessage = "Địa chỉ khu vực tối đa 255 ký tự")]
        [JsonPropertyName("sellerAddress")]
        public string SellerAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại hoặc Zalo liên hệ")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ")]
        [JsonPropertyName("sellerPhone")]
        public string SellerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng cung cấp ít nhất 1 ảnh đại diện chính của món đồ")]
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public string[] Images { get; set; } = Array.Empty<string>();

        [JsonPropertyName("videos")]
        public string[] Videos { get; set; } = Array.Empty<string>();

        [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết, tình trạng sử dụng và khuyết điểm món đồ")]
        [MinLength(10, ErrorMessage = "Mô tả chi tiết phải có tối thiểu 10 ký tự")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response trả về sau khi người dùng đăng bán thành công
    /// </summary>
    public class PostItemResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "Tin đăng đã được gửi và chuyển sang trạng thái chờ Admin duyệt!";
    }

    /// <summary>
    /// DTO hiển thị danh sách và modal kiểm duyệt tin đồ cũ cho Admin (ApproveProducts.cshtml)
    /// Đảm bảo serialize ra JSON ở dạng camelCase, đồng bộ 100% với frontend JS
    /// </summary>
    public class AdminProductViewDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("originalPrice")]
        public decimal OriginalPrice { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("imagesJson")]
        public string? ImagesJson { get; set; }

        [JsonPropertyName("videosJson")]
        public string? VideosJson { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; } = 1;

        [JsonPropertyName("sellerId")]
        public int SellerId { get; set; }

        [JsonPropertyName("sellerName")]
        public string SellerName { get; set; } = string.Empty;

        [JsonPropertyName("sellerPhone")]
        public string SellerPhone { get; set; } = string.Empty;

        [JsonPropertyName("sellerEmail")]
        public string? SellerEmail { get; set; }

        [JsonPropertyName("sellerAddress")]
        public string SellerAddress { get; set; } = string.Empty;

        [JsonPropertyName("sellerAvatar")]
        public string? SellerAvatar { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending"; // 'pending' | 'approved' | 'rejected' | 'sold'

        [JsonPropertyName("isSold")]
        public bool IsSold { get; set; } = false;

        [JsonPropertyName("rejectReason")]
        public string? RejectReason { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }
}
```

---

## Models/DTOs/OrderDtos.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Thông tin 1 món đồ trong đơn hàng (Đã bỏ hoàn toàn condition)
    /// </summary>
    public class OrderItemDto
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;

        [JsonPropertyName("subtotal")]
        public decimal Subtotal => Price * Quantity;
    }

    /// <summary>
    /// Payload đặt hàng gửi lên từ Checkout.cshtml
    /// </summary>
    public class CheckoutRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận hàng")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên người nhận từ 2 đến 100 ký tự")]
        [JsonPropertyName("custName")]
        public string CustName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại nhận hàng không hợp lệ")]
        [JsonPropertyName("custPhone")]
        public string CustPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [JsonPropertyName("custEmail")]
        public string? CustEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết nhận hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ nhận hàng tối đa 255 ký tự")]
        [JsonPropertyName("custAddress")]
        public string CustAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; } = "COD - Thanh toán khi nhận hàng";

        [JsonPropertyName("notes")]
        public string? Notes { get; set; } = "Đồng kiểm ngoại hình khi nhận hàng";

        [Required(ErrorMessage = "Giỏ hàng không có sản phẩm nào")]
        [JsonPropertyName("itemsJson")]
        public string ItemsJson { get; set; } = "[]";

        [Required(ErrorMessage = "Vui lòng cung cấp tổng tiền thanh toán")]
        [Range(1000, 1000000000, ErrorMessage = "Tổng tiền thanh toán không hợp lệ")]
        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Response tóm tắt đơn hàng cho trang Orders/Index.cshtml
    /// </summary>
    public class OrderSummaryDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty; // Format dạng 'DH000123'

        [JsonPropertyName("orderDate")]
        public string OrderDate { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        [JsonPropertyName("orderStatus")]
        public string OrderStatus { get; set; } = "pending"; // 'pending' | 'processing' | 'shipping' | 'delivered' | 'cancelled'

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("customerPhone")]
        public string CustomerPhone { get; set; } = string.Empty;

        [JsonPropertyName("shippingAddress")]
        public string ShippingAddress { get; set; } = string.Empty;

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
```

---

## Models/DTOs/AdminDtos.cs

```csharp
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Payload thêm/sửa danh mục đồ cũ từ Admin/Categories.cshtml
    /// </summary>
    public class SaveCategoryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục đồ cũ")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 100 ký tự")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập icon Font Awesome (VD: fas fa-tag)")]
        [StringLength(50, ErrorMessage = "Icon tối đa 50 ký tự")]
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "fas fa-tag";

        [StringLength(255, ErrorMessage = "Mô tả danh mục tối đa 255 ký tự")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Payload từ chối tin đăng bán đồ cũ từ Admin/ApproveProducts.cshtml
    /// </summary>
    public class RejectPostDto
    {
        [Required(ErrorMessage = "Vui lòng cung cấp mã tin đăng cần từ chối")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã tin đăng không hợp lệ")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do từ chối tin đăng bán")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Lý do từ chối từ 3 đến 255 ký tự")]
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
```

---

# BACKEND - CONTROLLERS

## Controllers/HomeController.cs

```csharp
using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Data;
using ThriftShop.Models;

namespace ThriftShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ThriftDbContext _db;

        public HomeController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: / (Trang chủ - Hiển thị sản phẩm đồ cũ, lọc danh mục, khoảng giá, sắp xếp, phân trang 9 sp bằng LINQ)
        public IActionResult Index(string? search, int[]? catFilter, decimal? minPrice, decimal? maxPrice, string? sort, int page = 1)
        {
            const int pageSize = 9;
            if (page < 1) page = 1;

            // 1. LINQ: Lấy sản phẩm đang mở bán (Đã duyệt, chưa bán, chưa xoá, còn tồn kho)
            var query = _db.Products.AsNoTracking()
                .Where(p => p.Status != null && p.Status.ToLower() == "approved" && !p.IsSold && !p.Deleted && p.Stock > 0);

            // 2. LINQ: Lọc theo danh mục
            if (catFilter != null && catFilter.Length > 0)
            {
                query = query.Where(p => catFilter.Contains(p.CategoryId));
            }

            // 3. LINQ: Lọc theo khoảng giá
            if (minPrice.HasValue && minPrice.Value > 0)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 4. LINQ: Tìm kiếm từ khoá
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(q) 
                                      || (p.Description != null && p.Description.ToLower().Contains(q))
                                      || p.CategoryName.ToLower().Contains(q));
            }

            // 5. LINQ: Sắp xếp
            query = sort switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.Id) // Mặc định: Mới nhất
            };

            // 6. LINQ: Tính tổng số lượng và phân trang
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var categories = _db.Categories.AsNoTracking().ToList();

            var viewModel = new HomeViewModel
            {
                Products = products,
                Categories = categories,
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = totalPages,
                Search = search,
                CatFilter = catFilter,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort ?? "newest"
            };

            return View(viewModel);
        }

        // GET: /Home/Warranty (Chính sách bảo đảm đồ cũ uy tín)
        public IActionResult Warranty()
        {
            return View();
        }

        // GET: /Home/Barter (Chuyển hướng sang Danh mục đồ cũ)
        public IActionResult Barter()
        {
            return RedirectToAction("Index", "Products");
        }

        // GET: /Home/Repair (Dịch vụ phục chế / spa đồ cũ)
        public IActionResult Repair()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
```

---

## Controllers/AccountController.cs

```csharp
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ThriftDbContext _db;

        public AccountController(ThriftDbContext db)
        {
            _db = db;
        }

        #region Helper Hashing
        private static string HashPassword(string password)
        {
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        private static bool VerifyPassword(string inputPassword, string storedHash)
        {
            if (string.IsNullOrEmpty(storedHash)) return false;
            if (storedHash == inputPassword) return true; // Hỗ trợ mật khẩu dạng plain text cũ từ dữ liệu seed
            return HashPassword(inputPassword) == storedHash.ToLower();
        }
        #endregion

        #region API Endpoints (Dành cho Fetch API / auth.js)

        /// <summary>
        /// POST /api/login
        /// Xác thực email và password từ frontend JSON payload
        /// </summary>
        [HttpPost("/api/login")]
        public IActionResult ApiLogin([FromBody] LoginRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    return BadRequest(new { message = firstError ?? "Dữ liệu đăng nhập không hợp lệ!" });
                }

                var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == dto.Email.Trim().ToLower());
                if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                {
                    return BadRequest(new { message = "Email hoặc mật khẩu không chính xác!" });
                }

                // Thiết lập Session đăng nhập
                HttpContext.Session.SetUser(user);

                return Ok(new
                {
                    success = true,
                    message = "Đăng nhập thành công!",
                    user = new
                    {
                        id = user.Id,
                        name = user.FullName,
                        email = user.Email,
                        role = user.Role,
                        avatar = user.Avatar
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ khi xử lý đăng nhập: " + ex.Message });
            }
        }

        /// <summary>
        /// POST /api/register
        /// Đăng ký tài khoản mới từ frontend JSON payload
        /// </summary>
        [HttpPost("/api/register")]
        public IActionResult ApiRegister([FromBody] RegisterRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    return BadRequest(new { message = firstError ?? "Dữ liệu đăng ký không hợp lệ!" });
                }

                var emailExists = _db.Users.Any(u => u.Email.ToLower() == dto.Email.Trim().ToLower());
                if (emailExists)
                {
                    return BadRequest(new { message = "Địa chỉ email này đã được sử dụng!" });
                }

                var newUser = new User
                {
                    FullName = dto.FullName.Trim(),
                    Email = dto.Email.Trim().ToLower(),
                    PasswordHash = HashPassword(dto.Password),
                    Phone = dto.Phone.Trim(),
                    Address = "TP. Hồ Chí Minh",
                    Avatar = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                    Bio = "Thành viên mới tham gia sàn mua bán đồ cũ ThriftShop.",
                    Role = "customer"
                };

                _db.Users.Add(newUser);
                _db.SaveChanges();

                return Ok(new { success = true, message = "Đăng ký thành công! Vui lòng đăng nhập." });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Lỗi máy chủ khi xử lý đăng ký: " + ex.Message });
            }
        }

        #endregion

        #region MVC Web Actions (Form Submit truyền thống)

        // GET: /Account/Login
        public IActionResult Login(string? returnUrl)
        {
            if (HttpContext.Session.IsLoggedIn())
            {
                if (HttpContext.Session.IsAdmin())
                    return RedirectToAction("Index", "Admin");
                return Redirect(returnUrl ?? "/");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = _db.Users.FirstOrDefault(u => u.Email.ToLower() == model.Email.Trim().ToLower());
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác!");
                return View(model);
            }

            HttpContext.Session.SetUser(user);
            TempData["SuccessMessage"] = $"Đăng nhập thành công! Chào mừng {user.FullName}";

            if (user.Role == "admin" || user.Email == "admin@thriftshop.vn")
            {
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                    return Redirect(returnUrl);
                return RedirectToAction("Index", "Admin");
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Register
        public IActionResult Register()
        {
            if (HttpContext.Session.IsLoggedIn())
                return RedirectToAction("Index", "Home");

            return View(new RegisterViewModel());
        }

        // POST: /Account/Register
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var emailExists = _db.Users.Any(u => u.Email.ToLower() == model.Email.Trim().ToLower());
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng!");
                return View(model);
            }

            var newUser = new User
            {
                FullName = model.Name.Trim(),
                Email = model.Email.Trim().ToLower(),
                PasswordHash = HashPassword(model.Password),
                Phone = model.Phone.Trim(),
                Address = model.Address.Trim(),
                Avatar = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                Bio = "Thành viên mới tham gia sàn mua bán đồ cũ ThriftShop.",
                Role = "customer"
            };

            _db.Users.Add(newUser);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập.";
            return RedirectToAction("Login", new { returnUrl = "/" });
        }

        // GET: /Account/Logout
        public IActionResult Logout()
        {
            HttpContext.Session.ClearUser();
            Response.Cookies.Delete(".AspNetCore.Session");
            TempData["SuccessMessage"] = "Đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });
            }

            var user = _db.Users.Find(userId.Value);
            if (user == null)
            {
                HttpContext.Session.ClearUser();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // POST: /Account/UpdateProfile ([FromForm] UpdateProfileDto)
        [HttpPost]
        public IActionResult UpdateProfile([FromForm] UpdateProfileDto dto)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện chức năng này!" });
                }

                var user = _db.Users.Find(userId.Value);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tài khoản người dùng!" });
                }

                if (!string.IsNullOrWhiteSpace(dto.Name))
                {
                    user.FullName = dto.Name.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Phone))
                {
                    user.Phone = dto.Phone.Trim();
                }

                if (dto.Address != null)
                {
                    user.Address = dto.Address.Trim();
                }

                if (dto.Bio != null)
                {
                    user.Bio = dto.Bio.Trim();
                }

                if (!string.IsNullOrWhiteSpace(dto.Avatar))
                {
                    user.Avatar = dto.Avatar.Trim();
                }

                _db.SaveChanges();
                HttpContext.Session.SetUser(user);

                return Json(new
                {
                    success = true,
                    message = "Cập nhật hồ sơ cá nhân thành công!",
                    user = new
                    {
                        id = user.Id,
                        name = user.FullName,
                        email = user.Email,
                        phone = user.Phone,
                        address = user.Address,
                        avatar = user.Avatar,
                        bio = user.Bio,
                        role = user.Role
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi cập nhật hồ sơ: " + ex.Message });
            }
        }

        // POST: /Account/ChangePassword ([FromForm] ChangePasswordDto)
        [HttpPost]
        public IActionResult ChangePassword([FromForm] ChangePasswordDto dto)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để đổi mật khẩu!" });
                }

                var user = _db.Users.Find(userId.Value);
                if (user == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy thông tin tài khoản!" });
                }

                if (!VerifyPassword(dto.OldPassword, user.PasswordHash))
                {
                    return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác!" });
                }

                if (string.IsNullOrWhiteSpace(dto.NewPassword) || dto.NewPassword.Length < 6)
                {
                    return Json(new { success = false, message = "Mật khẩu mới phải có tối thiểu 6 ký tự!" });
                }

                if (dto.NewPassword != dto.ConfirmPassword)
                {
                    return Json(new { success = false, message = "Mật khẩu xác nhận không khớp với mật khẩu mới!" });
                }

                user.PasswordHash = HashPassword(dto.NewPassword);
                _db.SaveChanges();

                HttpContext.Session.SetUser(user);
                return Json(new { success = true, message = "Đổi mật khẩu tài khoản thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi đổi mật khẩu: " + ex.Message });
            }
        }

        // GET: /Account/SellerProfile?userId=...
        public IActionResult SellerProfile(string? userId)
        {
            if (string.IsNullOrWhiteSpace(userId))
            {
                var curId = HttpContext.Session.GetUserId();
                if (curId.HasValue)
                    userId = curId.Value.ToString();
                else
                    return RedirectToAction("Login");
            }

            User? seller = null;
            if (int.TryParse(userId, out int idVal))
            {
                seller = _db.Users.Find(idVal);
            }
            if (seller == null)
            {
                seller = _db.Users.FirstOrDefault(u =>
                    u.Email.ToLower() == userId.ToLower() ||
                    u.FullName.ToLower() == userId.ToLower());
            }

            if (seller == null)
            {
                var sampleProd = _db.Products.FirstOrDefault(p =>
                    p.SellerId.ToString() == userId ||
                    p.SellerName.ToLower() == userId.ToLower());

                if (sampleProd != null)
                {
                    seller = new User
                    {
                        Id = sampleProd.SellerId,
                        FullName = sampleProd.SellerName,
                        Email = sampleProd.SellerEmail ?? "seller@thriftshop.vn",
                        PasswordHash = "user",
                        Phone = sampleProd.SellerPhone,
                        Address = sampleProd.SellerAddress,
                        Avatar = sampleProd.SellerAvatar ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                        Bio = "Thành viên mua bán đồ cũ trên sàn ThriftShop."
                    };
                }
                else
                {
                    seller = new User
                    {
                        FullName = userId,
                        PasswordHash = "user",
                        Avatar = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80"
                    };
                }
            }

            var activeProducts = _db.Products.AsNoTracking()
                .Where(p => (p.SellerId == seller.Id ||
                            (p.SellerEmail != null && p.SellerEmail.ToLower() == seller.Email.ToLower()) ||
                            p.SellerName.ToLower() == seller.FullName.ToLower())
                            && p.Status == "approved"
                            && !p.IsSold
                            && !p.Deleted)
                .OrderByDescending(p => p.Id)
                .ToList();

            var viewModel = new SellerProfileViewModel
            {
                User = seller,
                ActiveProducts = activeProducts
            };

            return View(viewModel);
        }

        // GET: /Account/UserPublic
        public IActionResult UserPublic(string? userId)
        {
            return RedirectToAction(nameof(SellerProfile), new { userId });
        }

        #endregion
    }
}
```

---

## Controllers/ProductsController.cs

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ThriftDbContext _db;

        public ProductsController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Products
        public IActionResult Index(string? search, int[]? catFilter, decimal? minPrice, decimal? maxPrice, string? sort, int page = 1)
        {
            return RedirectToAction("Index", "Home", new { search, catFilter, minPrice, maxPrice, sort, page });
        }

        // GET: /Products/Details/101
        public IActionResult Details(int? id)
        {
            if (!id.HasValue) return RedirectToAction("Index", "Home");

            var product = _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id.Value);
            if (product == null)
            {
                // Cố gắng tìm trong bảng PendingPosts nếu bài đăng chưa được đồng bộ hoặc tạo từ PendingPost
                var pending = _db.PendingPosts.AsNoTracking().FirstOrDefault(p => p.Id == id.Value);
                if (pending != null)
                {
                    product = new Product
                    {
                        Id = pending.Id,
                        Name = pending.Name,
                        CategoryId = pending.CategoryId,
                        CategoryName = pending.CategoryName,
                        Price = pending.Price,
                        OriginalPrice = pending.OriginalPrice,
                        Description = pending.Description,
                        Image = pending.Image,
                        ImagesJson = pending.ImagesJson,
                        VideosJson = pending.VideosJson,
                        Stock = pending.Stock,
                        SellerId = pending.SellerId,
                        SellerName = pending.SellerName,
                        SellerPhone = pending.SellerPhone,
                        SellerEmail = pending.SellerEmail,
                        SellerAddress = pending.SellerAddress,
                        SellerAvatar = pending.SellerAvatar,
                        Status = pending.Status,
                        IsSold = pending.IsSold
                    };
                }
            }

            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy món đồ hoặc tin đăng này đã bị gỡ!";
                return RedirectToAction("Index", "Home");
            }

            var related = _db.Products.AsNoTracking()
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Status == "approved" && !p.IsSold && !p.Deleted && p.Stock > 0)
                .Take(4)
                .ToList();

            User? seller = null;
            if (product.SellerId > 0)
            {
                seller = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == product.SellerId);
            }
            if (seller == null && !string.IsNullOrEmpty(product.SellerEmail))
            {
                seller = _db.Users.AsNoTracking().FirstOrDefault(u => u.Email.ToLower() == product.SellerEmail.ToLower());
            }

            var currentUserId = HttpContext.Session.GetUserId();
            bool isFav = false;
            if (currentUserId.HasValue)
            {
                isFav = _db.Favorites.Any(f => f.UserId == currentUserId.Value && f.ProductId == product.Id);
            }

            var imagesList = product.Images.ToList();
            if (imagesList.Count == 0 && !string.IsNullOrEmpty(product.Image))
            {
                imagesList.Add(product.Image);
            }

            var viewModel = new ProductDetailViewModel
            {
                Product = product,
                RelatedProducts = related,
                Seller = seller,
                Images = imagesList,
                Videos = product.Videos.ToList(),
                IsFavorite = isFav
            };

            return View(viewModel);
        }

        // GET: /Products/PostItem
        public IActionResult PostItem()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản để đăng bán đồ cũ!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Products/PostItem" });
            }

            ViewBag.Categories = _db.Categories.AsNoTracking().ToList();
            return View(new PendingPost());
        }

        // POST: /Products/PostItem ([FromForm] PostItemRequestDto)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PostItem([FromForm] PostItemRequestDto dto, string[]? images, string[]? videos)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại!";
                    return RedirectToAction("Login", "Account", new { returnUrl = "/Products/PostItem" });
                }

                // Gộp danh sách ảnh thực tế: kết hợp từ dto.Images và mảng tham số images
                var rawImages = (dto.Images ?? Array.Empty<string>()).Concat(images ?? Array.Empty<string>()).ToList();
                var validImages = rawImages.Where(img => !string.IsNullOrWhiteSpace(img)).Distinct().ToList();

                if (!string.IsNullOrWhiteSpace(dto.Image) && !validImages.Contains(dto.Image))
                {
                    validImages.Insert(0, dto.Image);
                }

                if (validImages.Count < 3)
                {
                    ModelState.AddModelError("Image", "Yêu cầu bắt buộc: Phải đăng tối thiểu ít nhất 3 hình ảnh thực tế của món đồ (hiện có " + validImages.Count + " ảnh)!");
                    ViewBag.Categories = _db.Categories.AsNoTracking().ToList();
                    return View(new PendingPost
                    {
                        Name = dto.Name,
                        CategoryId = dto.CategoryId,
                        Price = dto.Price,
                        OriginalPrice = dto.OriginalPrice,
                        SellerAddress = dto.SellerAddress,
                        SellerPhone = dto.SellerPhone,
                        Description = dto.Description
                    });
                }

                if (!ModelState.IsValid)
                {
                    ViewBag.Categories = _db.Categories.AsNoTracking().ToList();
                    return View(new PendingPost
                    {
                        Name = dto.Name,
                        CategoryId = dto.CategoryId,
                        Price = dto.Price,
                        OriginalPrice = dto.OriginalPrice,
                        SellerAddress = dto.SellerAddress,
                        SellerPhone = dto.SellerPhone,
                        Description = dto.Description
                    });
                }

                var user = _db.Users.Find(userId.Value);
                var cat = _db.Categories.Find(dto.CategoryId);

                var pendingPost = new PendingPost
                {
                    Name = dto.Name.Trim(),
                    CategoryId = dto.CategoryId,
                    CategoryName = cat?.Name ?? "Đồ cũ",
                    Price = dto.Price,
                    OriginalPrice = dto.OriginalPrice,
                    Description = dto.Description.Trim(),
                    Image = validImages.First(),
                    ImagesJson = JsonSerializer.Serialize(validImages),
                    Stock = 1,
                    SellerId = user?.Id ?? userId.Value,
                    SellerName = user?.FullName ?? HttpContext.Session.GetUserName() ?? "Người bán",
                    SellerEmail = user?.Email ?? HttpContext.Session.GetUserEmail(),
                    SellerPhone = string.IsNullOrWhiteSpace(dto.SellerPhone) ? (user?.Phone ?? "0901234567") : dto.SellerPhone.Trim(),
                    SellerAddress = string.IsNullOrWhiteSpace(dto.SellerAddress) ? (user?.Address ?? "TP.HCM") : dto.SellerAddress.Trim(),
                    SellerAvatar = user?.Avatar ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                    Status = "pending", // Bắt buộc: Chờ Ban Quản Trị duyệt
                    CreatedAt = DateTime.Now
                };

                var rawVideos = (dto.Videos ?? Array.Empty<string>()).Concat(videos ?? Array.Empty<string>()).Where(v => !string.IsNullOrWhiteSpace(v)).Distinct().ToList();
                if (rawVideos.Count > 0)
                {
                    pendingPost.VideosJson = JsonSerializer.Serialize(rawVideos);
                }

                _db.PendingPosts.Add(pendingPost);
                _db.SaveChanges();

                TempData["SuccessMessage"] = "Đăng tin thành công! Tin của bạn đã được chuyển vào hàng đợi để Ban Quản Trị kiểm duyệt chất lượng trước khi mở bán.";
                return RedirectToAction(nameof(MyListings));
            }
            catch (Exception ex)
            {
                ViewBag.Categories = _db.Categories.AsNoTracking().ToList();
                ModelState.AddModelError(string.Empty, "Lỗi máy chủ khi đăng tin: " + ex.Message);
                return View(new PendingPost());
            }
        }

        // GET: /Products/MyListings
        public IActionResult MyListings()
        {
            var userId = HttpContext.Session.GetUserId();
            var userEmail = HttpContext.Session.GetUserEmail();

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem danh sách tin đăng của bạn!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Products/MyListings" });
            }

            var myListings = _db.PendingPosts.AsNoTracking()
                .Where(p => (p.SellerId == userId.Value ||
                           (p.SellerEmail != null && userEmail != null && p.SellerEmail.ToLower() == userEmail.ToLower()))
                           && p.Status != "deleted")
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(myListings);
        }

        // POST: /Products/MarkSold?id=123
        [HttpPost]
        public IActionResult MarkSold(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập!" });
                }

                var isAdmin = HttpContext.Session.IsAdmin();
                var post = _db.PendingPosts.FirstOrDefault(p => p.Id == id && (p.SellerId == userId.Value || isAdmin));
                Product? prod = null;

                if (post != null)
                {
                    post.IsSold = true;
                    post.Status = "sold";
                    prod = _db.Products.FirstOrDefault(p => (p.PendingPostId == post.Id || p.Id == id) && (p.SellerId == userId.Value || isAdmin));
                }
                else
                {
                    prod = _db.Products.FirstOrDefault(p => p.Id == id && (p.SellerId == userId.Value || isAdmin));
                    if (prod != null && prod.PendingPostId.HasValue)
                    {
                        var linkedPost = _db.PendingPosts.FirstOrDefault(p => p.Id == prod.PendingPostId.Value && (p.SellerId == userId.Value || isAdmin));
                        if (linkedPost != null)
                        {
                            linkedPost.IsSold = true;
                            linkedPost.Status = "sold";
                        }
                    }
                }

                if (post == null && prod == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy món đồ hoặc bạn không có quyền thao tác trên tin này!" });
                }

                if (prod != null)
                {
                    prod.IsSold = true;
                    prod.Status = "sold";
                    prod.Stock = 0;
                }

                _db.SaveChanges();
                return Json(new { success = true, message = "Đã đánh dấu đã bán thành công! Món đồ đã được ẩn khỏi sàn và bảo toàn lịch sử chat." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi đánh dấu đã bán: " + ex.Message });
            }
        }

        // POST: /Products/DeletePermanently?id=123
        // CHỢ TỐT C2C: Dùng Soft-Delete (Status = "deleted"), KHÔNG XÓA CỨNG để bảo toàn dữ liệu ChatMessage
        [HttpPost]
        public IActionResult DeletePermanently(int id)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập!" });
                }

                var isAdmin = HttpContext.Session.IsAdmin();
                var post = _db.PendingPosts.FirstOrDefault(p => p.Id == id && (p.SellerId == userId.Value || isAdmin));
                Product? prod = null;

                if (post != null)
                {
                    post.Status = "deleted";
                    prod = _db.Products.FirstOrDefault(p => (p.PendingPostId == post.Id || p.Id == id) && (p.SellerId == userId.Value || isAdmin));
                }
                else
                {
                    prod = _db.Products.FirstOrDefault(p => p.Id == id && (p.SellerId == userId.Value || isAdmin));
                    if (prod != null && prod.PendingPostId.HasValue)
                    {
                        var linkedPost = _db.PendingPosts.FirstOrDefault(p => p.Id == prod.PendingPostId.Value && (p.SellerId == userId.Value || isAdmin));
                        if (linkedPost != null)
                        {
                            linkedPost.Status = "deleted";
                        }
                    }
                }

                if (post == null && prod == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy tin đăng hoặc bạn không có quyền xóa bài này!" });
                }

                if (prod != null)
                {
                    prod.Status = "deleted";
                    prod.Deleted = true;
                    prod.IsSold = true;
                    prod.Stock = 0;
                }

                _db.SaveChanges();
                return Json(new { success = true, message = "Đã xóa tin đăng thành công. Tin đăng đã được gỡ khỏi sàn và bảo toàn lịch sử tin nhắn." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa bài: " + ex.Message });
            }
        }

        // GET: /Products/Favorites
        public IActionResult Favorites()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem danh sách tin yêu thích!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Products/Favorites" });
            }

            var favProducts = _db.Favorites.AsNoTracking()
                .Where(f => f.UserId == userId.Value)
                .Join(_db.Products.AsNoTracking(),
                    f => f.ProductId,
                    p => p.Id,
                    (f, p) => p)
                .Where(p => !p.IsSold && !p.Deleted)
                .ToList();

            return View(favProducts);
        }

        // POST: /Products/ToggleFavorite?productId=123
        [HttpPost]
        public IActionResult ToggleFavorite(int productId)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập để lưu tin yêu thích!" });
                }

                var existing = _db.Favorites.FirstOrDefault(f => f.UserId == userId.Value && f.ProductId == productId);
                bool isFav;

                if (existing != null)
                {
                    _db.Favorites.Remove(existing);
                    isFav = false;
                }
                else
                {
                    _db.Favorites.Add(new Favorite
                    {
                        UserId = userId.Value,
                        ProductId = productId,
                        CreatedAt = DateTime.Now
                    });
                    isFav = true;
                }

                _db.SaveChanges();
                return Json(new { success = true, isFav = isFav });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi thao tác yêu thích: " + ex.Message });
            }
        }
    }
}
```

---

## Controllers/CartController.cs

```csharp
using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ThriftDbContext _db;

        public CartController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Cart (Trang Giỏ hàng & Trao đổi Chat với người bán)
        public IActionResult Index(int? productId)
        {
            var prodId = productId ?? 1;
            var product = _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == prodId);
            if (product == null)
            {
                var post = _db.PendingPosts.AsNoTracking().FirstOrDefault(p => p.Id == prodId);
                if (post != null)
                {
                    product = new Product
                    {
                        Id = post.Id,
                        Name = post.Name,
                        CategoryId = post.CategoryId,
                        CategoryName = post.CategoryName,
                        Price = post.Price,
                        OriginalPrice = post.OriginalPrice,
                        Description = post.Description,
                        Image = post.Image,
                        ImagesJson = post.ImagesJson,
                        VideosJson = post.VideosJson,
                        SellerId = post.SellerId,
                        SellerName = post.SellerName,
                        SellerPhone = post.SellerPhone,
                        SellerEmail = post.SellerEmail,
                        SellerAddress = post.SellerAddress,
                        SellerAvatar = post.SellerAvatar,
                        Status = post.Status,
                        IsSold = post.IsSold
                    };
                }
            }

            ViewBag.ActiveProductId = prodId;
            ViewBag.ActiveProduct = product;
            return View();
        }

        // GET: /Cart/GetMessages?productId=...
        [HttpGet]
        public async Task<IActionResult> GetMessages(int productId)
        {
            try
            {
                var messages = await _db.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.ProductId == productId)
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        id = m.Id,
                        productId = m.ProductId,
                        senderId = m.SenderId,
                        senderName = m.SenderName,
                        senderAvatar = m.SenderAvatar,
                        receiverId = m.ReceiverId,
                        receiverName = m.ReceiverName,
                        receiverAvatar = m.ReceiverAvatar,
                        message = m.Message,
                        sentAt = m.SentAt.ToString("HH:mm"),
                        sentAtFull = m.SentAt.ToString("dd/MM/yyyy HH:mm")
                    })
                    .ToListAsync();

                return Json(new { success = true, messages });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải lịch sử tin nhắn: " + ex.Message, messages = Array.Empty<object>() });
            }
        }

        // GET: /Cart/GetConversations
        [HttpGet]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập", conversations = Array.Empty<object>() });
                }

                var userProductIds = await _db.Products
                    .Where(p => p.SellerId == userId.Value)
                    .Select(p => p.Id)
                    .ToListAsync();

                var messages = await _db.ChatMessages
                    .AsNoTracking()
                    .Where(m => m.SenderId == userId.Value || m.ReceiverId == userId.Value || userProductIds.Contains(m.ProductId))
                    .OrderByDescending(m => m.SentAt)
                    .ToListAsync();

                var grouped = messages.GroupBy(m => m.ProductId);
                var convList = new List<object>();

                foreach (var group in grouped)
                {
                    var pId = group.Key;
                    var lastMsg = group.First();
                    var product = await _db.Products.FindAsync(pId);

                    string prodName = product?.Name ?? "Món đồ #" + pId;
                    decimal prodPrice = product?.Price ?? 0;
                    string prodImg = product?.Image ?? "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600";
                    bool isSold = product?.IsSold ?? false;
                    bool isDeleted = product?.Deleted ?? false;
                    int sellerId = product?.SellerId ?? lastMsg.ReceiverId;
                    string sellerName = product?.SellerName ?? "Người bán";
                    string sellerPhone = product?.SellerPhone ?? "";
                    string sellerAvatar = product?.SellerAvatar ?? "";

                    bool isCurrentUserSeller = (userId.Value == sellerId);
                    string partnerName;
                    string? partnerAvatar;
                    string partnerPhone = "";
                    int partnerId;

                    if (isCurrentUserSeller)
                    {
                        var buyerMsg = group.FirstOrDefault(m => m.SenderId != userId.Value);
                        partnerName = buyerMsg?.SenderName ?? "Người mua quan tâm";
                        partnerAvatar = buyerMsg?.SenderAvatar;
                        partnerId = buyerMsg?.SenderId ?? (lastMsg.SenderId == userId.Value ? lastMsg.ReceiverId : lastMsg.SenderId);
                    }
                    else
                    {
                        partnerName = sellerName;
                        partnerAvatar = sellerAvatar;
                        partnerPhone = sellerPhone;
                        partnerId = sellerId;
                    }

                    convList.Add(new
                    {
                        id = "chat-" + pId,
                        productId = pId,
                        product = new
                        {
                            id = pId,
                            name = prodName,
                            price = prodPrice,
                            image = prodImg,
                            isSold = isSold,
                            isDeleted = isDeleted,
                            statusText = isSold ? "[Đã bán]" : (isDeleted ? "[Tin đã đóng]" : "(Tin đang mở bán · Còn hàng)")
                        },
                        partnerId = partnerId,
                        partnerName = partnerName,
                        partnerAvatar = partnerAvatar,
                        partnerPhone = partnerPhone,
                        sellerId = sellerId,
                        sellerName = sellerName,
                        sellerAvatar = sellerAvatar,
                        sellerPhone = sellerPhone,
                        lastMessage = lastMsg.Message,
                        lastTime = lastMsg.SentAt.ToString("HH:mm"),
                        unread = false
                    });
                }

                return Json(new { success = true, conversations = convList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, conversations = Array.Empty<object>() });
            }
        }

        // POST: /Cart/SendMessage
        [HttpPost]
        public async Task<IActionResult> SendMessage(int productId, string message, int? receiverId = null)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                var userName = HttpContext.Session.GetUserName() ?? "Thành viên";
                var userAvatar = HttpContext.Session.GetUserAvatar();

                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để gửi tin nhắn trao đổi!" });
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { success = false, message = "Nội dung tin nhắn không được để trống!" });
                }

                var product = await _db.Products.FindAsync(productId);
                int sellerId = product?.SellerId ?? 1;
                string sellerName = product?.SellerName ?? "Người bán";
                string? sellerAvatar = product?.SellerAvatar;

                if (product == null)
                {
                    var post = await _db.PendingPosts.FirstOrDefaultAsync(p => p.Id == productId);
                    if (post != null)
                    {
                        sellerId = post.SellerId;
                        sellerName = post.SellerName;
                        sellerAvatar = post.SellerAvatar;
                    }
                }

                int targetReceiverId;
                string targetReceiverName;
                string? targetReceiverAvatar = null;

                // Nếu người gửi là người mua (khác sellerId của sản phẩm)
                if (userId.Value != sellerId)
                {
                    targetReceiverId = sellerId;
                    targetReceiverName = !string.IsNullOrWhiteSpace(sellerName) ? sellerName : "Người bán";
                    targetReceiverAvatar = sellerAvatar;
                }
                else
                {
                    // Người gửi là người bán -> người nhận là người mua
                    if (receiverId.HasValue && receiverId.Value > 0 && receiverId.Value != userId.Value)
                    {
                        targetReceiverId = receiverId.Value;
                        var buyerUser = await _db.Users.FindAsync(targetReceiverId);
                        targetReceiverName = buyerUser?.FullName ?? "Người mua";
                        targetReceiverAvatar = buyerUser?.Avatar;
                    }
                    else
                    {
                        // Lấy người mua gần nhất đã gửi tin nhắn về sản phẩm này
                        var lastBuyerMsg = await _db.ChatMessages
                            .Where(m => m.ProductId == productId && m.SenderId != userId.Value)
                            .OrderByDescending(m => m.SentAt)
                            .FirstOrDefaultAsync();

                        if (lastBuyerMsg != null)
                        {
                            targetReceiverId = lastBuyerMsg.SenderId;
                            targetReceiverName = lastBuyerMsg.SenderName;
                            targetReceiverAvatar = lastBuyerMsg.SenderAvatar;
                        }
                        else
                        {
                            targetReceiverId = 2; // Người dùng mẫu mặc định
                            var u2 = await _db.Users.FindAsync(2);
                            targetReceiverName = u2?.FullName ?? "Người mua";
                            targetReceiverAvatar = u2?.Avatar;
                        }
                    }
                }

                var chatMsg = new ChatMessage
                {
                    ProductId = productId,
                    SenderId = userId.Value,
                    SenderName = userName,
                    SenderAvatar = userAvatar,
                    ReceiverId = targetReceiverId,
                    ReceiverName = targetReceiverName,
                    ReceiverAvatar = targetReceiverAvatar,
                    Message = message.Trim(),
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                _db.ChatMessages.Add(chatMsg);
                await _db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    message = "Gửi tin nhắn thành công!",
                    data = new
                    {
                        id = chatMsg.Id,
                        productId = chatMsg.ProductId,
                        senderId = chatMsg.SenderId,
                        senderName = chatMsg.SenderName,
                        senderAvatar = chatMsg.SenderAvatar,
                        receiverId = chatMsg.ReceiverId,
                        receiverName = chatMsg.ReceiverName,
                        receiverAvatar = chatMsg.ReceiverAvatar,
                        message = chatMsg.Message,
                        sentAt = chatMsg.SentAt.ToString("HH:mm"),
                        sentAtFull = chatMsg.SentAt.ToString("dd/MM/yyyy HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi gửi tin nhắn: " + ex.Message });
            }
        }

        // POST: /Cart/DeleteChat?productId=...
        [HttpPost]
        public async Task<IActionResult> DeleteChat(int? productId)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập!" });
                }

                if (productId.HasValue && productId.Value > 0)
                {
                    var msgs = await _db.ChatMessages
                        .Where(m => m.ProductId == productId.Value && (m.SenderId == userId.Value || m.ReceiverId == userId.Value))
                        .ToListAsync();

                    if (msgs.Any())
                    {
                        _db.ChatMessages.RemoveRange(msgs);
                        await _db.SaveChangesAsync();
                    }
                }

                return Json(new { success = true, message = "Đã xóa đoạn chat thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa đoạn chat: " + ex.Message });
            }
        }

        // GET: /Cart/Checkout (Trang thanh toán đơn hàng)
        public IActionResult Checkout()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiến hành thanh toán đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
            }

            var userId = HttpContext.Session.GetUserId();
            var user = userId.HasValue ? _db.Users.Find(userId.Value) : null;
            return View(user);
        }

        // POST: /Cart/Checkout ([FromForm] CheckoutRequestDto)
        [HttpPost]
        public IActionResult Checkout([FromForm] CheckoutRequestDto dto)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để tiến hành đặt hàng!" });
                }

                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    return Json(new { success = false, message = firstError ?? "Thông tin nhận hàng không hợp lệ!" });
                }

                if (string.IsNullOrWhiteSpace(dto.ItemsJson) || dto.ItemsJson.Trim() == "[]")
                {
                    return Json(new { success = false, message = "Giỏ hàng của bạn đang trống! Vui lòng chọn món đồ trước khi đặt." });
                }

                using var doc = JsonDocument.Parse(dto.ItemsJson);
                var itemsArray = doc.RootElement;
                if (itemsArray.ValueKind != JsonValueKind.Array || itemsArray.GetArrayLength() == 0)
                {
                    return Json(new { success = false, message = "Danh sách sản phẩm giỏ hàng không hợp lệ!" });
                }

                var order = new Order
                {
                    CustName = dto.CustName.Trim(),
                    CustPhone = dto.CustPhone.Trim(),
                    CustEmail = dto.CustEmail?.Trim() ?? string.Empty,
                    CustAddress = dto.CustAddress.Trim(),
                    PaymentMethod = dto.PaymentMethod ?? "COD - Thanh toán khi nhận hàng",
                    Status = "pending",
                    OrderDate = DateTime.Now,
                    UserId = userId.Value,
                    Notes = dto.Notes?.Trim() ?? "Đồng kiểm ngoại hình khi nhận hàng"
                };

                decimal calculatedTotal = 0;

                foreach (var elem in itemsArray.EnumerateArray())
                {
                    int pId = elem.TryGetProperty("id", out var idProp) ? idProp.GetInt32()
                            : (elem.TryGetProperty("productId", out var pidProp) ? pidProp.GetInt32() : 0);
                    int qty = elem.TryGetProperty("quantity", out var qProp) ? qProp.GetInt32() : 1;
                    if (qty <= 0) qty = 1;

                    var prod = _db.Products.FirstOrDefault(p => p.Id == pId && !p.Deleted);
                    if (prod == null)
                    {
                        return Json(new { success = false, message = $"Món đồ (mã #{pId}) không còn tồn tại hoặc đã bị gỡ khỏi sàn!" });
                    }

                    if (prod.IsSold || prod.Stock < qty)
                    {
                        return Json(new { success = false, message = $"Món đồ '{prod.Name}' đã hết hàng hoặc không đủ tồn kho (còn: {prod.Stock})!" });
                    }

                    decimal lineTotal = prod.Price * qty;
                    calculatedTotal += lineTotal;

                    // Cập nhật tồn kho sản phẩm
                    prod.Stock -= qty;
                    if (prod.Stock <= 0)
                    {
                        prod.Stock = 0;
                        prod.IsSold = true;
                        prod.Status = "sold";

                        // Đồng bộ trạng thái đã bán sang PendingPost nếu có liên kết
                        if (prod.PendingPostId.HasValue)
                        {
                            var linkedPost = _db.PendingPosts.FirstOrDefault(p => p.Id == prod.PendingPostId.Value);
                            if (linkedPost != null)
                            {
                                linkedPost.IsSold = true;
                                linkedPost.Status = "sold";
                            }
                        }
                    }

                    var item = new OrderItem
                    {
                        ProductId = prod.Id,
                        ProductName = prod.Name,
                        Image = prod.Image,
                        Price = prod.Price,
                        Quantity = qty,
                        Subtotal = lineTotal
                    };
                    order.OrderItems.Add(item);
                }

                if (order.OrderItems.Count == 0)
                {
                    return Json(new { success = false, message = "Không có sản phẩm hợp lệ trong giỏ hàng!" });
                }

                order.TotalAmount = calculatedTotal > 0 ? calculatedTotal : dto.TotalAmount;

                _db.Orders.Add(order);
                _db.SaveChanges();

                return Json(new { success = true, orderId = order.OrderCode });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi máy chủ khi tạo đơn hàng: " + ex.Message });
            }
        }

        // GET: /Cart/OrderSuccess?orderId=DH000123
        public IActionResult OrderSuccess(string? orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return RedirectToAction("Index", "Orders");
            }

            int numericId = 0;
            if (orderId.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(orderId.Substring(2), out numericId);
            }
            else
            {
                int.TryParse(orderId, out numericId);
            }

            var order = _db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == numericId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng này!";
                return RedirectToAction("Index", "Orders");
            }

            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();
            if (!isAdmin && userId.HasValue && order.UserId != userId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem thông tin đơn hàng này!";
                return RedirectToAction("Index", "Orders");
            }

            ViewBag.OrderId = order.OrderCode;
            return View(order);
        }
    }
}
```

---

## Controllers/OrdersController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ThriftDbContext _db;

        public OrdersController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Orders
        public IActionResult Index(string? search, string? status)
        {
            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem danh sách đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Orders" });
            }

            var query = _db.Orders.Include(o => o.OrderItems).AsNoTracking();

            if (!isAdmin)
            {
                query = query.Where(o => o.UserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                query = query.Where(o => o.Status.ToLower() == status.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(o => o.CustName.ToLower().Contains(q) || o.CustPhone.Contains(q));
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // GET: /Orders/GetMyOrders
        // Trả về JSON mảng OrderSummaryDto cho frontend fetch
        [HttpGet]
        public IActionResult GetMyOrders()
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                var isAdmin = HttpContext.Session.IsAdmin();

                if (!userId.HasValue)
                {
                    return Json(new List<OrderSummaryDto>());
                }

                var query = _db.Orders.Include(o => o.OrderItems).AsNoTracking();
                if (!isAdmin)
                {
                    query = query.Where(o => o.UserId == userId.Value);
                }

                var rawOrders = query.OrderByDescending(o => o.OrderDate).ToList();

                var dtoList = rawOrders.Select(o => new OrderSummaryDto
                {
                    Id = o.OrderCode,
                    OrderDate = o.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    OrderStatus = (o.Status ?? "pending").ToLower(),
                    CustomerName = o.CustName,
                    CustomerPhone = o.CustPhone,
                    ShippingAddress = o.CustAddress,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Image = oi.Image,
                        Price = oi.Price,
                        Quantity = oi.Quantity
                    }).ToList()
                }).ToList();

                return Json(dtoList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
        }

        // GET: /Orders/Details/DH000123
        public IActionResult Details(string? id)
        {
            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem chi tiết đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Orders/Details/{id}" });
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction(nameof(Index));
            }

            int numericId = 0;
            if (id.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
                int.TryParse(id.Substring(2), out numericId);
            else
                int.TryParse(id, out numericId);

            var order = _db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == numericId);
            if (order == null || (!isAdmin && order.UserId != userId.Value))
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.OrderId = order.OrderCode;
            return View("Index", new List<Order> { order });
        }
    }
}
```

---

## Controllers/AdminController.cs

```csharp
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    public class AdminController : Controller
    {
        private readonly ThriftDbContext _db;

        public AdminController(ThriftDbContext db)
        {
            _db = db;
        }

        // Phương thức kiểm tra quyền Admin nghiêm ngặt Server-side
        private IActionResult? CheckAdminPermission(string? currentUrl = "/Admin")
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản Ban Quản Trị (admin@thriftshop.vn) để truy cập!";
                return RedirectToAction("Login", "Account", new { returnUrl = currentUrl });
            }

            if (!HttpContext.Session.IsAdmin())
            {
                TempData["ErrorMessage"] = "CẢNH BÁO: Bạn không có quyền Quản Trị Viên! Chỉ tài khoản Ban Quản Trị (admin@thriftshop.vn) mới được phép truy cập.";
                return RedirectToAction("Index", "Home");
            }

            return null;
        }

        // GET: /Admin
        public IActionResult Index()
        {
            var authCheck = CheckAdminPermission("/Admin");
            if (authCheck != null) return authCheck;

            var pendingCount = _db.PendingPosts.Count(p => p.Status == "pending");
            var categoriesCount = _db.Categories.Count();
            var productsCount = _db.Products.Count(p => p.Status == "approved" && !p.IsSold && !p.Deleted);
            var usersCount = _db.Users.Count();

            var recentPending = _db.PendingPosts.AsNoTracking()
                .Where(p => p.Status == "pending")
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToList();

            var recentProducts = _db.Products.AsNoTracking()
                .Where(p => p.Status == "approved" && !p.IsSold && !p.Deleted)
                .OrderByDescending(p => p.Id)
                .Take(5)
                .ToList();

            var viewModel = new AdminDashboardViewModel
            {
                PendingCount = pendingCount,
                CategoriesCount = categoriesCount,
                ProductsCount = productsCount,
                UsersCount = usersCount,
                RecentPending = recentPending,
                RecentProducts = recentProducts
            };

            return View(viewModel);
        }

        // GET: /Admin/ApproveProducts
        public IActionResult ApproveProducts()
        {
            var authCheck = CheckAdminPermission("/Admin/ApproveProducts");
            if (authCheck != null) return authCheck;

            var allPosts = _db.PendingPosts.AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(allPosts);
        }

        // POST: /Admin/ApprovePost?id=123
        [HttpPost]
        public IActionResult ApprovePost(int id)
        {
            try
            {
                var authCheck = CheckAdminPermission("/Admin/ApproveProducts");
                if (authCheck != null) return Json(new { success = false, message = "Bạn không có quyền quản trị viên!" });

                var post = _db.PendingPosts.FirstOrDefault(p => p.Id == id);
                if (post == null) return Json(new { success = false, message = "Không tìm thấy tin đăng!" });

                if (post.Status == "approved")
                {
                    return Json(new { success = false, message = "Tin đăng này đã được duyệt mở bán trước đó!" });
                }

                post.Status = "approved";

                var existingProduct = _db.Products.FirstOrDefault(p => p.PendingPostId == post.Id);
                if (existingProduct != null)
                {
                    existingProduct.Status = "approved";
                    existingProduct.IsSold = false;
                    existingProduct.Deleted = false;
                    existingProduct.Price = post.Price;
                    existingProduct.Name = post.Name;
                    existingProduct.Description = post.Description;
                }
                else
                {
                    var product = new Product
                    {
                        PendingPostId = post.Id,
                        Name = post.Name,
                        CategoryId = post.CategoryId,
                        CategoryName = post.CategoryName,
                        Price = post.Price,
                        OriginalPrice = post.OriginalPrice,
                        Description = post.Description,
                        Image = post.Image,
                        ImagesJson = post.ImagesJson,
                        VideosJson = post.VideosJson,
                        Stock = post.Stock,
                        SellerId = post.SellerId,
                        SellerName = post.SellerName,
                        SellerPhone = post.SellerPhone,
                        SellerEmail = post.SellerEmail,
                        SellerAddress = post.SellerAddress,
                        SellerAvatar = post.SellerAvatar,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = DateTime.Now
                    };
                    _db.Products.Add(product);
                }

                _db.SaveChanges();
                TempData["SuccessMessage"] = $"Đã duyệt thành công tin: '{post.Name}'. Món đồ đã được mở bán công khai trên sàn!";
                return Json(new { success = true, message = "Đã duyệt bài đăng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi duyệt bài: " + ex.Message });
            }
        }

        // POST: /Admin/RejectPost?id=123&reason=...
        [HttpPost]
        public IActionResult RejectPost(int id, string? reason)
        {
            try
            {
                var authCheck = CheckAdminPermission("/Admin/ApproveProducts");
                if (authCheck != null) return Json(new { success = false, message = "Bạn không có quyền quản trị viên!" });

                var post = _db.PendingPosts.FirstOrDefault(p => p.Id == id);
                if (post == null) return Json(new { success = false, message = "Không tìm thấy tin đăng!" });

                post.Status = "rejected";
                post.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Không đạt tiêu chuẩn kiểm duyệt sàn đồ cũ." : reason.Trim();

                var linkedProduct = _db.Products.FirstOrDefault(p => p.PendingPostId == post.Id);
                if (linkedProduct != null)
                {
                    _db.Products.Remove(linkedProduct);
                }

                _db.SaveChanges();
                return Json(new { success = true, message = "Đã từ chối bài đăng" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi từ chối bài: " + ex.Message });
            }
        }

        // GET: /Admin/Products
        public IActionResult Products()
        {
            var authCheck = CheckAdminPermission("/Admin/Products");
            if (authCheck != null) return authCheck;

            var products = _db.Products.AsNoTracking()
                .Where(p => !p.Deleted)
                .OrderByDescending(p => p.Id)
                .ToList();

            ViewBag.Categories = _db.Categories.AsNoTracking().ToList();
            return View(products);
        }

        // GET: /Admin/Users
        public IActionResult Users()
        {
            var authCheck = CheckAdminPermission("/Admin/Users");
            if (authCheck != null) return authCheck;

            var users = _db.Users.AsNoTracking()
                .OrderByDescending(u => u.Id)
                .ToList();

            return View(users);
        }

        // GET: /Admin/Categories
        public IActionResult Categories()
        {
            var authCheck = CheckAdminPermission("/Admin/Categories");
            if (authCheck != null) return authCheck;

            var categories = _db.Categories.AsNoTracking().ToList();

            var prodCounts = _db.Products
                .Where(p => !p.Deleted)
                .GroupBy(p => p.CategoryId)
                .ToDictionary(g => g.Key, g => g.Count());

            ViewBag.ProductCounts = prodCounts;
            return View(categories);
        }

        // POST: /Admin/SaveCategory ([FromForm] SaveCategoryDto)
        [HttpPost]
        public IActionResult SaveCategory([FromForm] SaveCategoryDto dto)
        {
            try
            {
                var authCheck = CheckAdminPermission("/Admin/Categories");
                if (authCheck != null) return Json(new { success = false, message = "Bạn không có quyền quản trị viên!" });

                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    return Json(new { success = false, message = firstError ?? "Dữ liệu danh mục không hợp lệ!" });
                }

                if (dto.Id > 0)
                {
                    var existing = _db.Categories.Find(dto.Id);
                    if (existing == null)
                    {
                        return Json(new { success = false, message = "Không tìm thấy danh mục cần sửa!" });
                    }

                    existing.Name = dto.Name.Trim();
                    existing.Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "fas fa-tag" : dto.Icon.Trim();
                    existing.Description = dto.Description?.Trim();
                }
                else
                {
                    var newCat = new Category
                    {
                        Name = dto.Name.Trim(),
                        Icon = string.IsNullOrWhiteSpace(dto.Icon) ? "fas fa-tag" : dto.Icon.Trim(),
                        Description = dto.Description?.Trim()
                    };
                    _db.Categories.Add(newCat);
                }

                _db.SaveChanges();
                return Json(new { success = true, message = "Đã lưu danh mục thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi lưu danh mục: " + ex.Message });
            }
        }

        // POST: /Admin/DeleteCategory?id=123
        [HttpPost]
        public IActionResult DeleteCategory(int id)
        {
            try
            {
                var authCheck = CheckAdminPermission("/Admin/Categories");
                if (authCheck != null) return Json(new { success = false, message = "Bạn không có quyền quản trị viên!" });

                var productCount = _db.Products.Count(p => p.CategoryId == id && !p.Deleted);
                if (productCount > 0)
                {
                    return Json(new
                    {
                        success = false,
                        message = $"KHÔNG THỂ XÓA! Danh mục này hiện đang có {productCount} sản phẩm đồ cũ thuộc về nó. Vui lòng chuyển hoặc xóa các sản phẩm liên quan trước."
                    });
                }

                var cat = _db.Categories.Find(id);
                if (cat == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy danh mục để xóa!" });
                }

                _db.Categories.Remove(cat);
                _db.SaveChanges();
                return Json(new { success = true, message = "Đã xóa danh mục thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa danh mục: " + ex.Message });
            }
        }

        // GET: /Admin/Orders
        public IActionResult Orders()
        {
            var authCheck = CheckAdminPermission("/Admin/Orders");
            if (authCheck != null) return authCheck;

            var orders = _db.Orders.Include(o => o.OrderItems).AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }
    }
}
```

---

# FRONTEND - RAZOR VIEWS

## Views/_ViewImports.cshtml

```html
@using ThriftShop
@using ThriftShop.Models
@using ThriftShop.Common
@addTagHelper *, Microsoft.AspNetCore.Mvc.TagHelpers
```

---

## Views/_ViewStart.cshtml

```html
@{
    Layout = "_Layout";
}
```

---

## Views/Shared/_Layout.cshtml

```html
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - ThriftShop | Website Mua Bán Đồ Cũ Secondhand</title>
    <!-- Fonts -->
    <link rel="preconnect" href="https://fonts.googleapis.com">
    <link rel="preconnect" href="https://fonts.gstatic.com" crossorigin="anonymous">
    <link href="https://fonts.googleapis.com/css2?family=Poppins:wght@300;400;500;600;700;800&display=swap" rel="stylesheet">
    <!-- Font Awesome -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/6.4.0/css/all.min.css" />
    <!-- Page Transition -->
    <link rel="stylesheet" href="~/css/page-transition.css" asp-append-version="true" />
    <!-- Custom Style (WebApplication8 aesthetic) -->
    <link rel="stylesheet" href="~/css/style.css" asp-append-version="true" />
    <!-- Immediate Synchronous Route Guard for Protected Pages -->
    <script>
        (function() {
            try {
                var p = window.location.pathname.toLowerCase();
                var protectedRoutes = ['/cart', '/products/postitem', '/products/mylistings', '/admin', '/account/profile', '/products/favorites'];
                var isProtected = protectedRoutes.some(function(r) { return p === r || p.indexOf(r + '/') === 0; });
                if (isProtected) {
                    var u = localStorage.getItem("thrift_current_user");
                    if (!u || u === "null" || u === "undefined") {
                        var returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
                        window.location.replace('/Account/Login?returnUrl=' + returnUrl);
                    }
                }
            } catch(e) {}
        })();
    </script>
</head>
<body>
    <!-- HEADER CHUẨN CAMERA917 -->
    <header>
        <div class="top-bar">
            <!-- Brand Logo -->
            <div class="logo">
                <a asp-controller="Home" asp-action="Index">Thrift<span>Shop</span></a>
            </div>

            <!-- Search Bar -->
            <div class="search">
                <form class="search-form" id="searchForm" asp-controller="Products" asp-action="Index" method="get">
                    <input name="search" id="searchInput" type="search" placeholder="Tìm kiếm đồ cũ: quần áo vintage, công nghệ, giày dép, sách cũ..." autocomplete="off">
                    <button type="submit" aria-label="Tìm kiếm">🔎</button>
                </form>
            </div>

            <!-- User Actions -->
            <div class="user">
                <!-- Người dùng & Tài khoản: RENDER TỪ SERVER-SIDE SESSION (ASP.NET MVC) -->
                <div id="user-header-auth" style="display: inline-flex; align-items: center; gap: 12px;">
                    @if (Context.Session.IsLoggedIn())
                    {
                        <div class="dropdown" style="position: relative; display: inline-block;">
                            <a href="javascript:void(0)" class="dropdown-toggle" data-toggle="dropdown" style="display: inline-flex; align-items: center; gap: 8px; text-decoration: none; color: #18181b; font-weight: 600;">
                                <img src="@(Context.Session.GetUserAvatar() ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80")" alt="Avatar" style="width: 32px; height: 32px; border-radius: 50%; object-fit: cover; border: 1.5px solid #ff3c00;">
                                <span>@Context.Session.GetUserName()</span>
                            </a>
                            <div class="dropdown-menu dropdown-menu-right shadow-sm border-0" style="min-width: 200px; border-radius: 10px; margin-top: 8px;">
                                <a class="dropdown-item" asp-controller="Account" asp-action="Profile">
                                    <i class="fas fa-user-cog mr-2"></i>Cài đặt tài khoản
                                </a>
                                <a class="dropdown-item" asp-controller="Products" asp-action="Favorites">
                                    <i class="fas fa-heart mr-2 text-danger"></i>Tin đã yêu thích
                                </a>
                                <a class="dropdown-item" asp-controller="Account" asp-action="SellerProfile" asp-route-userId="@Context.Session.GetUserId()">
                                    <i class="fas fa-id-badge mr-2"></i>Trang cá nhân của tôi
                                </a>
                                @if (Context.Session.IsAdmin())
                                {
                                    <div class="dropdown-divider"></div>
                                    <a class="dropdown-item text-warning font-weight-bold" asp-controller="Admin" asp-action="Index">
                                        <i class="fas fa-user-shield mr-2"></i>Trang Quản Trị (Admin)
                                    </a>
                                }
                                <div class="dropdown-divider"></div>
                                <a class="dropdown-item text-danger" asp-controller="Account" asp-action="Logout" onclick="try { localStorage.removeItem('thrift_current_user'); sessionStorage.clear(); } catch(e){}">
                                    <i class="fas fa-sign-out-alt mr-2"></i>Đăng xuất
                                </a>
                            </div>
                        </div>
                        <script>
                            try {
                                localStorage.setItem("thrift_current_user", JSON.stringify({
                                    id: @Context.Session.GetUserId(),
                                    name: "@Html.Raw(Context.Session.GetUserName())",
                                    email: "@Html.Raw(Context.Session.GetUserEmail())",
                                    role: "@Html.Raw(Context.Session.GetUserRole())",
                                    avatar: "@Html.Raw(Context.Session.GetUserAvatar())"
                                }));
                            } catch(e) {}
                        </script>
                    }
                    else
                    {
                        <a asp-controller="Account" asp-action="Login" id="auth-link">Đăng nhập</a>
                        <a asp-controller="Account" asp-action="Register" id="register-link">Đăng ký</a>
                        <script>
                            try {
                                localStorage.removeItem("thrift_current_user");
                                sessionStorage.removeItem("thrift_session_active");
                            } catch(e) {}
                        </script>
                    }
                </div>

                <!-- Liên hệ & Hộp thư Chat -->
                <a asp-controller="Cart" asp-action="Index" class="cart" title="Liên hệ & Hộp thư Chat" onclick="return handleCheckAuthNav(event, '/Cart');">
                    💬 Liên hệ <span class="cart-count" id="cartCount">0</span>
                </a>
            </div>
        </div>

        <!-- NAVIGATION MENU -->
        <nav>
            <ul>
                <li><a asp-controller="Home" asp-action="Index">Trang chủ</a></li>
                <li><a asp-controller="Products" asp-action="PostItem" onclick="return handleCheckAuthNav(event, '/Products/PostItem');">Đăng bán đồ cũ</a></li>
                <li><a asp-controller="Products" asp-action="MyListings" onclick="return handleCheckAuthNav(event, '/Products/MyListings');">Tin của tôi</a></li>
                <li><a asp-controller="Home" asp-action="Warranty">Chính sách bảo đảm</a></li>
            </ul>
        </nav>
    </header>

    <!-- MAIN CONTENT (FULL WIDTH - KHÔNG KHOẢNG TRẮNG 2 BÊN) -->
    <main style="width: 100%; margin: 0; padding: 0;">
        @RenderBody()
    </main>

    <!-- Toast Notification -->
    <div id="toast" class="toast">
        <i id="toast-icon" class="fas fa-check-circle"></i>
        <span id="toast-message">Thành công!</span>
    </div>

    <!-- FOOTER -->
    <footer>
        <div class="footer-container">
            <div class="footer-col">
                <h4>ThriftShop Secondhand</h4>
                <p>Nền tảng mua bán đồ cũ trực tuyến, thúc đẩy tái sử dụng và lối sống xanh bền vững.</p>
                <p style="margin-top: 8px; color: #cbd5e1;"><i class="fas fa-shield-alt text-warning mr-1" style="color: #fbbf24;"></i>Mọi tin rao đều được Ban Quản Trị kiểm duyệt chất lượng trước khi mở bán.</p>
            </div>
            <div class="footer-col">
                <h4>Danh Mục & Dịch Vụ</h4>
                <ul>
                    <li><a asp-controller="Products" asp-action="Index">Tất Cả Tin Bán Đồ Cũ</a></li>
                    <li><a asp-controller="Products" asp-action="PostItem">Đăng Bán Đồ Cũ Mới</a></li>
                    <li><a asp-controller="Products" asp-action="MyListings">Quản Lý Tin Của Tôi</a></li>
                    <li><a asp-controller="Products" asp-action="Favorites">Tin Đã Yêu Thích</a></li>
                </ul>
            </div>
            <div class="footer-col">
                <h4>Hỗ trợ</h4>
                <p><i class="fas fa-shield-alt mr-1"></i> <a asp-controller="Home" asp-action="Warranty" style="color: #cbd5e1; text-decoration: none;">Chính Sách Bảo Đảm</a></p>
                <p><i class="fas fa-phone mr-1"></i> Hotline hỗ trợ: 1900 6868</p>
                <p><i class="fas fa-envelope mr-1"></i> lienhe@thriftshop.vn</p>
            </div>
        </div>
        <div class="footer-bottom">
            <p>&copy; 2026 ThriftShop - Đề tài 30: Website Mua Bán Đồ Cũ Secondhand.</p>
        </div>
    </footer>

    <!-- Core Scripts (jQuery, Bootstrap, Data.js, Main.js, Transition.js) -->
    <script src="https://code.jquery.com/jquery-3.5.1.min.js"></script>
    <script src="https://cdn.jsdelivr.net/npm/bootstrap@4.5.2/dist/js/bootstrap.bundle.min.js"></script>
    <script src="~/js/data.js" asp-append-version="true"></script>
    <script src="~/js/main.js" asp-append-version="true"></script>
    <script src="~/js/page-transition.js" asp-append-version="true"></script>
    <script>
        function showToast(message, type = 'success') {
            const toast = document.getElementById('toast');
            const toastMessage = document.getElementById('toast-message');
            const toastIcon = document.getElementById('toast-icon');
            if (!toast) return;

            toastMessage.textContent = message;
            toast.className = `toast toast-${type} show`;

            if (type === 'success') {
                toastIcon.className = 'fas fa-check-circle';
            } else {
                toastIcon.className = 'fas fa-exclamation-circle';
            }

            setTimeout(() => {
                toast.classList.remove('show');
            }, 3000);
        }

        // Cập nhật số lượng tin chờ duyệt trên Header
        $(document).ready(function() {
            if (typeof ThriftDB !== 'undefined') {
                const pending = ThriftDB.getPendingCount();
                $('#nav-pending-count').text(pending);
            }

            @if (TempData["SuccessMessage"] != null)
            {
                <text>
                showToast("@Html.Raw(TempData["SuccessMessage"])", "success");
                </text>
            }
            @if (TempData["ErrorMessage"] != null)
            {
                <text>
                showToast("@Html.Raw(TempData["ErrorMessage"])", "error");
                </text>
            }
        });
    </script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

---

## Views/Shared/_AdminLayout.cshtml

```html
@inject ThriftShop.Data.ThriftDbContext _dbAdminLayout
@{
    int realServerPendingCount = 0;
    try
    {
        realServerPendingCount = _dbAdminLayout.PendingPosts.Count(p => p.Status == "pending");
    }
    catch { }
}
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="utf-8" />
    <meta name="viewport" content="width=device-width, initial-scale=1.0" />
    <title>@ViewData["Title"] - ThriftShop Quản Trị</title>
    <!-- Bootstrap 4.5.2 CSS -->
    <link rel="stylesheet" href="https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/css/bootstrap.min.css" />
    <!-- Font Awesome 5 Free -->
    <link rel="stylesheet" href="https://cdnjs.cloudflare.com/ajax/libs/font-awesome/5.15.1/css/all.min.css" />
    <!-- Admin CSS -->
    <link rel="stylesheet" href="~/css/admin.css" asp-append-version="true" />
</head>
<body class="admin-body">
    <script>
        window.serverRealPendingCount = @realServerPendingCount;
    </script>
    <div class="admin-wrapper">
        <!-- SIDEBAR -->
        <div class="admin-sidebar">
            <div class="sidebar-header">
                <a asp-controller="Admin" asp-action="Index" class="sidebar-brand">
                    THRIFTSHOP ADMIN
                </a>
            </div>
            @{
                var currentAction = ViewContext.RouteData.Values["Action"]?.ToString() ?? "";
            }
            <ul class="sidebar-menu">
                <li class="menu-header">Menu Quản Trị</li>
                <li class="@(currentAction == "Index" ? "active" : "")">
                    <a asp-controller="Admin" asp-action="Index">
                        Bảng Điều Khiển
                    </a>
                </li>
                <li class="@(currentAction == "ApproveProducts" ? "active" : "")">
                    <a asp-controller="Admin" asp-action="ApproveProducts">
                        Duyệt Tin Đăng
                        <span class="badge badge-secondary" id="admin-pending-badge">@realServerPendingCount</span>
                    </a>
                </li>
                <li class="@(currentAction == "Categories" ? "active" : "")">
                    <a asp-controller="Admin" asp-action="Categories">
                        Quản Lý Danh Mục
                    </a>
                </li>
                <li class="@(currentAction == "Products" ? "active" : "")">
                    <a asp-controller="Admin" asp-action="Products">
                        Quản Lý Tin Đăng
                    </a>
                </li>
                <li class="@(currentAction == "Users" ? "active" : "")">
                    <a asp-controller="Admin" asp-action="Users">
                        Quản Lý Người Dùng
                    </a>
                </li>

                <li class="menu-header">Hệ Thống</li>
                <li>
                    <a asp-controller="Home" asp-action="Index" target="_blank">
                        Xem Trang Khách Hàng
                    </a>
                </li>
                <li>
                    <a asp-controller="Account" asp-action="Logout" onclick="try { localStorage.removeItem('thrift_current_user'); sessionStorage.clear(); } catch(e){}" style="color: #495057;">
                        Đăng Xuất
                    </a>
                </li>
            </ul>
        </div>

        <!-- MAIN CONTENT -->
        <div class="admin-main">
            <!-- TOPBAR ĐƠN SẮC -->
            <header class="admin-topbar">
                <div>
                    <h5 class="mb-0 font-weight-bold text-dark" style="font-size: 14.5px;">@ViewData["Title"]</h5>
                </div>
                <div class="topbar-right">
                    <a asp-controller="Admin" asp-action="ApproveProducts" class="text-dark mr-3" style="text-decoration: none;">
                        Chờ duyệt: <span class="badge badge-secondary" id="admin-topbar-pending-alert">@realServerPendingCount</span>
                    </a>
                    <a asp-controller="Home" asp-action="Index" target="_blank" class="text-dark mr-3" style="text-decoration: none;">
                        Xem website
                    </a>
                    <span class="text-muted">
                        Tài khoản: <strong class="text-dark">admin@thriftshop.vn</strong>
                    </span>
                    <a asp-controller="Account" asp-action="Logout" class="text-dark font-weight-bold ml-2" onclick="try { localStorage.removeItem('thrift_current_user'); sessionStorage.clear(); } catch(e){}">
                        [Đăng xuất]
                    </a>
                </div>
            </header>

            <!-- PAGE CONTENT -->
            <main class="admin-page-content">
                @RenderBody()
            </main>
        </div>
    </div>

    <!-- Scripts -->
    <script src="https://ajax.googleapis.com/ajax/libs/jquery/3.5.1/jquery.min.js"></script>
    <script src="https://cdnjs.cloudflare.com/ajax/libs/popper.js/1.16.0/umd/popper.min.js"></script>
    <script src="https://maxcdn.bootstrapcdn.com/bootstrap/4.5.2/js/bootstrap.min.js"></script>
    <!-- Data & Admin scripts -->
    <script src="~/js/data.js" asp-append-version="true"></script>
    <script src="~/js/admin.js" asp-append-version="true"></script>
    @await RenderSectionAsync("Scripts", required: false)
</body>
</html>
```

---

## Views/Shared/Error.cshtml

```html
@model ErrorViewModel
@{
    ViewData["Title"] = "Lỗi";
}

<div class="container my-5 text-center">
    <h1 class="text-danger font-weight-bold">Đã xảy ra lỗi!</h1>
    <h4 class="text-muted mb-4">Yêu cầu của bạn không thể hoàn tất.</h4>

    @if (Model.ShowRequestId)
    {
        <p class="small text-muted">
            <strong>Mã Request:</strong> <code>@Model.RequestId</code>
        </p>
    }

    <a asp-controller="Home" asp-action="Index" class="btn btn-success font-weight-bold mt-3">
        <i class="fas fa-home mr-1"></i>Về Trang Chủ
    </a>
</div>
```

---

## Views/Home/Index.cshtml

```html
@model ThriftShop.Models.HomeViewModel
@{
    ViewData["Title"] = "ThriftShop - Mua Bán Đồ Cũ Secondhand Uy Tín";
}

<!-- 1. HERO BANNER (KHUNG GIAO DIỆN CAMERA917 - CHỦ ĐỀ THRIFT SHOP / SECONDHAND) -->
<section class="hero">
    <div class="overlay"></div>
    <div class="hero-content">
        <h1>♻ Sàn Đồ Cũ 2nd</h1>
        <p>Nền tảng thanh lý & mua bán đồ cũ Secondhand uy tín. Mọi món đồ đăng bán đều được Admin kiểm định chất lượng và tình trạng thực tế trước khi mở bán.</p>
        <div class="hero-badges-group">
            <span class="hero-note">Kiểm định chất lượng 100%</span>
            <a asp-controller="Products" asp-action="PostItem" class="hero-note-secondary">
                <i class="fas fa-plus-circle mr-1"></i> + Đăng bán đồ cũ ngay
            </a>
        </div>
    </div>
</section>


<!-- 3. KHU VỰC CHÍNH: BỘ LỌC 2ND (BÊN TRÁI) VÀ SẢN PHẨM MODERN (BÊN PHẢI) -->
<div class="shop-layout-container">
    <!-- CỘT TRÁI: BỘ LỌC 2ND (THEO MẪU ẢNH 3) -->
    <aside class="shop-filter-sidebar">
        <h3 class="filter-main-title">Bộ lọc 2nd</h3>

        <!-- NHÓM 1: DANH MỤC (5 DANH MỤC CHUẨN) -->
        <div class="filter-group-title">Danh mục</div>
        <div class="filter-checkbox-list">
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="1" onchange="onFilterChange()">
                <span>Đồ điện tử</span>
            </label>
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="2" onchange="onFilterChange()">
                <span>Đồ gia dụng</span>
            </label>
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="3" onchange="onFilterChange()">
                <span>Văn phòng phẩm</span>
            </label>
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="4" onchange="onFilterChange()">
                <span>Thời trang</span>
            </label>
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="5" onchange="onFilterChange()">
                <span>Khác</span>
            </label>
        </div>

        <hr class="filter-divider">

        <!-- NHÓM 2: KHOẢNG GIÁ (DUAL RANGE SLIDER + INPUT TỪ - ĐẾN) -->
        <div class="filter-group-title">Khoảng giá</div>
        <div class="dual-range-wrapper">
            <div class="dual-range-track"></div>
            <div class="dual-range-fill" id="sliderFill"></div>
            <input type="range" id="priceRangeMin" min="0" max="10000000" step="50000" value="0" oninput="onRangeMinInput()">
            <input type="range" id="priceRangeMax" min="0" max="10000000" step="50000" value="10000000" oninput="onRangeMaxInput()">
        </div>

        <div class="price-inputs-row">
            <input type="number" id="priceInputMin" placeholder="Từ" min="0" step="10000" onchange="onMinInputChange()">
            <input type="number" id="priceInputMax" placeholder="Đến" min="0" step="10000" onchange="onMaxInputChange()">
        </div>

        <div class="filter-action-buttons">
            <button type="button" class="btn-filter-apply" onclick="applyFilters()">Lọc</button>
            <button type="button" class="btn-filter-reset" onclick="resetFilters()">Xóa</button>
        </div>
    </aside>

    <!-- CỘT PHẢI: DANH SÁCH SẢN PHẨM (THEO MẪU ẢNH 4 & 5) -->
    <section class="shop-content-area">
        <div class="shop-content-header">
            <div class="shop-prod-count">
                Hiển thị <strong id="productCount">@Model.TotalCount</strong> sản phẩm
            </div>
            <div class="shop-sort-box">
                <label for="sortFilter">Sắp xếp:</label>
                <select id="sortFilter" onchange="onSortChange()">
                    <option value="newest" selected="@(Model.Sort == "newest")">Mới đăng</option>
                    <option value="price-asc" selected="@(Model.Sort == "price-asc")">Giá: Thấp &rarr; Cao</option>
                    <option value="price-desc" selected="@(Model.Sort == "price-desc")">Giá: Cao &rarr; Thấp</option>
                </select>
            </div>
        </div>

        <!-- PRODUCT GRID (DARK CARDS THEO ẢNH 4) - RENDER SERVER-SIDE TỪ LINQ & EF MODEL -->
        <div class="product-grid-dark" id="productGrid">
            @if (Model != null && Model.Products.Any())
            {
                @foreach (var p in Model.Products)
                {
                    <div class="card-dark-item">
                        <div style="position: relative;">
                            <a href="/Products/Details/@p.Id" class="card-dark-image" style="display: block; cursor: pointer; text-decoration: none;" title="Xem chi tiết @p.Name">
                                <img src="@p.Image" alt="@p.Name" onerror="this.src='https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600'">
                            </a>
                            <button class="btn-card-fav" onclick="toggleProductFavorite(@p.Id, event)" title="Yêu thích món đồ" data-fav-id="@p.Id">
                                <i class="far fa-heart"></i>
                            </button>
                        </div>
                        <div class="card-dark-body">
                            <div class="card-dark-category">@((p.CategoryName ?? "ĐỒ CŨ").ToUpper())</div>
                            <h3 class="card-dark-title" title="@p.Name">
                                <a href="/Products/Details/@p.Id" style="color: inherit; text-decoration: none;">@p.Name</a>
                            </h3>
                            <div class="card-dark-meta">
                                <i class="fas fa-map-marker-alt mr-1"></i>@(p.SellerAddress ?? "TP.HCM") · Người bán: <strong>@(p.SellerName ?? "Thành viên")</strong>
                            </div>
                            <div class="card-dark-price-row">
                                <span class="card-dark-price">@p.Price.ToString("N0") ₫</span>
                                @if (p.OriginalPrice > p.Price)
                                {
                                    <span class="card-dark-old-price">@p.OriginalPrice.ToString("N0") ₫</span>
                                }
                            </div>
                            <div class="card-dark-actions">
                                <a href="/Products/Details/@p.Id" class="btn-card-detail">Chi tiết</a>
                                <button class="btn-card-add" onclick="openChatWithSeller(@p.Id)" title="Nhắn tin với người bán">Chat</button>
                            </div>
                        </div>
                    </div>
                }
            }
            else
            {
                <div style="grid-column: 1/-1; text-align: center; padding: 60px 20px; color: #71717a; background: white; border-radius: 14px; border: 1px dashed #d4d4d8;">
                    Không tìm thấy món đồ cũ nào phù hợp với bộ lọc. Hãy thử điều chỉnh lại khoảng giá hoặc danh mục.
                </div>
            }
        </div>

        <!-- PHÂN TRANG (THEO MẪU ẢNH 5: 9 SẢN PHẨM / TRANG) -->
        <div class="pagination-container-modern" id="paginationContainer">
            @if (Model != null && Model.TotalPages > 1)
            {
                if (Model.CurrentPage > 1)
                {
                    <a href="@Url.Action("Index", "Home", new { page = Model.CurrentPage - 1, search = Model.Search, minPrice = Model.MinPrice, maxPrice = Model.MaxPrice, sort = Model.Sort })" class="modern-page-btn">&larr;</a>
                }
                for (int i = 1; i <= Model.TotalPages; i++)
                {
                    <a href="@Url.Action("Index", "Home", new { page = i, search = Model.Search, minPrice = Model.MinPrice, maxPrice = Model.MaxPrice, sort = Model.Sort })" class="modern-page-btn @(i == Model.CurrentPage ? "active" : "")">@i</a>
                }
                if (Model.CurrentPage < Model.TotalPages)
                {
                    <a href="@Url.Action("Index", "Home", new { page = Model.CurrentPage + 1, search = Model.Search, minPrice = Model.MinPrice, maxPrice = Model.MaxPrice, sort = Model.Sort })" class="modern-page-btn">&rarr;</a>
                }
            }
        </div>
    </section>
</div>

<!-- 4. QUY TRÌNH MUA BÁN 3 BƯỚC MINH BẠCH (HOW IT WORKS) -->
<section class="workflow-section">
    <div class="workflow-header">
        <span style="font-size: 12px; font-weight: 700; color: #ff3c00; text-transform: uppercase; letter-spacing: 0.5px;">
            <i class="fas fa-shield-alt mr-1"></i>An tâm giao dịch
        </span>
        <h2>Quy Trình Mua Bán Đồ Cũ Minh Bạch</h2>
        <p>ThriftShop kết nối người bán thanh lý đồ cũ nhanh chóng và người mua sở hữu món đồ ưng ý với chi phí tiết kiệm nhất.</p>
    </div>
    <div class="workflow-grid">
        <div class="workflow-card">
            <span class="workflow-step-num">01</span>
            <div class="workflow-icon"><i class="fas fa-camera-retro"></i></div>
            <h4>Người Bán Đăng Tin</h4>
            <p>Chụp ảnh thực tế sản phẩm, tự ghi rõ tình trạng món đồ trong tiêu đề và mô tả, tự do định giá thanh lý.</p>
        </div>
        <div class="workflow-card">
            <span class="workflow-step-num">02</span>
            <div class="workflow-icon"><i class="fas fa-user-shield"></i></div>
            <h4>Admin Thẩm Định & Duyệt Tin</h4>
            <p>Ban Quản Trị đối chiếu hình ảnh, kiểm tra nội dung và duyệt tin nhanh chóng để bảo vệ người mua.</p>
        </div>
        <div class="workflow-card">
            <span class="workflow-step-num">03</span>
            <div class="workflow-icon"><i class="fas fa-handshake"></i></div>
            <h4>Người Mua Đặt Hàng & Giao Nhận</h4>
            <p>Người mua chọn món đồ ưng ý, nhận hàng kiểm tra và được hưởng chính sách bảo đảm an tâm.</p>
        </div>
    </div>
</section>

<!-- 5. BANNER KÊU GỌI HÀNH ĐỘNG (CTA BANNER) -->
<div class="cta-banner-wrapper">
    <div class="cta-banner">
        <div class="cta-banner-content">
            <h3>Bạn có món đồ cũ không còn dùng đến?</h3>
            <p>Dọn dẹp góc phòng, kiếm thêm thu nhập và tiếp nối vòng đời cho món đồ của bạn. Hàng ngàn người mua đang tìm kiếm đồ cũ giá tốt mỗi ngày trên ThriftShop!</p>
        </div>
        <div class="cta-banner-actions">
            <a asp-controller="Products" asp-action="PostItem" class="btn-cta-primary">
                <i class="fas fa-plus-circle"></i> Đăng bán đồ cũ ngay
            </a>
        </div>
    </div>
</div>

@section Scripts {
<script>
    const MAX_PRICE = 10000000;
    const PAGE_SIZE = 9;
    let currentPage = 1;
    let currentFilteredList = [];
    let searchQuery = "";

    function updateSliderTrack() {
        const minVal = parseInt(document.getElementById('priceRangeMin').value) || 0;
        const maxVal = parseInt(document.getElementById('priceRangeMax').value) || MAX_PRICE;
        const fill = document.getElementById('sliderFill');
        if (fill) {
            const percent1 = (minVal / MAX_PRICE) * 100;
            const percent2 = (maxVal / MAX_PRICE) * 100;
            fill.style.left = percent1 + "%";
            fill.style.width = (percent2 - percent1) + "%";
        }
    }

    function onRangeMinInput() {
        const minEl = document.getElementById('priceRangeMin');
        const maxEl = document.getElementById('priceRangeMax');
        let minVal = parseInt(minEl.value) || 0;
        let maxVal = parseInt(maxEl.value) || MAX_PRICE;

        if (minVal > maxVal) {
            minVal = maxVal;
            minEl.value = minVal;
        }
        document.getElementById('priceInputMin').value = minVal || "";
        updateSliderTrack();
    }

    function onRangeMaxInput() {
        const minEl = document.getElementById('priceRangeMin');
        const maxEl = document.getElementById('priceRangeMax');
        let minVal = parseInt(minEl.value) || 0;
        let maxVal = parseInt(maxEl.value) || MAX_PRICE;

        if (maxVal < minVal) {
            maxVal = minVal;
            maxEl.value = maxVal;
        }
        document.getElementById('priceInputMax').value = maxVal >= MAX_PRICE ? "" : maxVal;
        updateSliderTrack();
    }

    function onMinInputChange() {
        let val = parseInt(document.getElementById('priceInputMin').value) || 0;
        if (val < 0) val = 0;
        if (val > MAX_PRICE) val = MAX_PRICE;
        const maxVal = parseInt(document.getElementById('priceRangeMax').value) || MAX_PRICE;
        if (val > maxVal) val = maxVal;
        document.getElementById('priceRangeMin').value = val;
        updateSliderTrack();
    }

    function onMaxInputChange() {
        let val = parseInt(document.getElementById('priceInputMax').value);
        if (isNaN(val) || val <= 0 || val > MAX_PRICE) val = MAX_PRICE;
        const minVal = parseInt(document.getElementById('priceRangeMin').value) || 0;
        if (val < minVal) val = minVal;
        document.getElementById('priceRangeMax').value = val;
        updateSliderTrack();
    }

    function onFilterChange() {
        applyFilters();
    }

    function onSortChange() {
        applyFilters();
    }

    function resetFilters() {
        window.location.href = '/';
    }

    function applyFilters() {
        const checkedCats = Array.from(document.querySelectorAll('input[name="catFilter"]:checked')).map(cb => cb.value);
        const minPrice = parseInt(document.getElementById('priceRangeMin').value) || 0;
        const maxPrice = parseInt(document.getElementById('priceRangeMax').value) || MAX_PRICE;
        const sortVal = document.getElementById('sortFilter') ? document.getElementById('sortFilter').value : 'newest';

        const params = new URLSearchParams();
        if (searchQuery) params.set('search', searchQuery);
        checkedCats.forEach(c => params.append('catFilter', c));
        if (minPrice > 0) params.set('minPrice', minPrice);
        if (maxPrice < MAX_PRICE) params.set('maxPrice', maxPrice);
        if (sortVal && sortVal !== 'newest') params.set('sort', sortVal);

        window.location.href = '/?' + params.toString();
    }
    function handleAddToCart(productId) {
        if (typeof ThriftDB !== 'undefined') {
            ThriftDB.addToCart(productId, 1);
            if (typeof updateCartBadge === 'function') updateCartBadge();
            const p = ThriftDB.getProductById(productId);
            showToast(`Đã thêm "${p ? p.name : 'Món đồ'}" vào giỏ hàng!`, 'success');
        }
    }

    function initHomePage() {
        updateSliderTrack();

        // Đọc query search từ url nếu có
        const urlParams = new URLSearchParams(window.location.search);
        const searchParam = urlParams.get('search');
        if (searchParam) {
            searchQuery = searchParam.trim();
            const searchInput = document.getElementById('searchInput');
            if (searchInput) searchInput.value = searchQuery;
        }

        const sortParam = urlParams.get('sort');
        if (sortParam) {
            const sortFilter = document.getElementById('sortFilter');
            if (sortFilter) sortFilter.value = sortParam;
        }

        // Đánh dấu checkbox danh mục từ URL
        const catParams = urlParams.getAll('catFilter');
        catParams.forEach(c => {
            const cb = document.querySelector(`input[name="catFilter"][value="${c}"]`);
            if (cb) cb.checked = true;
        });

        // Đặt giá trị thanh trượt từ URL
        const minP = urlParams.get('minPrice');
        if (minP) {
            document.getElementById('priceRangeMin').value = minP;
            document.getElementById('priceInputMin').value = minP;
        }
        const maxP = urlParams.get('maxPrice');
        if (maxP) {
            document.getElementById('priceRangeMax').value = maxP;
            document.getElementById('priceInputMax').value = maxP;
        }
        updateSliderTrack();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initHomePage);
    } else {
        initHomePage();
    }
    if (window.jQuery) {
        $(document).ready(initHomePage);
    }
</script>
}
```

---

## Views/Home/Barter.cshtml

```html
@{
    Layout = null;
}
<!DOCTYPE html>
<html>
<head>
    <meta http-equiv="refresh" content="0; url=/Products" />
    <title>Chuyển hướng - ThriftShop</title>
</head>
<body>
    <script>
        window.location.replace("/Products");
    </script>
</body>
</html>
```

---

## Views/Home/Repair.cshtml

```html
@{
    ViewData["Title"] = "Dịch Vụ Spa, Vệ Sinh & Phục Dựng Đồ Cũ - ThriftShop";
}

<div class="main-wrapper" style="margin-top: 30px;">
    <!-- KHUNG TẬP TRUNG (FOCUSED CONTAINER - KHÔNG CHIA 2 BÊN DÀY CỘP) -->
    <div class="focused-card">
        <h2>✨ Dịch Vụ Spa, Vệ Sinh & Phục Dựng Đồ Cũ (Thrift Spa)</h2>
        <p class="desc">Chăm sóc & làm mới đồ secondhand: Spa đồ da, vệ sinh sneaker, khử khuẩn đồ vintage, bảo dưỡng đồ công nghệ & nhạc cụ.</p>

        <!-- 1. FORM ĐẶT LỊCH GỌN GÀNG -->
        <div style="background: #f8fafc; border: 1px solid #e4e4e7; border-radius: 12px; padding: 22px; margin-bottom: 28px;">
            <h3 style="font-size: 16px; font-weight: 700; color: #18181b; margin-bottom: 14px;">
                📅 Đặt Lịch Spa / Phục Dựng Món Đồ
            </h3>
            <form id="repairForm">
                <div class="form-row-2">
                    <div class="form-group">
                        <label>Họ và tên (*)</label>
                        <input type="text" placeholder="Nguyễn Văn A" required>
                    </div>
                    <div class="form-group">
                        <label>Số điện thoại / Zalo (*)</label>
                        <input type="tel" placeholder="0901234567" required>
                    </div>
                </div>

                <div class="form-row-2">
                    <div class="form-group">
                        <label>Loại đồ cần làm mới (*)</label>
                        <select required>
                            <option value="fashion">Áo da / Áo khoác / Đồ hiệu Vintage</option>
                            <option value="shoes">Giày Sneaker / Boot da / Guốc cao gót</option>
                            <option value="tech">Thiết bị công nghệ / Điện thoại / Tai nghe cũ</option>
                            <option value="music">Nhạc cụ (Guitar, Ukulele, Phím đàn...)</option>
                            <option value="other">Đồ gốm / Đồng hồ / Phụ kiện khác</option>
                        </select>
                    </div>
                    <div class="form-group">
                        <label>Ngày hẹn mang qua (*)</label>
                        <input type="date" id="bookingDate" required>
                    </div>
                </div>

                <div class="form-group">
                    <label>Mô tả tình trạng món đồ & Yêu cầu xử lý</label>
                    <textarea rows="2" placeholder="VD: Giày Jordan ố đế cần tẩy ố vàng, Áo da vintage bị khô cứng cần dưỡng mềm..."></textarea>
                </div>

                <button type="submit" class="btn-primary-block" style="border-radius: 8px;">
                    Gửi Yêu Cầu Đặt Lịch
                </button>
            </form>
        </div>

        <!-- 2. BẢNG BÁO GIÁ THAM KHẢO GỌN GÀNG -->
        <div style="margin-bottom: 28px;">
            <h3 style="font-size: 16px; font-weight: 700; color: #18181b; margin-bottom: 12px;">
                📋 Bảng Giá Dịch Vụ Phổ Biến
            </h3>
            <table class="detail-specs-table" style="background: white; border: 1px solid #e4e4e7; border-radius: 10px; overflow: hidden;">
                <tbody>
                    <tr>
                        <td style="font-weight: 600; color: #18181b;">Vệ sinh chuyên sâu & Khử khuẩn tia UV Sneaker / Giày boot</td>
                        <td style="color: #ff3c00; font-weight: 700;">80.000₫ - 150.000₫</td>
                        <td style="color: #71717a; font-size: 13px;">1 - 2 ngày</td>
                    </tr>
                    <tr>
                        <td style="font-weight: 600; color: #18181b;">Dưỡng mềm da thật & Phục hồi màu áo khoác da, túi xách vintage</td>
                        <td style="color: #ff3c00; font-weight: 700;">200.000₫ - 450.000₫</td>
                        <td style="color: #71717a; font-size: 13px;">2 - 3 ngày</td>
                    </tr>
                    <tr>
                        <td style="font-weight: 600; color: #18181b;">Vệ sinh bụi mạch, thay pin & pad tai nghe Bluetooth / Máy chơi game</td>
                        <td style="color: #ff3c00; font-weight: 700;">150.000₫ - 350.000₫</td>
                        <td style="color: #71717a; font-size: 13px;">Lấy ngay trong ngày</td>
                    </tr>
                    <tr>
                        <td style="font-weight: 600; color: #18181b;">Căn chỉnh cần, thay dây & vệ sinh thùng đàn Guitar / Bass</td>
                        <td style="color: #ff3c00; font-weight: 700;">100.000₫ - 220.000₫</td>
                        <td style="color: #71717a; font-size: 13px;">2 - 4 giờ</td>
                    </tr>
                </tbody>
            </table>
        </div>

        <!-- 3. TRA CỨU TIẾN ĐỘ PHIẾU NHẬN MÁY -->
        <div style="background: #ffffff; border: 1px solid #e4e4e7; border-radius: 12px; padding: 20px;">
            <h3 style="font-size: 15px; font-weight: 700; color: #18181b; margin-bottom: 8px;">
                🔍 Tra Cứu Tiến Độ Phiếu Phục Dựng Đồ
            </h3>
            <div style="display: flex; gap: 10px;">
                <input type="text" id="orderCode" placeholder="Nhập mã phiếu (VD: SPA-3001)" style="flex: 1; padding: 10px 14px; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 13.5px; outline: none;">
                <button id="btnCheckOrder" class="choice-btn active" style="padding: 10px 20px;">Kiểm tra</button>
            </div>
            <div id="statusResult" style="display: none; margin-top: 14px; padding: 12px 16px; border-radius: 8px; font-size: 13.5px; line-height: 1.7;"></div>
        </div>
    </div>
</div>

@section Scripts {
<script>
    document.addEventListener('DOMContentLoaded', () => {
        const today = new Date().toISOString().split('T')[0];
        const bookingDate = document.getElementById('bookingDate');
        if (bookingDate) bookingDate.min = today;

        document.getElementById('repairForm').onsubmit = (e) => {
            e.preventDefault();
            showToast('Đặt lịch thành công! Nhân viên ThriftShop sẽ gọi xác nhận trong 15 phút.');
            e.target.reset();
        };

        const mockOrders = {
            'SPA-3001': { device: 'Áo da Vintage Biker Schott', service: 'Dưỡng dầu mềm da + Khử mùi UV', status: 'Đang xử lý (Dự kiến xong 17h hôm nay)', cost: '250.000₫' },
            'SPA-3002': { device: 'Giày Jordan 1 Chicago', service: 'Vệ sinh chuyên sâu + Tẩy ố viền đế', status: 'Đã hoàn thành - Sẵn sàng nhận đồ', cost: '120.000₫' }
        };

        document.getElementById('btnCheckOrder').onclick = () => {
            const code = document.getElementById('orderCode').value.trim().toUpperCase();
            const resultBox = document.getElementById('statusResult');

            if (!code) {
                showToast('Vui lòng nhập mã phiếu spa đồ!', 'error');
                return;
            }

            if (mockOrders[code]) {
                const info = mockOrders[code];
                resultBox.style.display = 'block';
                resultBox.style.background = '#f0fdf4';
                resultBox.style.border = '1px solid #bbf7d0';
                resultBox.style.color = '#166534';
                resultBox.innerHTML = `
                    <strong>Món đồ:</strong> ${info.device} | <strong>Dịch vụ:</strong> ${info.service}<br>
                    <strong>Tiến độ:</strong> ${info.status} | <strong>Chi phí:</strong> <strong style="color:#ff3c00;">${info.cost}</strong>
                `;
            } else {
                resultBox.style.display = 'block';
                resultBox.style.background = '#fef2f2';
                resultBox.style.border = '1px solid #fecaca';
                resultBox.color = '#991b1b';
                resultBox.innerHTML = `Không tìm thấy mã phiếu <strong>${code}</strong>. Vui lòng kiểm tra lại biên nhận.`;
            }
        };
    });
</script>
}
```

---

## Views/Home/Warranty.cshtml

```html
@{
    ViewData["Title"] = "Chính Sách Kiểm Định & Bảo Vệ Người Mua - ThriftShop";
}

<div class="main-wrapper" style="margin-top: 30px; margin-bottom: 60px;">
    <div class="focused-card" style="max-width: 960px; margin: 0 auto; background: white; border-radius: 16px; border: 1px solid #e4e4e7; padding: 36px 40px; box-shadow: 0 4px 20px rgba(0,0,0,0.04);">
        <!-- TIÊU ĐỀ CHÍNH -->
        <div style="text-align: center; margin-bottom: 32px; border-bottom: 1px solid #f4f4f5; padding-bottom: 24px;">
            <div style="display: inline-flex; align-items: center; justify-content: center; width: 56px; height: 56px; border-radius: 50%; background: #eff6ff; color: #2563eb; font-size: 26px; margin-bottom: 12px;">
                🛡️
            </div>
            <h1 style="font-size: 24px; font-weight: 800; color: #18181b; margin-bottom: 8px;">
                Chính Sách Kiểm Định & Bảo Vệ Người Mua Đồ Cũ
            </h1>
            <p style="font-size: 14.5px; color: #71717a; max-width: 720px; margin: 0 auto; line-height: 1.6;">
                ThriftShop cam kết tạo dựng môi trường mua bán đồ cũ minh bạch, uy tín. Toàn bộ tin đăng được kiểm duyệt, hỗ trợ đồng kiểm tận tay và bao test 7 ngày hoàn tiền 100%.
            </p>
        </div>

        <!-- 4 TRỤ CỘT BẢO ĐẢM CỦA THRIFTSHOP -->
        <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 20px; margin-bottom: 32px;">
            <!-- TRỤ CỘT 1 -->
            <div style="border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; background: #fafafa;">
                <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 8px;">
                    <div style="font-size: 22px;">🔍</div>
                    <strong style="font-size: 15px; color: #18181b;">1. Kiểm Duyệt Nghiêm Ngặt 100%</strong>
                </div>
                <p style="font-size: 13.5px; color: #52525b; line-height: 1.6; margin: 0;">
                    Trước khi bất kỳ món đồ nào được hiển thị công khai, Ban Quản Trị đều kiểm tra kỹ lưỡng hình ảnh thực tế, mô tả tình trạng, phân loại danh mục và đối soát lịch sử người bán nhằm loại bỏ hàng giả mạo, hàng ảo hoặc vi phạm quy định.
                </p>
            </div>

            <!-- TRỤ CỘT 2 -->
            <div style="border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; background: #fafafa;">
                <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 8px;">
                    <div style="font-size: 22px;">📦</div>
                    <strong style="font-size: 15px; color: #18181b;">2. Quyền Đồng Kiểm Khi Nhận Hàng (COD)</strong>
                </div>
                <p style="font-size: 13.5px; color: #52525b; line-height: 1.6; margin: 0;">
                    Khi đơn vị vận chuyển giao hàng tới nơi, người mua có quyền yêu cầu mở hộp kiểm tra ngoại quan món đồ cùng nhân viên giao hàng. Nếu món đồ không khớp mô tả hoặc bị lỗi nứt vỡ, bạn được từ chối nhận hàng hoàn toàn miễn phí ngay tại chỗ.
                </p>
            </div>

            <!-- TRỤ CỘT 3 -->
            <div style="border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; background: #fafafa;">
                <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 8px;">
                    <div style="font-size: 22px;">🔄</div>
                    <strong style="font-size: 15px; color: #18181b;">3. Bao Test 7 Ngày - Hoàn Tiền 100%</strong>
                </div>
                <p style="font-size: 13.5px; color: #52525b; line-height: 1.6; margin: 0;">
                    Sau khi nhận hàng, bạn có 7 ngày sử dụng thử tính năng. Nếu phát hiện món đồ có lỗi ẩn nghiêm trọng mà người bán chưa đề cập trong bài viết, bạn có quyền khiếu nại để được hỗ trợ đổi trả hoặc hoàn lại 100% số tiền đã chi trả.
                </p>
            </div>

            <!-- TRỤ CỘT 4 -->
            <div style="border: 1px solid #e2e8f0; border-radius: 12px; padding: 20px; background: #fafafa;">
                <div style="display: flex; align-items: center; gap: 10px; margin-bottom: 8px;">
                    <div style="font-size: 22px;">🚚</div>
                    <strong style="font-size: 15px; color: #18181b;">4. Bảo Hiểm Giao Vận An Toàn</strong>
                </div>
                <p style="font-size: 13.5px; color: #52525b; line-height: 1.6; margin: 0;">
                    Tất cả bưu kiện được kết nối qua sàn đều có mã định danh và bảo hiểm hàng hóa. Trong trường hợp xảy ra rủi ro thất lạc, hư hỏng trong quá trình chuyển phát, ThriftShop cùng đơn vị vận chuyển sẽ đền bù thỏa đáng giá trị đơn hàng.
                </p>
            </div>
        </div>

        <!-- QUY TRÌNH KHIẾU NẠI & HỖ TRỢ 3 BƯỚC -->
        <div style="background: #f0fdf4; border: 1px solid #bbf7d0; border-radius: 12px; padding: 24px; margin-bottom: 30px;">
            <h3 style="font-size: 16px; font-weight: 700; color: #166534; margin-bottom: 14px; display: flex; align-items: center; gap: 8px;">
                <i class="fas fa-clipboard-check"></i> Quy Trình 3 Bước Xử Lý Khiếu Nại & Bảo Vệ Quyền Lợi
            </h3>
            <div style="display: grid; grid-template-columns: 1fr 1fr 1fr; gap: 16px;">
                <div style="background: white; border-radius: 8px; padding: 14px; border: 1px solid #dcfce7;">
                    <div style="font-weight: 700; color: #15803d; font-size: 13.5px; margin-bottom: 4px;">Bước 1: Giữ nguyên kiện hàng</div>
                    <p style="font-size: 12.5px; color: #4b5563; margin: 0; line-height: 1.5;">Quay clip khi mở hộp và thử đồ để làm căn cứ xác thực khách quan nhất.</p>
                </div>
                <div style="background: white; border-radius: 8px; padding: 14px; border: 1px solid #dcfce7;">
                    <div style="font-weight: 700; color: #15803d; font-size: 13.5px; margin-bottom: 4px;">Bước 2: Chat với người bán / BQT</div>
                    <p style="font-size: 12.5px; color: #4b5563; margin: 0; line-height: 1.5;">Vào mục <strong>Liên hệ</strong> để trao đổi trực tiếp hoặc gọi Hotline <strong>1900 6868</strong>.</p>
                </div>
                <div style="background: white; border-radius: 8px; padding: 14px; border: 1px solid #dcfce7;">
                    <div style="font-weight: 700; color: #15803d; font-size: 13.5px; margin-bottom: 4px;">Bước 3: Thu hồi & Hoàn tiền</div>
                    <p style="font-size: 12.5px; color: #4b5563; margin: 0; line-height: 1.5;">Đội ngũ kiểm định tiếp nhận và tiến hành hoàn tiền cho người mua trong vòng 24 giờ.</p>
                </div>
            </div>
        </div>

        <!-- LƯU Ý QUAN TRỌNG -->
        <div style="background: #fffbeb; border: 1px solid #fde68a; border-radius: 12px; padding: 18px 22px; font-size: 13px; color: #92400e; line-height: 1.6;">
            <strong>💡 Khuyến nghị cho người mua & người bán:</strong> 
            Mọi thỏa thuận về thông số, tình trạng sản phẩm và phụ kiện nên được trao đổi trực tiếp qua tính năng <strong>Chat / Liên hệ</strong> trên website để hệ thống có thể đối soát và bảo vệ bạn trong mọi tình huống.
        </div>
    </div>
</div>
```

---

## Views/Account/Login.cshtml

```html
@model ThriftShop.Models.LoginViewModel
@{
    Layout = null;
}
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Đăng nhập | ThriftShop</title>
    <style>
        * {
            box-sizing: border-box;
        }

        body {
            margin: 0;
            font-family: Arial, sans-serif;
            background: #f5f5f5;
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }

        .login-box {
            width: 100%;
            max-width: 430px;
            background: white;
            padding: 40px;
            border-radius: 14px;
            box-shadow: 0 10px 35px rgba(0, 0, 0, 0.12);
        }

        .logo {
            text-align: center;
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 25px;
            color: #111;
            letter-spacing: -0.5px;
        }

        .logo span {
            color: #e53935;
        }

        h2 {
            text-align: center;
            margin-bottom: 8px;
            color: #111;
            font-size: 24px;
        }

        .description {
            text-align: center;
            color: #777;
            margin-bottom: 25px;
            font-size: 14px;
        }

        label {
            display: block;
            font-weight: bold;
            margin-top: 16px;
            margin-bottom: 7px;
            color: #111;
            font-size: 14px;
        }

        input {
            width: 100%;
            padding: 13px;
            border: 1px solid #ccc;
            border-radius: 7px;
            font-size: 15px;
            outline: none;
            box-sizing: border-box;
            transition: border-color 0.2s;
        }

        input:focus {
            border-color: #e53935;
        }

        button {
            width: 100%;
            padding: 14px;
            margin-top: 25px;
            border: none;
            border-radius: 7px;
            background: #e53935;
            color: white;
            font-size: 16px;
            font-weight: bold;
            cursor: pointer;
            transition: background 0.2s;
        }

        button:hover {
            background: #c62828;
        }

        .message {
            padding: 12px;
            margin-bottom: 15px;
            border-radius: 7px;
            text-align: center;
            font-size: 13.5px;
        }

        .message.error {
            background: #ffebee;
            color: #c62828;
            border: 1px solid #ffcdd2;
        }

        .message.success {
            background: #e8f5e9;
            color: #2e7d32;
            border: 1px solid #c8e6c9;
        }

        .validation-error {
            color: #c62828;
            font-size: 12px;
            margin-top: 4px;
            display: block;
        }

        .register {
            text-align: center;
            margin-top: 22px;
            font-size: 14px;
            color: #111;
        }

        .register a {
            color: #e53935;
            font-weight: bold;
            text-decoration: none;
        }

        .back {
            display: block;
            text-align: center;
            margin-top: 20px;
            color: #555;
            text-decoration: none;
            font-size: 14px;
        }

        .quick-accounts {
            margin-top: 20px;
            padding: 10px;
            background: #fafafa;
            border: 1px dashed #ddd;
            border-radius: 8px;
            text-align: center;
            font-size: 12px;
            color: #666;
        }

        .quick-accounts a {
            font-weight: bold;
            text-decoration: none;
            margin: 0 4px;
        }
    </style>
</head>
<body>

    <div class="login-box">
        <div class="logo">
            Thrift<span>Shop</span>
        </div>

        <h2>Đăng nhập</h2>

        <p class="description">
            Đăng nhập vào tài khoản ThriftShop
        </p>

        @if (!ViewData.ModelState.IsValid)
        {
            <div class="message error">
                @foreach (var error in ViewData.ModelState.Values.SelectMany(v => v.Errors))
                {
                    <div>@error.ErrorMessage</div>
                }
            </div>
        }

        @if (TempData["ErrorMessage"] != null)
        {
            <div class="message error">
                @TempData["ErrorMessage"]
            </div>
        }

        @if (TempData["SuccessMessage"] != null)
        {
            <div class="message success">
                @TempData["SuccessMessage"]
            </div>
        }

        <div id="client-message" class="message" style="display: none;"></div>

        <!-- FORM ASP.NET MVC CHUẨN: POST lên Controller Account -> Action Login -->
        <form asp-controller="Account" asp-action="Login" method="post" id="loginForm">
            @Html.AntiForgeryToken()
            <input type="hidden" name="returnUrl" value="@ViewBag.ReturnUrl" />

            <label asp-for="Email">Email</label>
            <input asp-for="Email"
                   type="email"
                   id="email"
                   placeholder="Nhập email"
                   required />
            <span asp-validation-for="Email" class="validation-error"></span>

            <label asp-for="Password">Mật khẩu</label>
            <input asp-for="Password"
                   type="password"
                   id="password"
                   placeholder="Nhập mật khẩu"
                   required />
            <span asp-validation-for="Password" class="validation-error"></span>

            <button type="submit">
                Đăng nhập
            </button>

            <div class="register">
                Chưa có tài khoản? <a href="/Account/Register">Đăng ký ngay</a>
            </div>

            <a href="/" class="back">
                ← Quay về trang chủ
            </a>

            <!-- Tiện ích điền nhanh 2 tài khoản mẫu -->
            <div class="quick-accounts">
                <span>Chọn nhanh tài khoản mẫu: </span>
                <a href="javascript:void(0)" onclick="fillAcc('user')" style="color: #1976d2;">👤 User</a> |
                <a href="javascript:void(0)" onclick="fillAcc('admin')" style="color: #e53935;">🛡️ Admin</a>
            </div>
        </form>
    </div>

    <script src="~/js/data.js"></script>
    <script>
        function fillAcc(type) {
            if (type === 'admin') {
                document.getElementById('email').value = 'admin@thriftshop.vn';
                document.getElementById('password').value = 'admin';
            } else {
                document.getElementById('email').value = 'user@thriftshop.vn';
                document.getElementById('password').value = 'user';
            }
        }

        // Đồng bộ trước khi submit form lên Server
        document.getElementById('loginForm').addEventListener('submit', function (e) {
            const email = document.getElementById('email').value.trim();
            const password = document.getElementById('password').value;

            // Đồng bộ LocalStorage cho các component client-side
            if (typeof ThriftDB !== 'undefined') {
                ThriftDB.login(email, password);
            }
        });
    </script>
</body>
</html>
```

---

## Views/Account/Register.cshtml

```html
@model ThriftShop.Models.RegisterViewModel
@{
    Layout = null;
}
<!DOCTYPE html>
<html lang="vi">
<head>
    <meta charset="UTF-8">
    <meta name="viewport" content="width=device-width, initial-scale=1.0">
    <title>Tạo tài khoản | ThriftShop</title>
    <style>
        * {
            box-sizing: border-box;
        }

        body {
            margin: 0;
            font-family: Arial, sans-serif;
            background: #f5f5f5;
            min-height: 100vh;
            display: flex;
            justify-content: center;
            align-items: center;
            padding: 20px;
        }

        .register-box {
            width: 100%;
            max-width: 450px;
            background: white;
            padding: 40px;
            border-radius: 14px;
            box-shadow: 0 10px 35px rgba(0, 0, 0, 0.12);
        }

        .logo {
            text-align: center;
            font-size: 32px;
            font-weight: bold;
            margin-bottom: 25px;
            color: #111;
            letter-spacing: -0.5px;
        }

        .logo span {
            color: #e53935;
        }

        h2 {
            text-align: center;
            margin-bottom: 8px;
            color: #111;
        }

        .description {
            text-align: center;
            color: #777;
            margin-bottom: 25px;
            font-size: 14px;
        }

        label {
            display: block;
            font-weight: bold;
            margin-top: 15px;
            margin-bottom: 7px;
            color: #111;
            font-size: 14px;
        }

        input {
            width: 100%;
            padding: 13px;
            border: 1px solid #ccc;
            border-radius: 7px;
            font-size: 15px;
            outline: none;
            box-sizing: border-box;
        }

        input:focus {
            border-color: #e53935;
        }

        button {
            width: 100%;
            padding: 14px;
            margin-top: 25px;
            border: none;
            border-radius: 7px;
            background: #e53935;
            color: white;
            font-size: 16px;
            font-weight: bold;
            cursor: pointer;
            transition: background 0.2s;
        }

        button:hover {
            background: #c62828;
        }

        .message {
            padding: 12px;
            margin-bottom: 15px;
            border-radius: 7px;
            text-align: center;
            font-size: 13.5px;
        }

        .message.error {
            background: #ffebee;
            color: #c62828;
            border: 1px solid #ffcdd2;
        }

        .message.success {
            background: #e8f5e9;
            color: #2e7d32;
            border: 1px solid #c8e6c9;
        }

        .validation-error {
            color: #c62828;
            font-size: 12px;
            margin-top: 4px;
            display: block;
        }

        .login {
            text-align: center;
            margin-top: 22px;
            font-size: 14px;
            color: #111;
        }

        .login a {
            color: #e53935;
            font-weight: bold;
            text-decoration: none;
        }

        .back {
            display: block;
            text-align: center;
            margin-top: 20px;
            color: #555;
            text-decoration: none;
            font-size: 14px;
        }
    </style>
</head>
<body>

    <div class="register-box">
        <div class="logo">
            Thrift<span>Shop</span>
        </div>

        <h2>Tạo tài khoản</h2>

        <p class="description">
            Đăng ký tài khoản ThriftShop
        </p>

        @if (!ViewData.ModelState.IsValid)
        {
            <div class="message error">
                @foreach (var error in ViewData.ModelState.Values.SelectMany(v => v.Errors))
                {
                    <div>@error.ErrorMessage</div>
                }
            </div>
        }

        <!-- FORM ASP.NET MVC CHUẨN: POST lên AccountController -> Action Register -->
        <form asp-controller="Account" asp-action="Register" method="post" id="registerForm">
            @Html.AntiForgeryToken()

            <label asp-for="Name">Họ và tên (*)</label>
            <input asp-for="Name"
                   type="text"
                   id="fullName"
                   placeholder="Nguyễn Văn A"
                   required />
            <span asp-validation-for="Name" class="validation-error"></span>

            <label asp-for="Email">Email (*)</label>
            <input asp-for="Email"
                   type="email"
                   id="email"
                   placeholder="example@gmail.com"
                   required />
            <span asp-validation-for="Email" class="validation-error"></span>

            <label asp-for="Phone">Số điện thoại (*)</label>
            <input asp-for="Phone"
                   type="tel"
                   id="phone"
                   placeholder="0901234567"
                   required />
            <span asp-validation-for="Phone" class="validation-error"></span>

            <label asp-for="Address">Địa chỉ / Khu vực (*)</label>
            <input asp-for="Address"
                   type="text"
                   id="address"
                   placeholder="Quận 1, TP. Hồ Chí Minh"
                   required />
            <span asp-validation-for="Address" class="validation-error"></span>

            <label asp-for="Password">Mật khẩu (*)</label>
            <input asp-for="Password"
                   type="password"
                   id="password"
                   placeholder="Ít nhất 6 ký tự"
                   required />
            <span asp-validation-for="Password" class="validation-error"></span>

            <label asp-for="ConfirmPassword">Nhập lại mật khẩu (*)</label>
            <input asp-for="ConfirmPassword"
                   type="password"
                   id="confirmPassword"
                   placeholder="Nhập lại mật khẩu"
                   required />
            <span asp-validation-for="ConfirmPassword" class="validation-error"></span>

            <button type="submit">
                Đăng ký
            </button>

            <div class="login">
                Đã có tài khoản? <a href="/Account/Login">Đăng nhập</a>
            </div>

            <a href="/" class="back">
                ← Quay về trang chủ
            </a>
        </form>
    </div>

    <script src="~/js/data.js"></script>
    <script>
        document.getElementById('registerForm').addEventListener('submit', function (e) {
            const fullName = document.getElementById('fullName').value.trim();
            const email = document.getElementById('email').value.trim();
            const phone = document.getElementById('phone').value.trim();
            const address = document.getElementById('address').value.trim();
            const password = document.getElementById('password').value;

            // Đồng bộ sang LocalStorage cho các component client
            if (typeof ThriftDB !== 'undefined') {
                ThriftDB.register({
                    name: fullName,
                    email: email,
                    phone: phone,
                    address: address,
                    password: password
                });
            }
        });
    </script>
</body>
</html>
```

---

## Views/Account/Profile.cshtml

```html
@{
    ViewData["Title"] = "Thiết Lập Hồ Sơ & Cài Đặt Tài Khoản - ThriftShop";
}

<!-- Route Guard: Chuyển hướng ngay nếu chưa đăng nhập -->
<script>
    (function() {
        try {
            var u = localStorage.getItem("thrift_current_user");
            if (!u || u === "null" || u === "undefined") {
                var returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
                window.location.replace('/Account/Login?returnUrl=' + returnUrl);
            }
        } catch(e) {}
    })();
</script>

<style>
    .profile-page-wrapper {
        width: 100%;
        max-width: 1240px;
        margin: 30px auto 50px;
        padding: 0 clamp(14px, 2.5vw, 30px);
        display: flex;
        gap: clamp(16px, 2vw, 28px);
        align-items: flex-start;
    }

    /* CỘT TRÁI: MENU THIẾT LẬP HỒ SƠ (CHUẨN ẢNH 2) */
    .profile-sidebar {
        width: clamp(240px, 24vw, 280px);
        flex-shrink: 0;
        background: #ffffff;
        border: 1px solid #e4e4e7;
        border-radius: 14px;
        padding: 20px 14px;
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.03);
    }

    .profile-sidebar-title {
        font-size: 16px;
        font-weight: 700;
        color: #18181b;
        margin: 0 0 16px 10px;
    }

    .profile-nav-list {
        display: flex;
        flex-direction: column;
        gap: 6px;
    }

    .profile-nav-item {
        display: flex;
        align-items: center;
        gap: 12px;
        padding: 12px 14px;
        border-radius: 10px;
        font-size: 14px;
        font-weight: 500;
        color: #3f3f46;
        cursor: pointer;
        transition: all 0.2s ease;
        border: none;
        background: transparent;
        width: 100%;
        text-align: left;
    }

    .profile-nav-item i {
        font-size: 16px;
        width: 20px;
        text-align: center;
        color: #71717a;
    }

    .profile-nav-item:hover {
        background: #f4f4f5;
        color: #18181b;
    }

    .profile-nav-item.active {
        background: #f4f4f5;
        color: #ff3c00;
        font-weight: 700;
    }

    .profile-nav-item.active i {
        color: #ff3c00;
    }

    /* CỘT PHẢI: KHUNG NỘI DUNG (CHUẨN ẢNH 2) */
    .profile-main-content {
        flex: 1;
        min-width: 0;
        background: #ffffff;
        border: 1px solid #e4e4e7;
        border-radius: 14px;
        padding: 28px clamp(18px, 3.5vw, 40px);
        box-shadow: 0 4px 16px rgba(0, 0, 0, 0.03);
    }

    .profile-breadcrumb {
        font-size: 13.5px;
        color: #2563eb;
        font-weight: 500;
        margin-bottom: 6px;
    }

    .profile-section-title {
        font-size: 22px;
        font-weight: 800;
        color: #18181b;
        margin-bottom: 24px;
        letter-spacing: -0.3px;
    }

    /* KHU VỰC ĐỔI AVATAR TÙY Ý */
    .avatar-upload-section {
        display: flex;
        align-items: center;
        gap: 20px;
        padding: 16px;
        background: #fafafa;
        border: 1px dashed #d4d4d8;
        border-radius: 12px;
        margin-bottom: 24px;
        flex-wrap: wrap;
    }

    .avatar-preview-wrap {
        position: relative;
        width: 86px;
        height: 86px;
        border-radius: 50%;
        overflow: hidden;
        border: 3px solid #ff3c00;
        flex-shrink: 0;
        box-shadow: 0 4px 12px rgba(0,0,0,0.1);
    }

    .avatar-preview-img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    .avatar-actions-wrap {
        flex: 1;
        min-width: 220px;
    }

    .avatar-actions-title {
        font-size: 14px;
        font-weight: 700;
        color: #18181b;
        margin-bottom: 6px;
    }

    .avatar-actions-hint {
        font-size: 12.5px;
        color: #71717a;
        margin-bottom: 10px;
    }

    .avatar-btn-row {
        display: flex;
        gap: 10px;
        flex-wrap: wrap;
    }

    .btn-avatar-upload {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        padding: 7px 14px;
        background: #18181b;
        color: white;
        border-radius: 8px;
        font-size: 13px;
        font-weight: 600;
        cursor: pointer;
        transition: background 0.2s;
    }

    .btn-avatar-upload:hover {
        background: #27272a;
    }

    .btn-avatar-preset {
        padding: 7px 12px;
        background: white;
        border: 1px solid #d4d4d8;
        border-radius: 8px;
        font-size: 13px;
        color: #3f3f46;
        cursor: pointer;
        font-weight: 500;
    }

    .btn-avatar-preset:hover {
        border-color: #ff3c00;
        color: #ff3c00;
    }

    /* FORM FIELDS (CHUẨN ẢNH 2) */
    .form-group-profile {
        margin-bottom: 20px;
    }

    .form-label-profile {
        display: block;
        font-size: 13px;
        font-weight: 600;
        color: #52525b;
        margin-bottom: 6px;
    }

    .form-label-profile span.required {
        color: #ef4444;
    }

    .input-with-clear {
        position: relative;
        display: flex;
        align-items: center;
    }

    .form-input-profile {
        width: 100%;
        padding: 12px 14px;
        border: 1px solid #d4d4d8;
        border-radius: 10px;
        font-size: 14.5px;
        color: #18181b;
        outline: none;
        background: #ffffff;
        transition: border-color 0.2s, box-shadow 0.2s;
        box-sizing: border-box;
    }

    .form-input-profile:focus {
        border-color: #ff3c00;
        box-shadow: 0 0 0 3px rgba(255, 60, 0, 0.12);
    }

    .btn-clear-input {
        position: absolute;
        right: 12px;
        background: none;
        border: none;
        color: #a1a1aa;
        cursor: pointer;
        font-size: 14px;
        display: none;
    }

    .btn-clear-input:hover {
        color: #52525b;
    }

    .form-textarea-profile {
        width: 100%;
        min-height: 90px;
        padding: 12px 14px;
        border: 1px solid #d4d4d8;
        border-radius: 10px;
        font-size: 14.5px;
        color: #18181b;
        outline: none;
        resize: vertical;
        box-sizing: border-box;
        font-family: inherit;
    }

    .form-textarea-profile:focus {
        border-color: #ff3c00;
        box-shadow: 0 0 0 3px rgba(255, 60, 0, 0.12);
    }

    .field-hint-bottom {
        font-size: 12px;
        color: #71717a;
        margin-top: 5px;
        display: flex;
        justify-content: space-between;
    }

    .url-preview-tag {
        font-size: 12.5px;
        color: #ff3c00;
        font-weight: 500;
        margin-top: 6px;
    }

    .btn-submit-profile {
        padding: 13px 36px;
        background: #ff3c00;
        color: white;
        border: none;
        border-radius: 10px;
        font-size: 15px;
        font-weight: 700;
        cursor: pointer;
        transition: background 0.2s, transform 0.1s;
        box-shadow: 0 4px 14px rgba(255, 60, 0, 0.3);
        margin-top: 10px;
    }

    .btn-submit-profile:hover {
        background: #e03500;
        transform: translateY(-1px);
    }

    /* RESPONSIVE & SCALING */
    @@media (max-width: 860px) {
        .profile-page-wrapper {
            flex-direction: column;
        }
        .profile-sidebar {
            width: 100%;
        }
    }
</style>

<div class="profile-page-wrapper">
    <!-- CỘT TRÁI: MENU ĐIỀU HƯỚNG (CHUẨN ẢNH 2: BỎ LỊCH SỬ ĐĂNG NHẬP, ĐỔI THÀNH ĐỔI MẬT KHẨU) -->
    <aside class="profile-sidebar">
        <h3 class="profile-sidebar-title">Thiết lập hồ sơ</h3>
        <div class="profile-nav-list">
            <button class="profile-nav-item active" id="tabBtnProfile" onclick="switchProfileTab('profile')">
                <i class="far fa-user-circle"></i>
                <span>Cá nhân, chuyên trang</span>
            </button>
            <button class="profile-nav-item" id="tabBtnPassword" onclick="switchProfileTab('password')">
                <i class="fas fa-lock"></i>
                <span>Đổi mật khẩu</span>
            </button>
        </div>
    </aside>

    <!-- CỘT PHẢI: CHI TIẾT CÀI ĐẶT -->
    <main class="profile-main-content">
        <!-- TAB 1: HỒ SƠ CÁ NHÂN (CHUẨN ẢNH 2) -->
        <div id="tabPaneProfile">
            <div class="profile-breadcrumb">Hồ sơ cá nhân</div>
            <h2 class="profile-section-title">Hồ sơ cá nhân</h2>

            <!-- ĐỔI AVATAR TÙY Ý (THEO YÊU CẦU CỦA BẠN) -->
            <div class="avatar-upload-section">
                <div class="avatar-preview-wrap">
                    <img id="avatarPreview" class="avatar-preview-img" src="https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80" alt="Avatar">
                </div>
                <div class="avatar-actions-wrap">
                    <div class="avatar-actions-title">Ảnh đại diện người bán & người mua</div>
                    <div class="avatar-actions-hint">Hỗ trợ tải ảnh từ máy (JPG, PNG) hoặc chọn mẫu có sẵn để tạo uy tín khi giao dịch đồ cũ.</div>
                    <div class="avatar-btn-row">
                        <label class="btn-avatar-upload">
                            <i class="fas fa-camera"></i> Tải ảnh từ máy
                            <input type="file" id="avatarFileInput" accept="image/*" style="display: none;" onchange="handleAvatarFileSelect(event)">
                        </label>
                        <button type="button" class="btn-avatar-preset" onclick="setPresetAvatar(1)">Mẫu 1</button>
                        <button type="button" class="btn-avatar-preset" onclick="setPresetAvatar(2)">Mẫu 2</button>
                        <button type="button" class="btn-avatar-preset" onclick="setPresetAvatar(3)">Mẫu 3</button>
                        <button type="button" class="btn-avatar-preset" onclick="promptAvatarUrl()">Dán URL ảnh</button>
                    </div>
                </div>
            </div>

            <form id="profileForm" onsubmit="handleSaveProfile(event)">
                <!-- Họ và tên * -->
                <div class="form-group-profile">
                    <label class="form-label-profile" for="inputFullName">
                        Họ và tên <span class="required">*</span>
                    </label>
                    <div class="input-with-clear">
                        <input type="text" id="inputFullName" class="form-input-profile" placeholder="Nhập họ và tên" required oninput="checkClearBtn(this, 'btnClearName')">
                        <button type="button" id="btnClearName" class="btn-clear-input" onclick="clearField('inputFullName', this)"><i class="fas fa-times-circle"></i></button>
                    </div>
                </div>

                <!-- Số điện thoại * -->
                <div class="form-group-profile">
                    <label class="form-label-profile" for="inputPhone">
                        Số điện thoại <span class="required">*</span>
                    </label>
                    <input type="tel" id="inputPhone" class="form-input-profile" placeholder="0901234567" required>
                </div>

                <!-- Địa chỉ -->
                <div class="form-group-profile">
                    <label class="form-label-profile" for="inputAddress">
                        Địa chỉ
                    </label>
                    <select id="inputAddress" class="form-input-profile">
                        <option value="Quận 1, TP. Hồ Chí Minh">Quận 1, TP. Hồ Chí Minh</option>
                        <option value="Quận 3, TP. Hồ Chí Minh">Quận 3, TP. Hồ Chí Minh</option>
                        <option value="Quận Bình Thạnh, TP. Hồ Chí Minh">Quận Bình Thạnh, TP. Hồ Chí Minh</option>
                        <option value="Quận Phú Nhuận, TP. Hồ Chí Minh">Quận Phú Nhuận, TP. Hồ Chí Minh</option>
                        <option value="Quận Gò Vấp, TP. Hồ Chí Minh">Quận Gò Vấp, TP. Hồ Chí Minh</option>
                        <option value="Quận Cầu Giấy, Hà Nội">Quận Cầu Giấy, Hà Nội</option>
                        <option value="Quận Đống Đa, Hà Nội">Quận Đống Đa, Hà Nội</option>
                        <option value="Quận Hải Châu, Đà Nẵng">Quận Hải Châu, Đà Nẵng</option>
                        <option value="Khác">Khu vực khác</option>
                    </select>
                </div>

                <!-- Giới thiệu về trang -->
                <div class="form-group-profile">
                    <label class="form-label-profile" for="inputBio">
                        Giới thiệu về trang
                    </label>
                    <textarea id="inputBio" class="form-textarea-profile" placeholder="Mô tả phong cách thanh lý đồ cũ của bạn, sở thích hoặc cam kết bảo đảm chất lượng..."></textarea>
                    <div class="field-hint-bottom">
                        <span>Tối đa 60 từ</span>
                    </div>
                </div>

                <button type="submit" class="btn-submit-profile">
                    <i class="fas fa-save mr-1"></i> Lưu thay đổi
                </button>
            </form>
        </div>

        <!-- TAB 2: ĐỔI MẬT KHẨU (ĐÃ THAY THẾ CHO CÀI ĐẶT CŨ THEO YÊU CẦU) -->
        <div id="tabPanePassword" style="display: none;">
            <div class="profile-breadcrumb">Bảo mật tài khoản</div>
            <h2 class="profile-section-title">Đổi mật khẩu</h2>

            <form id="passwordForm" onsubmit="handleSavePassword(event)">
                <div class="form-group-profile">
                    <label class="form-label-profile" for="inputOldPassword">
                        Mật khẩu hiện tại <span class="required">*</span>
                    </label>
                    <input type="password" id="inputOldPassword" class="form-input-profile" placeholder="Nhập mật khẩu hiện tại" required>
                </div>

                <div class="form-group-profile">
                    <label class="form-label-profile" for="inputNewPassword">
                        Mật khẩu mới <span class="required">*</span>
                    </label>
                    <input type="password" id="inputNewPassword" class="form-input-profile" placeholder="Ít nhất 8 ký tự" minlength="8" required>
                </div>

                <div class="form-group-profile">
                    <label class="form-label-profile" for="inputConfirmPassword">
                        Nhập lại mật khẩu mới <span class="required">*</span>
                    </label>
                    <input type="password" id="inputConfirmPassword" class="form-input-profile" placeholder="Nhập lại mật khẩu mới" minlength="8" required>
                </div>

                <button type="submit" class="btn-submit-profile">
                    <i class="fas fa-key mr-1"></i> Cập nhật mật khẩu
                </button>
            </form>
        </div>
    </main>
</div>

@section Scripts {
<script>
    let currentAvatarData = '';

    const PRESET_AVATARS = [
        "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
        "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
        "https://images.unsplash.com/photo-1570295999919-56ceb5ecca61?w=150&q=80"
    ];

    function initProfilePage() {
        if (typeof ThriftDB === 'undefined' || !ThriftDB.isLoggedIn()) {
            window.location.replace('/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname));
            return;
        }

        const user = ThriftDB.getCurrentUser();
        if (!user) return;

        // Điền dữ liệu vào form
        document.getElementById('inputFullName').value = user.name || '';
        document.getElementById('inputPhone').value = user.phone || '';
        if (user.address) {
            const addrSelect = document.getElementById('inputAddress');
            let found = false;
            for (let i = 0; i < addrSelect.options.length; i++) {
                if (addrSelect.options[i].value === user.address) {
                    addrSelect.selectedIndex = i;
                    found = true;
                    break;
                }
            }
            if (!found && user.address) {
                const opt = document.createElement('option');
                opt.value = user.address;
                opt.textContent = user.address;
                opt.selected = true;
                addrSelect.appendChild(opt);
            }
        }
        document.getElementById('inputBio').value = user.bio || '';

        currentAvatarData = user.avatar || PRESET_AVATARS[0];
        document.getElementById('avatarPreview').src = currentAvatarData;

        checkClearBtn(document.getElementById('inputFullName'), 'btnClearName');
    }

    function switchProfileTab(tab) {
        const tabProfile = document.getElementById('tabPaneProfile');
        const tabPassword = document.getElementById('tabPanePassword');
        const btnProfile = document.getElementById('tabBtnProfile');
        const btnPassword = document.getElementById('tabBtnPassword');

        if (tab === 'profile') {
            tabProfile.style.display = 'block';
            tabPassword.style.display = 'none';
            btnProfile.classList.add('active');
            btnPassword.classList.remove('active');
        } else {
            tabProfile.style.display = 'none';
            tabPassword.style.display = 'block';
            btnProfile.classList.remove('active');
            btnPassword.classList.add('active');
        }
    }

    function updateUrlPreview(val) {
        const clean = (val || 'ten-goi-nho').toLowerCase().replace(/\s+/g, '-');
        document.getElementById('urlPreview').textContent = `https://thriftshop.vn/user/${clean}`;
    }

    function checkClearBtn(input, btnId) {
        const btn = document.getElementById(btnId);
        if (btn) {
            btn.style.display = input.value.trim().length > 0 ? 'block' : 'none';
        }
    }

    function clearField(inputId, btn) {
        const input = document.getElementById(inputId);
        if (input) {
            input.value = '';
            input.focus();
            btn.style.display = 'none';
        }
    }

    function handleAvatarFileSelect(e) {
        const file = e.target.files[0];
        if (!file) return;

        if (file.size > 3 * 1024 * 1024) {
            alert("Vui lòng chọn ảnh có dung lượng dưới 3MB!");
            return;
        }

        const reader = new FileReader();
        reader.onload = function(evt) {
            currentAvatarData = evt.target.result;
            document.getElementById('avatarPreview').src = currentAvatarData;
            if (typeof showToast === 'function') {
                showToast("Đã tải ảnh đại diện lên! Nhấn 'Lưu thay đổi' để hoàn tất.", "info");
            }
        };
        reader.readAsDataURL(file);
    }

    function setPresetAvatar(idx) {
        const url = PRESET_AVATARS[idx - 1] || PRESET_AVATARS[0];
        currentAvatarData = url;
        document.getElementById('avatarPreview').src = url;
    }

    function promptAvatarUrl() {
        const url = prompt("Nhập đường link (URL) ảnh đại diện của bạn:", currentAvatarData);
        if (url && url.trim().length > 5) {
            currentAvatarData = url.trim();
            document.getElementById('avatarPreview').src = currentAvatarData;
        }
    }

    function handleSaveProfile(e) {
        e.preventDefault();
        const fullName = document.getElementById('inputFullName').value.trim();
        const phone = document.getElementById('inputPhone').value.trim();
        const address = document.getElementById('inputAddress').value;
        const bio = document.getElementById('inputBio').value.trim();

        if (!fullName) {
            alert("Vui lòng nhập họ và tên!");
            return;
        }

        const formData = new FormData();
        formData.append('name', fullName);
        formData.append('phone', phone);
        formData.append('address', address);
        formData.append('bio', bio);
        if (currentAvatarData) {
            formData.append('avatar', currentAvatarData);
        }

        fetch('/Account/UpdateProfile', {
            method: 'POST',
            body: formData
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                if (typeof ThriftDB !== 'undefined') {
                    ThriftDB.updateCurrentUser({
                        name: fullName,
                        phone: phone,
                        address: address,
                        bio: bio,
                        avatar: currentAvatarData
                    });
                }
                if (typeof showToast === 'function') {
                    showToast("Cập nhật thông tin hồ sơ cá nhân thành công! 🎉", "success");
                } else {
                    alert("Cập nhật thông tin thành công!");
                }
            } else {
                alert(data.message || "Cập nhật hồ sơ thất bại!");
            }
        })
        .catch(err => {
            alert("Lỗi kết nối máy chủ!");
        });
    }

    function handleSavePassword(e) {
        e.preventDefault();
        const oldPass = document.getElementById('inputOldPassword').value;
        const newPass = document.getElementById('inputNewPassword').value;
        const confirmPass = document.getElementById('inputConfirmPassword').value;

        if (newPass.length < 6) {
            alert("Mật khẩu mới phải có ít nhất 6 ký tự!");
            return;
        }

        if (newPass !== confirmPass) {
            alert("Mật khẩu xác nhận không khớp với mật khẩu mới!");
            return;
        }

        const formData = new FormData();
        formData.append('oldPassword', oldPass);
        formData.append('newPassword', newPass);
        formData.append('confirmPassword', confirmPass);

        fetch('/Account/ChangePassword', {
            method: 'POST',
            body: formData
        })
        .then(res => res.json())
        .then(data => {
            if (data.success) {
                document.getElementById('passwordForm').reset();
                if (typeof ThriftDB !== 'undefined') {
                    ThriftDB.changePassword(oldPass, newPass);
                }
                if (typeof showToast === 'function') {
                    showToast("Đổi mật khẩu tài khoản thành công! 🔒", "success");
                } else {
                    alert("Đổi mật khẩu thành công!");
                }
            } else {
                alert(data.message || "Đổi mật khẩu thất bại!");
            }
        })
        .catch(err => {
            alert("Lỗi kết nối máy chủ!");
        });
    }

    document.addEventListener('DOMContentLoaded', initProfilePage);
</script>
}
```

---

## Views/Account/SellerProfile.cshtml

```html
@{
    ViewData["Title"] = "Trang Cá Nhân & Tin Đăng Bán - ThriftShop";
    var queryUserId = ViewBag.UserId as string ?? "";
}

<style>
    .seller-profile-container {
        width: 100%;
        max-width: 1240px;
        margin: 30px auto 60px;
        padding: 0 clamp(14px, 2.5vw, 30px);
    }

    /* CARD HỒ SƠ NGƯỜI DÙNG */
    .seller-hero-card {
        background: #ffffff;
        border: 1px solid #e4e4e7;
        border-radius: 16px;
        padding: clamp(20px, 3vw, 32px);
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.03);
        margin-bottom: 35px;
    }

    .seller-hero-main {
        display: flex;
        align-items: center;
        gap: clamp(16px, 2.5vw, 28px);
        flex-wrap: wrap;
    }

    .seller-avatar-wrapper {
        position: relative;
        width: clamp(80px, 10vw, 100px);
        height: clamp(80px, 10vw, 100px);
        flex-shrink: 0;
    }

    .seller-avatar-img {
        width: 100%;
        height: 100%;
        border-radius: 50%;
        object-fit: cover;
        border: 3px solid #f4f4f5;
        box-shadow: 0 2px 10px rgba(0, 0, 0, 0.08);
    }

    .seller-status-badge {
        position: absolute;
        bottom: 4px;
        right: 4px;
        width: 18px;
        height: 18px;
        background: #22c55e;
        border: 3px solid #ffffff;
        border-radius: 50%;
    }

    .seller-info-col {
        flex: 1;
        min-width: 250px;
    }

    .seller-name-row {
        display: flex;
        align-items: center;
        gap: 8px;
        margin-bottom: 6px;
        flex-wrap: wrap;
    }

    .seller-name-title {
        font-size: clamp(20px, 2.4vw, 26px);
        font-weight: 800;
        color: #18181b;
        margin: 0;
    }

    .seller-verified-tag {
        color: #ff3c00;
        font-size: 16px;
    }

    .seller-meta-row {
        display: flex;
        align-items: center;
        gap: 16px;
        color: #71717a;
        font-size: 13.5px;
        margin-bottom: 12px;
        flex-wrap: wrap;
    }

    .seller-meta-row span {
        display: inline-flex;
        align-items: center;
        gap: 6px;
    }

    .seller-bio-text {
        font-size: 14px;
        color: #3f3f46;
        line-height: 1.6;
        background: #f8fafc;
        border-radius: 10px;
        padding: 12px 16px;
        border: 1px dashed #e2e8f0;
        margin: 0;
    }

    .seller-actions-col {
        display: flex;
        gap: 10px;
        align-items: center;
        flex-wrap: wrap;
    }

    .btn-seller-action {
        padding: 10px 18px;
        border-radius: 10px;
        font-weight: 600;
        font-size: 14px;
        text-decoration: none;
        display: inline-flex;
        align-items: center;
        gap: 6px;
        transition: all 0.2s;
        cursor: pointer;
    }

    .btn-seller-primary {
        background: #ff3c00;
        color: white;
        border: none;
        box-shadow: 0 4px 12px rgba(255, 60, 0, 0.25);
    }

    .btn-seller-primary:hover {
        background: #e03500;
        color: white;
    }

    .btn-seller-secondary {
        background: #ffffff;
        color: #3f3f46;
        border: 1.5px solid #e4e4e7;
    }

    .btn-seller-secondary:hover {
        border-color: #ff3c00;
        color: #ff3c00;
    }

    /* TIÊU ĐỀ KHU VỰC SẢN PHẨM */
    .seller-listings-header {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 24px;
        flex-wrap: wrap;
        gap: 12px;
        border-bottom: 1px solid #e4e4e7;
        padding-bottom: 16px;
    }

    .seller-listings-title {
        font-size: clamp(18px, 2vw, 22px);
        font-weight: 800;
        color: #18181b;
        margin: 0;
        display: flex;
        align-items: center;
        gap: 10px;
    }

    .seller-listings-badge {
        font-size: 13px;
        font-weight: 700;
        background: #fee2e2;
        color: #ef4444;
        padding: 4px 10px;
        border-radius: 20px;
    }

    .seller-listings-note {
        font-size: 13px;
        color: #71717a;
    }

    /* EMPTY STATE */
    .seller-empty-box {
        text-align: center;
        padding: 60px 20px;
        background: #ffffff;
        border-radius: 16px;
        border: 1px dashed #d4d4d8;
        max-width: 580px;
        margin: 30px auto;
    }

    .seller-empty-icon {
        font-size: 48px;
        color: #d4d4d8;
        margin-bottom: 14px;
    }

    .seller-empty-title {
        font-size: 18px;
        font-weight: 700;
        color: #18181b;
        margin-bottom: 6px;
    }

    .seller-empty-desc {
        font-size: 13.5px;
        color: #71717a;
        line-height: 1.6;
        margin-bottom: 20px;
    }
</style>

<div class="seller-profile-container">
    <!-- 1. HỒ SƠ NGƯỜI DÙNG / NGƯỜI BÁN -->
    <section class="seller-hero-card" id="sellerHeroCard">
        <div class="seller-hero-main">
            <div class="seller-avatar-wrapper">
                <img id="sellerAvatar" src="https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80" alt="Avatar" class="seller-avatar-img">
                <span class="seller-status-badge" title="Đang hoạt động"></span>
            </div>

            <div class="seller-info-col">
                <div class="seller-name-row">
                    <h1 class="seller-name-title" id="sellerName">Đang tải...</h1>
                    <i class="fas fa-check-circle seller-verified-tag" title="Người dùng uy tín"></i>
                </div>

                <div class="seller-meta-row">
                    <span><i class="fas fa-circle text-success mr-1" style="font-size: 9px; color: #22c55e;"></i> Đang hoạt động</span>
                    <span><i class="fas fa-bolt text-warning mr-1" style="color: #f59e0b;"></i> Phản hồi: Nhanh</span>
                    <span><i class="fas fa-map-marker-alt text-danger mr-1" style="color: #ff3c00;"></i> <span id="sellerAddress">TP. Hồ Chí Minh</span></span>
                    <span><i class="fas fa-phone mr-1"></i> <span id="sellerPhone">0903 124 567</span></span>
                </div>

                <p class="seller-bio-text" id="sellerBio">
                    Chuyên thanh lý đồ công nghệ & gia dụng cũ còn tốt, hỗ trợ giao dịch trực tiếp.
                </p>
            </div>

            <div class="seller-actions-col" id="sellerActionsContainer">
                <!-- Nút thao tác tương ứng (sửa thông tin nếu là chính mình, hoặc Chat nếu là người mua) -->
            </div>
        </div>
    </section>

    <!-- 2. BÀI ĐĂNG BÁN ĐỒ CŨ (BẮT BUỘC: ĐÃ BÁN / ĐÃ XOÁ KHÔNG HIỂN THỊ) -->
    <div class="seller-listings-header">
        <div>
            <h2 class="seller-listings-title">
                📦 Món Đồ Đang Đăng Bán
                <span class="seller-listings-badge" id="sellerListingsCount">0 tin</span>
            </h2>
            <div class="seller-listings-note">
                Chỉ hiển thị các món đồ đang mở bán công khai. Món đồ đã đánh dấu "Đã bán" hoặc đã xoá sẽ tự động ẩn khỏi trang cá nhân.
            </div>
        </div>
        <div>
            <a href="/Products" class="btn btn-outline-secondary" style="border-radius: 8px; font-weight: 600; font-size: 13.5px; padding: 7px 14px;">
                <i class="fas fa-search mr-1"></i> Khám phá sàn đồ cũ
            </a>
        </div>
    </div>

    <!-- KHUNG RENDER LƯỚI SẢN PHẨM -->
    <div id="sellerProductsGrid" class="modern-product-grid">
        <!-- Render bằng JS -->
    </div>
</div>

@section Scripts {
<script>
    const targetUserId = "@queryUserId";

    function loadSellerProfilePage() {
        if (typeof ThriftDB === 'undefined') return;

        const currentUser = ThriftDB.getCurrentUser();
        let targetIdentifier = targetUserId;

        // Nếu không có userId trên query, mặc định lấy người dùng hiện tại đang đăng nhập
        if (!targetIdentifier && currentUser) {
            targetIdentifier = currentUser.id || currentUser.email;
        }

        // Lấy thông tin user
        let userProfile = null;
        if (targetIdentifier) {
            userProfile = ThriftDB.getUserById(targetIdentifier);
        }

        // Fallback: nếu vẫn không tìm thấy, lấy currentUser hoặc default user
        if (!userProfile) {
            userProfile = currentUser || {
                id: 2,
                name: "Trần Nguyễn Anh Khoa",
                email: "user@thriftshop.vn",
                phone: "0889771935",
                address: "Quận 1, TP. Hồ Chí Minh",
                avatar: "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                bio: "Chuyên thanh lý đồ công nghệ & gia dụng cũ còn tốt, hỗ trợ giao dịch trực tiếp."
            };
        }

        // Cập nhật DOM thông tin người dùng
        const avatarEl = document.getElementById('sellerAvatar');
        if (avatarEl && userProfile.avatar) {
            avatarEl.src = userProfile.avatar;
        }
        const nameEl = document.getElementById('sellerName');
        if (nameEl) nameEl.textContent = userProfile.name;
        const addrEl = document.getElementById('sellerAddress');
        if (addrEl) addrEl.textContent = userProfile.address || "TP. Hồ Chí Minh";
        const phoneEl = document.getElementById('sellerPhone');
        if (phoneEl) phoneEl.textContent = userProfile.phone || "0903 124 567";
        const bioEl = document.getElementById('sellerBio');
        if (bioEl) bioEl.textContent = userProfile.bio || "Thành viên mua bán & trao đổi đồ cũ uy tín trên sàn ThriftShop.";

        // Xác định xem người xem có phải chính là chủ trang cá nhân này không
        const isSelf = currentUser && (
            String(currentUser.id) === String(userProfile.id) ||
            (currentUser.email && currentUser.email.toLowerCase() === (userProfile.email || '').toLowerCase())
        );

        const actionsContainer = document.getElementById('sellerActionsContainer');
        if (actionsContainer) {
            if (isSelf) {
                actionsContainer.innerHTML = `
                    <a href="/Account/Profile" class="btn-seller-action btn-seller-secondary" title="Chỉnh sửa thông tin, avatar, mật khẩu">
                        <i class="fas fa-user-edit"></i> Cài đặt tài khoản
                    </a>
                    <a href="/Products/PostItem" class="btn-seller-action btn-seller-primary" title="Đăng bán món đồ cũ mới">
                        <i class="fas fa-plus-circle"></i> Đăng bán món mới
                    </a>
                `;
            } else {
                actionsContainer.innerHTML = `
                    <button type="button" class="btn-seller-action btn-seller-primary" onclick="handleChatWithUser('${userProfile.id || userProfile.name}')" title="Nhắn tin trò chuyện với người bán">
                        <i class="fas fa-comment-dots"></i> Chat với người bán
                    </button>
                    <a href="tel:${userProfile.phone || '0903124567'}" class="btn-seller-action btn-seller-secondary">
                        <i class="fas fa-phone-alt"></i> Gọi điện
                    </a>
                `;
            }
        }

        // 3. TẢI CÁC MÓN ĐỒ ĐANG MỞ BÁN CỦA NGƯỜI BÁN NÀY
        // TUÂN THỦ NGHIÊM NGẶT: Các món "Đã bán" hoặc "Đã xoá" TUYỆT ĐỐI KHÔNG HIỆN!
        const activeItems = ThriftDB.getSellerActiveProducts(userProfile.id || userProfile.name || userProfile.email);
        
        const countBadge = document.getElementById('sellerListingsCount');
        if (countBadge) countBadge.textContent = `${activeItems.length} tin đang mở bán`;

        const grid = document.getElementById('sellerProductsGrid');
        if (!grid) return;

        if (activeItems.length === 0) {
            grid.innerHTML = `
                <div class="seller-empty-box" style="grid-column: 1/-1;">
                    <i class="fas fa-box-open seller-empty-icon"></i>
                    <h3 class="seller-empty-title">Chưa có món đồ nào đang mở bán</h3>
                    <p class="seller-empty-desc">
                        ${isSelf ? 'Bạn chưa có món đồ nào đang đăng bán hoặc tất cả món đồ cũ của bạn đã được thanh lý thành công.' : 'Người dùng này hiện không có món đồ cũ nào đang mở bán công khai hoặc tất cả sản phẩm đã được bán.'}
                    </p>
                    ${isSelf ? `
                        <a href="/Products/PostItem" class="btn-seller-action btn-seller-primary" style="display: inline-flex;">
                            <i class="fas fa-plus-circle"></i> Đăng bán món đồ cũ ngay
                        </a>
                    ` : `
                        <a href="/Products" class="btn-seller-action btn-seller-secondary" style="display: inline-flex;">
                            <i class="fas fa-search"></i> Xem các món đồ khác trên sàn
                        </a>
                    `}
                </div>
            `;
            return;
        }

        grid.innerHTML = activeItems.map(p => {
            const isFav = ThriftDB.isFavorite ? ThriftDB.isFavorite(p.id) : false;
            return `
                <div class="card-dark-item">
                    <div style="position: relative;">
                        <a href="/Products/Details/${p.id}" class="card-dark-image" style="display: block; cursor: pointer; text-decoration: none;" title="Xem chi tiết ${p.name}">
                            <img src="${p.image}" alt="${p.name}" onerror="this.src='https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600'">
                        </a>
                        <button class="btn-card-fav ${isFav ? 'active' : ''}" onclick="toggleProductFavorite(${p.id}, event)" title="${isFav ? 'Bỏ yêu thích' : 'Yêu thích món đồ'}" data-fav-id="${p.id}">
                            <i class="${isFav ? 'fas fa-heart text-danger' : 'far fa-heart'}"></i>
                        </button>
                    </div>
                    <div class="card-dark-body">
                        <div class="card-dark-category">${(p.categoryName || 'ĐỒ CŨ').toUpperCase()}</div>
                        <h3 class="card-dark-title" title="${p.name}">
                            <a href="/Products/Details/${p.id}" style="color: inherit; text-decoration: none;">${p.name}</a>
                        </h3>
                        <div class="card-dark-meta">
                            <i class="fas fa-map-marker-alt mr-1"></i>${p.sellerAddress || 'TP.HCM'}
                        </div>
                        <div class="card-dark-price-row">
                            <span class="card-dark-price">${ThriftDB.formatCurrency(p.price)}</span>
                            ${p.originalPrice && p.originalPrice > p.price ? `<span class="card-dark-old-price">${ThriftDB.formatCurrency(p.originalPrice)}</span>` : ''}
                        </div>
                        <div class="card-dark-actions">
                            <a href="/Products/Details/${p.id}" class="btn-card-detail">Chi tiết</a>
                            <button class="btn-card-add" onclick="openChatWithSeller(${p.id})" title="Nhắn tin với người bán">Chat</button>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    }

    function handleChatWithUser(sellerIdentifier) {
        if (!window.checkUserLoggedIn()) {
            const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
            window.location.href = `/Account/Login?returnUrl=${returnUrl}`;
            return;
        }
        const activeItems = ThriftDB.getSellerActiveProducts(sellerIdentifier);
        if (activeItems.length > 0) {
            openChatWithSeller(activeItems[0].id);
        } else {
            window.location.href = '/Cart';
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', loadSellerProfilePage);
    } else {
        loadSellerProfilePage();
    }
    if (window.jQuery) {
        $(document).ready(loadSellerProfilePage);
    }
</script>
}
```

---

## Views/Products/Index.cshtml

```html
@{
    ViewData["Title"] = "Kho Hàng Đồ Cũ Secondhand - ThriftShop";
}

<div class="shop-layout-container" style="margin-top: 35px;">
    <!-- CỘT TRÁI: BỘ LỌC 2ND (THEO MẪU ẢNH 3) -->
    <aside class="shop-filter-sidebar">
        <h3 class="filter-main-title">Bộ lọc 2nd</h3>

        <!-- NHÓM 1: DANH MỤC (5 DANH MỤC CHUẨN) -->
        <div class="filter-group-title">Danh mục</div>
        <div class="filter-checkbox-list">
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="1" onchange="onFilterChange()">
                <span>Đồ điện tử</span>
            </label>
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="2" onchange="onFilterChange()">
                <span>Đồ gia dụng</span>
            </label>
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="3" onchange="onFilterChange()">
                <span>Văn phòng phẩm</span>
            </label>
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="4" onchange="onFilterChange()">
                <span>Thời trang</span>
            </label>
            <label class="filter-checkbox-item">
                <input type="checkbox" name="catFilter" value="5" onchange="onFilterChange()">
                <span>Khác</span>
            </label>
        </div>

        <hr class="filter-divider">

        <!-- NHÓM 2: KHOẢNG GIÁ (DUAL RANGE SLIDER + INPUT TỪ - ĐẾN) -->
        <div class="filter-group-title">Khoảng giá</div>
        <div class="dual-range-wrapper">
            <div class="dual-range-track"></div>
            <div class="dual-range-fill" id="sliderFill"></div>
            <input type="range" id="priceRangeMin" min="0" max="10000000" step="50000" value="0" oninput="onRangeMinInput()">
            <input type="range" id="priceRangeMax" min="0" max="10000000" step="50000" value="10000000" oninput="onRangeMaxInput()">
        </div>

        <div class="price-inputs-row">
            <input type="number" id="priceInputMin" placeholder="Từ" min="0" step="10000" onchange="onMinInputChange()">
            <input type="number" id="priceInputMax" placeholder="Đến" min="0" step="10000" onchange="onMaxInputChange()">
        </div>

        <div class="filter-action-buttons">
            <button type="button" class="btn-filter-apply" onclick="applyFilters()">Lọc</button>
            <button type="button" class="btn-filter-reset" onclick="resetFilters()">Xóa</button>
        </div>
    </aside>

    <!-- CỘT PHẢI: DANH SÁCH SẢN PHẨM (THEO MẪU ẢNH 4 & 5) -->
    <section class="shop-content-area">
        <div class="shop-content-header">
            <div class="shop-prod-count">
                Hiển thị <strong id="productCount">0</strong> sản phẩm
            </div>
            <div class="shop-sort-box">
                <label for="sortFilter">Sắp xếp:</label>
                <select id="sortFilter" onchange="onSortChange()">
                    <option value="newest" selected>Mới đăng</option>
                    <option value="price-asc">Giá: Thấp &rarr; Cao</option>
                    <option value="price-desc">Giá: Cao &rarr; Thấp</option>
                </select>
            </div>
        </div>

        <!-- PRODUCT GRID (DARK CARDS THEO ẢNH 4) -->
        <div class="product-grid-dark" id="productGrid">
            <!-- Render bằng JavaScript -->
        </div>

        <!-- PHÂN TRANG (THEO MẪU ẢNH 5: 10 SẢN PHẨM / TRANG) -->
        <div class="pagination-container-modern" id="paginationContainer">
            <!-- Nút chuyển trang render bằng JS -->
        </div>
    </section>
</div>

@section Scripts {
<script>
    const MAX_PRICE = 10000000;
    const PAGE_SIZE = 9;
    let currentPage = 1;
    let currentFilteredList = [];
    let searchQuery = "";

    function updateSliderTrack() {
        const minVal = parseInt(document.getElementById('priceRangeMin').value) || 0;
        const maxVal = parseInt(document.getElementById('priceRangeMax').value) || MAX_PRICE;
        const fill = document.getElementById('sliderFill');
        if (fill) {
            const percent1 = (minVal / MAX_PRICE) * 100;
            const percent2 = (maxVal / MAX_PRICE) * 100;
            fill.style.left = percent1 + "%";
            fill.style.width = (percent2 - percent1) + "%";
        }
    }

    function onRangeMinInput() {
        const minEl = document.getElementById('priceRangeMin');
        const maxEl = document.getElementById('priceRangeMax');
        let minVal = parseInt(minEl.value) || 0;
        let maxVal = parseInt(maxEl.value) || MAX_PRICE;

        if (minVal > maxVal) {
            minVal = maxVal;
            minEl.value = minVal;
        }
        document.getElementById('priceInputMin').value = minVal || "";
        updateSliderTrack();
    }

    function onRangeMaxInput() {
        const minEl = document.getElementById('priceRangeMin');
        const maxEl = document.getElementById('priceRangeMax');
        let minVal = parseInt(minEl.value) || 0;
        let maxVal = parseInt(maxEl.value) || MAX_PRICE;

        if (maxVal < minVal) {
            maxVal = minVal;
            maxEl.value = maxVal;
        }
        document.getElementById('priceInputMax').value = maxVal >= MAX_PRICE ? "" : maxVal;
        updateSliderTrack();
    }

    function onMinInputChange() {
        let val = parseInt(document.getElementById('priceInputMin').value) || 0;
        if (val < 0) val = 0;
        if (val > MAX_PRICE) val = MAX_PRICE;
        const maxVal = parseInt(document.getElementById('priceRangeMax').value) || MAX_PRICE;
        if (val > maxVal) val = maxVal;
        document.getElementById('priceRangeMin').value = val;
        updateSliderTrack();
    }

    function onMaxInputChange() {
        let val = parseInt(document.getElementById('priceInputMax').value);
        if (isNaN(val) || val <= 0 || val > MAX_PRICE) val = MAX_PRICE;
        const minVal = parseInt(document.getElementById('priceRangeMin').value) || 0;
        if (val < minVal) val = minVal;
        document.getElementById('priceRangeMax').value = val;
        updateSliderTrack();
    }

    function onFilterChange() {
        applyFilters();
    }

    function onSortChange() {
        applyFilters();
    }

    function resetFilters() {
        document.querySelectorAll('input[name="catFilter"]').forEach(cb => cb.checked = false);
        document.getElementById('priceRangeMin').value = 0;
        document.getElementById('priceRangeMax').value = MAX_PRICE;
        document.getElementById('priceInputMin').value = "";
        document.getElementById('priceInputMax').value = "";
        document.getElementById('sortFilter').value = "newest";
        updateSliderTrack();
        applyFilters();
    }

    function applyFilters() {
        currentPage = 1;
        executeFilterAndRender();
    }

    function goToPage(page) {
        const totalPages = Math.ceil(currentFilteredList.length / PAGE_SIZE) || 1;
        if (page < 1 || page > totalPages) return;
        currentPage = page;
        renderCurrentPageProducts();
        renderPagination();

        const grid = document.getElementById('productGrid');
        if (grid) {
            grid.scrollIntoView({ behavior: 'smooth', block: 'start' });
        }
    }

    function calculateProductRelevance(p, query) {
        if (!query) return p.featured ? 50 : 0;
        let score = 0;
        const q = query.toLowerCase();
        const name = (p.name || '').toLowerCase();
        const cat = (p.categoryName || '').toLowerCase();
        const desc = (p.description || '').toLowerCase();

        if (name.startsWith(q)) score += 100;
        else if (name.includes(q)) score += 60;
        
        q.split(/\s+/).filter(Boolean).forEach(w => {
            if (name.includes(w)) score += 20;
            if (cat.includes(w)) score += 15;
            if (desc.includes(w)) score += 5;
        });

        if (p.featured) score += 15;
        return score;
    }

    function executeFilterAndRender() {
        if (typeof ThriftDB === 'undefined') return;

        let products = ThriftDB.getProducts();

        // 1. Lọc theo checkbox Danh mục đã chọn
        const checkedCats = Array.from(document.querySelectorAll('input[name="catFilter"]:checked')).map(cb => cb.value);
        if (checkedCats.length > 0) {
            products = products.filter(p => checkedCats.includes(String(p.categoryId)));
        }

        // 2. Lọc theo Khoảng giá
        const minPrice = parseInt(document.getElementById('priceRangeMin').value) || 0;
        const maxPrice = parseInt(document.getElementById('priceRangeMax').value) || MAX_PRICE;
        products = products.filter(p => p.price >= minPrice && p.price <= maxPrice);

        // 3. Lọc theo từ khóa tìm kiếm nếu có
        if (searchQuery) {
            const q = searchQuery.toLowerCase();
            products = products.filter(p => {
                const name = (p.name || '').toLowerCase();
                const desc = (p.description || '').toLowerCase();
                const cat = (p.categoryName || '').toLowerCase();
                return name.includes(q) || desc.includes(q) || cat.includes(q);
            });
        }

        // 4. Sắp xếp (Mới đăng, Giá cả)
        const sortVal = document.getElementById('sortFilter') ? document.getElementById('sortFilter').value : 'newest';
        if (sortVal === 'price-asc') {
            products.sort((a, b) => a.price - b.price);
        } else if (sortVal === 'price-desc') {
            products.sort((a, b) => b.price - a.price);
        } else {
            // Mặc định: Mới đăng
            products.sort((a, b) => {
                const dateA = a.createdAt ? new Date(a.createdAt).getTime() : a.id;
                const dateB = b.createdAt ? new Date(b.createdAt).getTime() : b.id;
                return dateB - dateA || b.id - a.id;
            });
        }

        currentFilteredList = products;

        // Cập nhật số lượng
        const countEl = document.getElementById('productCount');
        if (countEl) {
            countEl.textContent = products.length;
        }

        renderCurrentPageProducts();
        renderPagination();
    }

    function renderCurrentPageProducts() {
        const grid = document.getElementById('productGrid');
        if (!grid) return;

        if (currentFilteredList.length === 0) {
            grid.innerHTML = '<div style="grid-column: 1/-1; text-align: center; padding: 60px 20px; color: #71717a; background: white; border-radius: 14px; border: 1px dashed #d4d4d8;">Không tìm thấy món đồ cũ nào phù hợp với bộ lọc. Hãy thử điều chỉnh lại khoảng giá hoặc danh mục.</div>';
            return;
        }

        // Cắt đúng 9 sản phẩm cho trang hiện tại (PAGE_SIZE = 9, lưới 3x3 cân đối)
        const startIndex = (currentPage - 1) * PAGE_SIZE;
        const pageItems = currentFilteredList.slice(startIndex, startIndex + PAGE_SIZE);

        grid.innerHTML = pageItems.map(p => {
            const isFav = typeof ThriftDB !== 'undefined' && typeof ThriftDB.isFavorite === 'function' && ThriftDB.isFavorite(p.id);
            return `
                <div class="card-dark-item">
                    <div style="position: relative;">
                        <a href="/Products/Details/${p.id}" class="card-dark-image" style="display: block; cursor: pointer; text-decoration: none;" title="Xem chi tiết ${p.name}">
                            <img src="${p.image}" alt="${p.name}" onerror="this.src='https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600'">
                        </a>
                        <button class="btn-card-fav ${isFav ? 'active' : ''}" onclick="toggleProductFavorite(${p.id}, event)" title="${isFav ? 'Bỏ yêu thích' : 'Yêu thích món đồ'}" data-fav-id="${p.id}">
                            <i class="${isFav ? 'fas fa-heart text-danger' : 'far fa-heart'}"></i>
                        </button>
                    </div>
                    <div class="card-dark-body">
                        <div class="card-dark-category">${(p.categoryName || 'ĐỒ CŨ').toUpperCase()}</div>
                        <h3 class="card-dark-title" title="${p.name}">
                            <a href="/Products/Details/${p.id}" style="color: inherit; text-decoration: none;">${p.name}</a>
                        </h3>
                        <div class="card-dark-meta">
                            <i class="fas fa-map-marker-alt mr-1"></i>${p.sellerAddress || 'TP.HCM'} · Người bán: <strong>${p.sellerName || 'Thành viên'}</strong>
                        </div>
                        <div class="card-dark-price-row">
                            <span class="card-dark-price">${ThriftDB.formatCurrency(p.price)}</span>
                            ${p.originalPrice && p.originalPrice > p.price ? `<span class="card-dark-old-price">${ThriftDB.formatCurrency(p.originalPrice)}</span>` : ''}
                        </div>
                        <div class="card-dark-actions">
                            <a href="/Products/Details/${p.id}" class="btn-card-detail">Chi tiết</a>
                            <button class="btn-card-add" onclick="openChatWithSeller(${p.id})" title="Nhắn tin với người bán">Chat</button>
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    }

    function renderPagination() {
        const container = document.getElementById('paginationContainer');
        if (!container) return;

        const totalPages = Math.ceil(currentFilteredList.length / PAGE_SIZE);
        if (totalPages <= 1) {
            container.innerHTML = "";
            return;
        }

        let html = "";

        // Nút Prev (<-)
        html += `<button type="button" class="modern-page-btn ${currentPage === 1 ? 'disabled' : ''}" onclick="goToPage(${currentPage - 1})" title="Trang trước">&larr;</button>`;

        // Các nút số trang
        for (let i = 1; i <= totalPages; i++) {
            html += `<button type="button" class="modern-page-btn ${currentPage === i ? 'active' : ''}" onclick="goToPage(${i})">${i}</button>`;
        }

        // Nút Next (->)
        html += `<button type="button" class="modern-page-btn ${currentPage === totalPages ? 'disabled' : ''}" onclick="goToPage(${currentPage + 1})" title="Trang sau">&rarr;</button>`;

        container.innerHTML = html;
    }

    function handleAddToCart(productId) {
        if (typeof ThriftDB !== 'undefined') {
            ThriftDB.addToCart(productId, 1);
            if (typeof updateCartBadge === 'function') updateCartBadge();
            const p = ThriftDB.getProductById(productId);
            showToast(`Đã thêm "${p ? p.name : 'Món đồ'}" vào giỏ hàng!`);
        }
    }

    function initProductsPage() {
        updateSliderTrack();

        // Đọc query search từ url nếu có
        const urlParams = new URLSearchParams(window.location.search);
        const searchParam = urlParams.get('search');
        if (searchParam) {
            searchQuery = searchParam.trim();
            const searchInput = document.getElementById('searchInput');
            if (searchInput) searchInput.value = searchQuery;
            const sortFilter = document.getElementById('sortFilter');
            if (sortFilter) sortFilter.value = 'newest';
        }

        const catParam = urlParams.get('category');
        if (catParam) {
            const cb = document.querySelector(`input[name="catFilter"][value="${catParam}"]`);
            if (cb) cb.checked = true;
        }

        executeFilterAndRender();
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initProductsPage);
    } else {
        initProductsPage();
    }
    if (window.jQuery) {
        $(document).ready(initProductsPage);
    }
</script>
}
```

---

## Views/Products/Details.cshtml

```html
@model ThriftShop.Models.ProductDetailViewModel
@{
    var p = Model?.Product;
    var seller = Model?.Seller;
    ViewData["Title"] = (p != null ? p.Name : "Chi Tiết Món Đồ Cũ") + " - ThriftShop";
    var productId = p?.Id ?? (ViewBag.ProductId ?? 101);
}

<div class="container" style="max-width: 1440px; width: 95%; margin: 30px auto 60px; padding: 0 15px;">
    <!-- BREADCRUMB -->
    <div style="font-size: 13.5px; color: #71717a; margin-bottom: 20px;">
        <a asp-controller="Home" asp-action="Index">Trang chủ</a> / 
        <a asp-controller="Products" asp-action="Index">Sàn đồ cũ</a> / 
        <span id="detailBreadcrumb" style="color: #ff3c00; font-weight: 500;">@(p != null ? p.Name : "Chi tiết món đồ")</span>
    </div>

    <!-- MAIN PRODUCT DETAIL CARD -->
    <div class="product-detail-layout" id="productDetailCard">
        <!-- LEFT: ẢNH MÓN ĐỒ -->
        <div>
            <div style="position: relative;">
                <img id="detailImage" class="main-image" src="@(p != null ? p.Image : "")" alt="@(p != null ? p.Name : "Đồ cũ")" onerror="this.src='https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600'">
            </div>
            <div class="gallery-thumbs" id="detailThumbs">
                @if (Model != null && Model.Images.Count > 1)
                {
                    foreach (var img in Model.Images)
                    {
                        <img src="@img" alt="Ảnh chi tiết" onclick="document.getElementById('detailImage').src = '@img'" style="width: 62px; height: 62px; object-fit: cover; border-radius: 8px; cursor: pointer; border: 2px solid #e4e4e7; transition: border-color 0.2s;" onmouseover="this.style.borderColor='#ff3c00'" onmouseout="this.style.borderColor='#e4e4e7'">
                    }
                }
            </div>
        </div>

        <!-- RIGHT: THÔNG TIN CHI TIẾT & NGƯỜI BÁN -->
        <div>
            <span id="detailCategory" style="color: #71717a; font-size: 12px; font-weight: 700; text-transform: uppercase; letter-spacing: 0.5px;">
                @(p != null ? p.CategoryName : "ĐỒ CŨ SECONDHAND")
            </span>
            <h1 id="detailTitle" style="font-size: 22px; font-weight: 700; color: #18181b; margin: 6px 0 10px; line-height: 1.4;">
                @(p != null ? p.Name : "")
            </h1>

            <div class="product-detail-price" id="detailPriceBox">
                <span id="detailPrice" style="color: #ff3c00; font-size: 26px; font-weight: 800;">
                    @(p != null ? p.Price.ToString("N0") + " ₫" : "0 ₫")
                </span>
                @if (p != null && p.OriginalPrice > p.Price)
                {
                    <span id="detailOriginalPrice" style="font-size: 15px; color: #a1a1aa; font-weight: 400; text-decoration: line-through; margin-left: 10px;">
                        @p.OriginalPrice.ToString("N0") ₫
                    </span>
                }
            </div>

            <!-- MÔ TẢ TÌNH TRẠNG TỪ NGƯỜI BÁN -->
            <div style="background: #f8fafc; border: 1px solid #e4e4e7; border-radius: 12px; padding: 18px; margin-bottom: 20px;">
                <div style="font-weight: 700; font-size: 13.5px; margin-bottom: 8px; color: #18181b;">
                    <i class="fas fa-info-circle text-primary mr-1" style="color: #ff3c00;"></i> Mô tả tình trạng từ người bán:
                </div>
                <div id="detailConditionNotes" style="font-size: 13.5px; color: #3f3f46; line-height: 1.6;">
                    @(p != null && !string.IsNullOrEmpty(p.Description) ? p.Description : "Đồ dùng giữ gìn cẩn thận, đã được vệ sinh sạch sẽ trước khi thanh lý.")
                </div>
            </div>

            <!-- THÔNG TIN NGƯỜI ĐĂNG BÁN & KHU VỰC GIAO DỊCH (MÔ HÌNH CHỢ TỐT C2C) -->
            <div style="background: #ffffff; border: 1.5px solid #e4e4e7; border-radius: 14px; padding: 18px; margin-bottom: 20px; box-shadow: 0 2px 10px rgba(0,0,0,0.02);">
                <div style="display: flex; align-items: center; justify-content: space-between; margin-bottom: 14px; flex-wrap: wrap; gap: 12px;">
                    <div style="display: flex; align-items: center; gap: 14px;">
                        <div style="position: relative; width: 52px; height: 52px; flex-shrink: 0;">
                            <img id="detailSellerAvatar" src="@(seller?.Avatar ?? p?.SellerAvatar ?? "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&q=80")" alt="Seller" style="width: 100%; height: 100%; border-radius: 50%; object-fit: cover; border: 2px solid #f4f4f5;">
                            <span style="position: absolute; bottom: 1px; right: 1px; width: 12px; height: 12px; background: #22c55e; border: 2px solid white; border-radius: 50%;"></span>
                        </div>
                        <div>
                            <div style="display: flex; align-items: center; gap: 6px;">
                                <strong id="detailSellerName" style="font-size: 16px; color: #18181b; font-weight: 700;">
                                    @(seller?.Name ?? p?.SellerName ?? "Người bán ThriftShop")
                                </strong>
                                <i class="fas fa-check-circle" style="font-size: 13px; color: #2563eb;" title="Đã xác thực tài khoản"></i>
                            </div>
                            <div style="font-size: 13px; color: #71717a; margin-top: 2px;">
                                <span style="display: inline-flex; align-items: center; gap: 5px;">
                                    <span style="display: inline-block; width: 7px; height: 7px; background: #22c55e; border-radius: 50%;"></span> Đang hoạt động
                                </span>
                            </div>
                        </div>
                    </div>
                    <div>
                        <a id="btnViewSellerProfile" href="/Account/SellerProfile?userId=@(p?.SellerId ?? 0)" style="background: #f4f4f5; color: #18181b; font-weight: 600; font-size: 13px; border-radius: 20px; padding: 7px 16px; text-decoration: none; border: 1px solid #e4e4e7; transition: all 0.2s;">
                            Xem trang cá nhân
                        </a>
                    </div>
                </div>

                <!-- Số điện thoại & Địa chỉ giao dịch xem hàng trực tiếp -->
                <div style="border-top: 1px dashed #e4e4e7; padding-top: 12px; display: flex; flex-direction: column; gap: 8px; font-size: 13.5px; color: #3f3f46;">
                    <div style="display: flex; align-items: center; gap: 8px;">
                        <i class="fas fa-phone-alt" style="width: 18px; color: #16a34a;"></i>
                        <span>Hotline / Zalo: <strong style="color: #18181b; font-size: 14px;">@(p?.SellerPhone ?? "0901234567")</strong></span>
                    </div>
                    <div style="display: flex; align-items: flex-start; gap: 8px;">
                        <i class="fas fa-map-marker-alt" style="width: 18px; color: #ef4444; margin-top: 2px;"></i>
                        <span>Địa chỉ xem đồ trực tiếp: <strong style="color: #18181b;">@(p?.SellerAddress ?? "TP. Hồ Chí Minh")</strong></span>
                    </div>
                </div>
            </div>

            @{
                var currentUserId = Context.Session.GetUserId();
                bool isOwnPost = currentUserId.HasValue && (currentUserId.Value == p?.SellerId || (seller != null && seller.Id == currentUserId.Value));
                bool isSold = p != null && (p.IsSold || (p.Status != null && p.Status.ToLower() == "sold"));
                bool isDeleted = p != null && (p.Deleted || (p.Status != null && p.Status.ToLower() == "deleted"));
                bool isProductClosed = isSold || isDeleted;
            }

            @if (isProductClosed)
            {
                <div style="background: #f8fafc; border: 1.5px solid #cbd5e1; border-radius: 12px; padding: 14px 18px; margin-bottom: 20px; display: flex; align-items: center; gap: 14px;">
                    <div style="width: 42px; height: 42px; border-radius: 50%; background: @(isSold ? "#e2e8f0" : "#fee2e2"); display: flex; align-items: center; justify-content: center; flex-shrink: 0;">
                        <i class="@(isSold ? "fas fa-handshake" : "fas fa-ban")" style="color: @(isSold ? "#475569" : "#dc2626"); font-size: 18px;"></i>
                    </div>
                    <div>
                        <div style="font-weight: 700; color: #1e293b; font-size: 14.5px;">
                            @Html.Raw(isSold ? "Món đồ này đã bán (Đã hoàn tất giao dịch)" : "Tin đăng này đã được đóng hoặc gỡ bỏ")
                        </div>
                        <div style="font-size: 12.5px; color: #64748b; margin-top: 2px;">
                            Sản phẩm đã ẩn khỏi sàn công khai. Bạn và người bán vẫn có thể nhắn tin trao đổi hoặc tra cứu lịch sử trong Hộp thư Chat.
                        </div>
                    </div>
                </div>
            }

            <!-- HÀNH ĐỘNG CHÍNH: CHAT VỚI NGƯỜI BÁN & YÊU THÍCH -->
            <div style="display: flex; gap: 12px; align-items: center; margin-bottom: 24px;">
                @if (isOwnPost)
                {
                    <div style="flex: 1; padding: 13px 18px; background: #f1f5f9; border: 1.5px solid #cbd5e1; border-radius: 8px; color: #334155; font-size: 14px; display: flex; align-items: center; justify-content: space-between;">
                        <span><i class="fas fa-user-check text-primary mr-1" style="color: #2563eb;"></i> <strong>Đây là tin đăng bán của bạn</strong></span>
                        <a asp-controller="Products" asp-action="MyListings" style="font-weight: 700; color: #2563eb; text-decoration: underline; font-size: 13px;">
                            Quản lý tin đăng &rarr;
                        </a>
                    </div>
                }
                else
                {
                    <button id="btnChatWithSellerDetail" onclick="handleDetailChat(@productId, @(currentUserId.HasValue ? "true" : "false"))" class="choice-btn active" style="flex: 1; padding: 14px 20px; font-size: 15px; border-radius: 8px; background: #ff3c00; border: none; color: white; display: inline-flex; align-items: center; justify-content: center; gap: 10px; font-weight: 700; cursor: pointer; box-shadow: 0 4px 14px rgba(255, 60, 0, 0.25);">
                        <i class="fas fa-comment-dots" style="font-size: 19px;"></i> Chat Với Người Bán
                    </button>
                }
                
                <button id="btnFavDetail" type="button" data-fav-id="@productId" onclick="toggleProductFavorite(@productId, event)" style="padding: 14px 20px; font-size: 15px; border-radius: 8px; border: 1.5px solid #e4e4e7; background: #ffffff; color: #71717a; cursor: pointer; transition: all 0.2s;" title="Yêu thích món đồ">
                    <i class="@(Model != null && Model.IsFavorite ? "fas fa-heart text-danger mr-1" : "far fa-heart mr-1")" id="detailFavIcon"></i> 
                    <span id="detailFavText">@(Model != null && Model.IsFavorite ? "Đã thích" : "Yêu thích")</span>
                </button>
            </div>
        </div>
    </div>

    <!-- MÔ TẢ CHI TIẾT TỪ NGƯỜI BÁN -->
    <div style="margin-top: 35px; background: white; border: 1px solid #e4e4e7; border-radius: 16px; padding: 28px;">
        <h3 style="font-size: 18px; font-weight: 700; color: #18181b; margin-bottom: 12px; border-bottom: 1px solid #f4f4f5; padding-bottom: 10px;">
            📝 Mô Tả Chi Tiết Từ Người Bán
        </h3>
        <div id="detailDescription" style="font-size: 14px; color: #3f3f46; line-height: 1.8; white-space: pre-line;">
            @(p != null && !string.IsNullOrEmpty(p.Description) ? p.Description : "Không có mô tả thêm.")
        </div>
    </div>
</div>

@section Scripts {
<script>
    const currentId = @productId;

    function handleDetailChat(prodId, isLoggedIn) {
        if (!isLoggedIn) {
            alert("Vui lòng đăng nhập tài khoản để chat và thương lượng với người bán!");
            const chatUrl = '/Cart?productId=' + prodId;
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(chatUrl);
            return;
        }
        if (typeof openChatWithSeller === 'function') {
            openChatWithSeller(prodId);
        } else {
            window.location.href = '/Cart?productId=' + prodId;
        }
    }

    function initProductDetail() {
        const favBtn = document.getElementById('btnFavDetail');
        if (favBtn) {
            favBtn.onclick = (e) => {
                toggleProductFavorite(currentId, e);
            };
        }
    }

    if (document.readyState === 'loading') {
        document.addEventListener('DOMContentLoaded', initProductDetail);
    } else {
        initProductDetail();
    }
    if (window.jQuery) {
        $(document).ready(initProductDetail);
    }
</script>
}
```

---

## Views/Products/PostItem.cshtml

```html
@model ThriftShop.Models.PendingPost
@{
    ViewData["Title"] = "Đăng Bán Đồ Cũ (Chờ Admin Duyệt) - ThriftShop";
}

<div class="main-wrapper" style="margin-top: 30px;">
    <!-- KHUNG ĐĂNG TIN TẬP TRUNG (KHÔNG CHIA 2 BÊN DÀY) -->
    <div class="focused-card">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 16px; border-bottom: 1px solid #e4e4e7; padding-bottom: 14px; flex-wrap: wrap; gap: 10px;">
            <div>
                <h2>📸 Đăng Bán Đồ Cũ Của Bạn</h2>
                <div style="font-size: 13px; color: #71717a; margin-top: 2px;">
                    Cốt lõi Đề tài 30: Tin đăng sau khi gửi sẽ chuyển sang trạng thái <strong>Chờ Admin kiểm duyệt</strong> trước khi mở bán chính thức.
                </div>
            </div>
            <a asp-controller="Products" asp-action="MyListings" class="btn-detail" style="padding: 7px 15px; border-radius: 8px; font-size: 13px; text-decoration: none;">
                <i class="fas fa-list mr-1"></i> Quản lý tin của tôi
            </a>
        </div>

        @if (!ViewData.ModelState.IsValid)
        {
            <div class="alert alert-danger" style="border-radius: 10px; margin-bottom: 18px;">
                <h6 class="font-weight-bold mb-1"><i class="fas fa-exclamation-triangle mr-1"></i> Vui lòng kiểm tra lại thông tin:</h6>
                <ul class="mb-0 pl-3">
                    @foreach (var error in ViewData.ModelState.Values.SelectMany(v => v.Errors))
                    {
                        <li>@error.ErrorMessage</li>
                    }
                </ul>
            </div>
        }

        <!-- HƯỚNG DẪN QUY TRÌNH DUYỆT -->
        <div style="background: #fffbeb; border: 1px solid #fef08a; border-radius: 10px; padding: 14px 18px; margin-bottom: 22px; font-size: 13px; color: #854d0e; line-height: 1.6;">
            <i class="fas fa-shield-alt mr-1"></i> <strong>Quy trình kiểm định chất lượng:</strong> 
            Vui lòng chụp ảnh thật rõ nét và mô tả trung thực khuyết điểm (nếu có). Ban Quản Trị sẽ xem xét và phê duyệt trong vòng vài phút.
        </div>

        <!-- FORM ASP.NET MVC CHUẨN: POST lên ProductsController -> Action PostItem -->
        <form asp-controller="Products" asp-action="PostItem" method="post" id="postItemForm">
            @Html.AntiForgeryToken()

            <!-- 1. TÊN MÓN ĐỒ & DANH MỤC -->
            <div class="form-group">
                <label asp-for="Name">Tên món đồ cũ cần thanh lý (*)</label>
                <input asp-for="Name" type="text" id="postTitle" placeholder="VD: Áo khoác Jean Levis Vintage lót lông, Tai nghe Sony WH-1000XM4..." required>
                <span asp-validation-for="Name" class="text-danger small"></span>
            </div>

            <div class="form-group">
                <label asp-for="CategoryId">Danh mục sản phẩm (*)</label>
                <select asp-for="CategoryId" id="postCategory" required>
                    <option value="1">Đồ điện tử</option>
                    <option value="2">Đồ gia dụng</option>
                    <option value="3">Văn phòng phẩm</option>
                    <option value="4">Thời trang</option>
                    <option value="5">Khác</option>
                </select>
            </div>

            <!-- 2. GIÁ TIỀN & KHU VỰC -->
            <div class="form-row-2">
                <div class="form-group">
                    <label asp-for="Price">Giá thanh lý mong muốn (VNĐ) (*)</label>
                    <input asp-for="Price" type="number" id="postPrice" placeholder="VD: 450000" required>
                    <span asp-validation-for="Price" class="text-danger small"></span>
                </div>
                <div class="form-group">
                    <label asp-for="OriginalPrice">Giá mua ban đầu lúc mới (VNĐ)</label>
                    <input asp-for="OriginalPrice" type="number" id="postOriginalPrice" placeholder="VD: 1500000">
                </div>
            </div>

            <div class="form-row-2">
                <div class="form-group">
                    <label asp-for="SellerAddress">Khu vực giao dịch / Địa chỉ xem đồ (*)</label>
                    <input asp-for="SellerAddress" type="text" id="postLocation" placeholder="VD: Quận 1, TP. Hồ Chí Minh" required>
                    <span asp-validation-for="SellerAddress" class="text-danger small"></span>
                </div>
                <div class="form-group">
                    <label asp-for="SellerPhone">Số điện thoại / Zalo liên hệ (*)</label>
                    <input asp-for="SellerPhone" type="tel" id="postPhone" placeholder="0901234567" required>
                    <span asp-validation-for="SellerPhone" class="text-danger small"></span>
                </div>
            </div>

            <!-- 3. HÌNH ẢNH & VIDEO THỰC TẾ (BẮT BUỘC TỐI THIỂU 3 ẢNH) -->
            <div class="form-group" style="background: #f8fafc; border: 1.5px dashed #cbd5e1; border-radius: 12px; padding: 20px; margin-bottom: 22px;">
                <div style="display: flex; justify-content: space-between; align-items: baseline; flex-wrap: wrap; gap: 8px; margin-bottom: 6px;">
                    <label style="font-weight: 700; color: #18181b; margin-bottom: 0; font-size: 14.5px;">
                        📸 Hình ảnh & Video thực tế của món đồ <span style="color: #ef4444;">(*)</span>
                    </label>
                    <span id="mediaRequirementBadge" style="font-size: 12.5px; font-weight: 700; padding: 4px 12px; border-radius: 20px; background: #fee2e2; color: #ef4444;">
                        ⚠️ Tối thiểu 3 ảnh (Hiện có: 0 ảnh)
                    </span>
                </div>
                <div style="font-size: 13px; color: #64748b; margin-bottom: 14px; line-height: 1.5;">
                    Để đảm bảo giao dịch đồ cũ an toàn và trung thực, hệ thống <strong>bắt buộc đăng tối thiểu ít nhất 3 ảnh chụp thực tế</strong> (các góc, tem mác hoặc khuyết điểm). Bạn có thể tải nhiều ảnh và thêm video.
                </div>

                <!-- Container chứa các hidden input để POST lên C# Controller -->
                <div id="hiddenMediaInputs"></div>
                <input type="hidden" asp-for="Image" id="mainImageUrl" />

                <!-- Các nút hành động tải lên -->
                <div style="display: flex; gap: 10px; flex-wrap: wrap; margin-bottom: 16px;">
                    <button type="button" class="btn btn-outline-primary" onclick="document.getElementById('fileMediaInput').click()" style="border-radius: 8px; font-weight: 600; font-size: 13.5px; padding: 8px 16px; border-color: #ff3c00; color: #ff3c00; background: white;">
                        <i class="fas fa-cloud-upload-alt mr-1"></i> Tải ảnh / video từ máy
                    </button>
                    <input type="file" id="fileMediaInput" multiple accept="image/*,video/*" style="display: none;" onchange="handleMediaFilesSelected(event)">

                    <button type="button" class="btn btn-outline-secondary" onclick="toggleUrlInputBox()" style="border-radius: 8px; font-weight: 600; font-size: 13.5px; padding: 8px 16px;">
                        <i class="fas fa-link mr-1"></i> Thêm bằng link URL
                    </button>

                    <button type="button" class="btn btn-outline-info" onclick="addSampleMedia()" style="border-radius: 8px; font-weight: 600; font-size: 13.5px; padding: 8px 16px;">
                        <i class="fas fa-magic mr-1"></i> Chọn 3 ảnh mẫu nhanh
                    </button>
                </div>

                <!-- Khung nhập URL -->
                <div id="urlInputContainer" style="display: none; background: #ffffff; border: 1px solid #e2e8f0; border-radius: 8px; padding: 12px; margin-bottom: 16px;">
                    <div style="display: flex; gap: 8px; flex-wrap: wrap;">
                        <input type="url" id="customUrlInput" placeholder="Dán link ảnh (.jpg, .png, Unsplash...) hoặc video..." style="flex: 1; min-width: 200px; padding: 8px 12px; font-size: 13.5px; border: 1px solid #d4d4d8; border-radius: 6px;">
                        <button type="button" onclick="addMediaFromUrl()" style="background: #ff3c00; border: none; color: white; padding: 8px 16px; border-radius: 6px; font-weight: 600; font-size: 13.5px; cursor: pointer;">
                            + Thêm
                        </button>
                    </div>
                </div>

                <!-- LƯỚI PREVIEW MEDIA ĐÃ THÊM -->
                <div id="mediaPreviewGrid" style="display: grid; grid-template-columns: repeat(auto-fill, minmax(130px, 1fr)); gap: 12px;">
                    <!-- JS render preview -->
                </div>
            </div>

            <!-- 4. MÔ TẢ CHI TIẾT & KHUYẾT ĐIỂM -->
            <div class="form-group">
                <label asp-for="Description">Mô tả khuyết điểm & phụ kiện đi kèm (nếu có)</label>
                <textarea asp-for="Description" id="postDescription" rows="3" placeholder="VD: Khuy áo nguyên bản, hơi sờn nhẹ gấu tay, đã giặt thơm tho sạch sẽ..."></textarea>
            </div>

            <button type="submit" class="btn-primary-block" style="border-radius: 8px; margin-top: 10px;">
                🚀 Gửi Tin Đăng Bán (Chuyển Sang Chờ Admin Duyệt)
            </button>
        </form>
    </div>
</div>

@section Scripts {
<script>
    // Danh sách media đã thêm (gồm ảnh và video)
    let uploadedMediaList = [];

    function addSampleMedia() {
        const samples = [
            { id: 's1', type: 'image', url: 'https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80', name: 'Ảnh chính góc thẳng' },
            { id: 's2', type: 'image', url: 'https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80', name: 'Ảnh chi tiết mặt lưng' },
            { id: 's3', type: 'image', url: 'https://images.unsplash.com/photo-1520639888713-7851133b1ed0?w=600&q=80', name: 'Ảnh chụp cận tem mác' }
        ];
        samples.forEach(s => {
            if (!uploadedMediaList.some(m => m.url === s.url)) {
                uploadedMediaList.push(s);
            }
        });
        renderMediaPreview();
    }

    function toggleUrlInputBox() {
        const box = document.getElementById('urlInputContainer');
        if (box) {
            box.style.display = box.style.display === 'none' ? 'block' : 'none';
        }
    }

    function addMediaFromUrl() {
        const input = document.getElementById('customUrlInput');
        const url = input.value.trim();
        if (!url) return;

        const isVideo = url.endsWith('.mp4') || url.endsWith('.webm') || url.includes('video') || url.includes('youtube');
        uploadedMediaList.push({
            id: 'url_' + Date.now() + Math.random(),
            type: isVideo ? 'video' : 'image',
            url: url,
            name: isVideo ? 'Video liên kết' : 'Ảnh liên kết'
        });

        input.value = '';
        renderMediaPreview();
    }

    function handleMediaFilesSelected(event) {
        const files = Array.from(event.target.files);
        if (!files.length) return;

        files.forEach(file => {
            const isVideo = file.type.startsWith('video');
            const reader = new FileReader();
            reader.onload = function(e) {
                uploadedMediaList.push({
                    id: 'f_' + Date.now() + Math.random(),
                    type: isVideo ? 'video' : 'image',
                    url: e.target.result,
                    name: file.name
                });
                renderMediaPreview();
            };
            reader.readAsDataURL(file);
        });

        event.target.value = '';
    }

    function removeMediaItem(id) {
        uploadedMediaList = uploadedMediaList.filter(m => m.id !== id);
        renderMediaPreview();
    }

    function renderMediaPreview() {
        const grid = document.getElementById('mediaPreviewGrid');
        const badge = document.getElementById('mediaRequirementBadge');
        const hiddenInputs = document.getElementById('hiddenMediaInputs');
        const mainInput = document.getElementById('mainImageUrl');
        if (!grid || !badge) return;

        const imageCount = uploadedMediaList.filter(m => m.type === 'image').length;
        const videoCount = uploadedMediaList.filter(m => m.type === 'video').length;

        // Cập nhật badge yêu cầu
        if (imageCount >= 3) {
            badge.style.background = '#dcfce7';
            badge.style.color = '#15803d';
            badge.innerHTML = `✅ Đã đạt yêu cầu (${imageCount} ảnh${videoCount ? ', ' + videoCount + ' video' : ''})`;
        } else {
            badge.style.background = '#fee2e2';
            badge.style.color = '#ef4444';
            badge.innerHTML = `⚠️ Cần thêm ít nhất ${3 - imageCount} ảnh (Hiện có: ${imageCount}/3)`;
        }

        // Tạo hidden inputs để POST lên Controller
        if (hiddenInputs) {
            hiddenInputs.innerHTML = '';
            uploadedMediaList.forEach(item => {
                const input = document.createElement('input');
                input.type = 'hidden';
                input.name = item.type === 'image' ? 'images' : 'videos';
                input.value = item.url;
                hiddenInputs.appendChild(input);
            });
            const firstImg = uploadedMediaList.find(m => m.type === 'image');
            if (mainInput && firstImg) {
                mainInput.value = firstImg.url;
            }
        }

        if (uploadedMediaList.length === 0) {
            grid.innerHTML = '<div style="grid-column: 1/-1; color: #94a3b8; font-size: 13px; text-align: center; padding: 20px;">Chưa có ảnh/video nào được chọn. Nhấn nút bên trên để tải từ máy hoặc dán link.</div>';
            return;
        }

        grid.innerHTML = uploadedMediaList.map((item, idx) => {
            const isCover = idx === 0 && item.type === 'image';
            return `
                <div style="position: relative; border: 1.5px solid ${isCover ? '#ff3c00' : '#e2e8f0'}; border-radius: 10px; overflow: hidden; background: #000; height: 130px; display: flex; align-items: center; justify-content: center;">
                    ${item.type === 'image' 
                        ? `<img src="${item.url}" alt="${item.name}" style="width: 100%; height: 100%; object-fit: cover;">`
                        : `<video src="${item.url}" style="width: 100%; height: 100%; object-fit: cover;" muted></video><div style="position: absolute; color: white; font-size: 20px; background: rgba(0,0,0,0.5); border-radius: 50%; width: 34px; height: 34px; display: flex; align-items: center; justify-content: center;"><i class="fas fa-play"></i></div>`
                    }
                    <div style="position: absolute; top: 4px; left: 4px; background: ${item.type === 'video' ? '#3b82f6' : (isCover ? '#ff3c00' : 'rgba(0,0,0,0.6)')}; color: white; font-size: 10px; font-weight: 700; padding: 2px 6px; border-radius: 4px;">
                        ${item.type === 'video' ? 'VIDEO' : (isCover ? 'ẢNH BÌA' : `ẢNH #${idx+1}`)}
                    </div>
                    <button type="button" onclick="removeMediaItem('${item.id}')" title="Xoá ảnh này" style="position: absolute; top: 4px; right: 4px; width: 22px; height: 22px; border-radius: 50%; background: rgba(239, 68, 68, 0.9); color: white; border: none; font-size: 11px; cursor: pointer; display: flex; align-items: center; justify-content: center;">
                        <i class="fas fa-times"></i>
                    </button>
                </div>
            `;
        }).join('');
    }

    document.addEventListener('DOMContentLoaded', () => {
        addSampleMedia();
    });

    document.getElementById('postItemForm').addEventListener('submit', function (e) {
        // VALIDATION BẮT BUỘC TỐI THIỂU 3 ẢNH
        const imageCount = uploadedMediaList.filter(m => m.type === 'image').length;
        if (imageCount < 3) {
            e.preventDefault();
            alert(`❌ BẮT BUỘC TỐI THIỂU 3 ẢNH:\nHiện tại bạn chỉ mới thêm ${imageCount} ảnh. Vui lòng tải lên thêm ít nhất ${3 - imageCount} ảnh thực tế của món đồ trước khi gửi tin!`);
            const grid = document.getElementById('mediaPreviewGrid');
            if (grid) grid.scrollIntoView({ behavior: 'smooth', block: 'center' });
            return false;
        }

        // Đồng bộ sang LocalStorage
        const catSelect = document.getElementById('postCategory');
        const catName = catSelect.options[catSelect.selectedIndex].text;
        const price = parseInt(document.getElementById('postPrice').value) || 0;
        const origPrice = parseInt(document.getElementById('postOriginalPrice').value) || (price * 2);
        const imageList = uploadedMediaList.filter(m => m.type === 'image').map(m => m.url);
        const videoList = uploadedMediaList.filter(m => m.type === 'video').map(m => m.url);
        const curUser = (typeof ThriftDB !== 'undefined' && ThriftDB.getCurrentUser) ? ThriftDB.getCurrentUser() : null;

        if (typeof ThriftDB !== 'undefined') {
            ThriftDB.addPendingPost({
                id: 200 + Date.now() % 1000,
                name: document.getElementById('postTitle').value.trim(),
                categoryId: parseInt(catSelect.value),
                categoryName: catName,
                price: price,
                originalPrice: origPrice,
                sellerId: curUser ? curUser.id : 2,
                sellerName: curUser ? curUser.name : "Thành viên",
                sellerEmail: curUser ? curUser.email : "user@thriftshop.vn",
                sellerPhone: document.getElementById('postPhone').value.trim(),
                sellerAddress: document.getElementById('postLocation').value.trim(),
                sellerAvatar: curUser ? curUser.avatar : "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                stock: 1,
                image: imageList[0],
                images: imageList,
                videos: videoList,
                description: document.getElementById('postDescription').value.trim(),
                status: "pending",
                createdAt: new Date().toISOString().replace('T', ' ').substring(0, 16)
            });
        }
    });
</script>
}
```

---

## Views/Products/MyListings.cshtml

```html
@model IEnumerable<ThriftShop.Models.PendingPost>
@{
    ViewData["Title"] = "Quản Lý Tin Đăng Bán Của Tôi - ThriftShop";
    var myPostsJson = System.Text.Json.JsonSerializer.Serialize(Model ?? Enumerable.Empty<ThriftShop.Models.PendingPost>());
}

<div class="main-wrapper" style="margin-top: 30px; margin-bottom: 60px;">
    <div style="max-width: 1440px; width: 95%; margin: 0 auto;">
        <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 20px; flex-wrap: wrap; gap: 12px;">
            <div>
                <h1 style="font-size: 22px; font-weight: 700; color: #18181b;">
                    📑 Tin Đăng Bán Đồ Cũ Của Tôi
                </h1>
                <p style="font-size: 13.5px; color: #71717a; margin-top: 2px;">
                    Theo dõi trạng thái duyệt Admin, chỉnh sửa thông tin, đánh dấu đã bán hoặc xóa bài đăng.
                </p>
            </div>
            <div style="display: flex; gap: 10px;">
                <a asp-controller="Products" asp-action="PostItem" class="choice-btn active" style="padding: 9px 18px; text-decoration: none;">
                    + Đăng Thêm Món Đồ Mới
                </a>
            </div>
        </div>

        <!-- THỐNG KÊ TRẠNG THÁI KIỂM DUYỆT -->
        <div style="display: grid; grid-template-columns: repeat(auto-fit, minmax(180px, 1fr)); gap: 14px; margin-bottom: 24px;">
            <div style="background: #fffbeb; border: 1px solid #fde68a; padding: 14px; border-radius: 10px; text-align: center;">
                <div style="font-size: 12.5px; color: #b45309; font-weight: 600;">⏳ CHỜ ADMIN DUYỆT</div>
                <div id="countPending" style="font-size: 22px; font-weight: 800; color: #d97706; margin-top: 2px;">0</div>
            </div>
            <div style="background: #f0fdf4; border: 1px solid #bbf7d0; padding: 14px; border-radius: 10px; text-align: center;">
                <div style="font-size: 12.5px; color: #15803d; font-weight: 600;">✅ ĐÃ DUYỆT (MỞ BÁN)</div>
                <div id="countApproved" style="font-size: 22px; font-weight: 800; color: #16a34a; margin-top: 2px;">0</div>
            </div>
            <div style="background: #f1f5f9; border: 1px solid #cbd5e1; padding: 14px; border-radius: 10px; text-align: center;">
                <div style="font-size: 12.5px; color: #475569; font-weight: 600;">🤝 ĐÃ BÁN (ẨN KHỎI SÀN)</div>
                <div id="countSold" style="font-size: 22px; font-weight: 800; color: #334155; margin-top: 2px;">0</div>
            </div>
            <div style="background: #fef2f2; border: 1px solid #fecaca; padding: 14px; border-radius: 10px; text-align: center;">
                <div style="font-size: 12.5px; color: #b91c1c; font-weight: 600;">❌ BỊ TỪ CHỐI</div>
                <div id="countRejected" style="font-size: 22px; font-weight: 800; color: #dc2626; margin-top: 2px;">0</div>
            </div>
        </div>

        <!-- BẢNG DANH SÁCH BÀI ĐĂNG CỦA TÔI -->
        <div style="background: white; border-radius: 12px; border: 1px solid #e4e4e7; overflow: hidden; box-shadow: 0 1px 3px rgba(0,0,0,0.05);">
            <table class="cart-table">
                <thead>
                    <tr>
                        <th style="width: 38%;">Món Đồ Cũ</th>
                        <th>Giá Thanh Lý</th>
                        <th>Danh Mục</th>
                        <th>Trạng Thái</th>
                        <th style="text-align: center; width: 260px;">Thao Tác</th>
                    </tr>
                </thead>
                <tbody id="listingsBody">
                    <!-- Render bằng JS từ ThriftDB -->
                </tbody>
            </table>
        </div>
    </div>
</div>

<!-- MODAL CHỈNH SỬA BÀI ĐĂNG (BẮT BUỘC CHUYỂN VỀ PENDING CHO ADMIN DUYỆT LẠI) -->
<div id="editPostModal" style="display: none; position: fixed; top: 0; left: 0; width: 100%; height: 100%; background: rgba(0,0,0,0.5); z-index: 1050; align-items: center; justify-content: center;">
    <div style="background: white; width: 90%; max-width: 600px; border-radius: 14px; overflow: hidden; box-shadow: 0 10px 30px rgba(0,0,0,0.2);">
        <div style="padding: 18px 24px; border-bottom: 1px solid #e4e4e7; display: flex; justify-content: space-between; align-items: center; background: #fafafa;">
            <h3 style="font-size: 16px; font-weight: 700; color: #18181b; margin: 0;">
                ✏️ Chỉnh Sửa Bài Đăng Bán Đồ Cũ
            </h3>
            <button onclick="closeEditModal()" style="background: none; border: none; font-size: 18px; color: #71717a; cursor: pointer;">&times;</button>
        </div>
        
        <form id="editPostForm" style="padding: 24px;">
            <input type="hidden" id="editPostId">

            <div style="background: #fffbeb; border: 1px solid #fde68a; border-radius: 8px; padding: 12px; margin-bottom: 18px; font-size: 12.5px; color: #92400e; line-height: 1.5;">
                <i class="fas fa-exclamation-triangle mr-1"></i>
                <strong>Lưu ý:</strong> Khi bạn lưu nội dung chỉnh sửa, bài đăng sẽ <strong>tự động chuyển về trạng thái Chờ Admin duyệt lại</strong> trước khi tiếp tục hiển thị trên sàn.
            </div>

            <div class="form-group" style="margin-bottom: 14px;">
                <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 6px;">Tiêu đề món đồ (*)</label>
                <input type="text" id="editPostTitle" required style="width: 100%; padding: 10px 12px; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 13.5px; outline: none;">
            </div>

            <div style="display: grid; grid-template-columns: 1fr 1fr; gap: 14px; margin-bottom: 14px;">
                <div>
                    <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 6px;">Giá bán (VNĐ) (*)</label>
                    <input type="number" id="editPostPrice" required min="1000" style="width: 100%; padding: 10px 12px; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 13.5px; outline: none;">
                </div>
                <div>
                    <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 6px;">Danh mục (*)</label>
                    <select id="editPostCategory" style="width: 100%; padding: 10px 12px; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 13.5px; outline: none; background: white;">
                        <option value="1">Đồ điện tử</option>
                        <option value="2">Đồ gia dụng</option>
                        <option value="3">Văn phòng phẩm</option>
                        <option value="4">Thời trang</option>
                        <option value="5">Khác</option>
                    </select>
                </div>
            </div>

            <div class="form-group" style="margin-bottom: 14px;">
                <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 6px;">Link ảnh món đồ (*)</label>
                <input type="url" id="editPostImage" required style="width: 100%; padding: 10px 12px; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 13.5px; outline: none;">
            </div>

            <div class="form-group" style="margin-bottom: 20px;">
                <label style="display: block; font-size: 13px; font-weight: 600; margin-bottom: 6px;">Mô tả tình trạng chi tiết (*)</label>
                <textarea id="editPostDescription" rows="3" required style="width: 100%; padding: 10px 12px; border: 1px solid #cbd5e1; border-radius: 8px; font-size: 13.5px; outline: none; resize: vertical;"></textarea>
            </div>

            <div style="display: flex; justify-content: flex-end; gap: 10px;">
                <button type="button" onclick="closeEditModal()" style="padding: 10px 18px; border: 1px solid #cbd5e1; border-radius: 8px; background: white; color: #52525b; font-size: 13.5px; cursor: pointer;">
                    Hủy bỏ
                </button>
                <button type="submit" class="choice-btn active" style="padding: 10px 22px; font-size: 13.5px; border-radius: 8px;">
                    Lưu & Gửi Admin Duyệt Lại
                </button>
            </div>
        </form>
    </div>
</div>

@section Scripts {
<script>
    if (typeof ThriftDB !== 'undefined' && !ThriftDB.isLoggedIn()) {
        alert("Vui lòng đăng nhập tài khoản để quản lý tin đăng của bạn!");
        window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent('/Products/MyListings');
    }

    const serverMyPosts = @Html.Raw(myPostsJson);

    function renderMyListings() {
        const body = document.getElementById('listingsBody');
        let posts = serverMyPosts && serverMyPosts.length > 0
            ? serverMyPosts.map(p => ({
                id: p.Id || p.id,
                name: p.Name || p.name,
                categoryId: p.CategoryId || p.categoryId,
                categoryName: p.CategoryName || p.categoryName || 'Đồ cũ',
                price: p.Price || p.price,
                image: p.Image || p.image,
                description: p.Description || p.description,
                status: (p.Status || p.status || 'pending').toLowerCase(),
                isSold: p.IsSold || p.isSold || false,
                rejectReason: p.RejectReason || p.rejectReason,
                createdAt: p.CreatedAt || p.createdAt
            }))
            : ((typeof ThriftDB !== 'undefined') ? ThriftDB.getPendingPosts() : []);

        let pendingCount = 0;
        let approvedCount = 0;
        let soldCount = 0;
        let rejectedCount = 0;

        posts.forEach(p => {
            if (p.isSold || p.status === 'sold') soldCount++;
            else if (p.status === 'pending') pendingCount++;
            else if (p.status === 'approved') approvedCount++;
            else if (p.status === 'rejected') rejectedCount++;
        });

        document.getElementById('countPending').textContent = pendingCount;
        document.getElementById('countApproved').textContent = approvedCount;
        document.getElementById('countSold').textContent = soldCount;
        document.getElementById('countRejected').textContent = rejectedCount;

        if (posts.length === 0) {
            body.innerHTML = '<tr><td colspan="5" style="text-align: center; padding: 40px; color: #71717a;">Bạn chưa có tin đăng bán đồ cũ nào.</td></tr>';
            return;
        }

        body.innerHTML = posts.map(p => {
            let statusBadge = '';
            if (p.isSold || p.status === 'sold') {
                statusBadge = '<span style="background: #e2e8f0; color: #475569; padding: 4px 10px; border-radius: 20px; font-size: 12px; font-weight: 700;"><i class="fas fa-check-double mr-1"></i>ĐÃ BÁN</span><div style="font-size: 11px; color: #64748b; margin-top: 3px;">(Đã ẩn khỏi sàn)</div>';
            } else if (p.status === 'pending') {
                statusBadge = '<span style="background: #fef3c7; color: #b45309; padding: 4px 10px; border-radius: 20px; font-size: 12px; font-weight: 600;"><i class="fas fa-clock mr-1"></i>Chờ Admin duyệt</span>';
            } else if (p.status === 'approved') {
                statusBadge = '<span style="background: #dcfce7; color: #15803d; padding: 4px 10px; border-radius: 20px; font-size: 12px; font-weight: 600;"><i class="fas fa-check-circle mr-1"></i>Đã duyệt mở bán</span>';
            } else {
                statusBadge = `<span style="background: #fee2e2; color: #b91c1c; padding: 4px 10px; border-radius: 20px; font-size: 12px; font-weight: 600;"><i class="fas fa-times-circle mr-1"></i>Bị từ chối</span><div style="font-size: 11px; color: #dc2626; margin-top: 4px;">Lý do: ${p.rejectReason || 'Vi phạm tiêu chuẩn hình ảnh'}</div>`;
            }

            const isSold = p.isSold || p.status === 'sold';

            return `
                <tr>
                    <td>
                        <div style="display: flex; align-items: center; gap: 12px;">
                            <img src="${p.image}" class="cart-item-img" alt="${p.name}">
                            <div>
                                <div style="font-weight: 600; color: #18181b; font-size: 14px;">${p.name}</div>
                                <div style="font-size: 12px; color: #71717a;">Danh mục: ${p.categoryName || 'Đồ cũ'} · ${p.createdAt || 'Gần đây'}</div>
                            </div>
                        </div>
                    </td>
                    <td style="font-weight: 700; color: #ff3c00;">${ThriftDB.formatCurrency(p.price)}</td>
                    <td><span style="font-size: 13px; color: #3f3f46; font-weight: 500;">${p.categoryName || 'Đồ cũ'}</span></td>
                    <td>${statusBadge}</td>
                    <td style="text-align: center;">
                        <div style="display: flex; gap: 6px; justify-content: center; flex-wrap: wrap;">
                            <!-- NÚT CHỈNH SỬA (BẮT BUỘC ADMIN DUYỆT LẠI) -->
                            <button onclick="openEditModal(${p.id})" style="padding: 5px 10px; font-size: 12px; border: 1px solid #cbd5e1; border-radius: 6px; background: white; color: #2563eb; cursor: pointer; font-weight: 600;" title="Chỉnh sửa tin (Admin duyệt lại)">
                                <i class="fas fa-edit mr-1"></i>Sửa
                            </button>

                            <!-- NÚT ĐÃ BÁN (ẨN KHỎI MỌI NGƯỜI NHƯNG GIỮ LẠI TRONG TIN CỦA TÔI) -->
                            ${!isSold ? `
                                <button onclick="handleMarkAsSold(${p.id})" style="padding: 5px 10px; font-size: 12px; border: 1px solid #cbd5e1; border-radius: 6px; background: #f8fafc; color: #475569; cursor: pointer; font-weight: 600;" title="Đánh dấu đã bán để ẩn khỏi sàn">
                                    <i class="fas fa-check mr-1"></i>Đã bán
                                </button>
                            ` : ''}

                            <!-- NÚT XÓA BÀI HẲN (XÓA VĨNH VIỄN) -->
                            <button onclick="handleDeletePostPermanently(${p.id})" style="padding: 5px 10px; font-size: 12px; border: 1px solid #fca5a5; border-radius: 6px; background: #fef2f2; color: #dc2626; cursor: pointer; font-weight: 600;" title="Xóa vĩnh viễn khỏi hệ thống">
                                <i class="fas fa-trash-alt mr-1"></i>Xóa hẳn
                            </button>
                        </div>
                    </td>
                </tr>
            `;
        }).join('');
    }

    // 1. Mở modal chỉnh sửa
    function openEditModal(id) {
        const p = ThriftDB.getPendingPostById(id);
        if (!p) return;

        document.getElementById('editPostId').value = p.id;
        document.getElementById('editPostTitle').value = p.name;
        document.getElementById('editPostPrice').value = p.price;
        document.getElementById('editPostCategory').value = p.categoryId || 1;
        document.getElementById('editPostImage').value = p.image;
        document.getElementById('editPostDescription').value = p.description || '';

        const modal = document.getElementById('editPostModal');
        modal.style.display = 'flex';
    }

    function closeEditModal() {
        document.getElementById('editPostModal').style.display = 'none';
    }

    // 2. Submit form chỉnh sửa -> Bắt buộc Admin duyệt lại
    document.getElementById('editPostForm').addEventListener('submit', function(e) {
        e.preventDefault();
        const id = parseInt(document.getElementById('editPostId').value);
        const name = document.getElementById('editPostTitle').value.trim();
        const price = parseInt(document.getElementById('editPostPrice').value);
        const categoryId = parseInt(document.getElementById('editPostCategory').value);
        const catMap = { 1: 'Đồ điện tử', 2: 'Đồ gia dụng', 3: 'Văn phòng phẩm', 4: 'Thời trang', 5: 'Khác' };
        const categoryName = catMap[categoryId] || 'Đồ cũ';
        const image = document.getElementById('editPostImage').value.trim();
        const description = document.getElementById('editPostDescription').value.trim();

        ThriftDB.updatePost({
            id: id,
            name: name,
            price: price,
            categoryId: categoryId,
            categoryName: categoryName,
            image: image,
            description: description
        });

        closeEditModal();
        renderMyListings();
        showToast('Đã cập nhật bài đăng! Tin được chuyển sang Chờ Admin duyệt lại lần nữa.');
    });

    // 3. Xử lý đánh dấu Đã bán (Ẩn khỏi mọi người nhưng giữ trong mục Tin của tôi & bảo toàn đoạn chat)
    function handleMarkAsSold(id) {
        if (confirm('Đánh dấu món đồ này là "ĐÃ BÁN"? Món đồ sẽ được ẩn khỏi trang chủ và tìm kiếm, nhưng vẫn lưu trong mục Tin của bạn và bảo toàn toàn bộ lịch sử chat đối chiếu.')) {
            fetch('/Products/MarkSold?id=' + id, { method: 'POST' })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        if (typeof ThriftDB !== 'undefined') ThriftDB.markPostSold(id);
                        showToast('Đã đánh dấu ĐÃ BÁN! Món đồ đã được ẩn khỏi sàn.', 'success');
                        setTimeout(() => location.reload(), 400);
                    } else {
                        alert(res.message || 'Lỗi thao tác');
                    }
                })
                .catch(() => alert('Lỗi kết nối máy chủ!'));
        }
    }

    // 4. Xóa bài đăng (Soft Delete: Ẩn khỏi trang chủ & tìm kiếm nhưng bảo toàn lịch sử chat)
    function handleDeletePostPermanently(id) {
        if (confirm('Bạn có chắc chắn muốn XÓA TIN ĐĂNG này? Món đồ sẽ bị gỡ khỏi sàn nhưng toàn bộ lịch sử trao đổi qua chat với khách mua vẫn được lưu lại an toàn.')) {
            fetch('/Products/DeletePermanently?id=' + id, { method: 'POST' })
                .then(r => r.json())
                .then(res => {
                    if (res.success) {
                        if (typeof ThriftDB !== 'undefined') ThriftDB.deletePostPermanently(id);
                        showToast('Đã xóa tin đăng! Tin đã được gỡ khỏi sàn.', 'success');
                        setTimeout(() => location.reload(), 400);
                    } else {
                        alert(res.message || 'Lỗi thao tác');
                    }
                })
                .catch(() => alert('Lỗi kết nối máy chủ!'));
        }
    }

    document.addEventListener('DOMContentLoaded', () => {
        renderMyListings();
    });
</script>
}
```

---

## Views/Products/Favorites.cshtml

```html
@{
    ViewData["Title"] = "Tin Đã Yêu Thích - ThriftShop";
}

<!-- Route Guard: Chuyển hướng ngay nếu chưa đăng nhập -->
<script>
    (function() {
        try {
            var u = localStorage.getItem("thrift_current_user");
            if (!u || u === "null" || u === "undefined") {
                var returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
                window.location.replace('/Account/Login?returnUrl=' + returnUrl);
            }
        } catch(e) {}
    })();
</script>

<style>
    .fav-page-container {
        width: 100%;
        max-width: 1380px;
        margin: 30px auto 60px;
        padding: 0 clamp(14px, 2.5vw, 32px);
    }

    .fav-header-box {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-bottom: 25px;
        flex-wrap: wrap;
        gap: 12px;
        border-bottom: 1px solid #e4e4e7;
        padding-bottom: 18px;
    }

    .fav-title {
        font-size: clamp(22px, 2.5vw, 28px);
        font-weight: 800;
        color: #18181b;
        display: flex;
        align-items: center;
        gap: 10px;
        margin: 0;
    }

    .fav-title span.count-badge {
        font-size: 14px;
        font-weight: 700;
        background: #fee2e2;
        color: #ef4444;
        padding: 4px 12px;
        border-radius: 20px;
    }

    .fav-subtitle {
        font-size: 14px;
        color: #71717a;
        margin-top: 4px;
    }

    /* EMPTY STATE */
    .fav-empty-state {
        text-align: center;
        padding: 70px 20px;
        background: #ffffff;
        border-radius: 16px;
        border: 1px dashed #d4d4d8;
        max-width: 580px;
        margin: 40px auto;
    }

    .fav-empty-icon {
        font-size: 52px;
        color: #fca5a5;
        margin-bottom: 16px;
    }

    .fav-empty-title {
        font-size: 20px;
        font-weight: 700;
        color: #18181b;
        margin-bottom: 8px;
    }

    .fav-empty-desc {
        font-size: 14px;
        color: #71717a;
        line-height: 1.6;
        margin-bottom: 24px;
    }

    .btn-explore-thrift {
        display: inline-block;
        padding: 12px 28px;
        background: #ff3c00;
        color: white;
        border-radius: 10px;
        font-weight: 700;
        font-size: 14.5px;
        text-decoration: none;
        transition: background 0.2s;
        box-shadow: 0 4px 14px rgba(255, 60, 0, 0.3);
    }

    .btn-explore-thrift:hover {
        background: #e03500;
        color: white;
    }

    /* GRID SẢN PHẨM YÊU THÍCH */
    .fav-grid {
        display: grid;
        grid-template-columns: repeat(auto-fill, minmax(clamp(240px, 28vw, 320px), 1fr));
        gap: clamp(14px, 1.8vw, 24px);
    }
</style>

<div class="fav-page-container">
    <div class="fav-header-box">
        <div>
            <h1 class="fav-title">
                <i class="fas fa-heart text-danger"></i> Tin Đã Yêu Thích
                <span class="count-badge" id="favCountBadge">0 tin</span>
            </h1>
            <p class="fav-subtitle">Danh sách các món đồ cũ bạn đang quan tâm và lưu lại để theo dõi giá hoặc chat với người bán.</p>
        </div>
        <div>
            <a href="/Products" class="btn btn-outline-secondary" style="font-weight: 600; font-size: 13.5px; border-radius: 8px; padding: 8px 16px;">
                <i class="fas fa-search mr-1"></i> Khám phá thêm đồ cũ
            </a>
        </div>
    </div>

    <!-- KHUNG RENDER DANH SÁCH HOẶC EMPTY STATE -->
    <div id="favoritesListContainer">
        <!-- Render bằng JS -->
    </div>
</div>

@section Scripts {
<script>
    function loadFavoritePage() {
        if (typeof ThriftDB === 'undefined' || !ThriftDB.isLoggedIn()) {
            window.location.replace('/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname));
            return;
        }

        const container = document.getElementById('favoritesListContainer');
        const countBadge = document.getElementById('favCountBadge');
        if (!container) return;

        const favProds = ThriftDB.getFavoriteProducts();
        if (countBadge) {
            countBadge.textContent = `${favProds.length} tin`;
        }

        if (favProds.length === 0) {
            container.innerHTML = `
                <div class="fav-empty-state">
                    <div class="fav-empty-icon"><i class="far fa-heart"></i></div>
                    <h3 class="fav-empty-title">Bạn chưa có món đồ yêu thích nào</h3>
                    <p class="fav-empty-desc">Khi dạo xem các tin đăng bán đồ cũ trên ThriftShop, hãy bấm vào biểu tượng trái tim để lưu lại và xem lại bất cứ lúc nào nhé!</p>
                    <a href="/Products" class="btn-explore-thrift">Khám Phá Sàn Đồ Cũ Ngay</a>
                </div>
            `;
            return;
        }

        container.innerHTML = `
            <div class="fav-grid">
                ${favProds.map(p => {
                    return `
                        <div class="card-dark-item" id="fav-card-${p.id}">
                            <div style="position: relative;">
                                <a href="/Products/Details/${p.id}" class="card-dark-image" style="display: block;" title="${p.name}">
                                    <img src="${p.image}" alt="${p.name}" onerror="this.src='https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600'">
                                </a>
                                <button class="btn-card-fav active" onclick="removeFavItem(${p.id}, event)" title="Bỏ yêu thích" data-fav-id="${p.id}">
                                    <i class="fas fa-heart text-danger"></i>
                                </button>
                            </div>
                            <div class="card-dark-body">
                                <div class="card-dark-category">${(p.categoryName || 'ĐỒ CŨ').toUpperCase()}</div>
                                <h3 class="card-dark-title" title="${p.name}">
                                    <a href="/Products/Details/${p.id}" style="color: inherit; text-decoration: none;">${p.name}</a>
                                </h3>
                                <div class="card-dark-meta">
                                    <i class="fas fa-map-marker-alt mr-1"></i>${p.sellerAddress || 'TP.HCM'} · Người bán: <strong>${p.sellerName || 'Thành viên'}</strong>
                                </div>
                                <div class="card-dark-price-row">
                                    <span class="card-dark-price">${ThriftDB.formatCurrency(p.price)}</span>
                                    ${p.originalPrice && p.originalPrice > p.price ? `<span class="card-dark-old-price">${ThriftDB.formatCurrency(p.originalPrice)}</span>` : ''}
                                </div>
                                <div class="card-dark-actions">
                                    <a href="/Products/Details/${p.id}" class="btn-card-detail">Chi tiết</a>
                                    <button class="btn-card-add" onclick="openChatWithSeller(${p.id})" title="Nhắn tin với người bán">Chat</button>
                                </div>
                            </div>
                        </div>
                    `;
                }).join('')}
            </div>
        `;
    }

    function removeFavItem(productId, event) {
        if (event) {
            event.preventDefault();
            event.stopPropagation();
        }
        if (typeof ThriftDB !== 'undefined') {
            ThriftDB.toggleFavorite(productId);
            if (typeof showToast === 'function') {
                showToast("Đã bỏ món đồ khỏi danh sách yêu thích", "info");
            }
            loadFavoritePage();
        }
    }

    document.addEventListener('DOMContentLoaded', loadFavoritePage);
    window.addEventListener('favoritesChanged', loadFavoritePage);
</script>
}
```

---

## Views/Cart/Index.cshtml

```html
@using ThriftShop.Common
@{
    ViewData["Title"] = "Liên Hệ & Hộp Thư Chat - ThriftShop";
    var activeProduct = ViewBag.ActiveProduct as ThriftShop.Models.Product;
    var isSoldProd = activeProduct != null && (activeProduct.IsSold || (activeProduct.Status != null && activeProduct.Status.ToLower() == "sold"));
    var isDeletedProd = activeProduct != null && (activeProduct.Deleted || (activeProduct.Status != null && activeProduct.Status.ToLower() == "deleted"));
    var statusText = isSoldProd ? "[Đã bán]" :
                     isDeletedProd ? "[Tin đã đóng]" :
                     "(Tin đang mở bán · Còn hàng)";

    var activeProductJson = activeProduct != null ? System.Text.Json.JsonSerializer.Serialize(new {
        id = activeProduct.Id,
        name = activeProduct.Name,
        price = activeProduct.Price,
        image = activeProduct.Image,
        isSold = isSoldProd,
        isDeleted = isDeletedProd,
        status = activeProduct.Status ?? "approved",
        statusText = statusText,
        sellerId = activeProduct.SellerId,
        sellerName = !string.IsNullOrWhiteSpace(activeProduct.SellerName) ? activeProduct.SellerName : "Người bán ThriftShop",
        sellerAvatar = (!string.IsNullOrWhiteSpace(activeProduct.SellerAvatar) && !activeProduct.SellerAvatar.Contains("ui-avatars.com")) ? activeProduct.SellerAvatar : "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
        sellerPhone = !string.IsNullOrWhiteSpace(activeProduct.SellerPhone) ? activeProduct.SellerPhone : "0901234567",
        sellerEmail = activeProduct.SellerEmail ?? ""
    }) : "null";

    var currentUserId = Context.Session.GetUserId();
    var currentUserName = Context.Session.GetUserName();
    var currentUserAvatar = Context.Session.GetUserAvatar();
}

<script>
    (function() {
        try {
            @if (currentUserId.HasValue)
            {
                <text>
                localStorage.setItem("thrift_current_user", JSON.stringify({
                    id: @currentUserId.Value,
                    name: "@Html.Raw(currentUserName ?? "Thành viên")",
                    avatar: "@Html.Raw(currentUserAvatar ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80")"
                }));
                </text>
            }
        } catch(e) {}
    })();
</script>

<style>
    .chat-page-container {
        width: min(96%, 1400px);
        margin: 20px auto 40px;
        background: #ffffff;
        border: 1px solid #e4e4e7;
        border-radius: 14px;
        box-shadow: 0 4px 20px rgba(0, 0, 0, 0.05);
        display: flex;
        height: min(84vh, 820px);
        min-height: 540px;
        overflow: hidden;
    }

    /* CỘT TRÁI: DANH SÁCH CUỘC TRÒ CHUYỆN (CHUẨN ẢNH 3) */
    .chat-sidebar {
        width: clamp(280px, 30vw, 380px);
        border-right: 1px solid #e4e4e7;
        display: flex;
        flex-direction: column;
        background: #ffffff;
        flex-shrink: 0;
    }

    .chat-sidebar-header {
        padding: 16px 20px 12px;
        border-bottom: 1px solid #f4f4f5;
    }

    .chat-sidebar-title {
        font-size: 20px;
        font-weight: 700;
        color: #18181b;
        margin-bottom: 12px;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .chat-search-wrap {
        position: relative;
        display: flex;
        align-items: center;
        gap: 8px;
    }

    .chat-search-input {
        flex: 1;
        padding: 9px 14px 9px 36px;
        border: 1px solid #e4e4e7;
        border-radius: 8px;
        font-size: 13.5px;
        outline: none;
        background: #fafafa;
        color: #18181b;
    }

    .chat-search-input:focus {
        background: #ffffff;
        border-color: #ff3c00;
    }

    .chat-search-icon {
        position: absolute;
        left: 12px;
        color: #a1a1aa;
        font-size: 14px;
        pointer-events: none;
    }

    .chat-tabs {
        display: flex;
        gap: 8px;
        padding: 12px 20px;
        border-bottom: 1px solid #f4f4f5;
        background: #fafafa;
    }

    .chat-tab-btn {
        padding: 5px 14px;
        border-radius: 20px;
        font-size: 13px;
        font-weight: 600;
        border: 1px solid #e4e4e7;
        background: #ffffff;
        color: #52525b;
        cursor: pointer;
        transition: all 0.2s;
    }

    .chat-tab-btn:hover {
        background: #f4f4f5;
    }

    .chat-tab-btn.active {
        background: #18181b;
        color: #ffffff;
        border-color: #18181b;
    }

    .chat-list {
        flex: 1;
        overflow-y: auto;
    }

    .chat-list-item {
        display: flex;
        align-items: center;
        padding: 14px 18px;
        gap: 12px;
        border-bottom: 1px solid #f4f4f5;
        cursor: pointer;
        transition: background 0.15s;
        position: relative;
    }

    .chat-list-item:hover {
        background: #f8fafc;
    }

    .chat-list-item.active {
        background: #f1f5f9;
    }

    .chat-avatar-wrapper {
        position: relative;
        width: 52px;
        height: 52px;
        flex-shrink: 0;
    }

    .chat-avatar-img {
        width: 48px;
        height: 48px;
        border-radius: 50%;
        object-fit: cover;
    }

    .chat-prod-badge-thumb {
        position: absolute;
        bottom: 0;
        right: 0;
        width: 22px;
        height: 22px;
        border-radius: 4px;
        border: 2px solid #ffffff;
        object-fit: cover;
    }

    .chat-item-content {
        flex: 1;
        min-width: 0;
    }

    .chat-item-name {
        font-size: 14px;
        font-weight: 700;
        color: #18181b;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        display: flex;
        justify-content: space-between;
        align-items: center;
    }

    .chat-item-time {
        font-size: 11.5px;
        font-weight: 400;
        color: #a1a1aa;
    }

    .chat-item-del-btn {
        background: transparent;
        border: none;
        color: #a1a1aa;
        cursor: pointer;
        padding: 3px 6px;
        border-radius: 4px;
        font-size: 12px;
        opacity: 0.5;
        transition: all 0.2s;
        margin-left: 4px;
    }

    .chat-list-item:hover .chat-item-del-btn {
        opacity: 1;
    }

    .chat-item-del-btn:hover {
        color: #ef4444;
        background: #fee2e2;
    }

    .chat-item-snippet {
        font-size: 12.5px;
        color: #71717a;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
        margin-top: 2px;
    }

    .chat-unread-dot {
        width: 8px;
        height: 8px;
        border-radius: 50%;
        background: #ff3c00;
        display: inline-block;
        margin-left: 6px;
    }

    /* CỘT PHẢI: KHUNG TRÒ CHUYỆN CHI TIẾT (CHUẨN ẢNH 3) */
    .chat-main-pane {
        flex: 1;
        display: flex;
        flex-direction: column;
        background: #ffffff;
    }

    /* HEADER CỦA NGƯỜI ĐANG CHAT */
    .chat-main-header {
        padding: 14px 24px;
        border-bottom: 1px solid #e4e4e7;
        display: flex;
        justify-content: space-between;
        align-items: center;
        background: #ffffff;
    }

    .chat-user-profile {
        display: flex;
        align-items: center;
        gap: 12px;
    }

    .chat-user-avatar {
        width: 44px;
        height: 44px;
        border-radius: 50%;
        object-fit: cover;
    }

    .chat-user-name {
        font-size: 15px;
        font-weight: 700;
        color: #18181b;
    }

    .chat-user-status {
        font-size: 12px;
        color: #16a34a;
        display: flex;
        align-items: center;
        gap: 5px;
    }

    .chat-header-actions {
        display: flex;
        gap: 14px;
        color: #71717a;
        font-size: 16px;
    }

    .chat-header-action-btn {
        background: none;
        border: none;
        color: #71717a;
        cursor: pointer;
        font-size: 16px;
        padding: 6px;
        border-radius: 6px;
    }

    .chat-header-action-btn:hover {
        background: #f4f4f5;
        color: #18181b;
    }

    /* THANH GHIM THÔNG TIN SẢN PHẨM TRÊN ĐẦU (CHUẨN ẢNH 3) */
    .chat-pinned-product {
        padding: 12px 24px;
        background: #f8fafc;
        border-bottom: 1px solid #e4e4e7;
        display: flex;
        align-items: center;
        gap: 14px;
    }

    .chat-pinned-thumb-wrap {
        position: relative;
        width: 52px;
        height: 52px;
        border-radius: 8px;
        overflow: hidden;
        border: 1px solid #e2e8f0;
        flex-shrink: 0;
        background: white;
    }

    .chat-pinned-thumb-wrap img {
        width: 100%;
        height: 100%;
        object-fit: cover;
    }

    .badge-tot {
        position: absolute;
        bottom: 2px;
        left: 2px;
        background: #16a34a;
        color: white;
        font-size: 9px;
        font-weight: 800;
        padding: 1px 4px;
        border-radius: 3px;
        line-height: 1.2;
    }

    .chat-pinned-info {
        flex: 1;
        min-width: 0;
    }

    .chat-pinned-title {
        font-size: 13.5px;
        font-weight: 700;
        color: #18181b;
        white-space: nowrap;
        overflow: hidden;
        text-overflow: ellipsis;
    }

    .chat-pinned-price-row {
        font-size: 13px;
        margin-top: 2px;
    }

    .chat-pinned-price {
        font-weight: 800;
        color: #ff3c00;
    }

    .chat-pinned-status {
        color: #71717a;
        font-size: 12px;
        margin-left: 6px;
    }

    .chat-pinned-action-btn {
        padding: 6px 14px;
        font-size: 12.5px;
        font-weight: 600;
        background: white;
        border: 1px solid #cbd5e1;
        border-radius: 6px;
        color: #334155;
        text-decoration: none;
        cursor: pointer;
        transition: 0.2s;
    }

    .chat-pinned-action-btn:hover {
        background: #f1f5f9;
        color: #0f172a;
    }

    /* KHUNG DANH SÁCH TIN NHẮN */
    .chat-messages-container {
        flex: 1;
        padding: 20px 24px;
        overflow-y: auto;
        display: flex;
        flex-direction: column;
        gap: 12px;
        background: #fcfcfc;
    }

    .msg-group {
        display: flex;
        flex-direction: column;
        max-width: 70%;
    }

    .msg-group.received {
        align-self: flex-start;
    }

    .msg-group.sent {
        align-self: flex-end;
    }

    .msg-bubble {
        padding: 10px 16px;
        border-radius: 14px;
        font-size: 13.5px;
        line-height: 1.5;
        white-space: pre-line;
        word-break: break-word;
    }

    .msg-group.received .msg-bubble {
        background: #f1f5f9;
        color: #18181b;
        border-bottom-left-radius: 4px;
    }

    .msg-group.sent .msg-bubble {
        background: #ff3c00;
        color: #ffffff;
        border-bottom-right-radius: 4px;
    }

    .msg-time {
        font-size: 11px;
        color: #a1a1aa;
        margin-top: 3px;
        padding: 0 4px;
    }

    .msg-group.sent .msg-time {
        text-align: right;
    }

    /* THANH GỢI Ý CÂU HỎI NHANH (CHUẨN ẢNH 3) */
    .chat-suggestion-chips {
        padding: 8px 20px;
        background: #ffffff;
        border-top: 1px solid #f4f4f5;
        display: flex;
        gap: 8px;
        overflow-x: auto;
        white-space: nowrap;
    }

    .chat-suggestion-chips::-webkit-scrollbar {
        height: 4px;
    }

    .chat-suggestion-chips::-webkit-scrollbar-thumb {
        background: #e2e8f0;
        border-radius: 4px;
    }

    .chat-chip {
        display: inline-flex;
        align-items: center;
        gap: 6px;
        padding: 6px 14px;
        background: #f8fafc;
        border: 1px solid #e2e8f0;
        border-radius: 20px;
        font-size: 12.5px;
        color: #334155;
        cursor: pointer;
        transition: all 0.15s;
    }

    .chat-chip:hover {
        background: #f1f5f9;
        border-color: #cbd5e1;
        color: #0f172a;
    }

    /* KHUNG NHẬP TIN NHẮN (CHUẨN ẢNH 3) */
    .chat-input-bar {
        padding: 12px 24px 16px;
        background: #ffffff;
        border-top: 1px solid #e4e4e7;
    }

    .chat-input-textarea {
        width: 100%;
        min-height: 44px;
        max-height: 100px;
        padding: 10px 14px;
        border: 1px solid #e4e4e7;
        border-radius: 8px;
        font-size: 14px;
        outline: none;
        resize: none;
        font-family: inherit;
        background: #fafafa;
    }

    .chat-input-textarea:focus {
        background: #ffffff;
        border-color: #ff3c00;
    }

    .chat-toolbar-bottom {
        display: flex;
        justify-content: space-between;
        align-items: center;
        margin-top: 10px;
    }

    .chat-tools-left {
        display: flex;
        gap: 12px;
    }

    .chat-tool-btn {
        background: none;
        border: 1px solid #e4e4e7;
        border-radius: 6px;
        padding: 6px 12px;
        font-size: 12.5px;
        color: #52525b;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 6px;
    }

    .chat-tool-btn:hover {
        background: #f4f4f5;
        color: #18181b;
    }

    .chat-send-btn {
        padding: 8px 20px;
        background: #ff3c00;
        color: white;
        border: none;
        border-radius: 8px;
        font-size: 13.5px;
        font-weight: 600;
        cursor: pointer;
        display: flex;
        align-items: center;
        gap: 6px;
        transition: background 0.15s;
    }

    .chat-send-btn:hover {
        background: #e03500;
    }
</style>

<div class="chat-page-container">
    <!-- CỘT TRÁI: DANH SÁCH NGƯỜI CHAT & SẢN PHẨM (CHUẨN ẢNH 3) -->
    <div class="chat-sidebar">
        <div class="chat-sidebar-header">
            <div class="chat-sidebar-title">
                <span>Chat</span>
                <i class="fas fa-ellipsis-v" style="font-size: 14px; color: #a1a1aa; cursor: pointer;"></i>
            </div>
            <div class="chat-search-wrap">
                <i class="fas fa-search chat-search-icon"></i>
                <input type="text" id="chatSearchInput" class="chat-search-input" placeholder="Nhập 3 ký tự để bắt đầu tìm kiếm">
            </div>
        </div>

        <!-- TABS: TẤT CẢ / CHƯA ĐỌC / TIN RÁC -->
        <div class="chat-tabs">
            <button class="chat-tab-btn active" onclick="switchChatTab('all', this)">Tất cả</button>
            <button class="chat-tab-btn" onclick="switchChatTab('unread', this)">Chưa đọc</button>
            <button class="chat-tab-btn" onclick="switchChatTab('spam', this)">Tin rác / Bỏ qua</button>
        </div>

        <!-- DANH SÁCH HỘI THOẠI -->
        <div class="chat-list" id="chatListContainer">
            <!-- Render bằng JS -->
        </div>
    </div>

    <!-- CỘT PHẢI: CHI TIẾT ĐOẠN CHAT & THÔNG TIN MÓN ĐỒ -->
    <div class="chat-main-pane" id="chatMainPane">
        <!-- TRẠNG THÁI TRỐNG KHI CHƯA CHỌN HOẶC CHƯA CÓ ĐOẠN CHAT -->
        <div id="chatEmptyPlaceholder" style="display: flex; flex-direction: column; align-items: center; justify-content: center; height: 100%; min-height: 520px; text-align: center; padding: 40px 20px; color: #71717a;">
            <div style="width: 72px; height: 72px; border-radius: 50%; background: #f4f4f5; display: flex; align-items: center; justify-content: center; font-size: 32px; margin-bottom: 16px;">💬</div>
            <h3 style="font-size: 20px; font-weight: 700; color: #18181b; margin-bottom: 8px;">Hộp Thư Liên Hệ</h3>
            <p style="font-size: 14px; color: #71717a; max-width: 420px; line-height: 1.6; margin: 0 0 20px;">Hiện bạn chưa có cuộc trò chuyện nào. Hãy tìm kiếm món đồ cũ bạn yêu thích và bấm "Chat" để liên hệ trực tiếp với người bán.</p>
            <a href="/" style="display: inline-block; padding: 11px 26px; background: #e53935; color: white; border-radius: 8px; text-decoration: none; font-weight: 600; font-size: 14px; box-shadow: 0 4px 12px rgba(229,57,53,0.25);">Khám Phá Món Đồ Cũ Ngay</a>
        </div>

        <!-- PHẦN CHAT HOẠT ĐỘNG KHI CÓ ĐOẠN CHAT ĐƯỢC CHỌN -->
        <div id="chatActiveSection" style="display: none; flex-direction: column; height: 100%; width: 100%;">
            <!-- HEADER NGƯỜI BÁN -->
            <div class="chat-main-header">
                <div class="chat-user-profile">
                    <img id="activeSellerAvatar" class="chat-user-avatar" src="" alt="Avatar">
                    <div>
                        <div id="activeSellerName" class="chat-user-name"></div>
                        <div class="chat-user-status">
                            <span style="font-size: 8px;">🟢</span> <span id="activeSellerStatus">Đang hoạt động</span>
                        </div>
                    </div>
                </div>
                <div class="chat-header-actions">
                    <button class="chat-header-action-btn" title="Gắn thẻ phân loại"><i class="fas fa-tag"></i></button>
                    <button class="chat-header-action-btn" title="Tùy chọn đoạn chat"><i class="fas fa-list-ul"></i></button>
                    <button class="chat-header-action-btn" title="Xóa đoạn chat này" onclick="deleteActiveChat()" style="color: #ef4444;" onmouseover="this.style.background='#fee2e2'" onmouseout="this.style.background='transparent'"><i class="fas fa-trash-alt"></i></button>
                </div>
            </div>

            <!-- THANH GHIM SẢN PHẨM ĐANG TRAO ĐỔI (CHUẨN ẢNH 3) -->
            <div class="chat-pinned-product" id="pinnedProductBar" style="display: none;">
                <div class="chat-pinned-thumb-wrap">
                    <img id="pinnedProdImg" src="" alt="Sản phẩm">
                    <span class="badge-tot">TỐT</span>
                </div>
                <div class="chat-pinned-info">
                    <div id="pinnedProdTitle" class="chat-pinned-title"></div>
                    <div class="chat-pinned-price-row">
                        <span id="pinnedProdPrice" class="chat-pinned-price"></span>
                        <span id="pinnedProdStatus" class="chat-pinned-status"></span>
                    </div>
                </div>
                <a id="pinnedProdLink" href="#" class="chat-pinned-action-btn">
                    Xem tin &rarr;
                </a>
            </div>

            <!-- KHUNG TIN NHẮN -->
            <div class="chat-messages-container" id="chatMessages">
                <!-- Render tin nhắn -->
            </div>

            <!-- THANH GỢI Ý CÂU HỎI NHANH (CHUẨN ẢNH 3) -->
            <div class="chat-suggestion-chips" id="suggestionChipsBar">
                <div class="chat-chip" id="chipPhone" onclick="sendSuggestion('SĐT của bạn là gì ạ?')"><i class="fas fa-phone-alt"></i> SĐT liên hệ</div>
                <div class="chat-chip" onclick="sendSuggestion('Món đồ này còn không bạn?')">Món đồ này còn không?</div>
                <div class="chat-chip" onclick="sendSuggestion('Bạn có ship COD toàn quốc không?')">Bạn có ship hàng không?</div>
                <div class="chat-chip" onclick="sendSuggestion('Sản phẩm còn bảo hành không?')">Sản phẩm còn bảo hành không?</div>
                <div class="chat-chip" onclick="sendSuggestion('Sản phẩm này đã qua sửa chữa chưa?')">Sản phẩm này đã qua sửa chữa chưa?</div>
                <div class="chat-chip" onclick="sendSuggestion('Có phụ kiện đi kèm theo sản phẩm không?')">Có phụ kiện đi kèm theo sản phẩm?</div>
            </div>

            <!-- THANH NHẬP LIỆU GỬI TIN -->
            <div class="chat-input-bar">
                <textarea id="chatInputText" class="chat-input-textarea" placeholder="Nhập tin nhắn..." rows="1" onkeydown="handleChatKeyDown(event)"></textarea>
                <div class="chat-toolbar-bottom">
                    <div class="chat-tools-left">
                        <button class="chat-tool-btn" onclick="attachMediaPrompt()"><i class="fas fa-image"></i> Hình và video</button>
                        <button class="chat-tool-btn" onclick="attachAddressPrompt()"><i class="fas fa-map-marker-alt"></i> Địa chỉ</button>
                    </div>
                    <button class="chat-send-btn" onclick="submitSendMessage()">
                        Gửi tin <i class="fas fa-paper-plane"></i>
                    </button>
                </div>
            </div>
        </div>
    </div>
</div>

@section Scripts {
<script>
    const CURRENT_USER_ID = @(currentUserId.HasValue ? currentUserId.Value.ToString() : "null");
    const CURRENT_USER_NAME = "@Html.Raw(currentUserName ?? "")";
    const CURRENT_USER_AVATAR = "@Html.Raw(currentUserAvatar ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80")";
    const SERVER_ACTIVE_PRODUCT = @Html.Raw(activeProductJson);

    let currentTab = 'all';
    let activeChatId = null;
    let chats = [];
    let pollInterval = null;
    let isFetchingMessages = false;

    function escapeHtml(str) {
        if (!str) return '';
        return String(str)
            .replace(/&/g, "&amp;")
            .replace(/</g, "&lt;")
            .replace(/>/g, "&gt;")
            .replace(/"/g, "&quot;")
            .replace(/'/g, "&#039;");
    }

    async function initChatPage() {
        // KIỂM TRA ĐĂNG NHẬP: Khi chưa đăng nhập thì ngay lập tức chuyển hướng sang trang đăng nhập
        if (CURRENT_USER_ID === null && (typeof window.checkUserLoggedIn === 'function' ? !window.checkUserLoggedIn() : !localStorage.getItem('thrift_current_user'))) {
            window.location.replace('/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search));
            return;
        }

        // Tải danh sách các cuộc trò chuyện từ SQL Server
        await refreshConversations();

        // Kiểm tra sản phẩm đang xem
        if (SERVER_ACTIVE_PRODUCT) {
            const prodIdStr = String(SERVER_ACTIVE_PRODUCT.id);
            let existing = chats.find(c => String(c.productId) === prodIdStr || (c.product && String(c.product.id) === prodIdStr));
            if (!existing) {
                const newChat = {
                    id: "chat-" + SERVER_ACTIVE_PRODUCT.id,
                    productId: SERVER_ACTIVE_PRODUCT.id,
                    product: SERVER_ACTIVE_PRODUCT,
                    partnerId: SERVER_ACTIVE_PRODUCT.sellerId,
                    partnerName: SERVER_ACTIVE_PRODUCT.sellerName,
                    partnerAvatar: SERVER_ACTIVE_PRODUCT.sellerAvatar,
                    partnerPhone: SERVER_ACTIVE_PRODUCT.sellerPhone,
                    sellerId: SERVER_ACTIVE_PRODUCT.sellerId,
                    sellerName: SERVER_ACTIVE_PRODUCT.sellerName,
                    sellerAvatar: SERVER_ACTIVE_PRODUCT.sellerAvatar,
                    sellerPhone: SERVER_ACTIVE_PRODUCT.sellerPhone,
                    lastMessage: "Bắt đầu cuộc trao đổi về món đồ này",
                    lastTime: "",
                    unread: false
                };
                chats.unshift(newChat);
                activeChatId = newChat.id;
            } else {
                activeChatId = existing.id;
            }
        } else {
            const urlParams = new URLSearchParams(window.location.search);
            const pIdParam = urlParams.get('productId') || urlParams.get('chat_prod');
            if (pIdParam) {
                const found = chats.find(c => String(c.productId) === String(pIdParam) || (c.product && String(c.product.id) === String(pIdParam)));
                if (found) {
                    activeChatId = found.id;
                }
            }
        }

        if (!activeChatId && chats.length > 0) {
            activeChatId = chats[0].id;
        }

        renderChatList();
        renderActiveChat();

        // Bắt đầu chu trình tự động thăm dò (polling mỗi 3 giây)
        startChatPolling();

        // Gắn listener tìm kiếm
        const searchInput = document.getElementById('chatSearchInput');
        if (searchInput) {
            searchInput.addEventListener('input', function(e) {
                renderChatList(e.target.value.trim().toLowerCase());
            });
        }
    }

    async function refreshConversations(silent = false) {
        try {
            const res = await fetch('/Cart/GetConversations');
            const data = await res.json();
            if (data.success && Array.isArray(data.conversations)) {
                // Hợp nhất dữ liệu các hội thoại từ SQL Server
                const serverChats = data.conversations;

                // Nếu có sản phẩm active truyền từ server mà chưa có tin nhắn, bảo toàn vào danh sách
                if (SERVER_ACTIVE_PRODUCT) {
                    const existsOnServer = serverChats.some(c => String(c.productId) === String(SERVER_ACTIVE_PRODUCT.id));
                    if (!existsOnServer) {
                        serverChats.unshift({
                            id: "chat-" + SERVER_ACTIVE_PRODUCT.id,
                            productId: SERVER_ACTIVE_PRODUCT.id,
                            product: SERVER_ACTIVE_PRODUCT,
                            partnerId: SERVER_ACTIVE_PRODUCT.sellerId,
                            partnerName: SERVER_ACTIVE_PRODUCT.sellerName,
                            partnerAvatar: SERVER_ACTIVE_PRODUCT.sellerAvatar,
                            partnerPhone: SERVER_ACTIVE_PRODUCT.sellerPhone,
                            sellerId: SERVER_ACTIVE_PRODUCT.sellerId,
                            sellerName: SERVER_ACTIVE_PRODUCT.sellerName,
                            sellerAvatar: SERVER_ACTIVE_PRODUCT.sellerAvatar,
                            sellerPhone: SERVER_ACTIVE_PRODUCT.sellerPhone,
                            lastMessage: "Bắt đầu cuộc trao đổi về món đồ này",
                            lastTime: "",
                            unread: false
                        });
                    }
                }

                chats = serverChats;
                if (!silent) {
                    renderChatList();
                }
            }
        } catch (e) {
            console.error("Lỗi khi tải danh sách hội thoại từ SQL Server:", e);
        }
    }

    function switchChatTab(tab, btn) {
        currentTab = tab;
        document.querySelectorAll('.chat-tab-btn').forEach(b => b.classList.remove('active'));
        btn.classList.add('active');
        renderChatList();
    }

    function getChatPartner(c) {
        const isUserTheSeller = (CURRENT_USER_ID !== null && c.sellerId && String(c.sellerId) === String(CURRENT_USER_ID));

        if (isUserTheSeller) {
            // Đối tác đang chat là người mua
            const partnerName = c.partnerName || c.buyerName || 'Người mua liên hệ';
            const partnerAvatar = c.partnerAvatar || c.buyerAvatar || 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80';
            return {
                name: partnerName,
                avatar: partnerAvatar,
                phone: c.partnerPhone || c.buyerPhone || '',
                partnerId: c.partnerId || c.buyerId
            };
        } else {
            // Đối tác đang chat là người bán (chủ món đồ)
            const partnerName = c.sellerName || 'Người bán';
            const partnerAvatar = c.sellerAvatar || 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80';
            return {
                name: partnerName,
                avatar: partnerAvatar,
                phone: c.sellerPhone || '',
                partnerId: c.sellerId
            };
        }
    }

    function renderChatList(keyword = '') {
        const container = document.getElementById('chatListContainer');
        if (!container) return;
        let filtered = chats;

        if (currentTab === 'unread') {
            filtered = filtered.filter(c => c.unread);
        } else if (currentTab === 'spam') {
            filtered = filtered.filter(c => c.tab === 'spam');
        }

        if (keyword && keyword.length >= 2) {
            filtered = filtered.filter(c => 
                (c.sellerName && c.sellerName.toLowerCase().includes(keyword)) || 
                (c.partnerName && c.partnerName.toLowerCase().includes(keyword)) ||
                (c.buyerName && c.buyerName.toLowerCase().includes(keyword)) ||
                (c.product && c.product.name && c.product.name.toLowerCase().includes(keyword)) ||
                (c.lastMessage && c.lastMessage.toLowerCase().includes(keyword))
            );
        }

        if (filtered.length === 0) {
            container.innerHTML = '<div style="text-align: center; padding: 40px 16px; color: #a1a1aa; font-size: 13px;">Chưa có đoạn chat nào.</div>';
            return;
        }

        container.innerHTML = filtered.map(c => {
            const isActive = c.id === activeChatId;
            const partner = getChatPartner(c);
            let itemAvatar = partner.avatar || 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80';
            const prodImg = (c.product && c.product.image) ? c.product.image : 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600';
            const prodName = (c.product && c.product.name) ? c.product.name : 'Sản phẩm';

            return `
                <div class="chat-list-item ${isActive ? 'active' : ''}" onclick="selectChat('${c.id}')">
                    <div class="chat-avatar-wrapper">
                        <img class="chat-avatar-img" src="${itemAvatar}" onerror="this.src='https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80'" alt="${escapeHtml(partner.name)}">
                        <img class="chat-prod-badge-thumb" src="${prodImg}" onerror="this.src='https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600'" alt="">
                    </div>
                    <div class="chat-item-content">
                        <div class="chat-item-name">
                            <span style="overflow: hidden; text-overflow: ellipsis; white-space: nowrap;">${escapeHtml(partner.name)}</span>
                            <div style="display: flex; align-items: center; gap: 4px; flex-shrink: 0;">
                                <span class="chat-item-time">${c.lastTime || ''}</span>
                                <button class="chat-item-del-btn" title="Xóa đoạn chat này" onclick="event.stopPropagation(); deleteChatPrompt('${c.id}');">
                                    <i class="fas fa-trash-alt"></i>
                                </button>
                            </div>
                        </div>
                        <div class="chat-item-snippet" style="white-space: nowrap; overflow: hidden; text-overflow: ellipsis;">
                            ${c.unread ? '<span class="chat-unread-dot"></span> ' : ''}
                            ${c.product ? `
                                ${(c.product.isSold || c.product.status === 'sold' || (c.product.statusText && c.product.statusText.includes('Đã bán'))) ? '<span style="background: #e2e8f0; color: #475569; padding: 1px 6px; border-radius: 4px; font-size: 10px; font-weight: 700; margin-right: 4px;">[Đã bán]</span>' : ''}
                                ${(c.product.isDeleted || c.product.status === 'deleted' || (c.product.statusText && c.product.statusText.includes('đóng'))) ? '<span style="background: #fee2e2; color: #dc2626; padding: 1px 6px; border-radius: 4px; font-size: 10px; font-weight: 700; margin-right: 4px;">[Đã đóng]</span>' : ''}
                                <span style="font-weight: 600; color: #ff3c00;">[${escapeHtml(prodName)}]</span> 
                            ` : ''}${escapeHtml(c.lastMessage || 'Chưa có tin nhắn')}
                        </div>
                    </div>
                </div>
            `;
        }).join('');
    }

    function selectChat(chatId) {
        activeChatId = chatId;
        renderChatList();
        renderActiveChat();
    }

    async function deleteChatPrompt(chatId) {
        if (!chatId) return;
        const target = chats.find(c => c.id === chatId);
        const partner = target ? getChatPartner(target) : null;
        const partnerName = partner ? partner.name : 'người này';
        const prodName = (target && target.product && target.product.name) ? ` về món "${target.product.name}"` : '';

        if (!confirm(`Bạn có chắc muốn xóa cuộc trò chuyện với ${partnerName}${prodName}?`)) {
            return;
        }

        const prodId = target ? (target.productId || (target.product ? target.product.id : null)) : null;
        if (prodId) {
            try {
                await fetch('/Cart/DeleteChat?productId=' + encodeURIComponent(prodId), { method: 'POST' });
            } catch(e) {}
        }

        chats = chats.filter(c => c.id !== chatId);
        if (activeChatId === chatId) {
            activeChatId = chats.length > 0 ? chats[0].id : null;
        }

        renderChatList();
        renderActiveChat();
    }

    function deleteActiveChat() {
        if (!activeChatId) return;
        deleteChatPrompt(activeChatId);
    }

    function renderActiveChat() {
        const chat = chats.find(c => c.id === activeChatId);
        const emptyPlaceholder = document.getElementById('chatEmptyPlaceholder');
        const activeSection = document.getElementById('chatActiveSection');

        if (!chat) {
            if (emptyPlaceholder) emptyPlaceholder.style.display = 'flex';
            if (activeSection) activeSection.style.display = 'none';
            return;
        }

        if (emptyPlaceholder) emptyPlaceholder.style.display = 'none';
        if (activeSection) activeSection.style.display = 'flex';

        const partner = getChatPartner(chat);

        // Render header thông tin đối tác chat
        const avatarEl = document.getElementById('activeSellerAvatar');
        if (avatarEl) {
            avatarEl.src = partner.avatar || 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80';
            avatarEl.onerror = function() { this.src = 'https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&q=80'; };
        }
        const nameEl = document.getElementById('activeSellerName');
        if (nameEl) nameEl.textContent = partner.name || 'Người dùng';
        const statusEl = document.getElementById('activeSellerStatus');
        if (statusEl) statusEl.textContent = 'Đang hoạt động';

        // Render thanh ghim sản phẩm đang trao đổi
        const pinnedBar = document.getElementById('pinnedProductBar');
        if (chat.product) {
            pinnedBar.style.display = 'flex';
            const prodImg = document.getElementById('pinnedProdImg');
            if (prodImg) {
                prodImg.src = chat.product.image || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600';
                prodImg.onerror = function() { this.src = 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600'; };
            }
            const titleEl = document.getElementById('pinnedProdTitle');
            if (titleEl) titleEl.textContent = chat.product.name || 'Món đồ cũ';
            const priceEl = document.getElementById('pinnedProdPrice');
            const priceVal = Number(chat.product.price) || 0;
            if (priceEl) priceEl.textContent = priceVal.toLocaleString('vi-VN') + ' ₫';
            const statusBadgeEl = document.getElementById('pinnedProdStatus');
            const isSold = chat.product.isSold || chat.product.status === 'sold' || (chat.product.statusText && chat.product.statusText.includes('Đã bán'));
            const isDeleted = chat.product.isDeleted || chat.product.status === 'deleted' || (chat.product.statusText && chat.product.statusText.includes('đóng'));
            
            if (statusBadgeEl) {
                if (isSold) {
                    statusBadgeEl.innerHTML = '<span style="background: #e2e8f0; color: #475569; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 700;"><i class="fas fa-handshake mr-1"></i>[Đã bán]</span> <span style="font-size: 12px; color: #64748b;">(Món đồ đã giao dịch xong)</span>';
                } else if (isDeleted) {
                    statusBadgeEl.innerHTML = '<span style="background: #fee2e2; color: #dc2626; padding: 2px 8px; border-radius: 12px; font-size: 11px; font-weight: 700;"><i class="fas fa-ban mr-1"></i>[Tin đã đóng]</span> <span style="font-size: 12px; color: #dc2626;">(Tin đăng đã được gỡ bỏ)</span>';
                } else {
                    statusBadgeEl.innerHTML = '<span style="color: #16a34a; font-size: 12px; font-weight: 600;"><i class="fas fa-check-circle mr-1"></i>(Tin đang mở bán · Còn hàng)</span>';
                }
            }
            const linkEl = document.getElementById('pinnedProdLink');
            if (linkEl) linkEl.href = `/Products/Details/${chat.product.id}`;
        } else {
            pinnedBar.style.display = 'none';
        }

        // Cập nhật chip SĐT nếu có
        const chipPhone = document.getElementById('chipPhone');
        if (chipPhone) {
            const phone = partner.phone || chat.sellerPhone;
            if (phone) {
                chipPhone.innerHTML = `<i class="fas fa-phone-alt"></i> SĐT: ${escapeHtml(phone)}`;
                chipPhone.onclick = () => sendSuggestion(`Chào bạn, mình xin số điện thoại liên hệ trực tiếp: ${phone}`);
            } else {
                chipPhone.innerHTML = `<i class="fas fa-phone-alt"></i> SĐT liên hệ`;
                chipPhone.onclick = () => sendSuggestion('SĐT của bạn là gì ạ?');
            }
        }

        // TẢI LỊCH SỬ TIN NHẮN TỪ SQL SERVER
        const prodId = chat.productId || (chat.product ? chat.product.id : null);
        if (prodId) {
            loadMessagesFromDb(prodId, false);
        }
    }

    // Nạp tin nhắn trực tiếp từ SQL Server Database
    async function loadMessagesFromDb(prodId, isSilent = false) {
        if (!prodId || isFetchingMessages) return;
        isFetchingMessages = true;

        try {
            const res = await fetch('/Cart/GetMessages?productId=' + encodeURIComponent(prodId));
            const data = await res.json();
            if (!data.success) {
                isFetchingMessages = false;
                return;
            }

            const messages = data.messages || [];
            const msgContainer = document.getElementById('chatMessages');
            if (!msgContainer) {
                isFetchingMessages = false;
                return;
            }

            // Kiểm tra số lượng và tin nhắn cuối cùng để tránh render lại gây giật khi polling
            const lastRenderedId = msgContainer.dataset.lastMsgId;
            const latestMsgId = messages.length > 0 ? String(messages[messages.length - 1].id) : "0";
            if (isSilent && messages.length === parseInt(msgContainer.dataset.msgCount || "0") && lastRenderedId === latestMsgId) {
                isFetchingMessages = false;
                return;
            }

            const isScrolledToBottom = (msgContainer.scrollHeight - msgContainer.clientHeight <= msgContainer.scrollTop + 60);

            const chat = chats.find(c => String(c.productId) === String(prodId) || (c.product && String(c.product.id) === String(prodId)));
            const partner = chat ? getChatPartner(chat) : { name: 'Người bán', avatar: 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80' };

            if (messages.length === 0) {
                msgContainer.innerHTML = `
                    <div style="text-align: center; padding: 40px 20px; color: #a1a1aa; font-size: 13.5px;">
                        <i class="fas fa-comments" style="font-size: 28px; margin-bottom: 8px; color: #cbd5e1; display: block;"></i>
                        Chưa có tin nhắn nào trong cuộc trò chuyện này.<br>Hãy nhập tin nhắn bên dưới để bắt đầu trao đổi!
                    </div>
                `;
            } else {
                msgContainer.innerHTML = messages.map(m => {
                    const isSent = (CURRENT_USER_ID !== null && m.senderId === CURRENT_USER_ID);
                    const bubbleAvatar = isSent 
                        ? (CURRENT_USER_AVATAR || m.senderAvatar || 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80')
                        : (m.senderAvatar || partner.avatar || 'https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80');

                    return `
                        <div class="msg-group ${isSent ? 'sent' : 'received'}" style="display: flex; gap: 8px; align-items: flex-end; margin-bottom: 12px; ${isSent ? 'justify-content: flex-end;' : 'justify-content: flex-start;'}">
                            ${!isSent ? `<img src="${bubbleAvatar}" alt="${escapeHtml(m.senderName)}" style="width: 30px; height: 30px; border-radius: 50%; object-fit: cover; flex-shrink: 0; margin-bottom: 4px; border: 1.5px solid #e4e4e7;" onerror="this.src='https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80'">` : ''}
                            <div>
                                <div class="msg-bubble">${escapeHtml(m.message)}</div>
                                <div class="msg-time" style="font-size: 11px; color: #a1a1aa; margin-top: 3px; ${isSent ? 'text-align: right;' : 'text-align: left;'}">${escapeHtml(m.sentAt || '')}</div>
                            </div>
                        </div>
                    `;
                }).join('');

                // Cập nhật thuộc tính theo dõi số lượng
                msgContainer.dataset.msgCount = messages.length;
                msgContainer.dataset.lastMsgId = latestMsgId;

                // Cập nhật thông tin tin nhắn cuối cùng trên sidebar
                const lastM = messages[messages.length - 1];
                if (chat) {
                    chat.lastMessage = lastM.message;
                    chat.lastTime = lastM.sentAt;
                    renderChatList();
                }
            }

            if (!isSilent || isScrolledToBottom) {
                msgContainer.scrollTop = msgContainer.scrollHeight;
            }
        } catch (e) {
            console.error('Lỗi khi nạp tin nhắn từ CSDL SQL Server:', e);
        } finally {
            isFetchingMessages = false;
        }
    }

    // Tự động thăm dò cập nhật tin nhắn theo thời gian thực (Polling 3s)
    function startChatPolling() {
        if (pollInterval) clearInterval(pollInterval);
        pollInterval = setInterval(async () => {
            if (!activeChatId) return;
            const targetChat = chats.find(c => c.id === activeChatId);
            const prodId = targetChat ? (targetChat.productId || (targetChat.product ? targetChat.product.id : null)) : null;
            if (prodId) {
                await loadMessagesFromDb(prodId, true);
            }
        }, 3000);
    }

    function handleChatKeyDown(event) {
        if (event.key === 'Enter' && !event.shiftKey) {
            event.preventDefault();
            submitSendMessage();
        }
    }

    function sendSuggestion(text) {
        document.getElementById('chatInputText').value = text;
        submitSendMessage();
    }

    function attachMediaPrompt() {
        const url = prompt('Dán đường link ảnh hoặc video món đồ bạn muốn gửi:');
        if (url) {
            sendDirectMessage(`[Hình ảnh/Video]: ${url}`);
        }
    }

    function attachAddressPrompt() {
        const defaultAddr = 'Quận 1, TP. Hồ Chí Minh';
        const addr = prompt('Nhập địa chỉ giao dịch / hẹn gặp trực tiếp:', defaultAddr);
        if (addr) {
            sendDirectMessage(`📍 Địa chỉ hẹn giao dịch: ${addr}`);
        }
    }

    function submitSendMessage() {
        const input = document.getElementById('chatInputText');
        const text = input.value.trim();
        if (!text) return;
        input.value = '';
        sendDirectMessage(text);
    }

    // Gửi tin nhắn đồng bộ trực tiếp vào SQL Server Database
    async function sendDirectMessage(text) {
        if (!activeChatId) return;

        const targetChat = chats.find(c => c.id === activeChatId);
        const prodId = targetChat ? (targetChat.productId || (targetChat.product ? targetChat.product.id : 1)) : 1;

        try {
            const formData = new URLSearchParams();
            formData.append('productId', prodId);
            formData.append('message', text);
            if (targetChat && targetChat.partnerId) {
                formData.append('receiverId', targetChat.partnerId);
            }

            const res = await fetch('/Cart/SendMessage', {
                method: 'POST',
                headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
                body: formData.toString()
            });

            const data = await res.json();
            if (data.success) {
                // Tải lại toàn bộ lịch sử tin nhắn thực tế từ SQL Server lên khung chat
                await loadMessagesFromDb(prodId, false);
                // Cập nhật lại danh sách hội thoại
                await refreshConversations(true);
            } else {
                alert(data.message || 'Lỗi khi gửi tin nhắn');
            }
        } catch (e) {
            console.error('Lỗi khi gửi tin nhắn:', e);
            alert('Không thể kết nối đến máy chủ để gửi tin nhắn.');
        }
    }

    document.addEventListener('DOMContentLoaded', initChatPage);
</script>
}
```

---

## Views/Cart/Checkout.cshtml

```html
@model ThriftShop.Models.User
@{
    ViewData["Title"] = "Thanh Toán Đơn Hàng - ThriftShop";
}

<div class="main-wrapper" style="margin-top: 30px;">
    <div class="focused-card" style="max-width: 820px;">
        <h2>💳 Thanh Toán Đơn Hàng</h2>
        <p class="desc">Kiểm tra thông tin giao hàng và xác nhận phương thức thanh toán an toàn.</p>

        <form id="checkoutForm">
            <!-- 1. THÔNG TIN GIAO HÀNG -->
            <div style="background: #f8fafc; border: 1px solid #e4e4e7; border-radius: 12px; padding: 22px; margin-bottom: 24px;">
                <h3 style="font-size: 15.5px; font-weight: 700; color: #18181b; margin-bottom: 14px;">
                    1. Địa Chỉ Nhận Hàng
                </h3>

                <div class="form-group">
                    <label>Họ và tên người nhận (*)</label>
                    <input type="text" id="custName" value="@(Model?.Name ?? "")" placeholder="Nguyễn Văn A" required>
                </div>

                <div class="form-row-2">
                    <div class="form-group">
                        <label>Số điện thoại liên hệ (*)</label>
                        <input type="tel" id="custPhone" value="@(Model?.Phone ?? "")" placeholder="0901234567" required>
                    </div>
                    <div class="form-group">
                        <label>Email (Nhận xác nhận đơn)</label>
                        <input type="email" id="custEmail" value="@(Model?.Email ?? "")" placeholder="email@gmail.com">
                    </div>
                </div>

                <div class="form-group">
                    <label>Địa chỉ nhận hàng (*)</label>
                    <input type="text" id="custAddress" value="@(Model?.Address ?? "")" placeholder="Số nhà, đường, phường/xã, quận/huyện, tỉnh thành..." required>
                </div>
            </div>

            <!-- 2. PHƯƠNG THỨC THANH TOÁN -->
            <div style="background: #f8fafc; border: 1px solid #e4e4e7; border-radius: 12px; padding: 22px; margin-bottom: 24px;">
                <h3 style="font-size: 15.5px; font-weight: 700; color: #18181b; margin-bottom: 14px;">
                    2. Phương Thức Thanh Toán
                </h3>

                <div style="display: flex; flex-direction: column; gap: 10px;">
                    <label style="display: flex; align-items: center; gap: 12px; padding: 12px 16px; border: 1px solid #e4e4e7; border-radius: 8px; cursor: pointer; background: white;">
                        <input type="radio" name="paymentMethod" value="COD - Thanh toán khi nhận hàng" checked style="accent-color: #ff3c00; width: 18px; height: 18px;">
                        <div>
                            <strong style="display: block; font-size: 13.5px; color: #18181b;">Thanh toán khi nhận hàng (COD)</strong>
                            <span style="font-size: 12px; color: #71717a;">Đồng kiểm ngoại hình món đồ cùng bưu tá trước khi thanh toán.</span>
                        </div>
                    </label>

                    <label style="display: flex; align-items: center; gap: 12px; padding: 12px 16px; border: 1px solid #e4e4e7; border-radius: 8px; cursor: pointer; background: white;">
                        <input type="radio" name="paymentMethod" value="Chuyển khoản VietQR 24/7" style="accent-color: #ff3c00; width: 18px; height: 18px;">
                        <div>
                            <strong style="display: block; font-size: 13.5px; color: #18181b;">Chuyển khoản VietQR 24/7</strong>
                            <span style="font-size: 12px; color: #71717a;">Quét mã QR tự động xác nhận đơn hàng qua ngân hàng số.</span>
                        </div>
                    </label>
                </div>
            </div>

            <!-- 3. TÓM TẮT ĐƠN HÀNG GỌN GÀNG -->
            <div style="border: 1px solid #e4e4e7; border-radius: 12px; padding: 20px; margin-bottom: 24px;">
                <h3 style="font-size: 15.5px; font-weight: 700; color: #18181b; margin-bottom: 12px;">
                    3. Món Đồ Đã Đặt
                </h3>
                <div id="checkoutItemsList" style="margin-bottom: 14px;">
                    <!-- Render các món -->
                </div>
                <div style="display: flex; justify-content: space-between; align-items: center; border-top: 1px dashed #e4e4e7; padding-top: 14px;">
                    <div>
                        <span style="font-size: 13px; color: #71717a;">Tổng số tiền cần thanh toán:</span>
                        <div id="checkoutTotal" style="font-size: 22px; font-weight: 800; color: #ff3c00;">0₫</div>
                    </div>
                    <button type="submit" id="btnSubmitCheckout" class="choice-btn active" style="padding: 12px 28px; font-size: 14.5px; border-radius: 8px;">
                        Xác Nhận Đặt Hàng &rarr;
                    </button>
                </div>
            </div>
        </form>
    </div>
</div>

@section Scripts {
<script>
    document.addEventListener('DOMContentLoaded', () => {
        // Kiểm tra đăng nhập phía client đồng bộ với server
        if (typeof window.checkUserLoggedIn === 'function' ? !window.checkUserLoggedIn() : !localStorage.getItem('thrift_current_user')) {
            window.location.replace('/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search));
            return;
        }

        // Hỗ trợ đọc giỏ hàng từ cả thrift_cart lẫn cart
        let cart = [];
        try {
            const rawThrift = localStorage.getItem('thrift_cart');
            const rawCart = localStorage.getItem('cart');
            if (rawThrift) cart = JSON.parse(rawThrift);
            else if (rawCart) cart = JSON.parse(rawCart);
        } catch(e) { cart = []; }

        const itemsList = document.getElementById('checkoutItemsList');
        const totalEl = document.getElementById('checkoutTotal');

        if (!Array.isArray(cart) || cart.length === 0) {
            // Nếu chưa có món nào trong giỏ hàng, nạp 1 món mẫu để user trải nghiệm đặt hàng
            cart = [{
                id: 1,
                name: "Bàn phím cơ DareU EK87 Tenkeyless Blue Switch cũ",
                price: 450000,
                img: "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80",
                quantity: 1
            }];
            localStorage.setItem('thrift_cart', JSON.stringify(cart));
            localStorage.setItem('cart', JSON.stringify(cart));
        }

        let total = 0;
        itemsList.innerHTML = cart.map(item => {
            const qty = item.quantity || 1;
            const price = Number(item.price) || 0;
            const itemTotal = price * qty;
            total += itemTotal;
            const itemImg = item.img || item.image || 'https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600';
            return `
                <div style="display: flex; justify-content: space-between; align-items: center; padding: 10px 0; border-bottom: 1px solid #f4f4f5; font-size: 13.5px;">
                    <div style="display: flex; align-items: center; gap: 12px;">
                        <img src="${itemImg}" style="width: 44px; height: 44px; border-radius: 8px; object-fit: cover; border: 1px solid #e2e8f0;">
                        <div>
                            <div style="font-weight: 600; color: #18181b;">${item.name || item.productName || 'Món đồ cũ'}</div>
                            <div style="font-size: 12px; color: #71717a;">SL: ${qty} x ${price.toLocaleString('vi-VN')}₫</div>
                        </div>
                    </div>
                    <div style="font-weight: 700; color: #ff3c00; font-size: 14.5px;">
                        ${itemTotal.toLocaleString('vi-VN')}₫
                    </div>
                </div>
            `;
        }).join('');

        totalEl.textContent = total.toLocaleString('vi-VN') + '₫';

        document.getElementById('checkoutForm').onsubmit = function(e) {
            e.preventDefault();
            const btn = document.getElementById('btnSubmitCheckout');
            btn.disabled = true;
            btn.textContent = "Đang xử lý...";

            const name = document.getElementById('custName').value.trim();
            const phone = document.getElementById('custPhone').value.trim();
            const email = document.getElementById('custEmail').value.trim();
            const address = document.getElementById('custAddress').value.trim();
            const payMethodEl = document.querySelector('input[name="paymentMethod"]:checked');
            const payMethod = payMethodEl ? payMethodEl.value : 'COD - Thanh toán khi nhận hàng';

            const formData = new FormData();
            formData.append('custName', name);
            formData.append('custPhone', phone);
            formData.append('custEmail', email);
            formData.append('custAddress', address);
            formData.append('paymentMethod', payMethod);
            formData.append('notes', 'Đồng kiểm ngoại hình khi nhận hàng');
            formData.append('itemsJson', JSON.stringify(cart));
            formData.append('totalAmount', total);

            fetch('/Cart/Checkout', {
                method: 'POST',
                body: formData
            })
            .then(res => res.json())
            .then(data => {
                if (data.success) {
                    localStorage.removeItem('cart');
                    localStorage.removeItem('thrift_cart');
                    if (typeof ThriftDB !== 'undefined' && typeof ThriftDB.clearCart === 'function') {
                        ThriftDB.clearCart();
                    }
                    if (typeof showToast === 'function') {
                        showToast('Đặt hàng thành công! Mã đơn: ' + data.orderId, 'success');
                    }
                    setTimeout(() => {
                        window.location.href = '/Cart/OrderSuccess?orderId=' + data.orderId;
                    }, 600);
                } else {
                    alert(data.message || 'Lỗi đặt hàng!');
                    btn.disabled = false;
                    btn.textContent = "Xác Nhận Đặt Hàng →";
                }
            })
            .catch(err => {
                alert('Lỗi kết nối máy chủ khi tạo đơn hàng!');
                btn.disabled = false;
                btn.textContent = "Xác Nhận Đặt Hàng →";
            });
        };
    });
</script>
}
```

---

## Views/Cart/OrderSuccess.cshtml

```html
@model ThriftShop.Models.Order
@{
    ViewData["Title"] = "Đặt Hàng Thành Công - ThriftShop";
    var orderId = Model?.OrderCode ?? (string?)ViewBag.OrderId;
}

<div class="container" style="margin-top: 50px; margin-bottom: 70px;">
    <div style="max-width: 650px; margin: 0 auto; background: white; border-radius: 16px; border: 1px solid #e2e8f0; padding: 45px 35px; text-align: center; box-shadow: 0 10px 30px rgba(0,0,0,0.05);">
        <div style="width: 75px; height: 75px; border-radius: 50%; background: #ecfdf5; color: #10b981; font-size: 36px; display: inline-flex; align-items: center; justify-content: center; margin-bottom: 20px;">
            <i class="fas fa-check"></i>
        </div>

        <h1 style="font-size: 26px; font-weight: 700; color: #111827; margin-bottom: 8px;">
            Đặt Hàng Thành Công!
        </h1>
        <p style="color: #64748b; font-size: 15px; margin-bottom: 24px;">
            Cảm ơn bạn đã lựa chọn mua sắm đồ cũ tái sử dụng tại sàn ThriftShop.
        </p>

        @if (!string.IsNullOrWhiteSpace(orderId))
        {
            <div style="background: #f8fafc; border: 1px dashed #cbd5e1; border-radius: 10px; padding: 14px 20px; display: inline-block; margin-bottom: 20px;">
                Mã đơn hàng của bạn: <strong style="color: #ff3c00; font-size: 18px;" id="orderIdDisplay">@orderId</strong>
            </div>
        }

        @if (Model != null)
        {
            <div style="font-size: 14px; color: #475569; margin-bottom: 24px;">
                Người nhận: <strong>@Model.CustomerName</strong> (@Model.CustomerPhone) - Tổng thanh toán: <strong style="color: #ff3c00;">@Model.TotalAmount.ToString("N0") ₫</strong>
            </div>
        }

        <div style="text-align: left; background: #f8fafc; border-radius: 12px; padding: 20px; margin-bottom: 30px; font-size: 14px; color: #334155; line-height: 1.8;">
            <p><i class="fas fa-truck text-primary mr-2" style="color: #ff3c00;"></i>Người bán và ThriftShop sẽ đóng gói cẩn thận và bàn giao cho bưu tá giao hàng.</p>
            <p><i class="fas fa-shield-alt text-primary mr-2" style="color: #ff3c00;"></i>Chính sách bảo vệ người mua: Đồng kiểm COD khi nhận và bao test hoàn tiền 7 ngày.</p>
            <p><i class="fas fa-phone-alt text-primary mr-2" style="color: #ff3c00;"></i>Nhân viên chăm sóc khách hàng sẽ liên hệ xác nhận trong vòng 15 phút.</p>
        </div>

        <div style="display: flex; gap: 14px; justify-content: center; flex-wrap: wrap;">
            <a asp-controller="Orders" asp-action="Index" class="btn btn-secondary" style="background: white; border: 1px solid #cbd5e1; color: #1e293b;">
                <i class="fas fa-receipt mr-1"></i> Xem Đơn Hàng Của Tôi
            </a>
            <a asp-controller="Products" asp-action="Index" class="btn">
                <i class="fas fa-th-large mr-1"></i> Tiếp Tục Mua Sắm
            </a>
        </div>
    </div>
</div>
```

---

## Views/Orders/Index.cshtml

```html
@{
    ViewData["Title"] = "Đơn Hàng Của Tôi";
}

<!-- BREADCRUMB -->
<div class="bg-light py-2 border-bottom">
    <div class="container">
        <nav aria-label="breadcrumb">
            <ol class="breadcrumb mb-0 bg-transparent p-0 small">
                <li class="breadcrumb-item"><a asp-controller="Home" asp-action="Index">Trang chủ</a></li>
                <li class="breadcrumb-item active" aria-current="page">Lịch sử đơn hàng của tôi</li>
            </ol>
        </nav>
    </div>
</div>

<main class="container my-4">
    <div class="section-header">
        <h2 class="section-title"><i class="fas fa-receipt text-success"></i> Đơn Hàng Của Tôi</h2>
        <a asp-controller="Products" asp-action="Index" class="btn btn-outline-success btn-sm font-weight-bold">
            <i class="fas fa-shopping-bag mr-1"></i>Mua thêm đồ cũ
        </a>
    </div>

    <!-- THÔNG BÁO CHO KHÁCH CHƯA ĐĂNG NHẬP -->
    <div id="not-logged-in-banner" class="alert alert-warning border-0 shadow-sm d-flex align-items-center justify-content-between mb-4 d-none" style="border-radius: 10px;">
        <div>
            <i class="fas fa-info-circle mr-2 text-warning"></i>
            <strong>Bạn đang ở chế độ xem mẫu (Chưa đăng nhập):</strong> 
            Vui lòng <a asp-controller="Account" asp-action="Login" class="font-weight-bold text-dark text-underline">Đăng nhập tài khoản</a> 
            (User: <code>user@thriftshop.vn</code> / pass: <code>user</code>) để quản lý đơn hàng thực tế của bạn.
        </div>
        <a asp-controller="Account" asp-action="Login" class="btn btn-warning btn-sm font-weight-bold ml-2">Đăng nhập ngay</a>
    </div>

    <!-- TÌM KIẾM ĐƠN HÀNG (THEO SLIDE 2 KTRA BE) -->
    <div class="card border-0 shadow-sm mb-4">
        <div class="card-body p-3">
            <div class="row align-items-center">
                <div class="col-md-8 mb-2 mb-md-0">
                    <div class="input-group">
                        <div class="input-group-prepend">
                            <span class="input-group-text bg-white border-right-0"><i class="fas fa-search text-muted"></i></span>
                        </div>
                        <input type="text" id="order-search-input" class="form-control border-left-0" placeholder="Tìm theo Mã đơn hàng (VD: DH100234) hoặc Tên sản phẩm...">
                    </div>
                </div>
                <div class="col-md-4 text-md-right">
                    <button class="btn btn-success font-weight-bold px-3 mr-2" onclick="handleSearchOrder()">
                        <i class="fas fa-search mr-1"></i>Tìm đơn hàng
                    </button>
                    <button class="btn btn-outline-secondary btn-sm" onclick="resetOrderSearch()">
                        Đặt lại
                    </button>
                </div>
            </div>
        </div>
    </div>

    <!-- TABS TRẠNG THÁI -->
    <ul class="nav nav-tabs mb-4 font-weight-bold" id="order-status-tabs">
        <li class="nav-item">
            <a class="nav-link active text-success" href="javascript:void(0)" onclick="filterOrderByStatus('all', this)">
                <i class="fas fa-list mr-1"></i>Tất cả đơn
            </a>
        </li>
        <li class="nav-item">
            <a class="nav-link text-dark" href="javascript:void(0)" onclick="filterOrderByStatus('pending', this)">
                Chờ duyệt đơn
            </a>
        </li>
        <li class="nav-item">
            <a class="nav-link text-dark" href="javascript:void(0)" onclick="filterOrderByStatus('processing', this)">
                Đang xử lý
            </a>
        </li>
        <li class="nav-item">
            <a class="nav-link text-dark" href="javascript:void(0)" onclick="filterOrderByStatus('shipping', this)">
                Đang vận chuyển
            </a>
        </li>
        <li class="nav-item">
            <a class="nav-link text-dark" href="javascript:void(0)" onclick="filterOrderByStatus('delivered', this)">
                Đã giao hàng
            </a>
        </li>
    </ul>

    <!-- DANH SÁCH ĐƠN HÀNG -->
    <div id="orders-list-container">
        <!-- Render JS -->
    </div>

    <!-- THÔNG BÁO NẾU RỖNG -->
    <div id="no-order-alert" class="card border-0 shadow-sm text-center py-5 d-none">
        <div class="card-body">
            <i class="fas fa-box-open fa-4x text-muted mb-3"></i>
            <h4 class="font-weight-bold text-dark">BẠN CHƯA CÓ ĐƠN HÀNG NÀO. HÃY ĐẶT ĐƠN NGAY.</h4>
            <p class="text-muted">Hàng ngàn món đồ cũ vintage độc đáo đang chờ bạn khám phá trên sàn ThriftShop.</p>
            <a asp-controller="Home" asp-action="Index" class="btn btn-success btn-lg font-weight-bold mt-2">
                <i class="fas fa-home mr-2"></i>Quay Về Trang Chủ (HOME)
            </a>
        </div>
    </div>
</main>

<!-- MODAL HÓA ĐƠN CHI TIẾT -->
<div class="modal fade" id="orderInvoiceModal" tabindex="-1" role="dialog" aria-hidden="true">
    <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
        <div class="modal-content">
            <div class="modal-header bg-success text-white">
                <h5 class="modal-title font-weight-bold">
                    <i class="fas fa-file-invoice mr-2"></i>HÓA ĐƠN CHI TIẾT ĐƠN HÀNG
                </h5>
                <button type="button" class="close text-white" data-dismiss="modal">&times;</button>
            </div>
            <div class="modal-body p-4" id="invoice-modal-content"></div>
            <div class="modal-footer bg-light">
                <button type="button" class="btn btn-secondary" data-dismiss="modal">Đóng</button>
                <button type="button" class="btn btn-primary" onclick="window.print()">
                    <i class="fas fa-print mr-1"></i>In Hóa Đơn
                </button>
            </div>
        </div>
    </div>
</div>

@section Scripts {
<script>
    let currentStatusFilter = "all";
    let searchKeyword = "";

    $(document).ready(function() {
        renderOrders();
    });

    function filterOrderByStatus(status, element) {
        currentStatusFilter = status;
        $("#order-status-tabs .nav-link").removeClass("active text-success").addClass("text-dark");
        $(element).addClass("active text-success").removeClass("text-dark");
        renderOrders();
    }

    function handleSearchOrder() {
        searchKeyword = $("#order-search-input").val().trim().toLowerCase();
        renderOrders();
    }

    function resetOrderSearch() {
        $("#order-search-input").val("");
        searchKeyword = "";
        renderOrders();
    }

    function renderOrders() {
        fetch('/Orders/GetMyOrders')
            .then(res => res.json())
            .then(serverOrders => {
                let orders = Array.isArray(serverOrders) ? serverOrders : [];

                orders.forEach(o => {
                    if ((!o.items || o.items.length === 0) && o.itemsJson) {
                        try { o.items = JSON.parse(o.itemsJson); } catch(e){}
                    }
                    if (!o.items) o.items = [];
                });

                const user = (typeof ThriftDB !== 'undefined') ? ThriftDB.getCurrentUser() : null;
                if (!user && orders.length === 0) {
                    $("#not-logged-in-banner").removeClass("d-none");
                } else {
                    $("#not-logged-in-banner").addClass("d-none");
                }

                if (currentStatusFilter !== "all") {
                    orders = orders.filter(o => o.orderStatus === currentStatusFilter);
                }

                if (searchKeyword) {
                    orders = orders.filter(o => {
                        const matchId = (o.id || "").toLowerCase().includes(searchKeyword);
                        const matchProd = (o.items || []).some(i => (i.productName || "").toLowerCase().includes(searchKeyword));
                        return matchId || matchProd;
                    });
                }

                const container = $("#orders-list-container");
                container.empty();

                if (orders.length === 0) {
                    $("#no-order-alert").removeClass("d-none");
                    return;
                }

                $("#no-order-alert").addClass("d-none");

        orders.forEach(order => {
            let badgeStatusClass = "badge-warning";
            let statusText = "Chờ duyệt đơn";
            if (order.orderStatus === "processing") {
                badgeStatusClass = "badge-info";
                statusText = "Đang xử lý đóng gói";
            } else if (order.orderStatus === "shipping") {
                badgeStatusClass = "badge-primary";
                statusText = "Đang vận chuyển";
            } else if (order.orderStatus === "delivered") {
                badgeStatusClass = "badge-success";
                statusText = "Đã giao hàng thành công";
            } else if (order.orderStatus === "cancelled") {
                badgeStatusClass = "badge-danger";
                statusText = "Đã hủy đơn";
            }

            let itemsHtml = "";
            order.items.forEach(it => {
                itemsHtml += `
                    <div class="d-flex align-items-center py-2 border-bottom">
                        <img src="${it.image}" alt="${it.productName}" style="width: 55px; height: 55px; object-fit: cover; border-radius: 6px;" class="mr-3 border">
                        <div class="flex-fill">
                            <h6 class="mb-1 font-weight-bold text-dark">${it.productName}</h6>
                            <div class="small text-muted">
                                ${it.condition ? `<span class="badge badge-light border mr-2">${it.condition}</span>` : ''}
                                <span>Số lượng: x${it.quantity}</span>
                            </div>
                        </div>
                        <div class="text-right">
                            <div class="font-weight-bold text-danger">${ThriftDB.formatCurrency(it.subtotal)}</div>
                            <div class="mt-1">
                                <button class="btn btn-outline-success btn-sm py-0 px-2 mr-1" onclick="handleBuyAgain(${it.productId})">
                                    <i class="fas fa-redo-alt mr-1"></i>Mua lại
                                </button>
                                <a href="/Products/Details/${it.productId}" class="btn btn-link btn-sm p-0 text-muted">
                                    Xem sản phẩm
                                </a>
                            </div>
                        </div>
                    </div>
                `;
            });

            container.append(`
                <div class="card border-0 shadow-sm mb-4">
                    <div class="card-header bg-white d-flex flex-wrap justify-content-between align-items-center py-3 border-bottom">
                        <div>
                            <span class="font-weight-bold text-dark mr-3"><i class="fas fa-barcode mr-1 text-muted"></i>Mã đơn: <span class="text-success">${order.id}</span></span>
                            <span class="text-muted small mr-3"><i class="far fa-clock mr-1"></i>Ngày đặt: ${order.orderDate}</span>
                            <span class="badge ${badgeStatusClass} font-weight-bold px-2 py-1">${statusText}</span>
                        </div>
                        <div class="mt-2 mt-md-0">
                            <button class="btn btn-outline-info btn-sm font-weight-bold" onclick="showOrderInvoice('${order.id}')">
                                <i class="fas fa-file-invoice mr-1"></i>Xem Hóa Đơn Chi Tiết
                            </button>
                        </div>
                    </div>
                    <div class="card-body">
                        ${itemsHtml}
                        <div class="d-flex flex-wrap justify-content-between align-items-center mt-3 pt-2">
                            <div class="small text-muted mb-2 mb-md-0">
                                <i class="fas fa-map-marker-alt mr-1 text-danger"></i>Giao đến: <strong>${order.customerName}</strong> (${order.customerPhone}) - ${order.shippingAddress}
                            </div>
                            <div class="text-right">
                                <span class="text-muted mr-2">Tổng số tiền:</span>
                                <span class="h5 font-weight-bold text-danger mb-0">${ThriftDB.formatCurrency(order.totalAmount)}</span>
                            </div>
                        </div>
                    </div>
                </div>
            `);
        });
    }

    function handleBuyAgain(productId) {
        const res = ThriftDB.addToCart(productId, 1);
        if (res.success) {
            showToast("Đã thêm món đồ vào giỏ hàng!", "success");
            setTimeout(function() {
                window.location.href = "/Cart";
            }, 500);
        } else {
            showToast(res.message, "warning");
        }
    }

    function showOrderInvoice(orderId) {
        const order = ThriftDB.getOrderById(orderId);
        if (!order) return;

        let itemsHtml = "";
        order.items.forEach((it, idx) => {
            itemsHtml += `
                <tr>
                    <td class="text-center">${idx + 1}</td>
                    <td>
                        <strong>${it.productName}</strong><br>
                        ${it.condition ? `<small class="text-muted">Tình trạng: ${it.condition}</small>` : ''}
                    </td>
                    <td class="text-center">${ThriftDB.formatCurrency(it.price)}</td>
                    <td class="text-center">${it.quantity}</td>
                    <td class="text-right font-weight-bold text-danger">${ThriftDB.formatCurrency(it.subtotal)}</td>
                </tr>
            `;
        });

        const modalHtml = `
            <div class="invoice-box">
                <div class="invoice-header d-flex justify-content-between align-items-center">
                    <div>
                        <div class="invoice-company-title"><i class="fas fa-recycle mr-2"></i>ThriftShop Secondhand</div>
                        <small class="text-muted">Chợ Mua Bán & Ký Gửi Đồ Cũ Uy Tín</small>
                    </div>
                    <div class="text-right">
                        <h5 class="font-weight-bold text-dark mb-0">HÓA ĐƠN BÁN LẺ</h5>
                        <small class="text-muted">Mã đơn: <strong>${order.id}</strong></small><br>
                        <small class="text-muted">Ngày: ${order.orderDate}</small>
                    </div>
                </div>

                <div class="row my-3">
                    <div class="col-sm-6">
                        <h6 class="font-weight-bold text-success mb-1">Thông Tin Khách Hàng:</h6>
                        <p class="small mb-1"><strong>Họ tên:</strong> ${order.customerName}</p>
                        <p class="small mb-1"><strong>Điện thoại:</strong> ${order.customerPhone}</p>
                        <p class="small mb-0"><strong>Email:</strong> ${order.customerEmail || "Không có"}</p>
                    </div>
                    <div class="col-sm-6 text-sm-right">
                        <h6 class="font-weight-bold text-success mb-1">Địa Chỉ Nhận Hàng:</h6>
                        <p class="small mb-1">${order.shippingAddress}</p>
                        <p class="small mb-1"><strong>Thanh toán:</strong> ${order.paymentMethod}</p>
                        <p class="small mb-0"><strong>Trạng thái:</strong> <span class="badge badge-success">${order.orderStatusText || "Đang xử lý"}</span></p>
                    </div>
                </div>

                <table class="table table-bordered small">
                    <thead class="bg-light">
                        <tr>
                            <th style="width: 5%;" class="text-center">STT</th>
                            <th style="width: 45%;">Tên Món Đồ Cũ</th>
                            <th class="text-center">Đơn Giá</th>
                            <th class="text-center">SL</th>
                            <th class="text-right">Thành Tiền</th>
                        </tr>
                    </thead>
                    <tbody>
                        ${itemsHtml}
                    </tbody>
                    <tfoot>
                        <tr>
                            <td colspan="4" class="text-right font-weight-bold">Tạm tính tiền hàng:</td>
                            <td class="text-right font-weight-bold">${ThriftDB.formatCurrency(order.subtotal)}</td>
                        </tr>
                        <tr>
                            <td colspan="4" class="text-right font-weight-bold">Phí vận chuyển:</td>
                            <td class="text-right font-weight-bold text-success">${order.shippingFee === 0 ? "Miễn phí" : ThriftDB.formatCurrency(order.shippingFee)}</td>
                        </tr>
                        <tr class="bg-light">
                            <td colspan="4" class="text-right font-weight-bold h6 mb-0">TỔNG CỘNG THANH TOÁN:</td>
                            <td class="text-right font-weight-bold text-danger h6 mb-0">${ThriftDB.formatCurrency(order.totalAmount)}</td>
                        </tr>
                    </tfoot>
                </table>

                <div class="text-center mt-4 small text-muted">
                    <p class="font-italic mb-1">Cảm ơn bạn đã lựa chọn tái sử dụng đồ cũ cùng ThriftShop!</p>
                    <p class="mb-0">Hotline hỗ trợ kỹ thuật và bảo hành: 1900 6868</p>
                </div>
            </div>
        `;

        $("#invoice-modal-content").html(modalHtml);
        $("#orderInvoiceModal").modal("show");
    }
</script>
}
```

---

## Views/Admin/Index.cshtml

```html
@model ThriftShop.Models.AdminDashboardViewModel
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "Bảng Điều Khiển Quản Trị";
}

<div class="page-title-box">
  <h1 class="page-title">Bảng Điều Khiển Quản Trị</h1>
  <div class="page-breadcrumb">Quản lý hệ thống sàn đồ cũ ThriftShop</div>
</div>

<!-- 4 Ô THỐNG KÊ CƠ BẢN ĐƠN SẮC -->
<div class="row mb-3">
  <div class="col-md-3 col-6">
    <div class="simple-stat-box">
      <div class="simple-stat-title">Tin Chờ Duyệt</div>
      <div class="simple-stat-num" id="stat-pending-posts">@(Model?.PendingCount ?? 0)</div>
      <small><a asp-controller="Admin" asp-action="ApproveProducts" class="text-dark">Kiểm duyệt ngay &raquo;</a></small>
    </div>
  </div>

  <div class="col-md-3 col-6">
    <div class="simple-stat-box">
      <div class="simple-stat-title">Danh Mục Hàng</div>
      <div class="simple-stat-num" id="stat-categories">@(Model?.CategoriesCount ?? 0)</div>
      <small><a asp-controller="Admin" asp-action="Categories" class="text-dark">Quản lý danh mục &raquo;</a></small>
    </div>
  </div>

  <div class="col-md-3 col-6">
    <div class="simple-stat-box">
      <div class="simple-stat-title">Tin Đang Bán</div>
      <div class="simple-stat-num" id="stat-products">@(Model?.ProductsCount ?? 0)</div>
      <small><a asp-controller="Admin" asp-action="Products" class="text-dark">Xem danh sách &raquo;</a></small>
    </div>
  </div>

  <div class="col-md-3 col-6">
    <div class="simple-stat-box">
      <div class="simple-stat-title">Tổng Người Dùng</div>
      <div class="simple-stat-num" id="stat-users">@(Model?.UsersCount ?? 0)</div>
      <small><a asp-controller="Admin" asp-action="Users" class="text-dark">Quản lý tài khoản &raquo;</a></small>
    </div>
  </div>
</div>

<!-- BẢNG TIN ĐĂNG CHỜ DUYỆT -->
<div class="card mb-4">
  <div class="card-header bg-light d-flex justify-content-between align-items-center py-2">
    <strong class="text-dark">Tin đồ cũ đang chờ kiểm duyệt gần đây</strong>
    <a asp-controller="Admin" asp-action="ApproveProducts" class="small text-dark font-weight-bold">Xem tất cả (@(Model?.PendingCount ?? 0)) &raquo;</a>
  </div>
  <div class="table-responsive">
    <table class="table table-bordered table-striped mb-0" style="font-size: 13px;">
      <thead class="thead-light">
        <tr>
          <th>Tên món đồ</th>
          <th>Người đăng</th>
          <th>Danh mục</th>
          <th>Giá thanh lý</th>
          <th class="text-center" style="width: 120px;">Thao tác</th>
        </tr>
      </thead>
      <tbody>
        @if (Model != null && Model.RecentPending.Any())
        {
          @foreach (var p in Model.RecentPending)
          {
            <tr>
              <td>
                <div class="d-flex align-items-center">
                  <img src="@p.Image" style="width: 40px; height: 40px; object-fit: cover; margin-right: 10px; border: 1px solid #ccc;" onerror="this.src='https://images.unsplash.com/photo-1544816155-12df9643f363?w=100'">
                  <div>
                    <strong>@p.Name</strong>
                    <div class="text-muted" style="font-size: 11px;">Mã tin: #@p.Id</div>
                  </div>
                </div>
              </td>
              <td>@p.SellerName</td>
              <td>@p.CategoryName</td>
              <td class="font-weight-bold">@p.Price.ToString("N0") ₫</td>
              <td class="text-center">
                <a asp-controller="Admin" asp-action="ApproveProducts" class="btn btn-sm btn-outline-dark">
                  Duyệt tin
                </a>
              </td>
            </tr>
          }
        }
        else
        {
          <tr>
            <td colspan="5" class="text-center py-3 text-muted">
              Không có tin nào đang chờ duyệt.
            </td>
          </tr>
        }
      </tbody>
    </table>
  </div>
</div>

@section Scripts {
<script>
  // Dữ liệu được nạp trực tiếp từ LINQ Controller
</script>
}
```

---

## Views/Admin/ApproveProducts.cshtml

```html
@model IEnumerable<ThriftShop.Models.PendingPost>
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "Kiểm Duyệt Tin Đồ Cũ";
    var postsJson = System.Text.Json.JsonSerializer.Serialize(Model ?? Enumerable.Empty<ThriftShop.Models.PendingPost>());
}

<div class="page-title-box d-flex justify-content-between align-items-center mb-3">
  <div>
    <h1 class="page-title">Kiểm Duyệt Tin Đăng Bán Đồ Cũ</h1>
    <div class="page-breadcrumb">Quản lý và phê duyệt tin đăng bán đồ cũ từ người dùng trước khi mở bán</div>
  </div>
  <div>
    <span class="badge badge-secondary p-2" style="font-size: 13px;">
      Tin chờ duyệt: <span id="count-pending-badge">0</span>
    </span>
  </div>
</div>

<!-- Hướng dẫn nghiệp vụ kiểm duyệt đơn sắc -->
<div class="alert alert-secondary py-2 mb-3" style="font-size: 13px;">
  <strong>Quy trình kiểm duyệt:</strong> Kiểm tra ảnh thật, tình trạng món đồ và mô tả của người bán. Bấm <strong>"Duyệt"</strong> để mở bán lên trang chủ hoặc <strong>"Từ Chối"</strong> kèm lý do.
</div>

<!-- TABS TRẠNG THÁI KIỂM DUYỆT ĐƠN SẮC -->
<ul class="nav nav-tabs mb-3 font-weight-bold" id="approval-tabs">
  <li class="nav-item">
    <a class="nav-link active" href="javascript:void(0)" onclick="filterApproval('pending', this)">
      Chờ Duyệt (<span id="tab-pending-count">0</span>)
    </a>
  </li>
  <li class="nav-item">
    <a class="nav-link" href="javascript:void(0)" onclick="filterApproval('approved', this)">
      Đã Duyệt Mở Bán (<span id="tab-approved-count">0</span>)
    </a>
  </li>
  <li class="nav-item">
    <a class="nav-link" href="javascript:void(0)" onclick="filterApproval('rejected', this)">
      Đã Từ Chối (<span id="tab-rejected-count">0</span>)
    </a>
  </li>
  <li class="nav-item">
    <a class="nav-link" href="javascript:void(0)" onclick="filterApproval('all', this)">
      Tất Cả Tin Đăng
    </a>
  </li>
</ul>

<!-- BẢNG DANH SÁCH BÀI ĐĂNG CẦN DUYỆT -->
<div class="card mb-4">
  <div class="table-responsive">
    <table class="table table-bordered table-striped mb-0" style="font-size: 13px;">
      <thead class="thead-light">
        <tr>
          <th style="width: 38%;">Món Đồ Cũ & Danh Mục</th>
          <th style="width: 24%;">Người Đăng Bán</th>
          <th>Giá Thanh Lý</th>
          <th class="text-center">Trạng Thái</th>
          <th class="text-center" style="width: 200px;">Hành Động</th>
        </tr>
      </thead>
      <tbody id="approval-tbody">
        <!-- Render bởi JS -->
      </tbody>
    </table>
  </div>
</div>

<!-- THÔNG BÁO NẾU RỖNG -->
<div id="no-approval-alert" class="card text-center py-4 d-none">
  <div class="card-body">
    <h6 class="font-weight-bold mb-1">Không có tin đăng nào trong mục này.</h6>
    <p class="text-muted small mb-0">Tất cả các tin thanh lý đồ cũ đều đã được xử lý.</p>
  </div>
</div>

<!-- MODAL XEM CHI TIẾT ĐỂ KIỂM ĐỊNH -->
<div class="modal fade" id="reviewDetailModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-light">
        <h5 class="modal-title font-weight-bold text-dark">Chi Tiết Món Đồ Cũ Cần Kiểm Duyệt</h5>
        <button type="button" class="close" data-dismiss="modal">&times;</button>
      </div>
      <div class="modal-body p-3" id="review-modal-body">
        <!-- Render thông tin chi tiết an toàn -->
      </div>
      <div class="modal-footer bg-light" id="review-modal-footer">
        <!-- Render nút Duyệt / Từ chối -->
      </div>
    </div>
  </div>
</div>

<!-- MODAL NHẬP LÝ DO TỪ CHỐI -->
<div class="modal fade" id="rejectReasonModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-light">
        <h5 class="modal-title font-weight-bold text-dark">Từ Chối Tin Đăng Bán Đồ Cũ</h5>
        <button type="button" class="close" data-dismiss="modal">&times;</button>
      </div>
      <div class="modal-body">
        <input type="hidden" id="reject-post-id">
        <p class="font-weight-bold text-dark mb-2">Vui lòng nhập lý do từ chối để phản hồi cho người bán:</p>
        <div class="form-group">
          <textarea class="form-control" id="reject-reason-input" rows="3" placeholder="Ví dụ: Hình ảnh chưa rõ các góc bị trầy xước, thông tin chưa đầy đủ..."></textarea>
        </div>
        <div class="small text-muted">Lý do này sẽ hiển thị trong trang cá nhân của người bán để họ chỉnh sửa lại.</div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy</button>
        <button type="button" class="btn btn-dark" onclick="confirmRejectPost()">
          Xác Nhận Từ Chối
        </button>
      </div>
    </div>
  </div>
</div>

@section Scripts {
<script>
  let currentFilter = "pending";

  // Hàm an toàn tránh triệt để lỗi 'undefined'
  function safeVal(val, defaultVal) {
    if (val === undefined || val === null || val === 'undefined' || val === 'null' || String(val).trim() === '') {
      return defaultVal;
    }
    return val;
  }

  $(document).ready(function() {
    renderApprovalList();
  });

  function filterApproval(status, element) {
    currentFilter = status;
    $("#approval-tabs .nav-link").removeClass("active");
    $(element).addClass("active");
    renderApprovalList();
  }

  const serverPosts = @Html.Raw(postsJson);
  let allApprovalPosts = [];

  function renderApprovalList() {
    let posts = serverPosts && serverPosts.length > 0
        ? serverPosts.map(p => ({
            id: safeVal(p.Id, safeVal(p.id, 0)),
            name: safeVal(p.Name, safeVal(p.name, "Món đồ cũ")),
            categoryId: safeVal(p.CategoryId, safeVal(p.categoryId, 1)),
            categoryName: safeVal(p.CategoryName, safeVal(p.categoryName, "Đồ cũ")),
            price: Number(safeVal(p.Price, safeVal(p.price, 0))) || 0,
            originalPrice: Number(safeVal(p.OriginalPrice, safeVal(p.originalPrice, 0))) || 0,
            description: safeVal(p.Description, safeVal(p.description, "Không có mô tả chi tiết thêm.")),
            image: safeVal(p.Image, safeVal(p.image, "https://images.unsplash.com/photo-1544816155-12df9643f363?w=300")),
            imagesJson: safeVal(p.ImagesJson, safeVal(p.imagesJson, "")),
            videosJson: safeVal(p.VideosJson, safeVal(p.videosJson, "")),
            stock: Number(safeVal(p.Stock, safeVal(p.stock, 1))) || 1,
            sellerId: safeVal(p.SellerId, safeVal(p.sellerId, "user")),
            sellerName: safeVal(p.SellerName, safeVal(p.sellerName, "Người dùng")),
            sellerPhone: safeVal(p.SellerPhone, safeVal(p.sellerPhone, "Chưa cập nhật")),
            sellerEmail: safeVal(p.SellerEmail, safeVal(p.sellerEmail, "user@thriftshop.vn")),
            sellerAddress: safeVal(p.SellerAddress, safeVal(p.sellerAddress, "TP. Hồ Chí Minh")),
            sellerAvatar: safeVal(p.SellerAvatar, safeVal(p.sellerAvatar, "")),
            status: String(safeVal(p.Status, safeVal(p.status, "pending"))).toLowerCase(),
            createdAt: safeVal(p.CreatedAt, safeVal(p.createdAt, "Mới đây"))
        }))
        : ((typeof ThriftDB !== 'undefined') ? ThriftDB.getPendingPosts() : []);

    allApprovalPosts = posts;

    // Cập nhật số lượng tabs & đồng bộ các badge toàn hệ thống (không bao giờ lệch số)
    const pendingCount = posts.filter(p => p.status === "pending").length;
    const approvedCount = posts.filter(p => p.status === "approved").length;
    const rejectedCount = posts.filter(p => p.status === "rejected").length;

    $("#tab-pending-count").text(pendingCount);
    $("#tab-approved-count").text(approvedCount);
    $("#tab-rejected-count").text(rejectedCount);
    $("#count-pending-badge").text(pendingCount);

    // Đồng bộ ngay badge trên Sidebar và Topbar
    $("#admin-pending-badge").text(pendingCount);
    $("#admin-topbar-pending-alert").text(pendingCount);
    window.serverRealPendingCount = pendingCount;

    // Đồng bộ vào localStorage nếu có
    if (serverPosts && serverPosts.length > 0 && typeof ThriftDB !== 'undefined' && typeof ThriftDB.savePendingPosts === 'function') {
      ThriftDB.savePendingPosts(posts);
    }

    let filtered = posts;
    if (currentFilter !== "all") {
      filtered = posts.filter(p => p.status === currentFilter);
    }

    const tbody = $("#approval-tbody");
    tbody.empty();

    if (filtered.length === 0) {
      $("#no-approval-alert").removeClass("d-none");
      return;
    }

    $("#no-approval-alert").addClass("d-none");

    filtered.forEach(p => {
      let statusBadge = "";
      if (p.status === "pending") {
        statusBadge = '<span class="badge badge-secondary px-2 py-1">Chờ duyệt</span>';
      } else if (p.status === "approved") {
        statusBadge = '<span class="badge badge-secondary px-2 py-1">Đã mở bán</span>';
      } else if (p.status === "rejected") {
        statusBadge = '<span class="badge badge-secondary px-2 py-1">Đã từ chối</span>';
      }

      // Các nút hành động đơn sắc
      let actionButtons = "";
      if (p.status === "pending") {
        actionButtons = `
          <button class="btn btn-dark btn-sm px-2 mr-1" onclick="handleApprove(${p.id})">
            Duyệt
          </button>
          <button class="btn btn-outline-dark btn-sm px-2 mr-1" onclick="openRejectModal(${p.id})">
            Từ chối
          </button>
          <button class="btn btn-outline-dark btn-sm px-2" onclick="openReviewModal(${p.id})">
            Chi tiết
          </button>
        `;
      } else {
        actionButtons = `
          <button class="btn btn-outline-dark btn-sm px-3" onclick="openReviewModal(${p.id})">
            Xem lại
          </button>
        `;
      }

      const pPriceStr = (Number(p.price) || 0).toLocaleString('vi-VN') + ' ₫';
      const pOrigPriceStr = (Number(p.originalPrice) || 0).toLocaleString('vi-VN') + ' ₫';

      tbody.append(`
        <tr>
          <td>
            <div class="d-flex align-items-center">
              <img src="${p.image}" alt="${p.name}" style="width: 48px; height: 48px; object-fit: cover; border: 1px solid #ccc; margin-right: 10px;" onerror="this.src='https://images.unsplash.com/photo-1544816155-12df9643f363?w=100'">
              <div>
                <strong>${p.name}</strong>
                <div><span class="badge badge-light border text-muted">${p.categoryName}</span></div>
                <div class="small text-muted mt-1">${p.createdAt}</div>
              </div>
            </div>
          </td>
          <td>
            <strong>${p.sellerName}</strong>
            <div class="small text-muted">SĐT: ${p.sellerPhone}</div>
            <div class="small text-muted">KV: ${p.sellerAddress}</div>
          </td>
          <td>
            <strong>${pPriceStr}</strong><br>
            <small class="text-muted" style="text-decoration: line-through;">Gốc: ${pOrigPriceStr}</small>
          </td>
          <td class="text-center">
            ${statusBadge}
          </td>
          <td class="text-center text-nowrap">
            ${actionButtons}
          </td>
        </tr>
      `);
    });
  }

  // 1. Phê duyệt món đồ cũ lên sàn
  function handleApprove(postId) {
    if (confirm("Xác nhận duyệt món đồ cũ này lên sàn mở bán?")) {
      fetch('/Admin/ApprovePost?id=' + postId, { method: 'POST' })
        .then(r => r.json())
        .then(res => {
          if (res.success) {
            if (typeof ThriftDB !== 'undefined') ThriftDB.approvePost(postId);
            alert("THÀNH CÔNG!\n\nĐã duyệt món đồ cũ lên sàn mở bán.");
            location.reload();
          } else {
            alert(res.message || "Lỗi khi duyệt bài!");
          }
        })
        .catch(err => {
          alert("Lỗi kết nối máy chủ khi duyệt bài!");
        });
    }
  }

  // 2. Mở modal nhập lý do từ chối
  function openRejectModal(postId) {
    $("#reject-post-id").val(postId);
    $("#reject-reason-input").val("");
    $("#rejectReasonModal").modal("show");
  }

  function confirmRejectPost() {
    const postId = $("#reject-post-id").val();
    const reason = $("#reject-reason-input").val().trim();
    if (!reason) {
      alert("Vui lòng nhập lý do từ chối!");
      return;
    }

    fetch('/Admin/RejectPost?id=' + postId + '&reason=' + encodeURIComponent(reason), { method: 'POST' })
      .then(r => r.json())
      .then(res => {
        if (res.success) {
          if (typeof ThriftDB !== 'undefined') ThriftDB.rejectPost(postId, reason);
          $("#rejectReasonModal").modal("hide");
          alert("Đã từ chối bài đăng.");
          location.reload();
        } else {
          alert(res.message || "Lỗi khi từ chối bài!");
        }
      })
      .catch(err => {
        alert("Lỗi kết nối máy chủ khi từ chối bài!");
      });
  }

  // 3. Mở modal xem toàn bộ thông tin kiểm định (Đã khử hoàn toàn undefined)
  function openReviewModal(postId) {
    const rawPost = (allApprovalPosts && allApprovalPosts.find(p => p && (String(p.id) === String(postId) || String(p.Id) === String(postId))))
                 || (Array.isArray(serverPosts) && serverPosts.find(p => p && (String(p.Id || p.id) === String(postId))))
                 || ((typeof ThriftDB !== 'undefined') ? ThriftDB.getPendingPostById(postId) : null)
                 || {};

    const pId = safeVal(rawPost.Id, safeVal(rawPost.id, postId));
    const pPrice = Number(safeVal(rawPost.Price, safeVal(rawPost.price, 0))) || 0;
    const pOrigPrice = Number(safeVal(rawPost.OriginalPrice, safeVal(rawPost.originalPrice, 0))) || 0;
    const pName = safeVal(rawPost.Name, safeVal(rawPost.name, 'Món đồ cũ'));
    const pImg = safeVal(rawPost.Image, safeVal(rawPost.image, 'https://images.unsplash.com/photo-1544816155-12df9643f363?w=300'));
    const pCategory = safeVal(rawPost.CategoryName, safeVal(rawPost.categoryName, 'Đồ cũ'));
    const pDesc = safeVal(rawPost.Description, safeVal(rawPost.description, 'Không có mô tả chi tiết thêm.'));
    const pSellerName = safeVal(rawPost.SellerName, safeVal(rawPost.sellerName, 'Thành viên'));
    const pSellerPhone = safeVal(rawPost.SellerPhone, safeVal(rawPost.sellerPhone, 'Chưa cập nhật'));
    const pSellerAddress = safeVal(rawPost.SellerAddress, safeVal(rawPost.sellerAddress, 'TP.HCM'));
    const pStatus = String(safeVal(rawPost.Status, safeVal(rawPost.status, 'pending'))).toLowerCase();
    const pCreatedAt = safeVal(rawPost.CreatedAt, safeVal(rawPost.createdAt, 'Mới đây'));

    const discountPercent = pOrigPrice > pPrice ? Math.round((1 - (pPrice / pOrigPrice)) * 100) : 0;
    const formatVND = num => (Number(num) || 0).toLocaleString('vi-VN') + ' ₫';

    // Parse danh sách hình ảnh thực tế nếu có
    let imageList = [];
    try {
      const rawImages = rawPost.ImagesJson || rawPost.imagesJson;
      if (rawImages) {
        const parsed = typeof rawImages === 'string' ? JSON.parse(rawImages) : rawImages;
        if (Array.isArray(parsed) && parsed.length > 0) imageList = parsed.filter(x => !!x);
      }
    } catch(e) {}
    if (!imageList.includes(pImg) && pImg) {
      imageList.unshift(pImg);
    }

    const html = `
      <div class="row">
        <div class="col-md-5 text-center mb-3">
          <img id="reviewMainImage" src="${pImg}" class="img-fluid border" style="max-height: 240px; width: 100%; object-fit: cover;" onerror="this.src='https://images.unsplash.com/photo-1544816155-12df9643f363?w=300'">
          ${imageList.length > 1 ? `
            <div class="d-flex justify-content-center flex-wrap mt-2" style="gap: 6px;">
              ${imageList.map((img, idx) => `
                <img src="${img}" style="width: 44px; height: 44px; object-fit: cover; border: 1px solid #333; cursor: pointer;" onclick="document.getElementById('reviewMainImage').src = '${img}';" title="Ảnh ${idx + 1}">
              `).join('')}
            </div>
          ` : ''}
          <div class="small text-muted mt-2">Ảnh người bán tải lên (${imageList.length} ảnh)</div>
        </div>
        <div class="col-md-7">
          <div><span class="badge badge-light border mb-1">${pCategory}</span></div>
          <h5 class="font-weight-bold text-dark mb-2">${pName}</h5>

          <div class="p-2 bg-light border mb-3">
            <strong style="font-size: 16px;">${formatVND(pPrice)}</strong>
            ${pOrigPrice > pPrice ? `<span class="text-muted ml-2" style="text-decoration: line-through;">Gốc: ${formatVND(pOrigPrice)}</span>` : ''}
            ${discountPercent > 0 ? `<span class="badge badge-secondary ml-2">Giảm ${discountPercent}%</span>` : ''}
          </div>

          <div class="mb-3">
            <strong>Mô tả tình trạng & khuyết điểm thực tế:</strong>
            <div class="p-2 border bg-white small mt-1" style="max-height: 160px; overflow-y: auto; line-height: 1.6; white-space: pre-line;">${pDesc}</div>
          </div>
          <hr class="my-2">
          <div class="small text-muted">
            <div><strong>Người bán:</strong> ${pSellerName} | <strong>SĐT:</strong> ${pSellerPhone}</div>
            <div><strong>Khu vực:</strong> ${pSellerAddress} | <strong>Thời gian gửi:</strong> ${pCreatedAt}</div>
          </div>
        </div>
      </div>
    `;

    let footerHtml = '<button type="button" class="btn btn-secondary" data-dismiss="modal">Đóng</button>';
    if (pStatus === "pending") {
      footerHtml += `
        <button type="button" class="btn btn-outline-dark" onclick="$('#reviewDetailModal').modal('hide'); openRejectModal(${pId});">
          Từ Chối Tin
        </button>
        <button type="button" class="btn btn-dark" onclick="$('#reviewDetailModal').modal('hide'); handleApprove(${pId});">
          Duyệt Mở Bán
        </button>
      `;
    } else if (pStatus === "approved") {
      footerHtml = '<span class="badge badge-secondary p-2 mr-auto">Tin này đã được duyệt mở bán</span>' + footerHtml;
    } else if (pStatus === "rejected") {
      footerHtml = '<span class="badge badge-secondary p-2 mr-auto">Tin này đã bị từ chối</span>' + footerHtml;
    }

    $("#review-modal-body").html(html);
    $("#review-modal-footer").html(footerHtml);
    $("#reviewDetailModal").modal("show");
  }
</script>
}
```

---

## Views/Admin/Products.cshtml

```html
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "Quản Lý Tin Đăng Đồ Cũ";
}

<div class="page-title-box d-flex justify-content-between align-items-center mb-3">
  <div>
    <h1 class="page-title">Quản Lý Tin Đăng</h1>
    <div class="page-breadcrumb">Quản lý danh sách các món đồ cũ đang mở bán trên sàn, chỉnh sửa hoặc gỡ tin vi phạm</div>
  </div>
  <div>
    <button class="btn btn-dark" onclick="openAddProductModal()">
      + Thêm Tin Đăng Mới
    </button>
  </div>
</div>

<!-- Bộ lọc & Tìm kiếm hàng hóa -->
<div class="card p-3 mb-3 bg-light border">
  <div class="row align-items-center">
    <div class="col-md-5 mb-2 mb-md-0">
      <input type="text" id="admin-prod-search" class="form-control" placeholder="Tìm theo tên sản phẩm, mã món đồ..." onkeyup="filterAdminProducts()">
    </div>
    <div class="col-md-4 mb-2 mb-md-0">
      <select id="admin-prod-cat-filter" class="form-control" onchange="filterAdminProducts()">
        <option value="all">Tất cả chủng loại / danh mục</option>
      </select>
    </div>
    <div class="col-md-3 text-md-right">
      <span class="badge badge-secondary p-2" id="prod-total-count-badge">Tổng: 0 món</span>
    </div>
  </div>
</div>

<!-- Bảng danh sách hàng hóa -->
<div class="card mb-4">
  <div class="table-responsive">
    <table class="table table-bordered table-striped mb-0" style="font-size: 13px;">
      <thead class="thead-light">
        <tr>
          <th style="width: 50px;">ID</th>
          <th style="width: 60px;">Ảnh</th>
          <th>Tên Món Đồ Cũ</th>
          <th>Chủng Loại</th>
          <th>Giá Bán</th>
          <th class="text-center">Tồn Kho</th>
          <th>Người Bán</th>
          <th class="text-center" style="width: 170px;">Thao Tác</th>
        </tr>
      </thead>
      <tbody id="admin-products-tbody">
        <!-- Render bởi JS -->
      </tbody>
    </table>
  </div>
</div>

<!-- MODAL THÊM / SỬA SẢN PHẨM -->
<div class="modal fade" id="productModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-light">
        <h5 class="modal-title font-weight-bold text-dark" id="productModalTitle">Thêm Sản Phẩm Đồ Cũ</h5>
        <button type="button" class="close" data-dismiss="modal">&times;</button>
      </div>
      <form id="product-admin-form" onsubmit="handleSaveProduct(event)">
        <div class="modal-body p-3">
          <input type="hidden" id="edit-prod-id">

          <div class="form-group">
            <label for="prod-name" class="font-weight-bold">Tên sản phẩm *</label>
            <input type="text" class="form-control" id="prod-name" required>
          </div>

          <div class="form-row">
            <div class="form-group col-md-6">
              <label for="prod-cat" class="font-weight-bold">Danh mục *</label>
              <select class="form-control" id="prod-cat" required></select>
            </div>
            <div class="form-group col-md-6">
              <label for="prod-condition" class="font-weight-bold">Độ mới (%) *</label>
              <select class="form-control" id="prod-condition" required>
                <option value="99% - Như mới">99% - Như mới</option>
                <option value="95% - Rất mới">95% - Rất mới</option>
                <option value="90% - Tốt">90% - Tốt</option>
                <option value="85% - Khá">85% - Khá</option>
              </select>
            </div>
          </div>

          <div class="form-row">
            <div class="form-group col-md-4">
              <label for="prod-price" class="font-weight-bold">Giá thanh lý (₫) *</label>
              <input type="number" class="form-control" id="prod-price" required min="1000">
            </div>
            <div class="form-group col-md-4">
              <label for="prod-orig-price" class="font-weight-bold">Giá gốc mua mới (₫) *</label>
              <input type="number" class="form-control" id="prod-orig-price" required min="1000">
            </div>
            <div class="form-group col-md-4">
              <label for="prod-stock" class="font-weight-bold">Số lượng tồn kho *</label>
              <input type="number" class="form-control" id="prod-stock" required min="0">
            </div>
          </div>

          <div class="form-group">
            <label for="prod-image" class="font-weight-bold">Upload / Link URL hình ảnh *</label>
            <input type="url" class="form-control" id="prod-image" placeholder="https://..." required>
          </div>

          <div class="form-group">
            <label for="prod-cond-details" class="font-weight-bold">Ghi chú khuyết điểm & phụ kiện</label>
            <textarea class="form-control" id="prod-cond-details" rows="2" placeholder="Tình trạng thực tế..."></textarea>
          </div>

          <div class="form-group">
            <label for="prod-desc" class="font-weight-bold">Mô tả sản phẩm</label>
            <textarea class="form-control" id="prod-desc" rows="3" placeholder="Mô tả chi tiết..."></textarea>
          </div>

          <div class="form-row">
            <div class="form-group col-md-6">
              <label for="prod-seller" class="font-weight-bold">Người bán</label>
              <input type="text" class="form-control" id="prod-seller" value="Shop Hệ Thống">
            </div>
            <div class="form-group col-md-6">
              <label for="prod-seller-phone" class="font-weight-bold">Số điện thoại</label>
              <input type="text" class="form-control" id="prod-seller-phone" value="19006868">
            </div>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy</button>
          <button type="submit" class="btn btn-dark">
            Lưu Sản Phẩm
          </button>
        </div>
      </form>
    </div>
  </div>
</div>

<!-- MODAL XEM CHI TIẾT SẢN PHẨM -->
<div class="modal fade" id="productDetailModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-light">
        <h5 class="modal-title font-weight-bold text-dark">Chi Tiết Sản Phẩm Đồ Cũ</h5>
        <button type="button" class="close" data-dismiss="modal">&times;</button>
      </div>
      <div class="modal-body p-3" id="prod-detail-view-content"></div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Đóng</button>
      </div>
    </div>
  </div>
</div>

@section Scripts {
<script>
  $(document).ready(function() {
    loadCategoriesDropdown();
    renderAdminProductsTable();
  });

  function loadCategoriesDropdown() {
    const cats = ThriftDB.getCategories();
    const filterSelect = $("#admin-prod-cat-filter");
    const modalSelect = $("#prod-cat");
    modalSelect.empty();

    cats.forEach(c => {
      filterSelect.append(`<option value="${c.id}">${c.name}</option>`);
      modalSelect.append(`<option value="${c.id}">${c.name}</option>`);
    });
  }

  function renderAdminProductsTable(items) {
    const products = items || ThriftDB.getProducts();
    $("#prod-total-count-badge").text(`Tổng: ${products.length} món`);

    const tbody = $("#admin-products-tbody");
    tbody.empty();

    if (products.length === 0) {
      tbody.append('<tr><td colspan="8" class="text-center text-muted py-4">Không tìm thấy sản phẩm nào!</td></tr>');
      return;
    }

    products.forEach(p => {
      let stockBadge = p.stock > 0 
        ? `<span class="badge badge-secondary">${p.stock}</span>` 
        : `<span class="badge badge-light border text-muted">Hết hàng</span>`;

      tbody.append(`
        <tr>
          <td><strong class="text-muted">#${p.id}</strong></td>
          <td>
            <img src="${p.image}" alt="${p.name}" style="width: 44px; height: 44px; object-fit: cover; border: 1px solid #ccc;" onerror="this.src='https://images.unsplash.com/photo-1544816155-12df9643f363?w=100'">
          </td>
          <td>
            <strong class="d-block text-dark">${p.name}</strong>
            ${p.condition ? `<small class="text-muted"><span class="badge badge-light border">${p.condition}</span></small>` : ''}
          </td>
          <td><span class="badge badge-light border">${p.categoryName}</span></td>
          <td><strong>${ThriftDB.formatCurrency(p.price)}</strong></td>
          <td class="text-center">${stockBadge}</td>
          <td><small>${p.sellerName}</small></td>
          <td class="text-center text-nowrap">
            <button class="btn btn-outline-dark btn-sm mr-1" onclick="viewProductDetailAdmin(${p.id})">
              Xem
            </button>
            <button class="btn btn-outline-dark btn-sm mr-1" onclick="openEditProductModal(${p.id})">
              Sửa
            </button>
            <button class="btn btn-outline-dark btn-sm" onclick="deleteProduct(${p.id})">
              Xóa
            </button>
          </td>
        </tr>
      `);
    });
  }

  function filterAdminProducts() {
    const keyword = $("#admin-prod-search").val().trim().toLowerCase();
    const catId = $("#admin-prod-cat-filter").val();
    const all = ThriftDB.getProducts();

    const filtered = all.filter(p => {
      const matchName = p.name.toLowerCase().includes(keyword) || String(p.id).includes(keyword);
      const matchCat = catId === "all" || p.categoryId === parseInt(catId);
      return matchName && matchCat;
    });

    renderAdminProductsTable(filtered);
  }

  function openAddProductModal() {
    $("#productModalTitle").text("Thêm Món Đồ Cũ Mới");
    $("#edit-prod-id").val("");
    $("#product-admin-form")[0].reset();
    $("#prod-image").val("https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600&q=80");
    $("#productModal").modal("show");
  }

  function openEditProductModal(id) {
    const p = ThriftDB.getProductById(id);
    if (!p) return;

    $("#productModalTitle").text("Chỉnh Sửa Sản Phẩm #" + p.id);
    $("#edit-prod-id").val(p.id);
    $("#prod-name").val(p.name);
    $("#prod-cat").val(p.categoryId);
    $("#prod-condition").val(p.condition);
    $("#prod-price").val(p.price);
    $("#prod-orig-price").val(p.originalPrice);
    $("#prod-stock").val(p.stock);
    $("#prod-image").val(p.image);
    $("#prod-cond-details").val(p.conditionDetails || "");
    $("#prod-desc").val(p.description || "");
    $("#prod-seller").val(p.sellerName || "Shop Hệ Thống");
    $("#prod-seller-phone").val(p.sellerPhone || "19006868");

    $("#productModal").modal("show");
  }

  function handleSaveProduct(e) {
    e.preventDefault();
    const editId = $("#edit-prod-id").val();
    const catId = parseInt($("#prod-cat").val());
    const catObj = ThriftDB.getCategoryById(catId);
    const name = $("#prod-name").val().trim();
    const condition = $("#prod-condition").val();
    const price = parseInt($("#prod-price").val());
    const origPrice = parseInt($("#prod-orig-price").val());
    const stock = parseInt($("#prod-stock").val());
    const image = $("#prod-image").val().trim();
    const condDetails = $("#prod-cond-details").val().trim();
    const desc = $("#prod-desc").val().trim();
    const seller = $("#prod-seller").val().trim();
    const phone = $("#prod-seller-phone").val().trim();

    let conditionPercent = 90;
    if (condition.includes("99%")) conditionPercent = 99;
    else if (condition.includes("95%")) conditionPercent = 95;
    else if (condition.includes("85%")) conditionPercent = 85;

    const prods = ThriftDB.getProducts();

    if (editId) {
      // Cập nhật
      const p = prods.find(item => item.id === parseInt(editId));
      if (p) {
        p.name = name;
        p.categoryId = catId;
        p.categoryName = catObj ? catObj.name : "Đồ cũ";
        p.condition = condition;
        p.conditionPercent = conditionPercent;
        p.price = price;
        p.originalPrice = origPrice;
        p.stock = stock;
        p.image = image;
        p.conditionDetails = condDetails;
        p.description = desc;
        p.sellerName = seller;
        p.sellerPhone = phone;
      }
    } else {
      // Thêm mới
      const newP = {
        id: Date.now(),
        name: name,
        categoryId: catId,
        categoryName: catObj ? catObj.name : "Đồ cũ",
        condition: condition,
        conditionPercent: conditionPercent,
        price: price,
        originalPrice: origPrice,
        stock: stock,
        soldCount: 0,
        image: image,
        conditionDetails: condDetails,
        description: desc,
        sellerName: seller,
        sellerPhone: phone,
        sellerAddress: "Kho Quản Trị Hệ Thống",
        status: "approved",
        featured: false,
        createdAt: new Date().toLocaleString("vi-VN")
      };
      prods.unshift(newP);
    }

    ThriftDB.saveProducts(prods);
    $("#productModal").modal("hide");
    alert("Đã lưu thông tin sản phẩm thành công!");
    renderAdminProductsTable();
  }

  function deleteProduct(id) {
    if (confirm("Bạn có chắc chắn muốn xóa sản phẩm đồ cũ này khỏi hệ thống?")) {
      let prods = ThriftDB.getProducts();
      prods = prods.filter(p => p.id !== parseInt(id));
      ThriftDB.saveProducts(prods);
      alert("Đã xóa sản phẩm thành công!");
      renderAdminProductsTable();
    }
  }

  // Chi tiết sản phẩm trong Admin
  function viewProductDetailAdmin(id) {
    const p = (typeof ThriftDB !== 'undefined' ? ThriftDB.getProductById(id) : null);
    if (!p) return;

    const pName = p.Name || p.name || 'Món đồ cũ';
    const pImg = p.Image || p.image || 'https://images.unsplash.com/photo-1544816155-12df9643f363?w=300';
    const pCategory = p.CategoryName || p.categoryName || 'Đồ cũ';
    const pPrice = Number(p.Price || p.price) || 0;
    const pOrigPrice = Number(p.OriginalPrice || p.originalPrice) || 0;
    const pCondition = p.Condition || p.condition || '95%';
    const pStock = p.Stock || p.stock || 1;
    const pConditionDetails = p.ConditionDetails || p.conditionDetails || "Không có ghi chú khuyết điểm";
    const pDescription = p.Description || p.description || "Không có mô tả thêm";
    const pSellerName = p.SellerName || p.sellerName || 'Thành viên';
    const pSellerPhone = p.SellerPhone || p.sellerPhone || 'Chưa cập nhật';
    const pSellerAddress = p.SellerAddress || p.sellerAddress || "Chưa cập nhật";
    const formatVND = num => (Number(num) || 0).toLocaleString('vi-VN') + ' ₫';

    const html = `
      <div class="row">
        <div class="col-md-5 text-center mb-3">
          <img src="${pImg}" class="img-fluid border" style="max-height: 240px; width: 100%; object-fit: cover;" onerror="this.src='https://images.unsplash.com/photo-1544816155-12df9643f363?w=300'">
        </div>
        <div class="col-md-7">
          <div><span class="badge badge-light border mb-1">${pCategory}</span></div>
          <h5 class="font-weight-bold text-dark mb-2">${pName}</h5>
          <div class="p-2 bg-light border mb-2">
            <strong>${formatVND(pPrice)}</strong> 
            ${pOrigPrice > pPrice ? `<small class="text-muted ml-2" style="text-decoration: line-through;">${formatVND(pOrigPrice)}</small>` : ''}
          </div>
          <div class="mb-2"><strong>Độ mới:</strong> <span class="badge badge-secondary">${pCondition}</span> | <strong>Tồn kho:</strong> <span class="badge badge-secondary">${pStock}</span></div>
          <div class="mb-2"><strong>Tình trạng & Khuyết điểm:</strong> <div class="p-2 border bg-white small">${pConditionDetails}</div></div>
          <div class="mb-2"><strong>Mô tả:</strong> <div class="p-2 border bg-white small">${pDescription}</div></div>
          <hr class="my-2">
          <div class="small text-muted">
            <strong>Người đăng:</strong> ${pSellerName} (${pSellerPhone})<br>
            <strong>Địa chỉ người bán:</strong> ${pSellerAddress}
          </div>
        </div>
      </div>
    `;

    $("#prod-detail-view-content").html(html);
    $("#productDetailModal").modal("show");
  }
</script>
}
```

---

## Views/Admin/Categories.cshtml

```html
@model IEnumerable<ThriftShop.Models.Category>
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "Quản Lý Danh Mục";
    var productCounts = ViewBag.ProductCounts as Dictionary<int, int> ?? new Dictionary<int, int>();
    var totalCats = Model?.Count() ?? 0;
}

<div class="page-title-box d-flex justify-content-between align-items-center mb-3">
  <div>
    <h1 class="page-title">Quản Lý Danh Mục</h1>
    <div class="page-breadcrumb">Phân loại danh mục hàng hóa đồ cũ và thống kê số lượng sản phẩm</div>
  </div>
  <div>
    <button class="btn btn-dark" onclick="openAddCatModal()">
      + Thêm Danh Mục Mới
    </button>
  </div>
</div>

<div class="card mb-4">
  <div class="card-header bg-light d-flex justify-content-between align-items-center py-2">
    <strong class="text-dark">Danh Sách Danh Mục Hiện Có (@totalCats danh mục)</strong>
    <span class="small text-muted">Chuẩn giáo trình LTW</span>
  </div>
  <div class="table-responsive">
    <table class="table table-bordered table-striped mb-0" style="font-size: 13px;">
      <thead class="thead-light">
        <tr>
          <th style="width: 70px;">Mã</th>
          <th style="width: 65px;" class="text-center">Biểu tượng</th>
          <th>Tên Danh Mục Đồ Cũ</th>
          <th>Mô Tả Danh Mục</th>
          <th class="text-center" style="width: 140px;">Tổng Hàng Hóa</th>
          <th class="text-center" style="width: 150px;">Thao Tác</th>
        </tr>
      </thead>
      <tbody id="categories-tbody">
        @if (Model != null && Model.Any())
        {
          foreach (var c in Model)
          {
            var count = productCounts.ContainsKey(c.Id) ? productCounts[c.Id] : 0;
            <tr>
              <td><strong>#@c.Id</strong></td>
              <td class="text-center">
                <i class="@(string.IsNullOrEmpty(c.Icon) ? "fas fa-tag" : c.Icon)"></i>
              </td>
              <td><strong>@c.Name</strong></td>
              <td><span class="text-muted">@(string.IsNullOrEmpty(c.Description) ? "Không có mô tả" : c.Description)</span></td>
              <td class="text-center">
                <span class="badge badge-secondary px-2 py-1">@count món</span>
              </td>
              <td class="text-center text-nowrap">
                <button class="btn btn-outline-dark btn-sm mr-1" onclick="openEditCatModal(@c.Id, '@c.Name', '@c.Icon', '@(c.Description ?? "")')">
                  Sửa
                </button>
                <button class="btn btn-outline-dark btn-sm" onclick="deleteCategory(@c.Id, @count, '@c.Name')">
                  Xóa
                </button>
              </td>
            </tr>
          }
        }
        else
        {
          <tr>
            <td colspan="6" class="text-center py-4 text-muted">Chưa có danh mục nào trong hệ thống!</td>
          </tr>
        }
      </tbody>
    </table>
  </div>
</div>

<!-- MODAL THÊM / SỬA DANH MỤC -->
<div class="modal fade" id="categoryModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-light">
        <h5 class="modal-title font-weight-bold text-dark" id="catModalTitle">Thêm Danh Mục Mới</h5>
        <button type="button" class="close" data-dismiss="modal">&times;</button>
      </div>
      <form id="category-form" onsubmit="handleSaveCat(event)">
        <div class="modal-body">
          <input type="hidden" id="edit-cat-id" name="Id" value="0">
          <div class="form-group">
            <label for="cat-name" class="font-weight-bold text-dark">Tên danh mục đồ cũ *</label>
            <input type="text" class="form-control" id="cat-name" name="Name" placeholder="Ví dụ: Đồ gốm & Sưu tầm" minlength="2" maxlength="100" required>
          </div>
          <div class="form-group">
            <label for="cat-icon" class="font-weight-bold text-dark">Icon font class *</label>
            <input type="text" class="form-control" id="cat-icon" name="Icon" placeholder="fas fa-tag" value="fas fa-box" required>
            <small class="text-muted">Ví dụ: fas fa-tshirt, fas fa-laptop, fas fa-book, fas fa-couch, fas fa-tag...</small>
          </div>
          <div class="form-group">
            <label for="cat-desc" class="font-weight-bold text-dark">Mô tả ngắn</label>
            <textarea class="form-control" id="cat-desc" name="Description" rows="2" placeholder="Mô tả các món đồ cũ thuộc loại này..."></textarea>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy</button>
          <button type="submit" id="btnSaveCat" class="btn btn-dark">
            Lưu Danh Mục
          </button>
        </div>
      </form>
    </div>
  </div>
</div>

@section Scripts {
<script>
  function openAddCatModal() {
    $("#catModalTitle").text("Thêm Danh Mục Đồ Cũ Mới");
    $("#edit-cat-id").val("0");
    $("#category-form")[0].reset();
    $("#cat-icon").val("fas fa-box");
    $("#categoryModal").modal("show");
  }

  function openEditCatModal(id, name, icon, desc) {
    $("#catModalTitle").text("Chỉnh Sửa Danh Mục #" + id);
    $("#edit-cat-id").val(id);
    $("#cat-name").val(name);
    $("#cat-icon").val(icon || "fas fa-tag");
    $("#cat-desc").val(desc || "");
    $("#categoryModal").modal("show");
  }

  function handleSaveCat(e) {
    e.preventDefault();
    const btn = $("#btnSaveCat");
    btn.prop("disabled", true).text("Đang lưu...");

    const formData = new FormData(document.getElementById("category-form"));

    fetch('/Admin/SaveCategory', {
      method: 'POST',
      body: formData
    })
    .then(r => r.json())
    .then(res => {
      btn.prop("disabled", false).text("Lưu Danh Mục");
      if (res.success) {
        $("#categoryModal").modal("hide");
        alert("THÀNH CÔNG!\n\n" + (res.message || "Đã lưu danh mục thành công!"));
        location.reload();
      } else {
        alert("LỖI: " + (res.message || "Không thể lưu danh mục!"));
      }
    })
    .catch(err => {
      btn.prop("disabled", false).text("Lưu Danh Mục");
      alert("Lỗi kết nối máy chủ khi lưu danh mục!");
    });
  }

  // Kiểm tra ràng buộc khóa ngoại (Constraint) trước khi xóa danh mục theo checklist giáo trình
  function deleteCategory(id, count, name) {
    if (count > 0) {
      alert(`KHÔNG THỂ XÓA!\n\nDanh mục "${name}" hiện đang có ${count} sản phẩm đồ cũ thuộc về nó.\n\nVui lòng chuyển hoặc xóa các sản phẩm liên quan trước khi xóa danh mục để tránh lỗi toàn vẹn dữ liệu (Foreign Key Constraint)!`);
      return;
    }

    if (confirm(`Bạn có chắc chắn muốn xóa vĩnh viễn danh mục "${name}"?`)) {
      fetch('/Admin/DeleteCategory?id=' + id, { method: 'POST' })
        .then(r => r.json())
        .then(res => {
          if (res.success) {
            alert("THÀNH CÔNG!\n\nĐã xóa danh mục thành công.");
            location.reload();
          } else {
            alert(res.message || "Lỗi khi xóa danh mục!");
          }
        })
        .catch(err => {
          alert("Lỗi kết nối máy chủ khi xóa danh mục!");
        });
    }
  }
</script>
}
```

---

## Views/Admin/Orders.cshtml

```html
@model IEnumerable<ThriftShop.Models.Order>
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "Quản Lý Đơn Hàng";
    var serializerOpts = new System.Text.Json.JsonSerializerOptions
    {
        ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles,
        PropertyNamingPolicy = System.Text.Json.JsonNamingPolicy.CamelCase
    };
    var ordersJson = System.Text.Json.JsonSerializer.Serialize(Model ?? Enumerable.Empty<ThriftShop.Models.Order>(), serializerOpts);
}

<div class="page-title-box d-flex justify-content-between align-items-center">
  <div>
    <h1 class="page-title">Quản Lý Đơn Hàng</h1>
    <div class="page-breadcrumb">Theo dõi, cập nhật tiến độ giao hàng và in hóa đơn bán lẻ</div>
  </div>
  <div>
    <span class="badge badge-info p-2 font-weight-bold" style="font-size: 13px;" id="orders-total-badge">
      Tổng số đơn: 0
    </span>
  </div>
</div>

<!-- KHỐI TÌM KIẾM ĐƠN HÀNG (THEO SLIDE 3 KTRA BE) -->
<div class="card border-0 shadow-sm p-3 mb-4 bg-white">
  <div class="row align-items-center">
    <div class="col-md-4 mb-2 mb-md-0">
      <label class="small font-weight-bold text-muted mb-1">Tìm theo Tên khách / Mã đơn:</label>
      <input type="text" id="admin-order-search" class="form-control form-control-sm" placeholder="Nhập tên khách hàng..." onkeyup="filterAdminOrders()">
    </div>
    <div class="col-md-3 mb-2 mb-md-0">
      <label class="small font-weight-bold text-muted mb-1">Trạng thái đơn hàng:</label>
      <select id="admin-order-status-filter" class="form-control form-control-sm" onchange="filterAdminOrders()">
        <option value="all">Tất cả trạng thái</option>
        <option value="pending">Chờ duyệt đơn</option>
        <option value="processing">Đang đóng gói xử lý</option>
        <option value="shipping">Đang giao hàng</option>
        <option value="delivered">Đã giao thành công</option>
        <option value="cancelled">Đã hủy đơn</option>
      </select>
    </div>
    <div class="col-md-3 mb-2 mb-md-0">
      <label class="small font-weight-bold text-muted mb-1">Trạng thái thanh toán:</label>
      <select id="admin-order-payment-filter" class="form-control form-control-sm" onchange="filterAdminOrders()">
        <option value="all">Tất cả thanh toán</option>
        <option value="Chưa thanh toán">Chưa thanh toán (COD)</option>
        <option value="Đã thanh toán">Đã thanh toán</option>
      </select>
    </div>
    <div class="col-md-2 text-md-right mt-3 mt-md-0">
      <button class="btn btn-outline-secondary btn-sm btn-block" onclick="resetOrderFilter()">
        <i class="fas fa-undo mr-1"></i>Đặt lại
      </button>
    </div>
  </div>
</div>

<!-- BẢNG DANH SÁCH ĐƠN HÀNG -->
<div class="admin-card">
  <div class="table-responsive">
    <table class="table admin-table mb-0">
      <thead>
        <tr>
          <th>Mã Đơn</th>
          <th>Khách Hàng</th>
          <th>Số Điện Thoại</th>
          <th>Ngày Đặt</th>
          <th>Thanh Toán</th>
          <th>Tổng Tiền</th>
          <th class="text-center">Trạng Thái Đơn</th>
          <th class="text-center" style="width: 170px;">Thao Tác</th>
        </tr>
      </thead>
      <tbody id="admin-orders-tbody">
        <!-- Render bởi JS -->
      </tbody>
    </table>
  </div>
</div>

<!-- MODAL CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG -->
<div class="modal fade" id="updateStatusModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-primary text-white">
        <h5 class="modal-title font-weight-bold"><i class="fas fa-edit mr-2"></i>Cập Nhật Trạng Thái Đơn Hàng</h5>
        <button type="button" class="close text-white" data-dismiss="modal">&times;</button>
      </div>
      <div class="modal-body p-4">
        <input type="hidden" id="status-order-id">
        <div class="mb-3">
          Đơn hàng: <strong class="text-danger h5 mb-0" id="status-order-code">DH100234</strong>
        </div>
        <div class="form-group">
          <label for="new-order-status" class="font-weight-bold">Chọn trạng thái mới <span class="text-danger">*</span></label>
          <select class="form-control" id="new-order-status">
            <option value="pending">Chờ duyệt đơn</option>
            <option value="processing">Đang đóng gói xử lý</option>
            <option value="shipping">Đang vận chuyển giao hàng</option>
            <option value="delivered">Đã giao hàng thành công</option>
            <option value="cancelled">Đã hủy đơn hàng</option>
          </select>
        </div>
        <div class="form-group mb-0">
          <label for="new-payment-status" class="font-weight-bold">Trạng thái thanh toán:</label>
          <select class="form-control" id="new-payment-status">
            <option value="Chưa thanh toán">Chưa thanh toán</option>
            <option value="Đã thanh toán">Đã thanh toán</option>
          </select>
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy</button>
        <button type="button" class="btn btn-primary font-weight-bold" onclick="saveNewOrderStatus()">
          <i class="fas fa-check mr-1"></i>Lưu Thay Đổi
        </button>
      </div>
    </div>
  </div>
</div>

<!-- MODAL XEM CHI TIẾT HÓA ĐƠN BÁN LẺ -->
<div class="modal fade" id="adminInvoiceModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-dark text-white">
        <h5 class="modal-title font-weight-bold"><i class="fas fa-file-invoice-dollar mr-2 text-warning"></i>Chi Tiết Đơn Hàng & Hóa Đơn Bán Lẻ</h5>
        <button type="button" class="close text-white" data-dismiss="modal">&times;</button>
      </div>
      <div class="modal-body p-4" id="admin-invoice-content"></div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Đóng</button>
        <button type="button" class="btn btn-primary" onclick="window.print()">
          <i class="fas fa-print mr-1"></i>In Hóa Đơn
        </button>
      </div>
    </div>
  </div>
</div>

@section Scripts {
<script>
  const serverOrders = @Html.Raw(ordersJson);

  $(document).ready(function() {
    renderAdminOrdersTable();
  });

  function renderAdminOrdersTable(items) {
    const orders = items || (serverOrders && serverOrders.length > 0 ? serverOrders : ((typeof ThriftDB !== 'undefined') ? ThriftDB.getOrders() : []));
    $("#orders-total-badge").text(`Tổng số đơn: ${orders.length}`);

    const tbody = $("#admin-orders-tbody");
    tbody.empty();

    if (orders.length === 0) {
      tbody.append('<tr><td colspan="8" class="text-center text-muted py-4">Không tìm thấy đơn hàng nào phù hợp!</td></tr>');
      return;
    }

    orders.forEach(o => {
      let badgeClass = "badge-warning";
      let statusText = o.orderStatusText || "Chờ duyệt";
      if (o.orderStatus === "processing") {
        badgeClass = "badge-info";
        statusText = "Đang xử lý";
      } else if (o.orderStatus === "shipping") {
        badgeClass = "badge-primary";
        statusText = "Đang giao";
      } else if (o.orderStatus === "delivered") {
        badgeClass = "badge-success";
        statusText = "Đã giao";
      } else if (o.orderStatus === "cancelled") {
        badgeClass = "badge-danger";
        statusText = "Đã hủy";
      }

      let payBadge = o.paymentStatus === "Đã thanh toán" 
        ? '<span class="badge badge-success">Đã thanh toán</span>' 
        : '<span class="badge badge-secondary">Chưa thanh toán</span>';

      tbody.append(`
        <tr>
          <td><strong class="text-success">${o.id}</strong></td>
          <td><strong>${o.customerName}</strong></td>
          <td><small>${o.customerPhone}</small></td>
          <td><small class="text-muted">${o.orderDate}</small></td>
          <td>${payBadge}</td>
          <td class="font-weight-bold text-danger">${ThriftDB.formatCurrency(o.totalAmount)}</td>
          <td class="text-center">
            <span class="badge ${badgeClass} px-2 py-1">${statusText}</span>
          </td>
          <td class="text-center text-nowrap">
            <button class="btn btn-outline-info btn-sm px-2 mr-1" onclick="viewOrderInvoiceAdmin('${o.id}')" title="Xem hóa đơn chi tiết">
              <i class="fas fa-file-invoice"></i> Chi tiết
            </button>
            <button class="btn btn-outline-primary btn-sm px-2" onclick="openUpdateStatusModal('${o.id}')" title="Cập nhật trạng thái">
              <i class="fas fa-sync-alt"></i> Đổi TT
            </button>
          </td>
        </tr>
      `);
    });
  }

  function filterAdminOrders() {
    const keyword = $("#admin-order-search").val().trim().toLowerCase();
    const status = $("#admin-order-status-filter").val();
    const pay = $("#admin-order-payment-filter").val();
    const allOrders = ThriftDB.getOrders();

    const filtered = allOrders.filter(o => {
      const matchName = o.customerName.toLowerCase().includes(keyword) || o.id.toLowerCase().includes(keyword);
      const matchStatus = status === "all" || o.orderStatus === status;
      const matchPay = pay === "all" || (o.paymentStatus && o.paymentStatus.includes(pay));
      return matchName && matchStatus && matchPay;
    });

    renderAdminOrdersTable(filtered);
  }

  function resetOrderFilter() {
    $("#admin-order-search").val("");
    $("#admin-order-status-filter").val("all");
    $("#admin-order-payment-filter").val("all");
    renderAdminOrdersTable();
  }

  function openUpdateStatusModal(orderId) {
    const order = ThriftDB.getOrderById(orderId);
    if (!order) return;

    $("#status-order-id").val(order.id);
    $("#status-order-code").text(order.id);
    $("#new-order-status").val(order.orderStatus);
    $("#new-payment-status").val(order.paymentStatus || "Chưa thanh toán");
    $("#updateStatusModal").modal("show");
  }

  function saveNewOrderStatus() {
    const orderId = $("#status-order-id").val();
    const newStatus = $("#new-order-status").val();
    const newPay = $("#new-payment-status").val();

    const orders = ThriftDB.getOrders();
    const order = orders.find(o => o.id === orderId);
    if (order) {
      order.orderStatus = newStatus;
      if (newStatus === "pending") order.orderStatusText = "Chờ duyệt đơn";
      else if (newStatus === "processing") order.orderStatusText = "Đang đóng gói xử lý";
      else if (newStatus === "shipping") order.orderStatusText = "Đang giao hàng";
      else if (newStatus === "delivered") order.orderStatusText = "Đã giao hàng thành công";
      else if (newStatus === "cancelled") order.orderStatusText = "Đã hủy đơn";

      order.paymentStatus = newPay;
      ThriftDB.saveOrders(orders);
      $("#updateStatusModal").modal("hide");
      alert("Đã cập nhật trạng thái đơn hàng thành công!");
      renderAdminOrdersTable();
    }
  }

  // Hiển thị hóa đơn bán lẻ chi tiết
  function viewOrderInvoiceAdmin(orderId) {
    const order = ThriftDB.getOrderById(orderId);
    if (!order) return;

    let itemsHtml = "";
    order.items.forEach((it, idx) => {
      itemsHtml += `
        <tr>
          <td class="text-center">${idx + 1}</td>
          <td>
            <strong>${it.productName}</strong><br>
            ${it.condition ? `<small class="text-muted">Tình trạng: ${it.condition}</small>` : ''}
          </td>
          <td class="text-center">${ThriftDB.formatCurrency(it.price)}</td>
          <td class="text-center">${it.quantity}</td>
          <td class="text-right font-weight-bold text-danger">${ThriftDB.formatCurrency(it.subtotal)}</td>
        </tr>
      `;
    });

    const modalHtml = `
      <div class="invoice-box">
        <div class="invoice-header d-flex justify-content-between align-items-center">
          <div>
            <div class="invoice-company-title"><i class="fas fa-recycle mr-2"></i>ThriftShop Sài Gòn</div>
            <small class="text-muted">Hệ Thống Mua Bán Đồ Cũ Secondhand Kiểm Duyệt</small>
          </div>
          <div class="text-right">
            <h5 class="font-weight-bold text-dark mb-0">HÓA ĐƠN BÁN HÀNG</h5>
            <small class="text-muted">Mã đơn: <strong>${order.id}</strong></small><br>
            <small class="text-muted">Thời gian: ${order.orderDate}</small>
          </div>
        </div>

        <div class="row my-3">
          <div class="col-sm-6">
            <h6 class="font-weight-bold text-success mb-1">Khách Hàng:</h6>
            <p class="small mb-1"><strong>Họ tên:</strong> ${order.customerName}</p>
            <p class="small mb-1"><strong>Điện thoại:</strong> ${order.customerPhone}</p>
            <p class="small mb-0"><strong>Email:</strong> ${order.customerEmail || "Không có"}</p>
          </div>
          <div class="col-sm-6 text-sm-right">
            <h6 class="font-weight-bold text-success mb-1">Địa Chỉ Nhận:</h6>
            <p class="small mb-1">${order.shippingAddress}</p>
            <p class="small mb-1"><strong>PTTT:</strong> ${order.paymentMethod}</p>
            <p class="small mb-0"><strong>Trạng thái:</strong> <span class="badge badge-success">${order.orderStatusText || order.orderStatus}</span></p>
          </div>
        </div>

        <table class="table table-bordered small">
          <thead class="bg-light">
            <tr>
              <th style="width: 5%;" class="text-center">STT</th>
              <th style="width: 45%;">Tên Món Đồ Cũ</th>
              <th class="text-center">Đơn Giá</th>
              <th class="text-center">SL</th>
              <th class="text-right">Thành Tiền</th>
            </tr>
          </thead>
          <tbody>
            ${itemsHtml}
          </tbody>
          <tfoot>
            <tr>
              <td colspan="4" class="text-right font-weight-bold">Tạm tính:</td>
              <td class="text-right font-weight-bold">${ThriftDB.formatCurrency(order.subtotal)}</td>
            </tr>
            <tr>
              <td colspan="4" class="text-right font-weight-bold">Phí ship:</td>
              <td class="text-right font-weight-bold text-success">${order.shippingFee === 0 ? "Miễn phí" : ThriftDB.formatCurrency(order.shippingFee)}</td>
            </tr>
            <tr class="bg-light">
              <td colspan="4" class="text-right font-weight-bold h6 mb-0">TỔNG TIỀN THANH TOÁN:</td>
              <td class="text-right font-weight-bold text-danger h6 mb-0">${ThriftDB.formatCurrency(order.totalAmount)}</td>
            </tr>
          </tfoot>
        </table>

        <div class="small text-muted font-italic">
          Ghi chú từ khách: ${order.notes || "Không có ghi chú"}
        </div>
      </div>
    `;

    $("#admin-invoice-content").html(modalHtml);
    $("#adminInvoiceModal").modal("show");
  }
</script>
}
```

---

## Views/Admin/Inventory.cshtml

```html
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "Quản Lý Kho Hàng";
}

<div class="page-title-box">
  <h1 class="page-title">Quản Lý Kho Hàng</h1>
  <div class="page-breadcrumb">Thống kê số lượng hàng còn tồn kho và các mặt hàng đã hết</div>
</div>

<!-- 2 THẺ THỐNG KÊ TỒN KHO -->
<div class="row mb-4">
  <div class="col-md-6">
    <div class="stat-card border-success">
      <div>
        <div class="stat-card-title text-success">Danh Sách Hàng Tồn (Số lượng > 0)</div>
        <div class="stat-card-value text-success" id="inv-in-stock-count">0 món</div>
        <small class="text-muted">Đang sẵn sàng mở bán trên sàn</small>
      </div>
      <div class="stat-card-icon icon-revenue">
        <i class="fas fa-check-circle"></i>
      </div>
    </div>
  </div>
  <div class="col-md-6">
    <div class="stat-card border-danger">
      <div>
        <div class="stat-card-title text-danger">Danh Sách Cần Nhập (Số lượng = 0)</div>
        <div class="stat-card-value text-danger" id="inv-out-stock-count">0 món</div>
        <small class="text-muted">Đã bán hết hoặc cần liên hệ ký gửi thêm</small>
      </div>
      <div class="stat-card-icon icon-pending">
        <i class="fas fa-exclamation-triangle"></i>
      </div>
    </div>
  </div>
</div>

<!-- TABS CHUYỂN ĐỔI TRẠNG THÁI TỒN KHO -->
<ul class="nav nav-tabs mb-4 font-weight-bold" id="inventory-tabs">
  <li class="nav-item">
    <a class="nav-link active text-success" href="javascript:void(0)" onclick="switchInventoryTab('in-stock', this)">
      <i class="fas fa-boxes mr-1"></i>Hàng Còn Tồn Kho (Tồn > 0)
    </a>
  </li>
  <li class="nav-item">
    <a class="nav-link text-dark" href="javascript:void(0)" onclick="switchInventoryTab('out-of-stock', this)">
      <i class="fas fa-dolly-flatbed mr-1 text-danger"></i>Hàng Hết / Cần Ký Gửi Thêm (Tồn = 0)
    </a>
  </li>
</ul>

<!-- BẢNG HÀNG TỒN KHO -->
<div class="admin-card">
  <div class="table-responsive">
    <table class="table admin-table mb-0">
      <thead>
        <tr>
          <th>Ảnh</th>
          <th>Tên Món Đồ Cũ</th>
          <th>Chủng Loại</th>
          <th>Giá Bán</th>
          <th class="text-center">Số Lượng Tồn</th>
          <th class="text-center">Đã Bán</th>
          <th>Người Ký Gửi / Đăng Bán</th>
          <th class="text-center" style="width: 140px;">Hành Động</th>
        </tr>
      </thead>
      <tbody id="inventory-tbody">
        <!-- Render bởi JS -->
      </tbody>
    </table>
  </div>
</div>

<!-- MODAL CẬP NHẬT SỐ LƯỢNG KHO (NHẬP THÊM HÀNG) -->
<div class="modal fade" id="restockModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-success text-white">
        <h5 class="modal-title font-weight-bold"><i class="fas fa-plus-circle mr-2"></i>Nhập Thêm Số Lượng Kho</h5>
        <button type="button" class="close text-white" data-dismiss="modal">&times;</button>
      </div>
      <div class="modal-body p-4">
        <input type="hidden" id="restock-prod-id">
        <p class="font-weight-bold text-dark" id="restock-prod-name">Tên sản phẩm</p>
        <div class="form-group">
          <label for="restock-qty-input" class="font-weight-bold">Số lượng tồn kho cập nhật:</label>
          <input type="number" class="form-control" id="restock-qty-input" min="0" value="1">
        </div>
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy</button>
        <button type="button" class="btn btn-success font-weight-bold" onclick="saveRestock()">
          <i class="fas fa-save mr-1"></i>Lưu Số Lượng
        </button>
      </div>
    </div>
  </div>
</div>

@section Scripts {
<script>
  let currentTab = "in-stock";

  $(document).ready(function() {
    renderInventory();
  });

  function switchInventoryTab(tab, el) {
    currentTab = tab;
    $("#inventory-tabs .nav-link").removeClass("active text-success").addClass("text-dark");
    $(el).addClass("active text-success").removeClass("text-dark");
    renderInventory();
  }

  function renderInventory() {
    const all = ThriftDB.getProducts();
    const inStockList = all.filter(p => p.stock > 0);
    const outStockList = all.filter(p => p.stock === 0);

    $("#inv-in-stock-count").text(`${inStockList.length} món`);
    $("#inv-out-stock-count").text(`${outStockList.length} món`);

    const listToRender = currentTab === "in-stock" ? inStockList : outStockList;
    const tbody = $("#inventory-tbody");
    tbody.empty();

    if (listToRender.length === 0) {
      tbody.append(`<tr><td colspan="8" class="text-center text-muted py-4">Không có món đồ nào trong danh mục này!</td></tr>`);
      return;
    }

    listToRender.forEach(p => {
      let stockBadge = p.stock > 0 
        ? `<span class="badge badge-success font-weight-bold px-3 py-1">${p.stock}</span>` 
        : `<span class="badge badge-danger font-weight-bold px-3 py-1">Hết hàng (=0)</span>`;

      tbody.append(`
        <tr>
          <td>
            <img src="${p.image}" alt="${p.name}" style="width: 48px; height: 48px; object-fit: cover; border-radius: 6px;" class="border" onerror="this.src='https://images.unsplash.com/photo-1544816155-12df9643f363?w=100'">
          </td>
          <td>
            <strong>${p.name}</strong><br>
            ${p.condition ? `<small class="text-muted"><span class="badge badge-light border">${p.condition}</span></small>` : ''}
          </td>
          <td><small class="badge badge-light border text-uppercase">${p.categoryName}</small></td>
          <td class="font-weight-bold text-danger">${ThriftDB.formatCurrency(p.price)}</td>
          <td class="text-center">${stockBadge}</td>
          <td class="text-center font-weight-bold text-muted">${p.soldCount || 0}</td>
          <td><small>${p.sellerName} (${p.sellerPhone})</small></td>
          <td class="text-center">
            <button class="btn btn-outline-success btn-sm font-weight-bold" onclick="openRestockModal(${p.id})">
              <i class="fas fa-edit mr-1"></i>Cập nhật SL
            </button>
          </td>
        </tr>
      `);
    });
  }

  function openRestockModal(id) {
    const p = ThriftDB.getProductById(id);
    if (!p) return;

    $("#restock-prod-id").val(p.id);
    $("#restock-prod-name").text(p.name + ` (Hiện tại: ${p.stock} món)`);
    $("#restock-qty-input").val(p.stock > 0 ? p.stock : 1);
    $("#restockModal").modal("show");
  }

  function saveRestock() {
    const id = $("#restock-prod-id").val();
    const qty = parseInt($("#restock-qty-input").val());
    if (isNaN(qty) || qty < 0) {
      alert("Vui lòng nhập số lượng hợp lệ (>= 0)!");
      return;
    }

    const prods = ThriftDB.getProducts();
    const p = prods.find(item => item.id === parseInt(id));
    if (p) {
      p.stock = qty;
      ThriftDB.saveProducts(prods);
      $("#restockModal").modal("hide");
      alert("Đã cập nhật số lượng tồn kho thành công!");
      renderInventory();
    }
  }
</script>
}
```

---

## Views/Admin/Users.cshtml

```html
@{
    Layout = "_AdminLayout";
    ViewData["Title"] = "Quản Lý User";
}

<div class="page-title-box d-flex justify-content-between align-items-center mb-3">
  <div>
    <h1 class="page-title">Quản Lý Người Dùng</h1>
    <div class="page-breadcrumb">Danh sách tài khoản quản trị và thành viên người dùng trên sàn</div>
  </div>
  <div id="btn-add-admin-wrapper">
    <button class="btn btn-dark" onclick="openAddAdminModal()">
      + Thêm Quản Trị Viên Mới
    </button>
  </div>
</div>

<!-- TABS PHÂN LOẠI NGƯỜI DÙNG -->
<ul class="nav nav-tabs mb-3 font-weight-bold" id="user-tabs">
  <li class="nav-item">
    <a class="nav-link active" href="javascript:void(0)" onclick="switchUserTab('admins', this)">
      Tài Khoản Quản Trị (Admin)
    </a>
  </li>
  <li class="nav-item">
    <a class="nav-link" href="javascript:void(0)" onclick="switchUserTab('customers', this)">
      Danh Sách Khách Hàng
    </a>
  </li>
</ul>

<!-- BẢNG DANH SÁCH NGƯỜI DÙNG -->
<div class="card mb-4">
  <div class="table-responsive">
    <table class="table table-bordered table-striped mb-0" style="font-size: 13px;">
      <thead class="thead-light">
        <tr id="users-table-header">
          <!-- Render bởi JS -->
        </tr>
      </thead>
      <tbody id="users-tbody">
        <!-- Render bởi JS -->
      </tbody>
    </table>
  </div>
</div>

<!-- MODAL THÊM / SỬA TÀI KHOẢN ADMIN -->
<div class="modal fade" id="adminAccountModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-light">
        <h5 class="modal-title font-weight-bold text-dark" id="adminModalTitle">Thêm Tài Khoản Quản Trị Viên</h5>
        <button type="button" class="close" data-dismiss="modal">&times;</button>
      </div>
      <form id="admin-account-form" onsubmit="handleSaveAdmin(event)">
        <div class="modal-body p-3">
          <input type="hidden" id="edit-admin-id">
          <div class="form-group">
            <label for="adm-name" class="font-weight-bold">Họ và tên Quản trị viên *</label>
            <input type="text" class="form-control" id="adm-name" required>
          </div>
          <div class="form-group">
            <label for="adm-email" class="font-weight-bold">Email đăng nhập *</label>
            <input type="email" class="form-control" id="adm-email" required>
          </div>
          <div class="form-group">
            <label for="adm-phone" class="font-weight-bold">Số điện thoại</label>
            <input type="tel" class="form-control" id="adm-phone" placeholder="0901234567">
          </div>
          <div class="form-group">
            <label for="adm-password" class="font-weight-bold">Mật khẩu *</label>
            <input type="password" class="form-control" id="adm-password" placeholder="Nhập mật khẩu..." minlength="4" required>
          </div>
        </div>
        <div class="modal-footer">
          <button type="button" class="btn btn-secondary" data-dismiss="modal">Hủy</button>
          <button type="submit" class="btn btn-dark">
            Lưu Tài Khoản
          </button>
        </div>
      </form>
    </div>
  </div>
</div>

<!-- MODAL XEM CHI TIẾT KHÁCH HÀNG & ĐƠN HÀNG -->
<div class="modal fade" id="customerDetailModal" tabindex="-1" role="dialog" aria-hidden="true">
  <div class="modal-dialog modal-lg modal-dialog-centered" role="document">
    <div class="modal-content">
      <div class="modal-header bg-light">
        <h5 class="modal-title font-weight-bold text-dark">Chi Tiết Khách Hàng & Đơn Hàng Đã Đặt</h5>
        <button type="button" class="close" data-dismiss="modal">&times;</button>
      </div>
      <div class="modal-body p-3" id="customer-detail-content">
        <!-- Render thông tin khách và đơn hàng -->
      </div>
      <div class="modal-footer">
        <button type="button" class="btn btn-secondary" data-dismiss="modal">Đóng</button>
      </div>
    </div>
  </div>
</div>

@section Scripts {
<script>
  let activeTab = "admins";

  $(document).ready(function() {
    renderUsersTable();
  });

  function switchUserTab(tab, el) {
    activeTab = tab;
    $("#user-tabs .nav-link").removeClass("active");
    $(el).addClass("active");

    if (tab === "admins") {
      $("#btn-add-admin-wrapper").show();
    } else {
      $("#btn-add-admin-wrapper").hide();
    }

    renderUsersTable();
  }

  function renderUsersTable() {
    const allUsers = ThriftDB.getUsers();
    const thead = $("#users-table-header");
    const tbody = $("#users-tbody");
    thead.empty();
    tbody.empty();

    if (activeTab === "admins") {
      // TAB ADMIN: CÓ THÊM / SỬA / XÓA
      thead.html(`
        <th>ID</th>
        <th>Tên Quản Trị Viên</th>
        <th>Email</th>
        <th>Số Điện Thoại</th>
        <th>Quyền Hạn</th>
        <th class="text-center" style="width: 140px;">Thao Tác</th>
      `);

      const admins = allUsers.filter(u => u.role === "admin");
      admins.forEach(adm => {
        tbody.append(`
          <tr>
            <td><strong>#${adm.id}</strong></td>
            <td><strong>${adm.name}</strong></td>
            <td>${adm.email}</td>
            <td>${adm.phone || "---"}</td>
            <td><span class="badge badge-secondary">Quản trị viên (Admin)</span></td>
            <td class="text-center text-nowrap">
              <button class="btn btn-outline-dark btn-sm mr-1" onclick="openEditAdminModal(${adm.id})">
                Sửa
              </button>
              <button class="btn btn-outline-dark btn-sm" onclick="deleteAdmin(${adm.id})">
                Xóa
              </button>
            </td>
          </tr>
        `);
      });
    } else {
      // TAB KHÁCH HÀNG
      thead.html(`
        <th>ID</th>
        <th>Tên Khách Hàng</th>
        <th>Email</th>
        <th>Số Điện Thoại</th>
        <th>Địa Chỉ Đăng Ký</th>
        <th class="text-center" style="width: 180px;">Chi Tiết & Đơn Hàng</th>
      `);

      const customers = allUsers.filter(u => u.role !== "admin");
      customers.forEach(cus => {
        tbody.append(`
          <tr>
            <td><strong>#${cus.id}</strong></td>
            <td><strong>${cus.name}</strong></td>
            <td>${cus.email}</td>
            <td>${cus.phone || "---"}</td>
            <td><small>${cus.address || "Chưa cập nhật"}</small></td>
            <td class="text-center">
              <button class="btn btn-outline-dark btn-sm" onclick="viewCustomerDetail(${cus.id})">
                Xem Đơn Hàng
              </button>
            </td>
          </tr>
        `);
      });
    }
  }

  // Modal Admin
  function openAddAdminModal() {
    $("#adminModalTitle").text("Thêm Quản Trị Viên Mới");
    $("#edit-admin-id").val("");
    $("#admin-account-form")[0].reset();
    $("#adminAccountModal").modal("show");
  }

  function openEditAdminModal(id) {
    const users = ThriftDB.getUsers();
    const adm = users.find(u => u.id === parseInt(id));
    if (!adm) return;

    $("#adminModalTitle").text("Chỉnh Sửa Quản Trị Viên #" + adm.id);
    $("#edit-admin-id").val(adm.id);
    $("#adm-name").val(adm.name);
    $("#adm-email").val(adm.email);
    $("#adm-phone").val(adm.phone || "");
    $("#adm-password").val(adm.password);
    $("#adminAccountModal").modal("show");
  }

  function handleSaveAdmin(e) {
    e.preventDefault();
    const id = $("#edit-admin-id").val();
    const name = $("#adm-name").val().trim();
    const email = $("#adm-email").val().trim();
    const phone = $("#adm-phone").val().trim();
    const pass = $("#adm-password").val();

    const users = ThriftDB.getUsers();

    if (id) {
      const u = users.find(item => item.id === parseInt(id));
      if (u) {
        u.name = name;
        u.email = email;
        u.phone = phone;
        u.password = pass;
      }
    } else {
      const newAdm = {
        id: Date.now(),
        name: name,
        email: email,
        phone: phone,
        password: pass,
        role: "admin"
      };
      users.push(newAdm);
    }

    ThriftDB.saveUsers(users);
    $("#adminAccountModal").modal("hide");
    alert("Đã lưu thông tin Quản trị viên thành công!");
    renderUsersTable();
  }

  function deleteAdmin(id) {
    const users = ThriftDB.getUsers();
    if (users.filter(u => u.role === "admin").length <= 1) {
      alert("Không thể xóa tài khoản Quản trị viên duy nhất còn lại!");
      return;
    }

    if (confirm("Bạn có chắc chắn muốn xóa tài khoản Quản trị viên này?")) {
      let list = users.filter(u => u.id !== parseInt(id));
      ThriftDB.saveUsers(list);
      alert("Đã xóa tài khoản Quản trị viên!");
      renderUsersTable();
    }
  }

  // Trang Detail: hiển thị thông tin khách hàng và danh sách đơn hàng
  function viewCustomerDetail(customerId) {
    const user = ThriftDB.getUsers().find(u => u.id === parseInt(customerId));
    if (!user) return;

    const allOrders = ThriftDB.getOrders();
    const cusOrders = allOrders.filter(o => o.customerId === user.id || o.customerEmail === user.email);

    let ordersHtml = "";
    if (cusOrders.length === 0) {
      ordersHtml = `<tr><td colspan="5" class="text-center text-muted py-3">Khách hàng này chưa phát sinh đơn hàng nào.</td></tr>`;
    } else {
      cusOrders.forEach(o => {
        ordersHtml += `
          <tr>
            <td><strong>#${o.id}</strong></td>
            <td><small>${o.orderDate}</small></td>
            <td>${o.items.length} món đồ</td>
            <td><strong>${ThriftDB.formatCurrency(o.totalAmount)}</strong></td>
            <td><span class="badge badge-secondary">${o.orderStatusText || o.orderStatus}</span></td>
          </tr>
        `;
      });
    }

    const content = `
      <div class="row mb-3">
        <div class="col-md-6">
          <div class="font-weight-bold border-bottom pb-1 mb-2">Thông Tin Khách Hàng</div>
          <p class="small mb-1"><strong>Họ tên:</strong> ${user.name}</p>
          <p class="small mb-1"><strong>Email:</strong> ${user.email}</p>
          <p class="small mb-1"><strong>Số điện thoại:</strong> ${user.phone || "Chưa cập nhật"}</p>
          <p class="small mb-1"><strong>Địa chỉ:</strong> ${user.address || "Chưa cập nhật"}</p>
        </div>
        <div class="col-md-6">
          <div class="font-weight-bold border-bottom pb-1 mb-2">Thống Kê Mua Hàng</div>
          <p class="small mb-1"><strong>Tổng số đơn đã đặt:</strong> ${cusOrders.length} đơn hàng</p>
          <p class="small mb-1"><strong>Trạng thái tài khoản:</strong> Đang hoạt động bình thường</p>
        </div>
      </div>

      <div class="font-weight-bold mb-2">Lịch Sử Các Đơn Hàng Đã Đặt:</div>
      <div class="table-responsive">
        <table class="table table-bordered table-striped mb-0" style="font-size: 12.5px;">
          <thead class="thead-light">
            <tr>
              <th>Mã Đơn</th>
              <th>Thời Gian</th>
              <th>Số Lượng Món</th>
              <th>Tổng Tiền</th>
              <th>Trạng Thái</th>
            </tr>
          </thead>
          <tbody>
            ${ordersHtml}
          </tbody>
        </table>
      </div>
    `;

    $("#customer-detail-content").html(content);
    $("#customerDetailModal").modal("show");
  }
</script>
}
```

---

# FRONTEND - CLIENT SCRIPTS (WWWROOT/JS)

## wwwroot/js/auth.js

```javascript
﻿const messageBox =
    document.getElementById("message");


function showMessage(message, type) {

    messageBox.textContent = message;

    messageBox.className =
        "message " + type;
}


/*
====================================
ĐĂNG NHẬP
====================================
*/

const loginForm =
    document.getElementById("loginForm");


if (loginForm) {

    loginForm.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();

            const email =
                document.getElementById("email").value;

            const password =
                document.getElementById("password").value;


            try {

                const response =
                    await fetch(
                        "/api/login",
                        {
                            method: "POST",

                            headers: {
                                "Content-Type":
                                    "application/json"
                            },

                            body: JSON.stringify({
                                email: email,
                                password: password
                            })
                        }
                    );


                const data =
                    await response.json();


                if (!response.ok) {

                    showMessage(
                        data.message,
                        "error"
                    );

                    return;
                }


                showMessage(
                    "Đăng nhập thành công!",
                    "success"
                );


                setTimeout(function () {

                    document.body.classList.remove("page-loaded");
                    document.body.classList.add("page-exit");

                    setTimeout(function () {

                        window.location.href = "index.html";

                    }, 350);

                }, 700);

            }

            catch (error) {

                showMessage(
                    "Không thể kết nối máy chủ.",
                    "error"
                );

            }

        }
    );
}


/*
====================================
ĐĂNG KÝ
====================================
*/

const registerForm =
    document.getElementById(
        "registerForm"
    );


if (registerForm) {

    registerForm.addEventListener(
        "submit",
        async function (event) {

            event.preventDefault();


            const fullName =
                document.getElementById(
                    "fullName"
                ).value;

            const email =
                document.getElementById(
                    "email"
                ).value;

            const phone =
                document.getElementById(
                    "phone"
                ).value;

            const password =
                document.getElementById(
                    "password"
                ).value;

            const confirmPassword =
                document.getElementById(
                    "confirmPassword"
                ).value;


            if (password !== confirmPassword) {

                showMessage(
                    "Mật khẩu nhập lại không khớp.",
                    "error"
                );

                return;
            }


            try {

                const response =
                    await fetch(
                        "/api/register",
                        {
                            method: "POST",

                            headers: {
                                "Content-Type":
                                    "application/json"
                            },

                            body: JSON.stringify({

                                fullName:
                                    fullName,

                                email:
                                    email,

                                phone:
                                    phone,

                                password:
                                    password,

                                confirmPassword:
                                    confirmPassword

                            })
                        }
                    );


                const data =
                    await response.json();


                if (!response.ok) {

                    showMessage(
                        data.message,
                        "error"
                    );

                    return;
                }


                showMessage(
                    "Đăng ký thành công!",
                    "success"
                );


                setTimeout(function () {

                    document.body.classList.remove("page-loaded");
                    document.body.classList.add("page-exit");

                    setTimeout(function () {

                        window.location.href = "index.html";

                    }, 350);

                }, 700);

            }

            catch (error) {

                showMessage(
                    "Không thể kết nối máy chủ.",
                    "error"
                );

            }

        }
    );
}
```

---

## wwwroot/js/data.js

```javascript
/**
 * data.js - Quản lý dữ liệu Mock Data & LocalStorage
 * ThriftShop - Nền tảng Trao đổi & Bán đồ cũ Secondhand
 */

// Dữ liệu mẫu Danh mục
const DEFAULT_CATEGORIES = [
  { id: 1, name: "Đồ điện tử", icon: "fas fa-laptop", description: "Điện thoại, tai nghe, máy tính, máy ảnh cũ thanh lý" },
  { id: 2, name: "Đồ gia dụng", icon: "fas fa-couch", description: "Bàn ghế, quạt, đèn, đồ dùng gia đình thanh lý" },
  { id: 3, name: "Văn phòng phẩm", icon: "fas fa-book", description: "Sách cũ, giáo trình, truyện tranh, bút vở thanh lý" },
  { id: 4, name: "Thời trang", icon: "fas fa-tshirt", description: "Quần áo vintage, áo khoác, giày dép, túi xách secondhand" },
  { id: 5, name: "Khác", icon: "fas fa-shapes", description: "Nhạc cụ, đồ decor, phụ kiện, đồ sưu tầm độc lạ" }
];

// Dữ liệu mẫu Sản phẩm đồ cũ đang mở bán (Đã được Admin duyệt)
const DEFAULT_PRODUCTS = [
  {
    id: 101,
    name: "Áo khoác Jean Levis Vintage 1990s lót lông",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 420000,
    originalPrice: 1650000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 9,
    sellerName: "Chung Tấn Thành",
    sellerEmail: "thanh@thriftshop.vn",
    sellerPhone: "0908765432",
    sellerAddress: "Quận 1, TP. Hồ Chí Minh",
    sellerAvatar: "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80",
      "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80",
      "https://images.unsplash.com/photo-1520639888713-7851133b1ed0?w=600&q=80"
    ],
    description: "Áo jean Levis nguyên bản form boxy chuẩn vintage thập niên 90. Chất vải denim dày dặn, có bạc màu tự nhiên rất phong cách. Lớp lót lông cừu ấm áp không bị rách.",
    conditionDetails: "Khuy áo nguyên bản, hơi sờn nhẹ gấu tay áo (chất vintage), không rách, đã giặt sấy thơm tho sạch sẽ.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-01 10:30"
  },
  {
    id: 102,
    name: "Tai nghe chống ồn Sony WH-1000XM4 màu Bạc",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 3600000,
    originalPrice: 6990000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 11,
    sellerName: "Đặng Thuỳ Dương",
    sellerEmail: "duong.dang@thriftshop.vn",
    sellerPhone: "0918765432",
    sellerAddress: "Quận Phú Nhuận, TP.HCM",
    sellerAvatar: "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80",
      "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&q=80",
      "https://images.unsplash.com/photo-1484704849700-f032a568e944?w=600&q=80"
    ],
    description: "Tai nghe chống ồn chủ động đỉnh cao Sony WH-1000XM4. Mua chính hãng Sony Store Việt Nam, ít dùng do chuyển sang dùng in-ear. Pin còn nghe liên tục trên 28 tiếng chống ồn.",
    conditionDetails: "Đệm tai và headband còn nguyên vẹn không tróc da. Đầy đủ phụ kiện: hộp cứng bảo vệ, cáp Type-C, jack 3.5mm và adapter máy bay zin.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-02 14:15"
  },
  {
    id: 103,
    name: "Giày Sneaker Nike Air Jordan 1 Retro High OG (Size 42)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 1850000,
    originalPrice: 4200000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1552346154-21d32810aba3?w=600&q=80",
    description: "Giày Jordan 1 phối màu kinh điển, mua order từ Nhật. Đi giữ gìn, đế còn nguyên vân ngôi sao, chỉ nhăn nhẹ ở phần mũi giày toebox theo thời gian.",
    conditionDetails: "Còn box thay thế, lót giày zin, đã vệ sinh và phủ nano chống nước chuyên sâu.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-03 09:00"
  },
  {
    id: 104,
    name: "Máy ảnh Film Canon AE-1 kèm Lens 50mm f/1.8",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 2450000,
    originalPrice: 4500000,
    condition: "88% - Hoạt động hoàn hảo",
    conditionPercent: 88,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600&q=80",
    description: "Máy cơ huyền thoại Canon AE-1 cho các bạn bắt đầu chơi film. Cơ tốc chuẩn xác, đo sáng nhạy, buồng film sạch sẽ kín sáng không lọt sáng.",
    conditionDetails: "Kính lens trong veo không mốc rễ tre, body trầy xước nhẹ góc dưới do thời gian, tặng kèm pin mới và dây đeo vintage.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-04 16:45"
  },
  {
    id: 105,
    name: "Bộ trọn bộ truyện tranh Doraemon Đại Tuyển Tập (45 Tập)",
    categoryId: 3,
    categoryName: "Văn phòng phẩm",
    price: 680000,
    originalPrice: 1350000,
    condition: "96% - Sưu tầm đẹp",
    conditionPercent: 96,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80",
    description: "Bộ truyện tranh Doraemon trọn bộ 45 tập NXB Kim Đồng bản bìa gập dày. Giữ trong tủ kính chống ẩm, giấy vàng nhẹ tự nhiên, không rách trang.",
    conditionDetails: "Đầy đủ 45 tập từ tập 1 đến 45, không mất bìa, không vẽ bậy, thích hợp cho người sưu tầm kỷ niệm.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-05 11:20"
  },
  {
    id: 106,
    name: "Đàn Guitar Acoustic Yamaha FG800 gỗ thịt Solid Top",
    categoryId: 5,
    categoryName: "Khác",
    price: 2900000,
    originalPrice: 5800000,
    condition: "93% - Rất tốt",
    conditionPercent: 93,
    sellerId: 2,
    sellerName: "Trần Nguyễn Anh Khoa",
    sellerEmail: "user@thriftshop.vn",
    sellerPhone: "0889771935",
    sellerAddress: "Quận 1, TP. Hồ Chí Minh",
    sellerAvatar: "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&q=80",
    description: "Đàn guitar chính hãng Yamaha acoustic dòng FG nổi tiếng với mặt top gỗ vân sam nguyên tấm (Solid Spruce). Âm thanh vang ấm, bass dày, action êm tay không đau ngón.",
    conditionDetails: "Vừa thay bộ dây Elixir xịn 12-53, cần thẳng tắp, tặng kèm bao da 3 lớp chống sốc và capo hợp kim nhôm.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-06 08:30"
  },
  {
    id: 107,
    name: "Đèn bàn làm việc Vintage phong cách Bauhaus chao đồng",
    categoryId: 2,
    categoryName: "Đồ gia dụng",
    price: 350000,
    originalPrice: 850000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 15,
    sellerName: "Lê Mỹ Dung",
    sellerPhone: "0908112233",
    sellerAddress: "Quận 1, TP.HCM",
    stock: 2,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600&q=80",
    description: "Đèn bàn học kiêm decor góc làm việc thiết kế retro phong cách Đức cổ. Chao đèn kim loại màu xanh olive phối thân kim loại đồng giả cổ.",
    conditionDetails: "Dây điện và công tắc bập bênh hoạt động an toàn, tặng kèm bóng đèn LED Edison ánh sáng ấm 4W tiết kiệm điện.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-07 15:10"
  },
  {
    id: 108,
    name: "Áo sơ mi Flannel Caro Retro Unisex Size L",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 180000,
    originalPrice: 520000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 2,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=600&q=80",
    description: "Áo sơ mi dạ caro flannel hàng si tuyển chọn từ Nhật. Chất vải mềm mịn, giữ ấm nhẹ, phối layer với áo thun trắng cực ngầu.",
    conditionDetails: "Không xù lông, khuy cài đủ không sứt chỉ, form oversize trẻ trung năng động.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-08 13:00"
  },
  {
    id: 109,
    name: "Bàn phím cơ không dây NuPhy Air75 Low-Profile Switch Gateron",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 1950000,
    originalPrice: 3200000,
    condition: "97% - Như mới",
    conditionPercent: 97,
    sellerId: 11,
    sellerName: "Đặng Thuỳ Dương",
    sellerPhone: "0918765432",
    sellerAddress: "Quận Phú Nhuận, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80",
    description: "Bàn phím cơ siêu mỏng mượt mà dành cho Mac và Windows. Kết nối 3 chế độ: Bluetooth 5.0, 2.4Ghz và cắm dây. Switch bấm êm ái.",
    conditionDetails: "Hàng mua đập hộp được 2 tháng ít dùng, pin trâu, keycap PBT không bóng, đủ fullbox phụ kiện zin.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-09 10:00"
  },
  {
    id: 110,
    name: "Giày Boots da nam cổ lửng Dr. Martens 1460 (Size 41)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 1750000,
    originalPrice: 4800000,
    condition: "89% - Bền đẹp",
    conditionPercent: 89,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1520639888713-7851133b1ed0?w=600&q=80",
    description: "Đôi boots da bò thật 8 lỗ xỏ kinh điển của thương hiệu Dr. Martens nước Anh. Đế AirWair êm ái chống trơn trượt mài mòn cực tốt.",
    conditionDetails: "Da đã break-in mềm chân, xước nhẹ mũi giày đánh xi đen là bóng loáng, chỉ vàng đặc trưng còn nguyên.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-10 17:30"
  },
  {
    id: 111,
    name: "Sách cũ: Nhà Giả Kim & Nghìn Lẻ Một Đêm (Combo 2 cuốn)",
    categoryId: 3,
    categoryName: "Văn phòng phẩm",
    price: 110000,
    originalPrice: 280000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=600&q=80",
    description: "Bộ 2 tác phẩm văn học kinh điển thế giới bìa cứng đẹp. Sách đọc xong cất kệ cẩn thận, không quăn góc, không gạch chân chữ.",
    conditionDetails: "Sách sạch sẽ như mới, giấy ngà chống lóa mắt, thích hợp cho độc giả yêu sách văn học.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 09:15"
  },
  {
    id: 112,
    name: "Loa Bluetooth di động Marshall Emberton Black & Brass",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 2150000,
    originalPrice: 3890000,
    condition: "94% - Rất mới",
    conditionPercent: 94,
    sellerId: 14,
    sellerName: "Vũ Đình Trọng",
    sellerPhone: "0966554433",
    sellerAddress: "Quận Gò Vấp, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1545454675-3531b543be5d?w=600&q=80",
    description: "Loa bluetooth nhỏ gọn thiết kế rock cổ điển với núm đồng đa hướng. Âm thanh đa hướng 360 độ True Stereophonic, kháng nước chuẩn IPX7.",
    conditionDetails: "Lưới thép mặt trước không móp méo, pin còn rất tốt nghe liên tục hơn 15 tiếng, kèm cáp sạc Type-C.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 15:40"
  },
  {
    id: 113,
    name: "Áo Khoác Bomber Da Lộn Vintage Nâu Bò (Size L)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 520000,
    originalPrice: 1450000,
    condition: "94% - Rất mới",
    conditionPercent: 94,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80",
    description: "Áo khoác bomber da lộn lót dù ấm áp, phong cách retro cổ điển thập niên 90. Bo tay và gấu áo dệt kim chắc chắn không dão.",
    conditionDetails: "Chất da mềm mịn, màu nâu bò patina đều màu, khóa kéo kim loại YKK trơn tru không kẹt.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 16:00"
  },
  {
    id: 114,
    name: "Áo Măng Tô Dạ Dáng Dài Cổ Điển Hàn Quốc (Freesize)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 390000,
    originalPrice: 1100000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1539533018447-63fcce667823?w=600&q=80",
    description: "Áo dạ dáng dài màu be thanh lịch, 2 hàng khuy form suông chuẩn style vintage Ulzzang. Vải dạ ép dày dặn đứng form.",
    conditionDetails: "Không sờn chỉ, khuy sừng nguyên bản, đã giặt hấp thơm tho sạch sẽ.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 16:30"
  },
  {
    id: 115,
    name: "Túi Xách Da Thật Đeo Vai Retro Y2K Phong Cách Cổ Điển",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 340000,
    originalPrice: 850000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 15,
    sellerName: "Lê Mỹ Dung",
    sellerPhone: "0908112233",
    sellerAddress: "Quận 1, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?w=600&q=80",
    description: "Túi kẹp nách da bò thật form cứng cáp, quai đeo vai bản nhỏ retro. Đựng vừa điện thoại Pro Max, ví tiền và son môi.",
    conditionDetails: "Lớp lót vải satin sạch đẹp, khóa bấm kim loại mạ vàng đồng còn sáng bóng.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 17:00"
  },
  {
    id: 116,
    name: "Quần Jean Levi's 501 Original Ống Đứng 1995 (W31 L32)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 480000,
    originalPrice: 1800000,
    condition: "93% - Rất tốt",
    conditionPercent: 93,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1542272604-787c3835535d?w=600&q=80",
    description: "Dòng quần jean huyền thoại Levi's 501 cúc cài button-fly, vải denim 100% cotton dày 14oz không co giãn, giữ form cực đỉnh.",
    conditionDetails: "Màu wash xanh chàm tự nhiên, gấu quần nguyên bản chưa cắt line, tag da phía sau còn nguyên chữ.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 17:30"
  },
  {
    id: 117,
    name: "Giày Sneaker New Balance 550 White Green (Size 41)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 1350000,
    originalPrice: 2950000,
    condition: "93% - Rất tốt",
    conditionPercent: 93,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=600&q=80",
    description: "Mẫu sneaker bóng rổ vintage hot nhất của New Balance. Phối màu trắng phối xanh lá đậm thời thượng, da tổng hợp phối da lộn.",
    conditionDetails: "Đế giày mòn cực ít, da mũi giày chỉ gập nhẹ theo cử động bước đi, đã giặt hấp và xịt khử khuẩn UV.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 18:00"
  },
  {
    id: 118,
    name: "Giày Converse Chuck Taylor 70s Parchment Vintage (Size 42)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 750000,
    originalPrice: 1700000,
    condition: "91% - Tốt",
    conditionPercent: 91,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1607522370275-f14206abe5d3?w=600&q=80",
    description: "Bản Chuck 70s cổ thấp vải canvas dày dặn màu kem parchment ngà vintage. Đế cao su bóng retro êm ái hơn dòng Classic nhiều.",
    conditionDetails: "Heel logo đen còn nguyên chữ, đế chưa mòn gót, vải canvas sạch sẽ không ố vàng bẩn.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 18:30"
  },
  {
    id: 119,
    name: "Giày Loafer Da Bóng Vintage Penny Loafer (Size 40)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 620000,
    originalPrice: 1550000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 14,
    sellerName: "Vũ Đình Trọng",
    sellerPhone: "0966554433",
    sellerAddress: "Quận Gò Vấp, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1533867617858-e7b97e060509?w=600&q=80",
    description: "Giày tây lười penny loafer da bóng phong cách học viện Academia cổ điển. Dễ phối cùng quần tây, tất trắng và blazer.",
    conditionDetails: "Đế cao su đúc nguyên khối chống trơn, da được đánh xi bóng loáng không nứt nẻ.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 19:00"
  },
  {
    id: 120,
    name: "Giày Sneaker Adidas Samba OG White Black Gum (Size 41.5)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 1450000,
    originalPrice: 2700000,
    condition: "94% - Rất mới",
    conditionPercent: 94,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?w=600&q=80",
    description: "Đôi giày quốc dân Samba OG mũi da lộn chữ T kinh điển kèm đế cao su gum màu nâu bám sàn. Đôi giày không bao giờ lỗi mốt.",
    conditionDetails: "Đi lướt vài lần chụp ảnh, đế còn nguyên gai cao su, lót giày in logo vàng rõ nét.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 19:30"
  },
  {
    id: 121,
    name: "Máy Chơi Game Nintendo Switch OLED White Fullbox",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 5100000,
    originalPrice: 8200000,
    condition: "98% - Như mới",
    conditionPercent: 98,
    sellerId: 11,
    sellerName: "Đặng Thuỳ Dương",
    sellerPhone: "0918765432",
    sellerAddress: "Quận Phú Nhuận, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1578301978693-85fa9c0320b9?w=600&q=80",
    description: "Máy Switch OLED màn hình tràn viền 7 inch sắc nét rực rỡ, màu trắng ngọc trai sang trọng. Máy zin nguyên bản chưa hack, online mượt mà.",
    conditionDetails: "Màn hình đã dán kính cường lực từ ngày đầu, Joycon không trôi analog, đủ dock TV, sạc zin và dây strap.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 20:00"
  },
  {
    id: 122,
    name: "Máy Đọc Sách Kindle Paperwhite 4 Chống Nước 8GB",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 1450000,
    originalPrice: 2800000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1592434134753-a70baf7979d5?w=600&q=80",
    description: "Máy đọc sách E-ink màn hình 300 ppi sắc nét như trang giấy thật, có đèn nền đọc ban đêm và kháng nước IPX8. Pin dùng cả tháng chỉ với 1 lần sạc.",
    conditionDetails: "Màn hình không điểm chết không trầy xước, tặng kèm cover bảo vệ da tự động đóng tắt màn hình.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 20:30"
  },
  {
    id: 123,
    name: "Máy Tính Bảng iPad Gen 9 10.2 inch 64GB Wifi Space Grey",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 4600000,
    originalPrice: 7990000,
    condition: "97% - Như mới",
    conditionPercent: 97,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=600&q=80",
    description: "iPad Gen 9 chip Apple A13 Bionic mạnh mẽ phục vụ học tập, xem phim, vẽ tranh hoặc đọc tài liệu. Màn hình Retina sắc nét kèm phím Home vật lý bền bỉ.",
    conditionDetails: "Pin zin 96% sạc ít lần, máy không cấn móp góc cạnh, iCloud chính chủ reset thoải mái tại chỗ.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 21:00"
  },
  {
    id: 124,
    name: "Bộ Truyện Tranh One Piece Tập 1 - 30 (Bản gốc NXB Kim Đồng)",
    categoryId: 3,
    categoryName: "Văn phòng phẩm",
    price: 450000,
    originalPrice: 950000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=600&q=80",
    description: "30 tập đầu tiên của hành trình Vua Hải Tặc One Piece (từ Làng Cối Xay Gió tới Đảo Trên Trời Skypiea). Giấy thơm phẳng phiu.",
    conditionDetails: "Đủ tập từ 1 đến 30, không rách trang, không ướt nước, bảo quản trong bọc nilon cẩn thận.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 21:30"
  },
  {
    id: 125,
    name: "Bộ 3 Cuốn Sách Lịch Sử Văn Minh Thế Giới (Bìa Cứng Ép Kim)",
    categoryId: 3,
    categoryName: "Văn phòng phẩm",
    price: 260000,
    originalPrice: 680000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 15,
    sellerName: "Lê Mỹ Dung",
    sellerPhone: "0908112233",
    sellerAddress: "Quận 1, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80",
    description: "Bộ tuyển tập lịch sử văn minh nhân loại khổ lớn bìa cứng ép nhũ vàng sang trọng. Trình bày chi tiết với nhiều tranh minh họa lịch sử quý giá.",
    conditionDetails: "Sách như mới đập hộp, gáy sách chắc nịch không lung lay, ruột sách sạch không vết bút bi.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 22:00"
  },
  {
    id: 126,
    name: "Máy Pha Cà Phê Espresso DeLonghi Dedica EC685 Đỏ Kim Loại",
    categoryId: 2,
    categoryName: "Đồ gia dụng",
    price: 2850000,
    originalPrice: 6500000,
    condition: "94% - Rất tốt",
    conditionPercent: 94,
    sellerId: 14,
    sellerName: "Vũ Đình Trọng",
    sellerPhone: "0966554433",
    sellerAddress: "Quận Gò Vấp, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=600&q=80",
    description: "Máy pha cà phê gia đình nhỏ gọn bề ngang chỉ 15cm áp suất bơm 15 bar tiêu chuẩn Ý. Cho lớp crema dày béo ngậy và vòi đánh sữa latte mượt mà.",
    conditionDetails: "Máy hoạt động hoàn hảo vừa tẩy cặn định kỳ, tặng kèm tay pha portafilter, tamper nén inox và ca đánh sữa 350ml.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 22:30"
  },
  {
    id: 127,
    name: "Cây Đèn Đứng Decor Phòng Khách Phong Cách Bắc Âu Scandinavian",
    categoryId: 2,
    categoryName: "Đồ gia dụng",
    price: 420000,
    originalPrice: 950000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 15,
    sellerName: "Lê Mỹ Dung",
    sellerPhone: "0908112233",
    sellerAddress: "Quận 1, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1513506003901-1e6a229e2d15?w=600&q=80",
    description: "Đèn sàn uốn cong decor góc sofa hoặc bàn đọc sách. Thân kim loại sơn tĩnh điện đen mờ phối đế đá cẩm thạch đầm chắc chắn.",
    conditionDetails: "Có công tắc đạp chân tiện lợi, bóng đèn LED 3 chế độ màu (vàng, trung tính, trắng) tiết kiệm điện.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 23:00"
  },
  {
    id: 128,
    name: "Bộ Ấm Chén Gốm Mộc Men Rạn Cổ Điển Kèm Khay Gỗ Trúc",
    categoryId: 2,
    categoryName: "Đồ gia dụng",
    price: 280000,
    originalPrice: 650000,
    condition: "98% - Như mới",
    conditionPercent: 98,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1578749556568-bc2c40e68b61?w=600&q=80",
    description: "Bộ trà gốm mộc vuốt tay gồm 1 ấm trà, 6 chén và 1 khay gỗ tre tự nhiên thoát nước. Nước men rạn xanh ngọc mang đậm phong vị thiền trà.",
    conditionDetails: "Hàng trưng bày tủ kính chưa qua sử dụng, không mẻ miệng chén, quai ấm bọc đồng chắc chắn.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 23:30"
  },
  {
    id: 129,
    name: "Đàn Classic Yamaha C40 Cho Người Mới Bắt Đầu (Dây Nilon)",
    categoryId: 5,
    categoryName: "Khác",
    price: 1650000,
    originalPrice: 3100000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 14,
    sellerName: "Vũ Đình Trọng",
    sellerPhone: "0966554433",
    sellerAddress: "Quận Gò Vấp, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1550291652-6ea9114a47b1?w=600&q=80",
    description: "Cây đàn cổ điển huyền thoại được các trường nhạc khuyên dùng. Dây nilon mềm mại êm tay, âm thanh tròn trịa chuẩn cao độ.",
    conditionDetails: "Cần đàn thẳng tắp, phím đàn không mòn, vừa thay bộ dây D'Addario EJ27N mới tinh, tặng kèm bao da.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-12 00:00"
  },
  {
    id: 130,
    name: "Đàn Kalimba 17 Phím Gỗ Mahogany Kèm Búa Chỉnh Âm",
    categoryId: 5,
    categoryName: "Khác",
    price: 1900000,
    originalPrice: 450000,
    condition: "97% - Như mới",
    conditionPercent: 97,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1520523839898-50712825e617?w=600&q=80",
    description: "Đàn ngón tay Kalimba 17 nốt kim loại khắc sẵn chữ số nốt nhạc, thân đàn gỗ gụ Mahogany nguyên khối vang trong veo.",
    conditionDetails: "Phím sáng bóng không rỉ sét, âm chuẩn tone C, tặng kèm hộp chống sốc, búa chỉnh âm và sách hướng dẫn tự học.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-12 00:30"
  },
  {
    id: 131,
    name: "Micro Thu Âm Audio-Technica AT2020 USB+ Kèm Giá Đỡ Chống Rung",
    categoryId: 5,
    categoryName: "Khác",
    price: 1850000,
    originalPrice: 3600000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 11,
    sellerName: "Đặng Thuỳ Dương",
    sellerPhone: "0918765432",
    sellerAddress: "Quận Phú Nhuận, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1590602847861-f357a9332bbc?w=600&q=80",
    description: "Microphone condenser thu âm chuyên nghiệp cho podcast, livestream, thu giọng hát acoustic. Cắm cổng USB nhận ngay không cần soundcard.",
    conditionDetails: "Màng lọc âm sạch sẽ, cổng jack tai nghe kiểm âm 3.5mm hoạt động hoàn hảo, tặng kèm shockmount và chân đế bàn kim loại.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-12 01:00"
  }
];

// Dữ liệu mẫu TIN ĐĂNG BÁN ĐỒ CŨ DO NGƯỜI DÙNG GỬI LÊN (Chờ Admin duyệt hoặc đã duyệt/từ chối)
const DEFAULT_PENDING_POSTS = [
  {
    id: 201,
    name: "Máy chơi game Sony PlayStation 4 Slim 500GB kèm 2 tay cầm",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 3200000,
    originalPrice: 7500000,
    condition: "93% - Rất tốt",
    conditionPercent: 93,
    sellerId: 2,
    sellerName: "Trần Nhật Hào (Bạn)",
    sellerPhone: "0909123456",
    sellerAddress: "123 Nguyễn Huệ, Quận 1, TP.HCM",
    stock: 1,
    image: "https://images.unsplash.com/photo-1507457379470-08b800bebc67?w=600&q=80",
    description: "Máy PS4 Slim 500GB mua chính hãng Sony VN, đã nâng cấp firmware mới nhất. Ít chơi do đi làm bận, máy chạy êm mát không kêu to.",
    conditionDetails: "Kèm 2 tay cầm DualShock 4 không trôi cần analog, cáp nguồn, cáp HDMI, tặng kèm đĩa game God of War.",
    status: "pending", // 'pending' | 'approved' | 'rejected'
    rejectReason: "",
    createdAt: "2026-10-12 08:30"
  },
  {
    id: 202,
    name: "Bàn phím cơ không dây Keychron K2 V2 nhôm Hot-swap RGB",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 1150000,
    originalPrice: 2150000,
    condition: "97% - Như mới",
    conditionPercent: 97,
    sellerId: 20,
    sellerName: "Lê Quốc Bảo",
    sellerPhone: "0912345678",
    sellerAddress: "Quận Tân Bình, TP.HCM",
    stock: 1,
    image: "https://images.unsplash.com/photo-1595225476474-87563907a212?w=600&q=80",
    description: "Keychron K2 V2 bản viền nhôm cao cấp, switch Gateron Brown gõ nảy đầm tay. Dùng được cả Bluetooth và cắm dây.",
    conditionDetails: "Fullbox đầy đủ phụ kiện keycap cho cả Mac/Windows, cáp Type-C, gắp phím.",
    status: "pending",
    rejectReason: "",
    createdAt: "2026-10-12 09:10"
  },
  {
    id: 203,
    name: "Túi đeo chéo da bò mộc Handmade Secondhand",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 390000,
    originalPrice: 950000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 21,
    sellerName: "Ngô Mai Phương",
    sellerPhone: "0988776655",
    sellerAddress: "Quận 7, TP.HCM",
    stock: 1,
    image: "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=600&q=80",
    description: "Túi da bò thật may tay thủ công đựng vừa iPad 11 inch và sổ tay. Màu nâu sáp lên màu patina vintage rất đẹp theo thời gian.",
    conditionDetails: "Khóa đồng sáng bóng, quai đeo êm chắc chắn, không trầy rách sâu.",
    status: "pending",
    rejectReason: "",
    createdAt: "2026-10-11 20:15"
  },
  {
    id: 204,
    name: "Bộ 10 đĩa than Vinyl Nhạc Jazz Cổ Điển US",
    categoryId: 5,
    categoryName: "Khác",
    price: 1200000,
    originalPrice: 2800000,
    condition: "85% - Khá",
    conditionPercent: 85,
    sellerId: 22,
    sellerName: "Phan Văn Hưng",
    sellerPhone: "0933221100",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    image: "https://images.unsplash.com/photo-1539185441755-769473a23570?w=600&q=80",
    description: "Bộ sưu tập 10 đĩa than nhạc Jazz thập niên 60-70 xách tay từ Mỹ gồm Miles Davis, John Coltrane, Bill Evans.",
    conditionDetails: "Vỏ giấy hơi ố theo thời gian, bề mặt đĩa sạch, nghe mượt mà có tiếng lách tách đặc trưng của đĩa than.",
    status: "pending",
    rejectReason: "",
    createdAt: "2026-10-11 18:40"
  }
];

// Dữ liệu mẫu Danh sách Đơn hàng (Dùng cho cả Khách hàng và Admin)
const DEFAULT_ORDERS = [
  {
    id: "DH100234",
    orderDate: "2026-10-10 14:20",
    customerId: 2,
    customerName: "Trần Nhật Hào",
    customerPhone: "0909123456",
    customerEmail: "user@gmail.com",
    shippingAddress: "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
    paymentMethod: "COD - Thanh toán khi nhận hàng",
    paymentStatus: "Chưa thanh toán",
    orderStatus: "delivered", // 'pending' | 'processing' | 'shipping' | 'delivered' | 'cancelled'
    orderStatusText: "Đã giao hàng thành công",
    items: [
      {
        productId: 101,
        productName: "Áo khoác Jean Levis Vintage 1990s lót lông",
        image: "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80",
        condition: "92% - Tốt",
        price: 420000,
        quantity: 1,
        subtotal: 420000
      }
    ],
    subtotal: 420000,
    shippingFee: 30000,
    totalAmount: 450000,
    notes: "Giao giờ hành chính, gọi trước 15 phút."
  },
  {
    id: "DH100235",
    orderDate: "2026-10-11 16:45",
    customerId: 2,
    customerName: "Trần Nhật Hào",
    customerPhone: "0909123456",
    customerEmail: "user@gmail.com",
    shippingAddress: "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
    paymentMethod: "Chuyển khoản Ngân hàng / MoMo",
    paymentStatus: "Đã thanh toán",
    orderStatus: "shipping",
    orderStatusText: "Đang giao hàng",
    items: [
      {
        productId: 105,
        productName: "Bộ trọn bộ truyện tranh Doraemon Đại Tuyển Tập (45 Tập)",
        image: "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80",
        condition: "96% - Sưu tầm đẹp",
        price: 680000,
        quantity: 1,
        subtotal: 680000
      }
    ],
    subtotal: 680000,
    shippingFee: 30000,
    totalAmount: 710000,
    notes: "Bọc chống sốc kỹ giúp mình vì là sách sưu tầm."
  },
  {
    id: "DH100236",
    orderDate: "2026-10-12 08:15",
    customerId: 2,
    customerName: "Trần Nhật Hào",
    customerPhone: "0909123456",
    customerEmail: "user@gmail.com",
    shippingAddress: "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
    paymentMethod: "COD - Thanh toán khi nhận hàng",
    paymentStatus: "Chưa thanh toán",
    orderStatus: "pending",
    orderStatusText: "Chờ duyệt đơn",
    items: [
      {
        productId: 107,
        productName: "Đèn bàn làm việc Vintage phong cách Bauhaus chao đồng",
        image: "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600&q=80",
        condition: "90% - Tốt",
        price: 350000,
        quantity: 1,
        subtotal: 350000
      }
    ],
    subtotal: 350000,
    shippingFee: 30000,
    totalAmount: 380000,
    notes: ""
  },
  {
    id: "DH100237",
    orderDate: "2026-10-12 09:30",
    customerId: 3,
    customerName: "Nguyễn Thị Mai Lan",
    customerPhone: "0912987654",
    customerEmail: "mailan@gmail.com",
    shippingAddress: "45 Lê Lợi, Phường Bến Nghé, Quận 1, TP.HCM",
    paymentMethod: "Chuyển khoản Ngân hàng / MoMo",
    paymentStatus: "Đã thanh toán",
    orderStatus: "processing",
    orderStatusText: "Đang đóng gói xử lý",
    items: [
      {
        productId: 102,
        productName: "Tai nghe chống ồn Sony WH-1000XM4 màu Bạc",
        image: "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80",
        condition: "95% - Rất mới",
        price: 3600000,
        quantity: 1,
        subtotal: 3600000
      }
    ],
    subtotal: 3600000,
    shippingFee: 0,
    totalAmount: 3600000,
    notes: "Đơn hàng giá trị cao, yêu cầu đồng kiểm."
  }
];

// Dữ liệu mẫu Cuộc trò chuyện / Liên hệ theo sản phẩm (Mặc định để trống)
const DEFAULT_CHATS = [];

// Dữ liệu mẫu Tài khoản người dùng (2 tài khoản chuẩn và các người bán tiêu biểu)
const DEFAULT_USERS = [
  {
    id: 1,
    name: "Ban Quản Trị (Admin)",
    email: "admin@thriftshop.vn",
    password: "admin",
    phone: "0901234567",
    role: "admin",
    address: "Văn phòng BQT ThriftShop, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80",
    bio: "Ban Quản Trị hệ thống kiểm duyệt và hỗ trợ sàn giao dịch đồ cũ ThriftShop."
  },
  {
    id: 2,
    name: "Trần Nguyễn Anh Khoa",
    email: "user@thriftshop.vn",
    password: "user",
    phone: "0889771935",
    role: "customer",
    address: "Quận 1, TP. Hồ Chí Minh",
    avatar: "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
    bio: "Chuyên thanh lý đồ công nghệ & gia dụng cũ còn tốt, hỗ trợ giao dịch trực tiếp."
  },
  {
    id: 3,
    name: "Chung Tấn Thành",
    email: "thanh@thriftshop.vn",
    password: "user",
    phone: "0908765432",
    role: "customer",
    address: "Quận 1, TP. Hồ Chí Minh",
    avatar: "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
    bio: "Đam mê đồ công nghệ & thiết bị điện tử, máy ảnh cũ. Món đồ nào thanh lý cũng được vệ sinh và test kỹ càng."
  },
  {
    id: 4,
    name: "Đặng Thuỳ Dương",
    email: "duong.dang@thriftshop.vn",
    password: "user",
    phone: "0918765432",
    role: "customer",
    address: "Quận Phú Nhuận, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
    bio: "Thanh lý đồ công nghệ chính hãng mua dùng lướt ít lần còn nguyên hộp."
  },
  {
    id: 5,
    name: "Hoàng Minh Trí",
    email: "tri.hoang@thriftshop.vn",
    password: "user",
    phone: "0903124567",
    role: "customer",
    address: "Quận 3, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
    bio: "Tín đồ thời trang vintage và phụ kiện cổ điển thập niên 90."
  },
  {
    id: 12,
    name: "Nguyễn Tuấn Kiệt",
    email: "kiet.nguyen@thriftshop.vn",
    password: "user",
    phone: "0977889900",
    role: "customer",
    address: "Quận Bình Thạnh, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&q=80",
    bio: "Sưu tầm giày sneaker và quần áo streetwear secondhand chính hãng."
  },
  {
    id: 13,
    name: "Phạm Thảo Vy",
    email: "vy.pham@thriftshop.vn",
    password: "user",
    phone: "0934567891",
    role: "customer",
    address: "Quận 10, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&q=80",
    bio: "Yêu thích sách và truyện tranh vintage, giữ gìn sách cẩn thận."
  },
  {
    id: 14,
    name: "Vũ Đình Trọng",
    email: "trong.vu@thriftshop.vn",
    password: "user",
    phone: "0966554433",
    role: "customer",
    address: "Quận Gò Vấp, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=150&q=80",
    bio: "Đam mê âm nhạc và nhạc cụ acoustic chính hãng."
  },
  {
    id: 15,
    name: "Lê Mỹ Dung",
    email: "dung.le@thriftshop.vn",
    password: "user",
    phone: "0908112233",
    role: "customer",
    address: "Quận 1, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&q=80",
    bio: "Sưu tầm đồ trang trí và phụ kiện decor phong cách hoài cổ."
  }
];

// KHỞI TẠO LOCALSTORAGE
function initThriftData() {
  const savedCats = JSON.parse(localStorage.getItem("thrift_categories") || "[]");
  // Kiểm tra nếu danh mục chưa đúng 5 danh mục chuẩn thì thiết lập lại và đồng bộ sản phẩm
  if (!savedCats.length || savedCats.length !== 5 || savedCats[0].name !== "Đồ điện tử") {
    localStorage.setItem("thrift_categories", JSON.stringify(DEFAULT_CATEGORIES));
    localStorage.setItem("thrift_products", JSON.stringify(DEFAULT_PRODUCTS));
    localStorage.setItem("thrift_pending_posts", JSON.stringify(DEFAULT_PENDING_POSTS));
  }

  // Tự động hợp nhất sản phẩm mặc định và đồng bộ chính xác ảnh đại diện người bán
  const existingProds = JSON.parse(localStorage.getItem("thrift_products") || "[]");
  if (!existingProds.length) {
    localStorage.setItem("thrift_products", JSON.stringify(DEFAULT_PRODUCTS));
  } else {
    let prodsChanged = false;
    const existingIds = new Set(existingProds.map(p => p.id));
    DEFAULT_PRODUCTS.forEach(dp => {
      if (!existingIds.has(dp.id)) {
        existingProds.push(dp);
        prodsChanged = true;
      }
    });
    // Đồng bộ chuẩn hóa avatar và tên người bán cho các sản phẩm mẫu
    existingProds.forEach(p => {
      const def = DEFAULT_PRODUCTS.find(d => d.id === p.id);
      if (def && (!p.sellerAvatar || p.sellerAvatar !== def.sellerAvatar || p.sellerName !== def.sellerName)) {
        p.sellerAvatar = def.sellerAvatar;
        p.sellerName = def.sellerName;
        p.sellerId = def.sellerId;
        p.sellerPhone = def.sellerPhone;
        prodsChanged = true;
      }
    });
    if (prodsChanged) {
      localStorage.setItem("thrift_products", JSON.stringify(existingProds));
    }
  }

  // Đồng bộ tin chờ duyệt nếu chưa có
  const existingPending = JSON.parse(localStorage.getItem("thrift_pending_posts") || "[]");
  if (!existingPending.length) {
    localStorage.setItem("thrift_pending_posts", JSON.stringify(DEFAULT_PENDING_POSTS));
  }

  if (!localStorage.getItem("thrift_orders")) {
    localStorage.setItem("thrift_orders", JSON.stringify(DEFAULT_ORDERS));
  }
  // Đồng bộ danh sách người dùng mẫu (Đảm bảo avatar đúng cho từng seller, tránh bị gán nhầm)
  const existingUsers = JSON.parse(localStorage.getItem("thrift_users") || "[]");
  if (!existingUsers.length) {
    localStorage.setItem("thrift_users", JSON.stringify(DEFAULT_USERS));
  } else {
    const userMap = new Map();
    DEFAULT_USERS.forEach(u => userMap.set(u.id, u));
    existingUsers.forEach(u => {
      const def = userMap.get(u.id);
      if (def) {
        userMap.set(u.id, { ...def, ...u, avatar: def.avatar });
      } else {
        userMap.set(u.id, u);
      }
    });
    localStorage.setItem("thrift_users", JSON.stringify(Array.from(userMap.values())));
  }

  if (!localStorage.getItem("thrift_cart")) {
    localStorage.setItem("thrift_cart", JSON.stringify([]));
  }

  // Khởi tạo danh sách Liên hệ / Chat: Xóa sạch lịch sử chat mặc định
  if (!sessionStorage.getItem("thrift_chat_cleared_v2")) {
    localStorage.setItem("thrift_chats", JSON.stringify([]));
    sessionStorage.setItem("thrift_chat_cleared_v2", "true");
  } else if (!localStorage.getItem("thrift_chats")) {
    localStorage.setItem("thrift_chats", JSON.stringify([]));
  }

  // MẶC ĐỊNH KHI VÀO WEBSITE: CHƯA ĐĂNG NHẬP
  if (!sessionStorage.getItem("thrift_session_active")) {
    localStorage.removeItem("thrift_current_user");
  }
}

// CÁC HÀM TIỆN ÍCH DỮ LIỆU
const ThriftDB = {
  // Định dạng tiền tệ VND
  formatCurrency(amount) {
    if (isNaN(amount)) return "0 ₫";
    return new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(amount);
  },

  // Danh mục
  getCategories() {
    return JSON.parse(localStorage.getItem("thrift_categories")) || DEFAULT_CATEGORIES;
  },
  saveCategories(cats) {
    localStorage.setItem("thrift_categories", JSON.stringify(cats));
  },
  getCategoryById(id) {
    const list = this.getCategories();
    return list.find(c => c.id === parseInt(id));
  },

  // Sản phẩm sàn (Đã duyệt - tự động ẩn món đã bán)
  getProducts() {
    let prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    if (!prods.length) {
      localStorage.setItem("thrift_products", JSON.stringify(DEFAULT_PRODUCTS));
      prods = DEFAULT_PRODUCTS;
    }
    // Lọc bỏ những sản phẩm đã được đánh dấu bán hoặc có trạng thái sold
    return prods.filter(p => !p.isSold && p.status !== 'sold');
  },
  saveProducts(prods) {
    localStorage.setItem("thrift_products", JSON.stringify(prods));
  },
  getProductById(id) {
    const list = this.getProducts();
    return list.find(p => p.id === parseInt(id));
  },

  // Quản lý tin đồ cũ chờ duyệt
  getPendingPosts() {
    return JSON.parse(localStorage.getItem("thrift_pending_posts")) || DEFAULT_PENDING_POSTS;
  },
  savePendingPosts(posts) {
    localStorage.setItem("thrift_pending_posts", JSON.stringify(posts));
  },
  getPendingPostById(id) {
    const list = this.getPendingPosts();
    return list.find(p => p.id === parseInt(id));
  },

  // Duyệt tin đồ cũ (Chuyển sang bảng sản phẩm mở bán)
  approvePost(postId) {
    const posts = this.getPendingPosts();
    const postIdx = posts.findIndex(p => p.id === parseInt(postId));
    if (postIdx === -1) return false;

    const post = posts[postIdx];
    post.status = "approved";
    this.savePendingPosts(posts);

    // Thêm vào danh sách sản phẩm sàn
    const products = this.getProducts();
    const newProduct = {
      id: Date.now(), // ID mới
      name: post.name,
      categoryId: post.categoryId,
      categoryName: post.categoryName,
      price: post.price,
      originalPrice: post.originalPrice,
      condition: post.condition,
      conditionPercent: post.conditionPercent,
      sellerId: post.sellerId,
      sellerName: post.sellerName,
      sellerPhone: post.sellerPhone,
      sellerAddress: post.sellerAddress,
      stock: post.stock || 1,
      soldCount: 0,
      image: post.image || (post.images && post.images[0]) || "",
      images: (post.images && post.images.length) ? post.images : (post.image ? [post.image] : []),
      videos: (post.videos && post.videos.length) ? post.videos : [],
      description: post.description,
      conditionDetails: post.conditionDetails,
      featured: false,
      status: "approved",
      createdAt: new Date().toLocaleString("vi-VN")
    };
    products.unshift(newProduct);
    this.saveProducts(products);
    return true;
  },

  // Lấy avatar chuẩn riêng biệt theo tên hoặc ID người dùng/người bán (ảnh người thật, không dùng ui-avatars)
  getAvatarForName(name) {
    if (!name) return "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&q=80";
    const n = String(name).toLowerCase().trim();
    if (n === "3" || n === "9" || n.includes("thành") || n.includes("thanh")) return "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80"; // Chung Tấn Thành (Nam)
    if (n === "4" || n === "11" || n.includes("dương") || n.includes("duong")) return "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80"; // Đặng Thuỳ Dương (Nữ)
    if (n === "5" || n === "10" || n.includes("trí") || n.includes("tri")) return "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80"; // Hoàng Minh Trí (Nam)
    if (n === "6" || n === "12" || n.includes("kiệt") || n.includes("kiet")) return "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&q=80"; // Nguyễn Tuấn Kiệt (Nam)
    if (n === "7" || n === "13" || n.includes("vy")) return "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&q=80"; // Phạm Thảo Vy (Nữ)
    if (n === "8" || n === "14" || n.includes("trọng") || n.includes("trong")) return "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=150&q=80"; // Vũ Đình Trọng (Nam)
    if (n === "9" || n === "15" || n.includes("dung")) return "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&q=80"; // Lê Mỹ Dung (Nữ)
    if (n === "2" || n.includes("khoa") || n.includes("mẫu")) return "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80"; // Người Dùng Mẫu
    if (n === "1" || n.includes("admin") || n.includes("quản trị")) return "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80"; // Ban Quản Trị
    return "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&q=80";
  },

  // Lấy thông tin người dùng / người bán theo ID, Email hoặc Tên
  getUserById(identifier) {
    if (!identifier) return null;
    const users = JSON.parse(localStorage.getItem("thrift_users")) || DEFAULT_USERS;
    const str = String(identifier).toLowerCase().trim();
    let user = users.find(u => String(u.id) === str || (u.email && u.email.toLowerCase() === str) || (u.name && u.name.toLowerCase() === str));
    if (user) {
      if (!user.avatar || user.avatar.includes('ui-avatars.com')) {
        user.avatar = this.getAvatarForName(user.name || user.id);
      }
      return user;
    }

    // Tìm trong DEFAULT_USERS nếu localStorage chưa đồng bộ
    user = DEFAULT_USERS.find(u => String(u.id) === str || (u.email && u.email.toLowerCase() === str) || (u.name && u.name.toLowerCase() === str));
    if (user) return user;

    // Nếu chưa có trong bảng user, tìm trong danh sách sản phẩm để trích xuất hồ sơ người bán
    const prods = (typeof this.getProducts === "function") ? this.getProducts() : (JSON.parse(localStorage.getItem("thrift_products")) || []);
    const p = prods.find(item => String(item.sellerId) === str || (item.sellerName && item.sellerName.toLowerCase() === str) || (item.sellerEmail && item.sellerEmail.toLowerCase() === str));
    if (p) {
      return {
        id: p.sellerId || identifier,
        name: p.sellerName || "Thành viên ThriftShop",
        email: p.sellerEmail || "user@thriftshop.vn",
        phone: p.sellerPhone || "0903124567",
        address: p.sellerAddress || "TP. Hồ Chí Minh",
        avatar: (!p.sellerAvatar || p.sellerAvatar.includes('ui-avatars.com')) ? this.getAvatarForName(p.sellerName || p.sellerId) : p.sellerAvatar,
        bio: "Thành viên mua bán & trao đổi đồ cũ uy tín trên sàn ThriftShop.",
        role: "customer"
      };
    }
    return {
      id: identifier,
      name: isNaN(identifier) ? identifier : ("Người bán #" + identifier),
      avatar: this.getAvatarForName(identifier),
      role: "customer"
    };
  },

  // Lấy các bài đăng ĐANG MỞ BÁN của một người dùng / người bán
  // TUÂN THỦ QUY TẮC BẮT BUỘC: Những thứ đã bán (isSold / status: 'sold') hay đã xoá (deleted) TUYỆT ĐỐI KHÔNG HIỆN!
  getSellerActiveProducts(identifier) {
    if (!identifier) return [];
    const prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    const str = String(identifier).toLowerCase().trim();

    return prods.filter(p => {
      // Khớp theo ID người bán, email người bán hoặc tên người bán
      const isMatch = (String(p.sellerId) === str) ||
                      (p.sellerEmail && p.sellerEmail.toLowerCase() === str) ||
                      (p.sellerName && p.sellerName.toLowerCase() === str) ||
                      (p.userId && String(p.userId) === str);

      // BẮT BUỘC: Không hiện đồ đã bán hay đã xoá
      const isSold = p.isSold === true || p.status === 'sold';
      const isDeleted = p.deleted === true;
      const isApproved = p.status === 'approved' || !p.status;

      return isMatch && !isSold && !isDeleted && isApproved;
    });
  },

  // Từ chối tin đồ cũ kèm lý do
  rejectPost(postId, reason) {
    const posts = this.getPendingPosts();
    const post = posts.find(p => p.id === parseInt(postId));
    if (!post) return false;
    post.status = "rejected";
    post.rejectReason = reason || "Thông tin món đồ chưa rõ ràng hoặc hình ảnh mờ.";
    this.savePendingPosts(posts);
    return true;
  },

  // Giỏ hàng
  getCart() {
    return JSON.parse(localStorage.getItem("thrift_cart")) || [];
  },
  saveCart(cart) {
    localStorage.setItem("thrift_cart", JSON.stringify(cart));
    // Phát event để cập nhật badge
    window.dispatchEvent(new Event("cartUpdated"));
  },
  getCartCount() {
    const cart = this.getCart();
    return cart.reduce((sum, item) => sum + item.quantity, 0);
  },
  addToCart(productId, quantity = 1) {
    const product = this.getProductById(productId);
    if (!product || product.stock <= 0) {
      return { success: false, message: "Sản phẩm đã hết hàng hoặc không tồn tại!" };
    }

    const cart = this.getCart();
    const existing = cart.find(i => i.productId === parseInt(productId));

    if (existing) {
      if (existing.quantity + quantity > product.stock) {
        return { success: false, message: `Số lượng món đồ thanh lý trong kho chỉ còn ${product.stock} món!` };
      }
      existing.quantity += quantity;
    } else {
      if (quantity > product.stock) {
        return { success: false, message: `Số lượng món đồ thanh lý trong kho chỉ còn ${product.stock} món!` };
      }
      cart.push({ productId: parseInt(productId), quantity: quantity });
    }

    this.saveCart(cart);
    return { success: true, message: "Đã thêm món đồ vào giỏ hàng thành công!" };
  },
  updateCartQuantity(productId, newQty) {
    const product = this.getProductById(productId);
    if (!product) return false;

    let cart = this.getCart();
    const item = cart.find(i => i.productId === parseInt(productId));
    if (!item) return false;

    if (newQty <= 0) {
      this.removeFromCart(productId);
      return true;
    }

    if (newQty > product.stock) {
      alert(`Món đồ cũ này chỉ có duy nhất ${product.stock} sản phẩm trong kho!`);
      item.quantity = product.stock;
    } else {
      item.quantity = newQty;
    }

    this.saveCart(cart);
    return true;
  },
  removeFromCart(productId) {
    let cart = this.getCart();
    cart = cart.filter(i => i.productId !== parseInt(productId));
    this.saveCart(cart);
    return true;
  },
  clearCart() {
    this.saveCart([]);
  },

  // Đơn hàng
  getOrders() {
    return JSON.parse(localStorage.getItem("thrift_orders")) || DEFAULT_ORDERS;
  },
  saveOrders(orders) {
    localStorage.setItem("thrift_orders", JSON.stringify(orders));
  },
  getOrderById(id) {
    const list = this.getOrders();
    return list.find(o => o.id === id);
  },
  createOrder(orderData) {
    const orders = this.getOrders();
    const newOrder = {
      id: "DH" + Math.floor(100000 + Math.random() * 900000),
      orderDate: new Date().toLocaleString("vi-VN"),
      orderStatus: "pending",
      orderStatusText: "Chờ duyệt đơn",
      ...orderData
    };
    orders.unshift(newOrder);
    this.saveOrders(orders);

    // Cập nhật giảm tồn kho các sản phẩm đã mua
    const products = this.getProducts();
    orderData.items.forEach(item => {
      const prod = products.find(p => p.id === item.productId);
      if (prod) {
        prod.stock = Math.max(0, prod.stock - item.quantity);
        prod.soldCount = (prod.soldCount || 0) + item.quantity;
      }
    });
    this.saveProducts(products);

    // Xoá giỏ hàng
    this.clearCart();
    return newOrder;
  },

  // Người dùng & Auth
  getUsers() {
    return JSON.parse(localStorage.getItem("thrift_users")) || DEFAULT_USERS;
  },
  saveUsers(users) {
    localStorage.setItem("thrift_users", JSON.stringify(users));
  },
  getCurrentUser() {
    try {
      const u = localStorage.getItem("thrift_current_user");
      return (u && u !== "null" && u !== "undefined") ? JSON.parse(u) : null;
    } catch (e) {
      return null;
    }
  },
  isLoggedIn() {
    return !!this.getCurrentUser();
  },
  setCurrentUser(user) {
    if (user) {
      localStorage.setItem("thrift_current_user", JSON.stringify(user));
    } else {
      localStorage.removeItem("thrift_current_user");
    }
    window.dispatchEvent(new Event("userChanged"));
  },
  login(email, password) {
    const users = this.getUsers();
    const user = users.find(u => u.email.toLowerCase() === email.toLowerCase() && u.password === password);
    if (user) {
      sessionStorage.setItem("thrift_session_active", "true");
      this.setCurrentUser(user);
      return { success: true, user: user };
    }
    return { success: false, message: "Sai email hoặc mật khẩu!" };
  },
  logout() {
    sessionStorage.removeItem("thrift_session_active");
    this.setCurrentUser(null);
  },
  register(userData) {
    const users = this.getUsers();
    if (users.some(u => u.email.toLowerCase() === userData.email.toLowerCase())) {
      return { success: false, message: "Email này đã được đăng ký tài khoản!" };
    }
    const newUser = {
      id: Date.now(),
      role: "customer",
      ...userData
    };
    users.push(newUser);
    this.saveUsers(users);
    return { success: true, user: newUser };
  },

  // === QUẢN LÝ TIN ĐÃ YÊU THÍCH (THEO SẢN PHẨM) ===
  getFavoritesKey() {
    const user = this.getCurrentUser();
    return user ? `thrift_favs_${user.id}` : 'thrift_favs_guest';
  },
  getFavorites() {
    return JSON.parse(localStorage.getItem(this.getFavoritesKey()) || "[]");
  },
  saveFavorites(favList) {
    localStorage.setItem(this.getFavoritesKey(), JSON.stringify(favList));
    window.dispatchEvent(new Event("favoritesChanged"));
  },
  isFavorite(productId) {
    const pId = parseInt(productId);
    return this.getFavorites().includes(pId);
  },
  toggleFavorite(productId) {
    const pId = parseInt(productId);
    let favs = this.getFavorites();
    const idx = favs.indexOf(pId);
    let isFav = false;
    if (idx >= 0) {
      favs.splice(idx, 1);
      isFav = false;
    } else {
      favs.push(pId);
      isFav = true;
    }
    this.saveFavorites(favs);
    return { success: true, isFav: isFav, count: favs.length };
  },
  getFavoriteProducts() {
    const favIds = this.getFavorites();
    const allProds = JSON.parse(localStorage.getItem("thrift_products")) || DEFAULT_PRODUCTS;
    return allProds.filter(p => favIds.includes(p.id));
  },

  // === THIẾT LẬP HỒ SƠ & ĐỔI MẬT KHẨU TÀI KHOẢN (CHUẨN ẢNH) ===
  updateCurrentUser(updatedData) {
    let current = this.getCurrentUser();
    if (!current) return { success: false, message: "Chưa đăng nhập!" };
    
    current = { ...current, ...updatedData };
    this.setCurrentUser(current);

    // Đồng bộ vào danh sách users
    let users = this.getUsers();
    const idx = users.findIndex(u => u.id === current.id || u.email.toLowerCase() === current.email.toLowerCase());
    if (idx >= 0) {
      users[idx] = { ...users[idx], ...updatedData };
      this.saveUsers(users);
    }
    return { success: true, user: current };
  },
  changePassword(oldPassword, newPassword) {
    let current = this.getCurrentUser();
    if (!current) return { success: false, message: "Chưa đăng nhập!" };

    if (current.password !== oldPassword) {
      return { success: false, message: "Mật khẩu hiện tại không chính xác!" };
    }

    current.password = newPassword;
    this.setCurrentUser(current);

    let users = this.getUsers();
    const idx = users.findIndex(u => u.id === current.id || u.email.toLowerCase() === current.email.toLowerCase());
    if (idx >= 0) {
      users[idx].password = newPassword;
      this.saveUsers(users);
    }
    return { success: true, message: "Đổi mật khẩu thành công!" };
  },

  // === QUẢN LÝ TIN ĐĂNG BÁN CỦA TÔI (CHỈNH SỬA DUYỆT LẠI, ĐÃ BÁN, XÓA HẲN) ===
  // 1. Đánh dấu ĐÃ BÁN (Ẩn khỏi sàn nhưng vẫn giữ trong mục Tin đăng của tôi & bảo toàn đoạn chat)
  markPostSold(postId) {
    const id = parseInt(postId);
    const pending = this.getPendingPosts();
    const pPost = pending.find(p => p.id === id);
    if (pPost) {
      pPost.isSold = true;
      pPost.status = 'sold';
      this.savePendingPosts(pending);
    }

    // Ẩn món đồ này khỏi danh mục sản phẩm công khai
    let prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    prods = prods.filter(p => p.id !== id);
    this.saveProducts(prods);

    // C2C: Bảo toàn toàn bộ lịch sử đoạn chat, gắn nhãn [Đã bán] vào sản phẩm trong hộp thư
    try {
      let chats = JSON.parse(localStorage.getItem("thrift_chats")) || [];
      chats.forEach(c => {
        if (c.product && (String(c.product.id) === String(id) || String(c.productId) === String(id))) {
          c.product.isSold = true;
          c.product.status = 'sold';
          c.product.statusText = '[Đã bán]';
        }
      });
      this.saveChats(chats);
    } catch(e) {}

    return true;
  },

  // 2. Xóa bài đăng (Ẩn khỏi sàn, nhưng bảo toàn dữ liệu đoạn chat cho 2 bên tra cứu)
  deletePostPermanently(postId) {
    const id = parseInt(postId);
    let pending = this.getPendingPosts();
    pending = pending.filter(p => p.id !== id);
    this.savePendingPosts(pending);

    let prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    prods = prods.filter(p => p.id !== id);
    this.saveProducts(prods);

    // C2C: Bảo toàn toàn bộ lịch sử đoạn chat, gắn nhãn [Tin đã đóng] vào sản phẩm trong hộp thư
    try {
      let chats = JSON.parse(localStorage.getItem("thrift_chats")) || [];
      chats.forEach(c => {
        if (c.product && (String(c.product.id) === String(id) || String(c.productId) === String(id))) {
          c.product.isDeleted = true;
          c.product.status = 'deleted';
          c.product.statusText = '[Tin đã đóng]';
        }
      });
      this.saveChats(chats);
    } catch(e) {}

    return true;
  },

  // 3. Chỉnh sửa tin đăng (YÊU CẦU BẮT BUỘC: Admin duyệt lại lần nữa nếu đã duyệt)
  updatePost(updatedData) {
    const id = parseInt(updatedData.id);
    let pending = this.getPendingPosts();
    const idx = pending.findIndex(p => p.id === id);
    const oldPost = (idx >= 0) ? pending[idx] : {};

    const updatedPost = {
      ...oldPost,
      ...updatedData,
      id: id,
      status: "pending", // BẮT BUỘC CHUYỂN VỀ CHỜ ADMIN DUYỆT LẠI
      rejectReason: null,
      updatedAt: new Date().toLocaleString("vi-VN")
    };

    if (idx >= 0) {
      pending[idx] = updatedPost;
    } else {
      pending.unshift(updatedPost);
    }
    this.savePendingPosts(pending);

    // Gỡ khỏi danh sách sản phẩm mở bán công khai cho đến khi Admin duyệt lại
    let prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    prods = prods.filter(p => p.id !== id);
    this.saveProducts(prods);

    return updatedPost;
  },

  // === QUẢN LÝ LIÊN HỆ & CHAT THEO SẢN PHẨM (CHUẨN ẢNH 3) ===
  getChats() {
    const raw = localStorage.getItem("thrift_chats");
    let chats = raw ? JSON.parse(raw) : DEFAULT_CHATS;
    let modified = false;

    // Tự động kiểm tra và đồng bộ lại ảnh đại diện chuẩn của người bán trong các cuộc trò chuyện (tránh cache nhầm avatar ui-avatars)
    chats.forEach(c => {
      let isAvatarBad = !c.sellerAvatar || 
                        c.sellerAvatar.includes('ui-avatars.com') || 
                        c.sellerAvatar.length < 10;
      if (isAvatarBad) {
        let correctAvatar = null;
        if (c.sellerName) {
          const u = this.getUserById(c.sellerName);
          if (u && u.avatar && !u.avatar.includes('ui-avatars.com')) correctAvatar = u.avatar;
          if (!correctAvatar) correctAvatar = this.getAvatarForName(c.sellerName);
        }
        if (!correctAvatar && c.sellerId) {
          const u = this.getUserById(c.sellerId);
          if (u && u.avatar && !u.avatar.includes('ui-avatars.com')) correctAvatar = u.avatar;
        }
        if (!correctAvatar && c.product) {
          const p = (typeof this.getProductById === 'function') ? this.getProductById(c.product.id) : null;
          if (p && p.sellerAvatar && !p.sellerAvatar.includes('ui-avatars.com')) correctAvatar = p.sellerAvatar;
        }
        if (!correctAvatar) {
          correctAvatar = this.getAvatarForName(c.sellerName || c.sellerId);
        }
        c.sellerAvatar = correctAvatar;
        modified = true;
      }
    });

    if (modified) {
      localStorage.setItem("thrift_chats", JSON.stringify(chats));
    }
    return chats;
  },
  deleteChat(chatId) {
    let chats = this.getChats();
    const target = chats.find(c => c.id === chatId);
    chats = chats.filter(c => c.id !== chatId);
    this.saveChats(chats);

    if (target && target.product && target.product.id) {
      try {
        fetch('/Cart/DeleteChat', {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: `productId=${encodeURIComponent(target.product.id)}`
        }).catch(() => {});
      } catch (e) {}
    }
    return true;
  },
  saveChats(chats) {
    localStorage.setItem("thrift_chats", JSON.stringify(chats));
    window.dispatchEvent(new Event("chatsUpdated"));
  },
  getChatById(chatId) {
    const chats = this.getChats();
    return chats.find(c => c.id === chatId);
  },
  getChatByProductId(productId) {
    const chats = this.getChats();
    return chats.find(c => c.product && c.product.id === parseInt(productId));
  },
  getUnreadChatCount() {
    const chats = this.getChats();
    return chats.filter(c => c.unread).length;
  },
  sendChatMessage(chatId, text, sender = "buyer") {
    const chats = this.getChats();
    const chat = chats.find(c => c.id === chatId);
    if (!chat) return null;
    const now = new Date();
    const timeStr = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
    const newMsg = {
      sender: sender,
      text: text,
      time: timeStr
    };
    chat.messages.push(newMsg);
    chat.lastMessage = text;
    chat.lastTime = "Vừa xong";
    this.saveChats(chats);
    return newMsg;
  },
  startOrGetChatForProduct(product) {
    if (!product) return null;
    const chats = this.getChats();
    const prodIdStr = String(product.id || '');
    let chat = chats.find(c => c.product && String(c.product.id) === prodIdStr);
    const currentUser = (typeof this.getCurrentUser === 'function') ? this.getCurrentUser() : null;

    // Xác định đúng ảnh đại diện chuẩn của người bán (người muốn chat)
    let sellerAvatar = product.sellerAvatar;
    if (!sellerAvatar || sellerAvatar.includes('ui-avatars.com') || sellerAvatar.length < 10) {
      if (product.sellerName) {
        const u = this.getUserById(product.sellerName);
        if (u && u.avatar && !u.avatar.includes('ui-avatars.com')) sellerAvatar = u.avatar;
        if (!sellerAvatar) sellerAvatar = this.getAvatarForName(product.sellerName);
      }
      if (!sellerAvatar && product.sellerId) {
        const u = this.getUserById(product.sellerId);
        if (u && u.avatar && !u.avatar.includes('ui-avatars.com')) sellerAvatar = u.avatar;
      }
      if (!sellerAvatar) {
        sellerAvatar = this.getAvatarForName(product.sellerId || product.sellerName);
      }
    }

    if (!chat) {
      chat = {
        id: "chat-" + Date.now(),
        sellerId: product.sellerId || 99,
        sellerName: product.sellerName || "Người bán",
        sellerAvatar: sellerAvatar || "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
        sellerPhone: product.sellerPhone || "0901234567",
        sellerEmail: product.sellerEmail || "",
        buyerId: currentUser ? currentUser.id : 2,
        buyerName: currentUser ? currentUser.name : "Bạn",
        buyerAvatar: currentUser ? (currentUser.avatar || "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80") : "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
        isOnline: true,
        tab: "all",
        unread: false,
        product: {
          id: product.id,
          name: product.name,
          price: product.price,
          image: product.image,
          statusText: "Tin đang mở bán · Còn hàng"
        },
        lastMessage: "",
        lastTime: "",
        messages: []
      };
      chats.unshift(chat);
      this.saveChats(chats);
    } else {
      // Cập nhật lại thông tin người bán và vật muốn mua chuẩn xác
      if (product.sellerName) chat.sellerName = product.sellerName;
      if (sellerAvatar && !sellerAvatar.includes('ui-avatars.com')) chat.sellerAvatar = sellerAvatar;
      if (product.sellerPhone) chat.sellerPhone = product.sellerPhone;
      if (!chat.product) {
        chat.product = {
          id: product.id,
          name: product.name,
          price: product.price,
          image: product.image,
          statusText: "Tin đang mở bán · Còn hàng"
        };
      } else {
        if (product.name) chat.product.name = product.name;
        if (product.price) chat.product.price = product.price;
        if (product.image) chat.product.image = product.image;
      }
      this.saveChats(chats);
    }
    return chat;
  }
};

// Tự động khởi tạo dữ liệu
initThriftData();

```

---

## wwwroot/js/main.js

```javascript
/**
 * main.js - Common UI & Event Handlers for Client Pages
 * Chủ đề 30: Website trao đổi / Bán đồ cũ (Thrift Shop / Secondhand)
 */

// Hàm mở / đóng dropdown menu người dùng khi click (CHỈ MỞ KHI CLICK, KHÔNG MỞ KHI HOVER)
window.toggleUserDropdown = function(event) {
  if (event) {
    if (typeof event.preventDefault === "function") event.preventDefault();
    if (typeof event.stopPropagation === "function") event.stopPropagation();
  }
  const container = document.getElementById("userDropdownContainer") || document.querySelector(".user-dropdown");
  if (!container) return;
  const menu = document.getElementById("userDropdownMenu") || container.querySelector(".dropdown-menu");
  
  const isOpen = container.classList.contains("show") || container.classList.contains("open") || (menu && menu.classList.contains("show"));
  
  // Đóng tất cả dropdown khác nếu có
  document.querySelectorAll(".user-dropdown, .dropdown").forEach(el => el.classList.remove("show", "open"));
  document.querySelectorAll(".dropdown-menu").forEach(el => {
    el.classList.remove("show", "open");
    el.style.display = "";
  });

  if (!isOpen) {
    container.classList.add("show", "open");
    if (menu) {
      menu.classList.add("show", "open");
      menu.style.display = "block";
    }
  }
};

$(document).ready(function() {
  updateCartBadge();
  renderUserStatus();
  initHeaderSearch();

  // Bắt sự kiện click mở / đóng dropdown menu người dùng
  $(document).on("click", "#userDropdownBtn, .user-auth-btn, .user-dropdown > a", function(e) {
    window.toggleUserDropdown(e);
  });

  // Đóng khi click ra bên ngoài
  $(document).on("click", function(e) {
    if (!e.target.closest("#userDropdownContainer") && !e.target.closest(".user-dropdown")) {
      document.querySelectorAll(".user-dropdown, .dropdown").forEach(el => el.classList.remove("show", "open"));
      document.querySelectorAll(".dropdown-menu").forEach(el => {
        el.classList.remove("show", "open");
        el.style.display = "";
      });
    }
  });

  // Khi click vào các mục bên trong dropdown-menu
  $(document).on("click", "#userDropdownMenu, .dropdown-menu", function(e) {
    if (e.target.closest(".dropdown-item")) {
      document.querySelectorAll(".user-dropdown, .dropdown").forEach(el => el.classList.remove("show", "open"));
      document.querySelectorAll(".dropdown-menu").forEach(el => {
        el.classList.remove("show", "open");
        el.style.display = "";
      });
    } else {
      e.stopPropagation();
    }
  });

  // Lắng nghe sự kiện cập nhật giỏ hàng hoặc người dùng hoặc chat
  window.addEventListener("cartUpdated", updateCartBadge);
  window.addEventListener("userChanged", renderUserStatus);
  window.addEventListener("chatsUpdated", updateCartBadge);
});

// Cập nhật số lượng liên hệ / tin nhắn chưa đọc trên Header
function updateCartBadge() {
  const count = (typeof ThriftDB !== 'undefined' && ThriftDB.getUnreadChatCount) ? ThriftDB.getUnreadChatCount() : 2;
  const badges = $(".cart-count, #cartCount, #unreadChatCount, .badge-cart");
  if (badges.length) {
    badges.text(count > 0 ? count : 0);
  }
}

// Kiểm tra trạng thái đã đăng nhập hay chưa
window.checkUserLoggedIn = function() {
  try {
    if (typeof ThriftDB !== 'undefined' && typeof ThriftDB.isLoggedIn === 'function') {
      return ThriftDB.isLoggedIn();
    }
    const u = localStorage.getItem("thrift_current_user");
    return !!(u && u !== "null" && u !== "undefined");
  } catch (e) {
    return false;
  }
};

// Chuyển sang hộp thoại Chat với người bán của sản phẩm
window.openChatWithSeller = function(productId) {
  const isHtml = window.location.pathname.endsWith('.html');
  const loginUrl = isHtml ? 'login.html' : '/Account/Login';
  const pId = parseInt(productId);
  const chatUrl = isHtml ? `cart.html?productId=${pId}` : `/Cart?productId=${pId}`;

  // KIỂM TRA ĐĂNG NHẬP: Khi chưa đăng nhập thì lập tức chuyển hướng đến trang đăng nhập ngay
  if (!window.checkUserLoggedIn()) {
    window.location.href = `${loginUrl}?returnUrl=${encodeURIComponent(chatUrl)}`;
    return;
  }

  if (typeof ThriftDB !== 'undefined') {
    const prod = ThriftDB.getProductById(pId);
    if (prod && ThriftDB.startOrGetChatForProduct) {
      ThriftDB.startOrGetChatForProduct(prod);
    }
  }
  window.location.href = chatUrl;
};
window.handleChatWithSeller = window.openChatWithSeller;

// Hàm kiểm tra đăng nhập khi bấm vào các tính năng yêu cầu tài khoản
window.handleCheckAuthNav = function(event, targetUrl) {
  if (!window.checkUserLoggedIn()) {
    if (event) {
      if (typeof event.preventDefault === "function") event.preventDefault();
      if (typeof event.stopPropagation === "function") event.stopPropagation();
    }
    const isHtml = window.location.pathname.endsWith('.html');
    const loginUrl = isHtml ? 'login.html' : '/Account/Login';
    window.location.href = `${loginUrl}?returnUrl=${encodeURIComponent(targetUrl)}`;
    return false;
  }
  return true;
};

// Cập nhật trạng thái Đăng nhập / Hồ sơ người dùng trên Header
function renderUserStatus() {
  const user = ThriftDB.getCurrentUser();
  const container = $("#user-header-auth");
  if (!container.length) return;

  const isHtml = window.location.pathname.endsWith('.html');
  const postLink = isHtml ? 'post-item.html' : '/Products/PostItem';
  const myListingsLink = isHtml ? 'my-listings.html' : '/Products/MyListings';
  const favoritesLink = isHtml ? 'favorites.html' : '/Products/Favorites';
  const profileLink = isHtml ? 'profile.html' : '/Account/Profile';
  const sellerProfileLink = isHtml ? `seller-profile.html?userId=${encodeURIComponent(user ? (user.id || user.email) : '')}` : `/Account/SellerProfile?userId=${encodeURIComponent(user ? (user.id || user.email) : '')}`;
  const adminUrl = isHtml ? 'admin/index.html' : '/Admin';
  const loginUrl = isHtml ? 'login.html' : '/Account/Login';
  const registerUrl = isHtml ? 'register.html' : '/Account/Register';

  if (user) {
    let adminLink = '';
    // CHỈ HIỂN THỊ LINK QUẢN TRỊ ADMIN CHO TÀI KHOẢN ADMIN
    if (user.role === 'admin' || user.email === 'admin@thriftshop.vn') {
      adminLink = `
        <a class="dropdown-item text-warning font-weight-bold" href="${adminUrl}">
          <i class="fas fa-user-shield mr-2"></i>Trang Quản Trị (Admin)
        </a>
        <div class="dropdown-divider"></div>
      `;
    }

    container.html(`
      <div class="dropdown user-dropdown" id="userDropdownContainer">
        <a class="user-auth-btn" id="userDropdownBtn" href="javascript:void(0)" onclick="toggleUserDropdown(event)" role="button" aria-haspopup="true" aria-expanded="false" title="${user.name}">
          ${user.avatar ? `<img src="${user.avatar}" alt="${user.name}" style="width: 22px; height: 22px; border-radius: 50%; object-fit: cover; border: 1.5px solid #22c55e;">` : `<i class="fas fa-user-circle fa-lg text-success" style="font-size: 16px;"></i>`}
          <span class="user-display-name">${user.name}</span>
          <i class="fas fa-chevron-down" style="font-size: 10px; opacity: 0.7; margin-left: 2px;"></i>
        </a>
        <div class="dropdown-menu dropdown-menu-right mt-2" id="userDropdownMenu">
          <a class="dropdown-header user-profile-header-link" href="${sellerProfileLink}" style="display: block; text-decoration: none; padding: 10px 14px; border-radius: 8px; background: rgba(255, 255, 255, 0.05); margin-bottom: 6px; transition: all 0.2s; cursor: pointer;" title="Xem trang cá nhân & bài đăng bán của tôi">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2px;">
              <small class="text-muted" style="font-size: 11px; text-transform: uppercase; font-weight: 600; letter-spacing: 0.5px;">Tài khoản đăng nhập:</small>
              <span style="font-size: 11px; color: #22c55e; font-weight: 600; display: inline-flex; align-items: center; gap: 4px;">
                Trang cá nhân <i class="fas fa-arrow-right" style="font-size: 9px;"></i>
              </span>
            </div>
            <strong class="text-white" style="font-size: 13.5px; word-break: break-all; display: block;">${user.email}</strong>
          </a>
          <div class="dropdown-divider"></div>
          ${adminLink}
          <a class="dropdown-item" href="${postLink}">
            <i class="fas fa-plus-circle text-warning mr-2"></i>Đăng bán đồ cũ mới
          </a>
          <a class="dropdown-item" href="${myListingsLink}">
            <i class="fas fa-tags text-info mr-2"></i>Tin đăng bán của tôi
          </a>
          <a class="dropdown-item" href="${favoritesLink}">
            <i class="fas fa-heart text-danger mr-2"></i>Tin đã yêu thích
          </a>
          <a class="dropdown-item" href="${profileLink}">
            <i class="fas fa-user-cog text-secondary mr-2"></i>Cài đặt tài khoản
          </a>
          <div class="dropdown-divider"></div>
          <a class="dropdown-item text-muted" href="javascript:void(0)" onclick="handleLogout()">
            <i class="fas fa-sign-out-alt mr-2"></i>Đăng xuất
          </a>
        </div>
      </div>
    `);
  } else {
    container.html(`
      <div class="d-flex align-items-center">
        <a href="${loginUrl}" class="btn btn-link text-white font-weight-bold p-0 mr-3 text-decoration-none">
          <i class="fas fa-sign-in-alt mr-1"></i>Đăng nhập
        </a>
        <a href="${registerUrl}" class="btn btn-sm btn-outline-success font-weight-bold">
          <i class="fas fa-user-plus mr-1"></i>Đăng ký
        </a>
      </div>
    `);
  }
}

// Hàm chuyển đổi trạng thái yêu thích món đồ (Dấu tim)
window.toggleProductFavorite = function(productId, event) {
  if (event) {
    if (typeof event.preventDefault === "function") event.preventDefault();
    if (typeof event.stopPropagation === "function") event.stopPropagation();
  }
  const isHtml = window.location.pathname.endsWith('.html');
  const loginUrl = isHtml ? 'login.html' : '/Account/Login';

  // KIỂM TRA ĐĂNG NHẬP: Chưa đăng nhập thì chuyển hướng ngay sang Login
  if (!window.checkUserLoggedIn()) {
    const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
    window.location.href = `${loginUrl}?returnUrl=${returnUrl}`;
    return false;
  }

  // Gửi cập nhật trực tiếp lên Backend ASP.NET Core SQL Server
  fetch('/Products/ToggleFavorite?productId=' + productId, { method: 'POST' })
    .then(r => r.json())
    .then(data => {
      if (data.requireLogin) {
        const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
        window.location.href = `${loginUrl}?returnUrl=${returnUrl}`;
        return;
      }
      if (data.success) {
        const isFav = data.isFav;
        if (typeof ThriftDB !== 'undefined') {
          const localIsFav = ThriftDB.isFavorite(productId);
          if (localIsFav !== isFav) ThriftDB.toggleFavorite(productId);
        }
        const btns = document.querySelectorAll(`[data-fav-id="${productId}"]`);
        btns.forEach(btn => {
          const icon = btn.querySelector('i');
          const span = btn.querySelector('span');
          if (isFav) {
            btn.classList.add('active');
            if (icon) icon.className = 'fas fa-heart text-danger mr-1';
            if (span) span.textContent = 'Đã thích';
          } else {
            btn.classList.remove('active');
            if (icon) icon.className = 'far fa-heart mr-1';
            if (span) span.textContent = 'Yêu thích';
          }
        });

        if (typeof showToast === 'function') {
          showToast(isFav ? 'Đã thêm món đồ vào Tin đã yêu thích ❤️' : 'Đã bỏ món đồ khỏi Tin đã yêu thích', isFav ? 'success' : 'info');
        }
      }
    })
    .catch(() => {
      // Fallback cục bộ nếu mất mạng
      if (typeof ThriftDB !== 'undefined') {
        const res = ThriftDB.toggleFavorite(productId);
        const btns = document.querySelectorAll(`[data-fav-id="${productId}"]`);
        btns.forEach(btn => {
          const icon = btn.querySelector('i');
          const span = btn.querySelector('span');
          if (res.isFav) {
            btn.classList.add('active');
            if (icon) icon.className = 'fas fa-heart text-danger mr-1';
            if (span) span.textContent = 'Đã thích';
          } else {
            btn.classList.remove('active');
            if (icon) icon.className = 'far fa-heart mr-1';
            if (span) span.textContent = 'Yêu thích';
          }
        });
      }
    });

  return true;
};

// Xử lý đăng xuất đồng bộ cả Client (localStorage) và Server (Session/Cookie)
function handleLogout() {
  if (confirm("Bạn có chắc chắn muốn đăng xuất tài khoản?")) {
    if (typeof ThriftDB !== 'undefined' && typeof ThriftDB.logout === 'function') {
      ThriftDB.logout();
    }
    try {
      localStorage.removeItem('thrift_current_user');
      sessionStorage.clear();
    } catch(e){}
    showToast("Đã đăng xuất tài khoản thành công!", "info");
    setTimeout(function() {
      const isHtml = window.location.pathname.endsWith('.html');
      window.location.href = isHtml ? 'index.html' : '/Account/Logout';
    }, 300);
  }
}

// Xử lý tìm kiếm từ Header
function initHeaderSearch() {
  $("#header-search-form").on("submit", function(e) {
    e.preventDefault();
    const keyword = $("#header-search-input").val().trim();
    const categoryId = $("#header-search-category").val();
    let url = "products.html?";
    if (keyword) url += "search=" + encodeURIComponent(keyword) + "&";
    if (categoryId) url += "category=" + encodeURIComponent(categoryId);
    window.location.href = url;
  });

  // Load danh mục vào dropdown tìm kiếm header nếu có
  const catSelect = $("#header-search-category");
  if (catSelect.length) {
    const cats = ThriftDB.getCategories();
    cats.forEach(c => {
      catSelect.append(`<option value="${c.id}">${c.name}</option>`);
    });
  }
}

// Hiển thị thông báo Toast
function showToast(message, type = "success") {
  let toastContainer = $("#toast-container");
  if (!toastContainer.length) {
    $("body").append('<div id="toast-container"></div>');
    toastContainer = $("#toast-container");
  }

  let icon = "fa-check-circle";
  let bg = "#1b4332";
  if (type === "error" || type === "danger") {
    icon = "fa-exclamation-circle";
    bg = "#d90429";
  } else if (type === "warning") {
    icon = "fa-exclamation-triangle";
    bg = "#e76f51";
  } else if (type === "info") {
    icon = "fa-info-circle";
    bg = "#2a9d8f";
  }

  const toastId = "toast_" + Date.now();
  const html = `
    <div id="${toastId}" class="custom-toast" style="background-color: ${bg};">
      <i class="fas ${icon} fa-lg"></i>
      <div>${message}</div>
    </div>
  `;

  toastContainer.append(html);
  setTimeout(function() {
    $("#" + toastId).fadeOut(400, function() {
      $(this).remove();
    });
  }, 3000);
}

// Hàm thêm nhanh vào giỏ hàng từ thẻ sản phẩm
function handleAddToCart(productId, event) {
  if (event) event.preventDefault();
  const res = ThriftDB.addToCart(productId, 1);
  if (res.success) {
    showToast(res.message, "success");
  } else {
    showToast(res.message, "warning");
  }
}
```

---

## wwwroot/js/admin.js

```javascript
/**
 * admin.js - Shared scripts for Admin Dashboard & Management Pages
 */

$(document).ready(function() {
  updateAdminPendingBadge();
  checkAdminAuth();
});

// Cập nhật số lượng tin đăng đồ cũ đang chờ duyệt trên Sidebar & Topbar
function updateAdminPendingBadge(overrideCount) {
  let pendingCount = 0;
  if (typeof overrideCount === "number") {
    pendingCount = overrideCount;
  } else if (typeof window.serverRealPendingCount === "number") {
    pendingCount = window.serverRealPendingCount;
  } else if (typeof serverPosts !== "undefined" && Array.isArray(serverPosts)) {
    pendingCount = serverPosts.filter(p => (p.Status || p.status) === "pending").length;
  } else if (typeof ThriftDB !== 'undefined' && ThriftDB.getPendingPosts) {
    const posts = ThriftDB.getPendingPosts();
    pendingCount = posts.filter(p => (p.Status || p.status || "").toLowerCase() === "pending").length;
  }

  const badge = $("#admin-pending-badge");
  if (badge.length) {
    badge.text(pendingCount);
    if (pendingCount > 0) {
      badge.show();
    } else {
      badge.text("0");
    }
  }

  const topbarBadge = $("#admin-topbar-pending-alert");
  if (topbarBadge.length) {
    topbarBadge.text(pendingCount);
  }
}

// Kiểm tra nghiêm ngặt quyền Admin: CHỈ TÀI KHOẢN ADMIN MỚI ĐƯỢC TRUY CẬP, CÒN LẠI CẤM VÀO
function checkAdminAuth() {
  const user = (typeof ThriftDB !== 'undefined' && ThriftDB.getCurrentUser) ? ThriftDB.getCurrentUser() : JSON.parse(localStorage.getItem('thrift_current_user') || 'null');
  const isHtml = window.location.pathname.endsWith('.html');
  const loginUrl = isHtml ? '../login.html' : '/Account/Login';
  const homeUrl = isHtml ? '../index.html' : '/';
  const currentPath = window.location.pathname + window.location.search;

  // 1. Chưa đăng nhập -> Chặn ngay và chuyển hướng sang trang đăng nhập
  if (!user) {
    alert("Vui lòng đăng nhập tài khoản Ban Quản Trị (admin@thriftshop.vn) để truy cập trang quản trị!");
    window.location.replace(`${loginUrl}?returnUrl=${encodeURIComponent(currentPath)}`);
    return false;
  }

  // 2. Đã đăng nhập nhưng KHÔNG PHẢI TÀI KHOẢN ADMIN (ví dụ user thường) -> CẤM VÀO!
  if (user.role !== "admin" && user.email !== "admin@thriftshop.vn") {
    alert("CẢNH BÁO: Bạn không có quyền truy cập trang quản trị! Chỉ tài khoản Ban Quản Trị (admin@thriftshop.vn) mới được phép vào.");
    window.location.replace(homeUrl);
    return false;
  }

  return true;
}

// Render Sidebar chung cho trang Admin
function renderAdminSidebar(activeMenu) {
  const posts = ThriftDB.getPendingPosts();
  const pendingCount = posts.filter(p => p.status === "pending").length;

  return `
    <div class="admin-sidebar">
      <div class="sidebar-header">
        <a href="index.html" class="sidebar-brand">
          <i class="fas fa-recycle fa-lg"></i>
          <span>ThriftAdmin</span>
        </a>
      </div>
      <ul class="sidebar-menu">
        <li class="menu-header">Chức Năng Chính</li>
        <li class="${activeMenu === 'dashboard' ? 'active' : ''}">
          <a href="index.html">
            <i class="fas fa-tachometer-alt"></i>Bảng Điều Khiển
          </a>
        </li>
        <li class="${activeMenu === 'approve' ? 'active' : ''}">
          <a href="approve-products.html" class="text-warning">
            <i class="fas fa-clipboard-check"></i>Duyệt Tin Đăng
            <span class="badge badge-warning" id="admin-pending-badge">${pendingCount}</span>
          </a>
        </li>
        <li class="${activeMenu === 'categories' ? 'active' : ''}">
          <a href="categories.html">
            <i class="fas fa-th-large text-success"></i>Quản Lý Danh Mục
          </a>
        </li>
        <li class="${activeMenu === 'products' ? 'active' : ''}">
          <a href="products.html">
            <i class="fas fa-boxes"></i>Quản Lý Tin Đăng
          </a>
        </li>
        <li class="${activeMenu === 'users' ? 'active' : ''}">
          <a href="users.html">
            <i class="fas fa-users-cog"></i>Quản Lý User
          </a>
        </li>

        <li class="menu-header">Hệ Thống</li>
        <li>
          <a href="../index.html" target="_blank" class="text-info">
            <i class="fas fa-external-link-alt"></i>Xem Trang Khách Hàng
          </a>
        </li>
        <li>
          <a href="javascript:void(0)" onclick="handleAdminLogout()">
            <i class="fas fa-sign-out-alt"></i>Đăng Xuất
          </a>
        </li>
      </ul>
    </div>
  `;
}

// Render Topbar chung cho trang Admin
function renderAdminTopbar(pageTitle) {
  const posts = ThriftDB.getPendingPosts();
  const pendingCount = posts.filter(p => p.status === "pending").length;

  return `
    <header class="admin-topbar">
      <div>
        <h5 class="mb-0 font-weight-bold text-dark">${pageTitle}</h5>
      </div>
      <div class="topbar-right">
        <a href="approve-products.html" class="btn btn-sm btn-outline-warning font-weight-bold mr-2">
          <i class="fas fa-bell mr-1"></i>Tin chờ duyệt: <span class="badge badge-danger">${pendingCount}</span>
        </a>
        <a href="../index.html" class="btn-client-preview mr-3">
          <i class="fas fa-store mr-1"></i>Trang Mua Sắm
        </a>
        <div class="admin-user-badge">
          <div class="admin-avatar">AD</div>
          <div>
            <div class="small font-weight-bold">Quản Trị Viên</div>
            <div class="text-muted" style="font-size: 11px;">admin@thriftshop.vn</div>
          </div>
        </div>
      </div>
    </header>
  `;
}

function handleAdminLogout() {
  try {
    localStorage.removeItem("thrift_current_user");
    sessionStorage.clear();
  } catch(e) {}
  const isHtml = window.location.pathname.endsWith('.html');
  window.location.href = isHtml ? "../index.html" : "/Account/Logout";
}
```

---

## wwwroot/js/page-transition.js

```javascript
﻿/* =========================================
   CAMERA917 - PAGE TRANSITION
========================================= */

document.addEventListener("DOMContentLoaded", function () {

    /* ================================
       HIỆU ỨNG KHI TRANG VỪA MỞ
    ================================= */

    requestAnimationFrame(function () {

        document.body.classList.add("page-loaded");

    });


    /* ================================
       HIỆU ỨNG KHI BẤM LINK
    ================================= */

    document.querySelectorAll("a").forEach(function (link) {

        link.addEventListener("click", function (event) {

            const href = this.getAttribute("href");


            /* Không xử lý link đặc biệt */

            if (
                !href ||
                href.startsWith("#") ||
                href.startsWith("http") ||
                href.startsWith("mailto:") ||
                href.startsWith("tel:")
            ) {
                return;
            }


            /* Nếu mở tab mới thì bỏ qua */

            if (this.target === "_blank") {
                return;
            }


            /* Ngăn chuyển trang ngay */

            event.preventDefault();


            /* Chạy hiệu ứng thoát */

            document.body.classList.remove("page-loaded");

            document.body.classList.add("page-exit");


            /* Chờ animation xong rồi chuyển trang */

            setTimeout(function () {

                window.location.href = href;

            }, 350);

        });

    });

});
```

---

