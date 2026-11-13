using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;

var builder = WebApplication.CreateBuilder(args);

// Đăng ký HttpContextAccessor & Session (Quản lý dữ liệu tạm & tương thích ngược)
builder.Services.AddHttpContextAccessor();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromHours(2);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});

// Đăng ký dịch vụ Cookie Authentication chuẩn công nghiệp (Persistent Session)
builder.Services.AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
    .AddCookie(options =>
    {
        options.Cookie.Name = ".ThriftShop.Auth";
        options.Cookie.HttpOnly = true;
        options.Cookie.SameSite = SameSiteMode.Lax;
        options.ExpireTimeSpan = TimeSpan.FromDays(14); // Giữ đăng nhập 14 ngày
        options.SlidingExpiration = true;              // Tự động gia hạn khi người dùng có hoạt động
        options.LoginPath = "/Account/Login";
        options.AccessDeniedPath = "/Account/AccessDenied";
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

// Middleware xác thực & phân quyền Cookie Authentication (Bắt buộc UseAuthentication trước UseAuthorization)
app.UseAuthentication();
app.UseAuthorization();

// Sử dụng Session middleware cho dữ liệu tạm
app.UseSession();

// Tự động bù đắp (Re-hydrate) dữ liệu Session từ Cookie Claims khi Server vừa Restart
app.Use(async (context, next) =>
{
    if (context.User.Identity?.IsAuthenticated == true && !context.Session.IsLoggedIn())
    {
        var idClaim = context.User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
        if (int.TryParse(idClaim, out int uid))
        {
            context.Session.SetInt32(ThriftShop.Common.SessionExtensions.SessionIdKey, uid);
            context.Session.SetString(ThriftShop.Common.SessionExtensions.SessionNameKey, context.User.Identity?.Name ?? "");
            context.Session.SetString(ThriftShop.Common.SessionExtensions.SessionEmailKey, context.User.FindFirst(System.Security.Claims.ClaimTypes.Email)?.Value ?? "");
            context.Session.SetString(ThriftShop.Common.SessionExtensions.SessionRoleKey, context.User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value ?? "customer");
            context.Session.SetString(ThriftShop.Common.SessionExtensions.SessionAvatarKey, context.User.FindFirst("Avatar")?.Value ?? "");
        }
    }
    await next();
});

app.MapControllerRoute(
    name: "default",
    pattern: "{controller=Home}/{action=Index}/{id?}");

Console.WriteLine("==================================================================");
Console.WriteLine(" ThriftShop - Website Mua Bán Đồ Cũ Secondhand");
Console.WriteLine(" Backend ASP.NET MVC + Entity Framework + LINQ + SQL Server");
Console.WriteLine("==================================================================");

app.Run();
