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
