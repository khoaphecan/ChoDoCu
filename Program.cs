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
builder.Services.AddControllersWithViews();

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
