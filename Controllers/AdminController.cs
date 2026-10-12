using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;

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

        // GET: /Admin (Dashboard thống kê tối giản: Duyệt tin, Quản lý tin, Quản lý User)
        public IActionResult Index()
        {
            var authCheck = CheckAdminPermission("/Admin");
            if (authCheck != null) return authCheck;

            // LINQ: Thống kê số lượng
            var pendingCount = _db.PendingPosts.Count(p => p.Status == "pending");
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
                ProductsCount = productsCount,
                UsersCount = usersCount,
                RecentPending = recentPending,
                RecentProducts = recentProducts
            };

            return View(viewModel);
        }

        // GET: /Admin/ApproveProducts (Kiểm duyệt tin đăng bán đồ cũ - Cốt lõi Đề tài 30)
        public IActionResult ApproveProducts()
        {
            var authCheck = CheckAdminPermission("/Admin/ApproveProducts");
            if (authCheck != null) return authCheck;

            // Lấy toàn bộ danh sách tin để Admin lọc theo các tabs: Chờ duyệt, Đã duyệt, Bị từ chối
            var allPosts = _db.PendingPosts.AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(allPosts);
        }

        // POST: /Admin/ApprovePost (Duyệt tin -> chuyển thành sản phẩm chính thức mở bán)
        [HttpPost]
        public IActionResult ApprovePost(int id)
        {
            var authCheck = CheckAdminPermission("/Admin/ApproveProducts");
            if (authCheck != null) return authCheck;

            var post = _db.PendingPosts.FirstOrDefault(p => p.Id == id);
            if (post == null) return Json(new { success = false, message = "Không tìm thấy tin đăng!" });

            // Kiểm tra tránh duyệt lại tạo sản phẩm trùng
            if (post.Status == "approved")
            {
                return Json(new { success = false, message = "Tin đăng này đã được duyệt mở bán trước đó!" });
            }

            post.Status = "approved";

            // Kiểm tra xem sản phẩm đã từng tồn tại chưa (tránh tạo trùng lặp)
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
                    Condition = post.Condition,
                    ConditionPercent = post.ConditionPercent,
                    ConditionDetails = post.ConditionDetails,
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
                    CreatedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                };
                _db.Products.Add(product);
            }

            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Đã duyệt thành công tin: '{post.Name}'. Món đồ đã được mở bán công khai trên sàn!";
            return Json(new { success = true });
        }

        // POST: /Admin/RejectPost (Từ chối tin kèm lý do)
        [HttpPost]
        public IActionResult RejectPost(int id, string? reason)
        {
            var authCheck = CheckAdminPermission("/Admin/ApproveProducts");
            if (authCheck != null) return authCheck;

            var post = _db.PendingPosts.FirstOrDefault(p => p.Id == id);
            if (post == null) return Json(new { success = false, message = "Không tìm thấy tin đăng!" });

            post.Status = "rejected";
            post.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Thông tin món đồ chưa rõ ràng hoặc hình ảnh mờ." : reason.Trim();

            // Nếu sản phẩm đã từng lên sàn thì ẩn đi
            var linkedProduct = _db.Products.FirstOrDefault(p => p.PendingPostId == post.Id);
            if (linkedProduct != null)
            {
                linkedProduct.Status = "rejected";
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = $"Đã từ chối tin đăng: '{post.Name}'. Lý do đã được gửi lại cho người bán.";
            return Json(new { success = true });
        }

        // GET: /Admin/Products (Quản lý tin đăng trên sàn)
        public IActionResult Products()
        {
            var authCheck = CheckAdminPermission("/Admin/Products");
            if (authCheck != null) return authCheck;

            var products = _db.Products.AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(products);
        }

        // GET: /Admin/Users (Quản lý User)
        public IActionResult Users()
        {
            var authCheck = CheckAdminPermission("/Admin/Users");
            if (authCheck != null) return authCheck;

            var users = _db.Users.AsNoTracking()
                .OrderByDescending(u => u.Id)
                .ToList();

            return View(users);
        }

        // GET: /Admin/Categories (Quản lý Danh mục)
        public IActionResult Categories()
        {
            var authCheck = CheckAdminPermission("/Admin/Categories");
            if (authCheck != null) return authCheck;

            var categories = _db.Categories.AsNoTracking().ToList();
            return View(categories);
        }

        // GET: /Admin/Inventory (Quản lý Tồn kho)
        public IActionResult Inventory()
        {
            var authCheck = CheckAdminPermission("/Admin/Inventory");
            if (authCheck != null) return authCheck;

            var inventory = _db.Products.AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToList();
            return View(inventory);
        }

        // GET: /Admin/Orders (Quản lý Đơn hàng)
        public IActionResult Orders()
        {
            var authCheck = CheckAdminPermission("/Admin/Orders");
            if (authCheck != null) return authCheck;

            var orders = _db.Orders.Include(o => o.Items).AsNoTracking()
                .OrderByDescending(o => o.CreatedAt)
                .ToList();
            return View(orders);
        }
    }
}
