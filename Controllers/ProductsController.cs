using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;

namespace ThriftShop.Controllers
{
    public class ProductsController : Controller
    {
        private readonly ThriftDbContext _db;

        public ProductsController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Products (Danh sách tin bán đồ cũ & Lọc)
        public IActionResult Index(string? search, int[]? catFilter, decimal? minPrice, decimal? maxPrice, string? sort, int page = 1)
        {
            return RedirectToAction("Index", "Home", new { search, catFilter, minPrice, maxPrice, sort, page });
        }

        // GET: /Products/Details/101 (Chi tiết sản phẩm đồ cũ & Người bán)
        public IActionResult Details(int? id)
        {
            if (!id.HasValue) return RedirectToAction("Index", "Home");

            // LINQ: Lấy chi tiết sản phẩm theo ID
            var product = _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == id.Value);
            if (product == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy món đồ hoặc tin đăng này đã bị gỡ!";
                return RedirectToAction("Index", "Home");
            }

            // LINQ: Lấy sản phẩm liên quan cùng danh mục (còn hàng)
            var related = _db.Products.AsNoTracking()
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Status == "approved" && !p.IsSold && !p.Deleted && p.Stock > 0)
                .Take(4)
                .ToList();

            // LINQ: Lấy thông tin người bán
            User? seller = null;
            if (product.SellerId > 0)
            {
                seller = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == product.SellerId);
            }
            if (seller == null && !string.IsNullOrEmpty(product.SellerEmail))
            {
                seller = _db.Users.AsNoTracking().FirstOrDefault(u => u.Email.ToLower() == product.SellerEmail.ToLower());
            }

            // Kiểm tra trạng thái yêu thích
            var currentUserId = HttpContext.Session.GetUserId();
            bool isFav = false;
            if (currentUserId.HasValue)
            {
                isFav = _db.Favorites.Any(f => f.UserId == currentUserId.Value && f.ProductId == product.Id);
            }

            // Parse danh sách ảnh
            var imagesList = new List<string>();
            if (!string.IsNullOrEmpty(product.ImagesJson))
            {
                try
                {
                    var parsed = JsonSerializer.Deserialize<List<string>>(product.ImagesJson);
                    if (parsed != null && parsed.Count > 0) imagesList.AddRange(parsed);
                }
                catch { }
            }
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
                IsFavorite = isFav
            };

            return View(viewModel);
        }

        // GET: /Products/PostItem (Trang đăng bán đồ cũ - Bắt buộc đăng nhập)
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

        // POST: /Products/PostItem (Xử lý đăng bán - Tối thiểu 3 ảnh - Chờ Admin duyệt)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult PostItem(PendingPost model, string[]? images, string[]? videos)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Phiên làm việc đã hết hạn. Vui lòng đăng nhập lại!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Products/PostItem" });
            }

            // YÊU CẦU BẮT BUỘC: Tối thiểu ít nhất 3 hình ảnh
            var validImages = images?.Where(img => !string.IsNullOrWhiteSpace(img)).ToList() ?? new List<string>();
            if (!string.IsNullOrWhiteSpace(model.Image) && !validImages.Contains(model.Image))
            {
                validImages.Insert(0, model.Image);
            }

            if (validImages.Count < 3)
            {
                ModelState.AddModelError("Image", "Yêu cầu bắt buộc: Phải đăng tối thiểu ít nhất 3 hình ảnh thực tế của món đồ (hiện có " + validImages.Count + " ảnh)!");
                ViewBag.Categories = _db.Categories.AsNoTracking().ToList();
                return View(model);
            }

            // Kiểm tra Data Annotations
            if (!ModelState.IsValid)
            {
                ViewBag.Categories = _db.Categories.AsNoTracking().ToList();
                return View(model);
            }

            // Gán thông tin người bán từ Session
            var user = _db.Users.Find(userId.Value);
            var cat = _db.Categories.Find(model.CategoryId);

            model.CategoryName = cat?.Name ?? "Đồ cũ";
            model.SellerId = user?.Id ?? userId.Value;
            model.SellerName = user?.Name ?? HttpContext.Session.GetUserName() ?? "Người bán";
            model.SellerEmail = user?.Email ?? HttpContext.Session.GetUserEmail();
            model.SellerPhone = string.IsNullOrWhiteSpace(model.SellerPhone) ? (user?.Phone ?? "0901234567") : model.SellerPhone;
            model.SellerAddress = string.IsNullOrWhiteSpace(model.SellerAddress) ? (user?.Address ?? "TP.HCM") : model.SellerAddress;
            model.SellerAvatar = user?.Avatar ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80";

            model.Image = validImages.First();
            model.ImagesJson = JsonSerializer.Serialize(validImages);

            if (videos != null && videos.Length > 0)
            {
                model.VideosJson = JsonSerializer.Serialize(videos.Where(v => !string.IsNullOrWhiteSpace(v)));
            }

            model.Status = "pending"; // Bắt buộc: Chờ Ban Quản Trị duyệt
            model.CreatedAt = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

            // Lưu vào Database qua Entity Framework
            _db.PendingPosts.Add(model);
            _db.SaveChanges();

            TempData["SuccessMessage"] = "Đăng tin thành công! Tin của bạn đã được chuyển vào hàng đợi để Ban Quản Trị kiểm duyệt chất lượng trước khi mở bán.";
            return RedirectToAction("MyListings");
        }

        // GET: /Products/MyListings (Quản lý tin đăng bán của tôi)
        public IActionResult MyListings()
        {
            var userId = HttpContext.Session.GetUserId();
            var userEmail = HttpContext.Session.GetUserEmail();

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem danh sách tin đăng của bạn!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Products/MyListings" });
            }

            // LINQ: Lấy toàn bộ tin đăng của người dùng hiện tại
            var myListings = _db.PendingPosts.AsNoTracking()
                .Where(p => p.SellerId == userId.Value || 
                           (p.SellerEmail != null && userEmail != null && p.SellerEmail.ToLower() == userEmail.ToLower()))
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(myListings);
        }

        // POST: /Products/MarkSold (Đánh dấu Đã bán: ẩn khỏi sàn nhưng vẫn lưu trong Tin của tôi)
        [HttpPost]
        public IActionResult MarkSold(int id)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return Json(new { success = false, message = "Chưa đăng nhập!" });
            var isAdmin = HttpContext.Session.IsAdmin();

            // Tìm PendingPost của đúng người bán (hoặc Admin)
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
                return Json(new { success = false, message = "Không tìm thấy món đồ hoặc bạn không có quyền thực hiện thao tác này!" });
            }

            if (prod != null)
            {
                prod.IsSold = true;
                prod.Status = "sold";
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = "Đã đánh dấu món đồ là 'Đã bán'. Món đồ đã được ẩn khỏi sàn công khai.";
            return Json(new { success = true });
        }

        // POST: /Products/DeletePermanently (Xóa vĩnh viễn bài đăng)
        [HttpPost]
        public IActionResult DeletePermanently(int id)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue) return Json(new { success = false, message = "Chưa đăng nhập!" });
            var isAdmin = HttpContext.Session.IsAdmin();

            var post = _db.PendingPosts.FirstOrDefault(p => p.Id == id && (p.SellerId == userId.Value || isAdmin));
            Product? prod = null;

            if (post != null)
            {
                prod = _db.Products.FirstOrDefault(p => (p.PendingPostId == post.Id || p.Id == id) && (p.SellerId == userId.Value || isAdmin));
                _db.PendingPosts.Remove(post);
            }
            else
            {
                prod = _db.Products.FirstOrDefault(p => p.Id == id && (p.SellerId == userId.Value || isAdmin));
                if (prod != null && prod.PendingPostId.HasValue)
                {
                    var linkedPost = _db.PendingPosts.FirstOrDefault(p => p.Id == prod.PendingPostId.Value && (p.SellerId == userId.Value || isAdmin));
                    if (linkedPost != null) _db.PendingPosts.Remove(linkedPost);
                }
            }

            if (post == null && prod == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tin đăng hoặc bạn không có quyền xóa bài này!" });
            }

            if (prod != null)
            {
                _db.Products.Remove(prod);
            }

            _db.SaveChanges();
            TempData["SuccessMessage"] = "Đã xóa vĩnh viễn tin đăng khỏi hệ thống.";
            return Json(new { success = true });
        }

        // GET: /Products/Favorites (Danh sách tin đã yêu thích)
        public IActionResult Favorites()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem danh sách tin yêu thích!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Products/Favorites" });
            }

            // LINQ: Lấy sản phẩm đã yêu thích
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

        // POST: /Products/ToggleFavorite (Thả tim yêu thích sản phẩm)
        [HttpPost]
        public IActionResult ToggleFavorite(int productId)
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
    }
}
