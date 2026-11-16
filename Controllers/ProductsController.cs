using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Filters;
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

            var product = _db.Products.IgnoreQueryFilters().AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .FirstOrDefault(p => p.Id == id.Value);

            if (product == null)
            {
                // Cố gắng tìm trong bảng PendingPosts nếu bài đăng chưa được đồng bộ hoặc tạo từ PendingPost
                var pending = _db.PendingPosts.IgnoreQueryFilters().AsNoTracking().FirstOrDefault(p => p.Id == id.Value);
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
                .Where(p => p.CategoryId == product.CategoryId && p.Id != product.Id && p.Status == "approved" && !p.IsSold && !p.Deleted)
                .Take(4)
                .ToList();

            User? seller = product.Seller;
            if (seller == null && product.SellerId > 0)
            {
                seller = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == product.SellerId);
            }
            if (seller == null && !string.IsNullOrEmpty(product.SellerEmail))
            {
                seller = _db.Users.AsNoTracking().FirstOrDefault(u => u.Email.ToLower() == product.SellerEmail.ToLower());
            }

            int currentUserId = 0;
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? HttpContext.Session.GetInt32("UserId")?.ToString()
                            ?? HttpContext.Session.GetUserId()?.ToString();
            int.TryParse(userIdStr, out currentUserId);

            bool isFav = false;
            var favoriteProductIds = new List<int>();
            if (currentUserId > 0)
            {
                favoriteProductIds = _db.Favorites.AsNoTracking()
                    .Where(f => f.UserId == currentUserId)
                    .Select(f => f.ProductId)
                    .ToList();
                isFav = favoriteProductIds.Contains(product.Id);
            }

            ViewBag.IsFavorited = isFav;
            ViewBag.FavoriteProductIds = favoriteProductIds;

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
        [UserAuthorize]
        public IActionResult PostItem()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập tài khoản để đăng bán đồ cũ!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Products/PostItem" });
            }

            ViewBag.Categories = _db.Categories.AsNoTracking().OrderBy(c => c.Id).ToList();
            return View(new PendingPost());
        }

        // POST: /Products/PostItem ([FromForm] PostItemRequestDto)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [UserAuthorize]
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
                    ViewBag.Categories = _db.Categories.AsNoTracking().OrderBy(c => c.Id).ToList();
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
                    ViewBag.Categories = _db.Categories.AsNoTracking().OrderBy(c => c.Id).ToList();
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

                var processedImages = ImageHelper.ProcessImagesList(validImages);

                var pendingPost = new PendingPost
                {
                    Name = dto.Name.Trim(),
                    CategoryId = dto.CategoryId,
                    CategoryName = cat?.Name ?? "Đồ cũ",
                    Price = dto.Price,
                    OriginalPrice = dto.OriginalPrice,
                    Description = dto.Description.Trim(),
                    Image = processedImages.FirstOrDefault() ?? "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600",
                    ImagesJson = JsonSerializer.Serialize(processedImages),
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
                ViewBag.Categories = _db.Categories.AsNoTracking().OrderBy(c => c.Id).ToList();
                ModelState.AddModelError(string.Empty, "Lỗi máy chủ khi đăng tin: " + ex.Message);
                return View(new PendingPost());
            }
        }

        // GET: /Products/MyListings
        [UserAuthorize]
        public IActionResult MyListings()
        {
            var userId = HttpContext.Session.GetUserId();
            var userEmail = HttpContext.Session.GetUserEmail();

            if (!userId.HasValue)
            {
                var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int uid))
                {
                    userId = uid;
                    HttpContext.Session.SetInt32(ThriftShop.Common.SessionExtensions.SessionIdKey, uid);
                    var u = _db.Users.Find(uid);
                    if (u != null)
                    {
                        HttpContext.Session.SetUser(u);
                        userEmail = u.Email;
                    }
                }
            }

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

            // Đồng bộ trạng thái Deleted từ bảng Products (nếu Admin đã xóa mềm sản phẩm liên kết)
            var deletedPostIds = _db.Products.IgnoreQueryFilters().AsNoTracking()
                .Where(p => p.Deleted && (p.SellerId == userId.Value || (p.SellerEmail != null && userEmail != null && p.SellerEmail.ToLower() == userEmail.ToLower())))
                .Select(p => p.PendingPostId ?? p.Id)
                .ToHashSet();

            foreach (var p in myListings)
            {
                if (deletedPostIds.Contains(p.Id))
                {
                    p.Deleted = true;
                }
            }

            // Cập nhật lại các biến đếm thống kê trên 4 thẻ đầu trang theo đúng logic:
            // PendingCount: !p.Deleted && !p.IsSold && p.Status == "pending"
            // ApprovedCount: !p.Deleted && !p.IsSold && p.Status == "approved" (KHÔNG đếm tin đã bị xóa p.Deleted == true)
            // SoldCount: !p.Deleted && p.IsSold
            // RejectedCount: !p.Deleted && p.Status == "rejected"
            ViewBag.PendingCount = myListings.Count(p => !p.Deleted && !p.IsSold && p.Status == "pending");
            ViewBag.ApprovedCount = myListings.Count(p => !p.Deleted && !p.IsSold && p.Status == "approved");
            ViewBag.SoldCount = myListings.Count(p => !p.Deleted && (p.IsSold || p.Status == "sold"));
            ViewBag.RejectedCount = myListings.Count(p => !p.Deleted && p.Status == "rejected");

            return View("MyListings", myListings);
        }

        // GET: /Products/MyPosts (Alias)
        [UserAuthorize]
        [HttpGet]
        public IActionResult MyPosts() => MyListings();

        // GET: /Products/MyProducts (Alias)
        [UserAuthorize]
        [HttpGet]
        public IActionResult MyProducts() => MyListings();

        // POST: /Products/MarkSold?id=123
        [HttpPost]
        [UserAuthorize]
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
        [HttpPost]
        [UserAuthorize]
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
        [UserAuthorize]
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
        public async Task<IActionResult> ToggleFavorite(int productId)
        {
            try
            {
                var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                                ?? HttpContext.Session.GetInt32("UserId")?.ToString()
                                ?? HttpContext.Session.GetUserId()?.ToString();

                if (!int.TryParse(userIdStr, out int userId) || userId <= 0)
                {
                    return Json(new { success = false, requireLogin = true, message = "Vui lòng đăng nhập để lưu sản phẩm yêu thích!" });
                }

                var productExists = await _db.Products.AnyAsync(p => p.Id == productId && !p.Deleted);
                if (!productExists)
                {
                    return Json(new { success = false, message = "Sản phẩm không tồn tại hoặc đã bị gỡ!" });
                }

                var existing = await _db.Favorites
                    .FirstOrDefaultAsync(f => f.UserId == userId && f.ProductId == productId);

                bool isFavorited;
                if (existing != null)
                {
                    _db.Favorites.Remove(existing);
                    isFavorited = false;
                }
                else
                {
                    _db.Favorites.Add(new Favorite
                    {
                        UserId = userId,
                        ProductId = productId,
                        CreatedAt = DateTime.Now
                    });
                    isFavorited = true;
                }

                await _db.SaveChangesAsync();
                return Json(new { success = true, isFavorited = isFavorited, isFav = isFavorited });
            }
            catch (Exception ex)
            {
                var msg = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi thao tác yêu thích: " + msg });
            }
        }
    }
}
