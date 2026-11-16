using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Cryptography;
using System.Text;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Filters;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private readonly ThriftDbContext _db;

        public AdminController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Admin
        public async Task<IActionResult> Index()
        {
            var stats = await _db.Categories
                .Select(c => new CategoryStatsVM
                {
                    CategoryId = c.Id,
                    CategoryName = c.Name,
                    ProductCount = _db.Products.Count(p => p.CategoryId == c.Id && !p.Deleted),
                    MaxPrice = _db.Products.Where(p => p.CategoryId == c.Id && !p.Deleted).Max(p => (decimal?)p.Price) ?? 0,
                    MinPrice = _db.Products.Where(p => p.CategoryId == c.Id && !p.Deleted).Min(p => (decimal?)p.Price) ?? 0,
                    AvgPrice = _db.Products.Where(p => p.CategoryId == c.Id && !p.Deleted).Average(p => (decimal?)p.Price) ?? 0
                })
                .ToListAsync();

            return View(stats);
        }

        // GET: /Admin/ApproveProducts
        public IActionResult ApproveProducts()
        {
            var allPosts = _db.PendingPosts.IgnoreQueryFilters().AsNoTracking()
                .OrderByDescending(p => p.Id)
                .ToList();

            return View(allPosts);
        }

        // GET: /Admin/Approve
        [HttpGet]
        public IActionResult Approve()
        {
            return RedirectToAction(nameof(ApproveProducts));
        }

        // GET: /Admin/ApproveDetails/5
        public IActionResult ApproveDetails(int id)
        {
            var post = _db.PendingPosts.IgnoreQueryFilters().AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .FirstOrDefault(p => p.Id == id);

            if (post == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy tin đăng #" + id;
                return RedirectToAction(nameof(ApproveProducts));
            }

            var linkedProduct = _db.Products.IgnoreQueryFilters().AsNoTracking()
                .FirstOrDefault(p => p.PendingPostId == post.Id || p.Id == post.Id);
            ViewBag.LinkedProduct = linkedProduct;

            return View(post);
        }

        // POST: /Admin/UnapprovePost?id=123
        [HttpPost]
        public IActionResult UnapprovePost(int id, string? returnUrl)
        {
            try
            {
                var post = _db.PendingPosts.IgnoreQueryFilters().FirstOrDefault(p => p.Id == id);
                if (post == null)
                {
                    if (IsAjax()) return Json(new { success = false, message = "Không tìm thấy tin đăng!" });
                    TempData["ErrorMessage"] = "Không tìm thấy tin đăng!";
                    return RedirectToAction(nameof(ApproveProducts));
                }

                post.Status = "pending";

                var linkedProduct = _db.Products.IgnoreQueryFilters().FirstOrDefault(p => p.PendingPostId == post.Id || p.Id == post.Id);
                if (linkedProduct != null)
                {
                    linkedProduct.Deleted = true;
                    linkedProduct.Status = "pending";
                }

                _db.SaveChanges();
                TempData["SuccessMessage"] = $"Đã hạ bài đăng '{post.Name}' khỏi sàn thành công. Tin được chuyển về trạng thái Chờ kiểm duyệt.";

                if (IsAjax()) return Json(new { success = true, message = "Đã hạ bài đăng khỏi sàn thành công!" });
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction(nameof(ApproveDetails), new { id = post.Id });
            }
            catch (Exception ex)
            {
                if (IsAjax()) return Json(new { success = false, message = "Lỗi khi hạ bài: " + ex.Message });
                TempData["ErrorMessage"] = "Lỗi khi hạ bài: " + ex.Message;
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction(nameof(ApproveDetails), new { id });
            }
        }

        // POST: /Admin/ApprovePost?id=123
        [HttpPost]
        public IActionResult ApprovePost(int id, string? returnUrl)
        {
            try
            {
                var post = _db.PendingPosts.IgnoreQueryFilters().FirstOrDefault(p => p.Id == id);
                if (post == null)
                {
                    if (IsAjax()) return Json(new { success = false, message = "Không tìm thấy tin đăng!" });
                    TempData["ErrorMessage"] = "Không tìm thấy tin đăng!";
                    return RedirectToAction(nameof(ApproveProducts));
                }

                if (post.Status == "approved")
                {
                    if (IsAjax()) return Json(new { success = false, message = "Tin đăng này đã được duyệt mở bán trước đó!" });
                    TempData["ErrorMessage"] = "Tin đăng này đã được duyệt mở bán trước đó!";
                    if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                    return RedirectToAction(nameof(ApproveProducts));
                }

                post.Status = "approved";

                var existingProduct = _db.Products.IgnoreQueryFilters().FirstOrDefault(p => p.PendingPostId == post.Id);
                var cleanImage = ImageHelper.SaveBase64OrUrl(post.Image);
                string? cleanImagesJson = post.ImagesJson;
                if (!string.IsNullOrWhiteSpace(post.ImagesJson))
                {
                    try
                    {
                        var rawImgs = System.Text.Json.JsonSerializer.Deserialize<List<string>>(post.ImagesJson);
                        if (rawImgs != null && rawImgs.Count > 0)
                        {
                            cleanImagesJson = System.Text.Json.JsonSerializer.Serialize(ImageHelper.ProcessImagesList(rawImgs));
                        }
                    }
                    catch { }
                }

                if (existingProduct != null)
                {
                    existingProduct.Status = "approved";
                    existingProduct.IsSold = false;
                    existingProduct.Deleted = false;
                    existingProduct.Price = post.Price;
                    existingProduct.Name = post.Name;
                    existingProduct.Description = post.Description;
                    existingProduct.Stock = 1;
                    existingProduct.Image = cleanImage;
                    existingProduct.ImagesJson = cleanImagesJson;
                    existingProduct.VideosJson = post.VideosJson;
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
                        Image = cleanImage,
                        ImagesJson = cleanImagesJson,
                        VideosJson = post.VideosJson,
                        Stock = 1,
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

                if (IsAjax()) return Json(new { success = true, message = "Đã duyệt tin thành công!" });
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction(nameof(ApproveProducts));
            }
            catch (Exception ex)
            {
                if (IsAjax()) return Json(new { success = false, message = "Lỗi khi duyệt bài: " + ex.Message });
                TempData["ErrorMessage"] = "Lỗi khi duyệt bài: " + ex.Message;
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction(nameof(ApproveProducts));
            }
        }

        // POST: /Admin/RejectPost?id=123&reason=...
        [HttpPost]
        public IActionResult RejectPost(int id, string? reason, string? returnUrl)
        {
            try
            {
                var post = _db.PendingPosts.IgnoreQueryFilters().FirstOrDefault(p => p.Id == id);
                if (post == null)
                {
                    if (IsAjax()) return Json(new { success = false, message = "Không tìm thấy tin đăng!" });
                    TempData["ErrorMessage"] = "Không tìm thấy tin đăng!";
                    return RedirectToAction(nameof(ApproveProducts));
                }

                post.Status = "rejected";
                post.RejectReason = string.IsNullOrWhiteSpace(reason) ? "Không đạt tiêu chuẩn kiểm duyệt sàn đồ cũ." : reason.Trim();

                var linkedProduct = _db.Products.IgnoreQueryFilters().FirstOrDefault(p => p.PendingPostId == post.Id);
                if (linkedProduct != null)
                {
                    _db.Products.Remove(linkedProduct);
                }

                _db.SaveChanges();
                TempData["SuccessMessage"] = $"Đã từ chối tin đăng: '{post.Name}'.";

                if (IsAjax()) return Json(new { success = true, message = "Đã từ chối bài đăng thành công!" });
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction(nameof(ApproveProducts));
            }
            catch (Exception ex)
            {
                if (IsAjax()) return Json(new { success = false, message = "Lỗi khi từ chối bài: " + ex.Message });
                TempData["ErrorMessage"] = "Lỗi khi từ chối bài: " + ex.Message;
                if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl)) return Redirect(returnUrl);
                return RedirectToAction(nameof(ApproveProducts));
            }
        }

        private bool IsAjax()
        {
            var req = Request;
            var accept = req.Headers["Accept"].ToString();
            return req.Headers["X-Requested-With"] == "XMLHttpRequest"
                || accept.Contains("application/json")
                || req.Query.ContainsKey("ajax")
                || (req.Headers.ContainsKey("X-Requested-With") && req.Headers["X-Requested-With"] == "XMLHttpRequest")
                || req.ContentType?.Contains("application/json") == true;
        }

        // ========================================================
        // QUẢN LÝ SẢN PHẨM & TIN ĐĂNG (PURE MVC - CHUẨN MYSTORE)
        // ========================================================

        // GET: /Admin/Products
        public IActionResult Products(string? searchTerm, decimal? minPrice, decimal? maxPrice, string? sortOrder, int? categoryId)
        {
            var query = _db.Products.AsNoTracking()
                .Include(p => p.Category)
                .Where(p => !p.Deleted)
                .AsQueryable();

            // Lọc theo danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId.Value);
            }

            // Tìm kiếm sản phẩm theo từ khóa (tên, mô tả, danh mục)
            if (!string.IsNullOrWhiteSpace(searchTerm))
            {
                string term = searchTerm.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)) ||
                    (p.Category != null && p.Category.Name.Contains(term)) ||
                    p.CategoryName.Contains(term));
            }

            // Tìm theo giá tối thiểu
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }

            // Tìm theo giá tối đa
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // Sắp xếp theo yêu cầu
            switch (sortOrder)
            {
                case "name_asc":
                    query = query.OrderBy(p => p.Name);
                    break;
                case "name_desc":
                    query = query.OrderByDescending(p => p.Name);
                    break;
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                default:
                    query = query.OrderByDescending(p => p.Id);
                    break;
            }

            var products = query.ToList();

            ViewBag.Categories = _db.Categories.AsNoTracking().ToList();
            ViewBag.CategoryId = categoryId;
            ViewBag.SearchTerm = searchTerm;
            ViewBag.MinPrice = minPrice;
            ViewBag.MaxPrice = maxPrice;
            ViewBag.SortOrder = sortOrder;

            return View(products);
        }

        // GET: /Admin/ProductDetails/5
        public IActionResult ProductDetails(int id)
        {
            var product = _db.Products.IgnoreQueryFilters().AsNoTracking()
                .Include(p => p.Category)
                .Include(p => p.Seller)
                .FirstOrDefault(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // GET: /Admin/CreateProduct
        public IActionResult CreateProduct()
        {
            ViewBag.Categories = _db.Categories.ToList();
            return View(new Product { Stock = 1 });
        }

        // POST: /Admin/CreateProduct
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                var cat = await _db.Categories.FindAsync(product.CategoryId);
                product.CategoryName = cat?.Name ?? "Đồ cũ";
                if (string.IsNullOrEmpty(product.Status)) product.Status = "approved";
                
                // Giải mã và lưu file ảnh đĩa thay cho chuỗi Base64
                if (!string.IsNullOrWhiteSpace(product.Image))
                {
                    product.Image = ImageHelper.SaveBase64OrUrl(product.Image);
                }
                else
                {
                    product.Image = "https://images.unsplash.com/photo-1523275335684-37898b6baf30?w=600";
                }

                if (!string.IsNullOrWhiteSpace(product.ImagesJson))
                {
                    try
                    {
                        var rawImgs = System.Text.Json.JsonSerializer.Deserialize<List<string>>(product.ImagesJson);
                        if (rawImgs != null && rawImgs.Count > 0)
                        {
                            var processed = ImageHelper.ProcessImagesList(rawImgs);
                            product.ImagesJson = System.Text.Json.JsonSerializer.Serialize(processed);
                        }
                    }
                    catch { }
                }

                if (product.SellerId == 0)
                {
                    var adminUser = await _db.Users.FirstOrDefaultAsync(u => u.Role == "admin");
                    product.SellerId = adminUser?.Id ?? 1;
                    product.SellerName = adminUser?.FullName ?? "Admin";
                }
                _db.Products.Add(product);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm sản phẩm mới thành công!";
                return RedirectToAction("Products");
            }
            ViewBag.Categories = _db.Categories.ToList();
            return View(product);
        }

        // GET: /Admin/EditProduct/5
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            ViewBag.Categories = _db.Categories.ToList();
            return View(product);
        }

        // POST: /Admin/EditProduct/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(Product product)
        {
            if (ModelState.IsValid)
            {
                var existing = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == product.Id);
                if (existing == null) return NotFound();

                existing.Name = product.Name;
                existing.CategoryId = product.CategoryId;
                var cat = await _db.Categories.FindAsync(product.CategoryId);
                existing.CategoryName = cat?.Name ?? existing.CategoryName;
                existing.Price = product.Price;
                existing.OriginalPrice = product.OriginalPrice;
                existing.Stock = product.Stock;
                existing.Description = product.Description;
                if (!string.IsNullOrWhiteSpace(product.Image))
                {
                    existing.Image = ImageHelper.SaveBase64OrUrl(product.Image);
                }
                if (!string.IsNullOrWhiteSpace(product.ImagesJson))
                {
                    try
                    {
                        var rawImgs = System.Text.Json.JsonSerializer.Deserialize<List<string>>(product.ImagesJson);
                        if (rawImgs != null && rawImgs.Count > 0)
                        {
                            var processed = ImageHelper.ProcessImagesList(rawImgs);
                            existing.ImagesJson = System.Text.Json.JsonSerializer.Serialize(processed);
                        }
                        else
                        {
                            existing.ImagesJson = product.ImagesJson;
                        }
                    }
                    catch
                    {
                        existing.ImagesJson = product.ImagesJson;
                    }
                }
                if (!string.IsNullOrWhiteSpace(product.VideosJson))
                {
                    existing.VideosJson = product.VideosJson;
                }

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật sản phẩm thành công!";
                return RedirectToAction("Products");
            }
            ViewBag.Categories = _db.Categories.ToList();
            return View(product);
        }

        // GET: /Admin/DeleteProduct/5
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _db.Products.IgnoreQueryFilters()
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);
            if (product == null) return NotFound();
            return View(product);
        }

        // POST: /Admin/DeleteProduct/5
        [HttpPost, ActionName("DeleteProduct")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProductConfirmed(int id)
        {
            var product = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == id);
            if (product != null)
            {
                product.Deleted = true; // Xóa mềm an toàn

                // Đồng bộ sang PendingPost tương ứng để người bán thấy trạng thái Đã bị xóa bởi Admin
                var post = await _db.PendingPosts.FirstOrDefaultAsync(p => (product.PendingPostId.HasValue && p.Id == product.PendingPostId.Value) || p.Id == product.Id);
                if (post != null)
                {
                    post.Deleted = true;
                }

                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Đã xóa sản phẩm thành công!";
            }
            return RedirectToAction("Products");
        }

        // GET: /Admin/Users
        public IActionResult Users()
        {
            var users = _db.Users.AsNoTracking()
                .OrderByDescending(u => u.Id)
                .ToList();

            int currentAdminId = HttpContext.Session.GetUserId() ?? 1;
            ViewBag.CurrentAdminId = currentAdminId;

            return View(users);
        }

        // GET: /Admin/UserDetails/5
        public IActionResult UserDetails(int id)
        {
            var user = _db.Users.AsNoTracking()
                .Include(u => u.Orders)
                .FirstOrDefault(u => u.Id == id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng #" + id;
                return RedirectToAction(nameof(Users));
            }

            ViewBag.ListingCount = _db.Products.IgnoreQueryFilters().Count(p => p.SellerId == id);
            ViewBag.Orders = user.Orders.OrderByDescending(o => o.OrderDate).ToList();

            return View(user);
        }

        // GET: /Admin/CreateUser
        public IActionResult CreateUser()
        {
            return View(new User { Role = "customer" });
        }

        // POST: /Admin/CreateUser
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateUser(User model, string password)
        {
            if (string.IsNullOrWhiteSpace(model.FullName))
            {
                ModelState.AddModelError("FullName", "Vui lòng nhập họ và tên.");
            }
            if (string.IsNullOrWhiteSpace(model.Email))
            {
                ModelState.AddModelError("Email", "Vui lòng nhập địa chỉ email.");
            }
            if (string.IsNullOrWhiteSpace(password) || password.Length < 4)
            {
                ModelState.AddModelError("Password", "Mật khẩu tối thiểu 4 ký tự.");
            }

            if (!string.IsNullOrWhiteSpace(model.Email))
            {
                bool emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
                if (emailExists)
                {
                    ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng trên hệ thống.");
                }
            }

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            string role = (model.Role ?? "customer").Trim().ToLower();
            if (role != "admin") role = "customer";

            var newUser = new User
            {
                FullName = model.FullName.Trim(),
                Email = model.Email.Trim().ToLower(),
                PasswordHash = HashPassword(password),
                Phone = model.Phone?.Trim(),
                Address = model.Address?.Trim(),
                Role = role,
                Avatar = !string.IsNullOrWhiteSpace(model.Avatar)
                    ? model.Avatar.Trim()
                    : (role == "admin" ? "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80" : "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80"),
                Bio = model.Bio?.Trim()
            };

            _db.Users.Add(newUser);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Tạo tài khoản {newUser.FullName} ({(role == "admin" ? "Quản trị viên" : "Khách hàng")}) thành công!";
            return RedirectToAction(nameof(Users));
        }

        // GET: /Admin/EditUser/5
        public async Task<IActionResult> EditUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng #" + id;
                return RedirectToAction(nameof(Users));
            }

            return View(user);
        }

        // POST: /Admin/EditUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditUser(int id, User model, string? NewPassword)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null) return NotFound();

            // Loại bỏ validation của các trường không bắt buộc khi Edit
            ModelState.Remove("PasswordHash");
            ModelState.Remove("Password");
            ModelState.Remove("NewPassword");
            ModelState.Remove("newPassword");

            if (string.IsNullOrWhiteSpace(model.FullName) || string.IsNullOrWhiteSpace(model.Email))
            {
                ViewBag.Error = "Họ tên và Email không được để trống!";
                return View(user);
            }

            // Kiểm tra trùng Email với tài khoản khác
            var emailExists = await _db.Users.AnyAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower() && u.Id != id);
            if (emailExists)
            {
                ViewBag.Error = "Email này đã được sử dụng bởi tài khoản khác!";
                return View(user);
            }

            user.FullName = model.FullName.Trim();
            user.Email = model.Email.Trim();
            user.Phone = model.Phone?.Trim();
            user.Address = model.Address?.Trim();
            user.Role = string.IsNullOrWhiteSpace(model.Role) ? user.Role : model.Role.Trim();

            // Chỉ cập nhật mật khẩu nếu Admin có nhập mật khẩu mới
            if (!string.IsNullOrWhiteSpace(NewPassword))
            {
                if (NewPassword.Trim().Length < 4)
                {
                    ViewBag.Error = "Mật khẩu mới phải có ít nhất 4 ký tự!";
                    return View(user);
                }
                user.PasswordHash = HashPassword(NewPassword.Trim());
            }

            await _db.SaveChangesAsync();
            TempData["Success"] = $"Đã cập nhật thông tin tài khoản #{user.Id} thành công!";
            TempData["SuccessMessage"] = $"Đã cập nhật thông tin tài khoản #{user.Id} thành công!";
            return RedirectToAction(nameof(Users));
        }

        // POST: /Admin/DeleteUser/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(int id)
        {
            var user = await _db.Users.FindAsync(id);
            if (user == null)
            {
                var notFoundMsg = "Không tìm thấy người dùng #" + id;
                if (IsAjax()) return Json(new { success = false, message = notFoundMsg });
                TempData["Error"] = notFoundMsg;
                TempData["ErrorMessage"] = notFoundMsg;
                return RedirectToAction(nameof(Users));
            }

            int currentAdminId = HttpContext.Session.GetUserId() ?? 1;
            // Chặn không cho phép xóa tài khoản Quản trị viên
            if (id == 1 || id == currentAdminId || user.Role?.ToLower() == "admin")
            {
                var adminBlockMsg = "Không thể xóa tài khoản Quản trị viên!";
                if (IsAjax()) return Json(new { success = false, message = adminBlockMsg });
                TempData["Error"] = adminBlockMsg;
                TempData["ErrorMessage"] = adminBlockMsg;
                return RedirectToAction(nameof(Users));
            }

            try
            {
                var strategy = _db.Database.CreateExecutionStrategy();
                await strategy.ExecuteAsync(async () =>
                {
                    using var transaction = await _db.Database.BeginTransactionAsync();

                    // 1. Lấy danh sách ID sản phẩm do User này đăng bán (kể cả sản phẩm bị xóa mềm)
                    var userProducts = await _db.Products
                        .IgnoreQueryFilters()
                        .Where(p => p.SellerId == id)
                        .ToListAsync();
                    var userProductIds = userProducts.Select(p => p.Id).ToList();

                    // 2. Xóa tin nhắn & hội thoại liên quan đến User này:
                    // - Hội thoại mà user là Buyer hoặc Seller, hoặc hội thoại về sản phẩm của user này
                    var relatedConvs = await _db.Conversations
                        .Where(c => c.BuyerId == id || c.SellerId == id || (c.ProductId != null && userProductIds.Contains(c.ProductId.Value)))
                        .ToListAsync();
                    var relatedConvIds = relatedConvs.Select(c => c.Id).ToList();

                    // - Tin nhắn thuộc các hội thoại trên HOẶC tin nhắn do user gửi/nhận HOẶC tin nhắn trỏ tới sản phẩm của user này
                    var relatedMessages = await _db.ChatMessages
                        .Where(m => (m.ConversationId != null && relatedConvIds.Contains(m.ConversationId.Value))
                                 || m.SenderId == id 
                                 || m.ReceiverId == id 
                                 || (m.ProductId != null && userProductIds.Contains(m.ProductId.Value)))
                        .ToListAsync();

                    if (relatedMessages.Any())
                    {
                        _db.ChatMessages.RemoveRange(relatedMessages);
                    }

                    if (relatedConvs.Any())
                    {
                        _db.Conversations.RemoveRange(relatedConvs);
                    }
                    await _db.SaveChangesAsync();

                    // 3. Xóa danh sách yêu thích (Favorites):
                    // - Favorites do user này lưu (f.UserId == id)
                    // - Favorites của người khác lưu sản phẩm của user này (userProductIds.Contains(f.ProductId))
                    var relatedFavs = await _db.Favorites
                        .Where(f => f.UserId == id || userProductIds.Contains(f.ProductId))
                        .ToListAsync();

                    if (relatedFavs.Any())
                    {
                        _db.Favorites.RemoveRange(relatedFavs);
                        await _db.SaveChangesAsync();
                    }

                    // 4. Xóa chi tiết đơn hàng (OrderItems) trỏ tới sản phẩm của User này & gỡ liên kết Orders:
                    if (userProductIds.Any())
                    {
                        var relatedOrderItems = await _db.OrderItems
                            .Where(oi => userProductIds.Contains(oi.ProductId))
                            .ToListAsync();

                        if (relatedOrderItems.Any())
                        {
                            var affectedOrderIds = relatedOrderItems.Select(oi => oi.OrderId).Distinct().ToList();
                            _db.OrderItems.RemoveRange(relatedOrderItems);
                            await _db.SaveChangesAsync();

                            // Kiểm tra nếu đơn hàng trống sau khi xóa mặt hàng thì xóa đơn hàng, ngược lại cập nhật lại TotalAmount
                            foreach (var orderId in affectedOrderIds)
                            {
                                var order = await _db.Orders
                                    .Include(o => o.OrderItems)
                                    .FirstOrDefaultAsync(o => o.Id == orderId);

                                if (order != null)
                                {
                                    if (!order.OrderItems.Any())
                                    {
                                        _db.Orders.Remove(order);
                                    }
                                    else
                                    {
                                        order.TotalAmount = order.OrderItems.Sum(oi => oi.Subtotal);
                                    }
                                }
                            }
                            await _db.SaveChangesAsync();
                        }
                    }

                    // Gán o.UserId = null cho các đơn hàng mà user này từng đặt mua để giữ lại lịch sử doanh thu
                    var buyerOrders = await _db.Orders
                        .Where(o => o.UserId == id)
                        .ToListAsync();
                    foreach (var order in buyerOrders)
                    {
                        order.UserId = null;
                    }
                    await _db.SaveChangesAsync();

                    // 5. Xóa sản phẩm (Products) và bài chờ duyệt (PendingPosts) của User này:
                    if (userProducts.Any())
                    {
                        _db.Products.RemoveRange(userProducts);
                        await _db.SaveChangesAsync();
                    }

                    var userPendingPosts = await _db.PendingPosts
                        .IgnoreQueryFilters()
                        .Where(pp => pp.SellerId == id)
                        .ToListAsync();
                    if (userPendingPosts.Any())
                    {
                        _db.PendingPosts.RemoveRange(userPendingPosts);
                        await _db.SaveChangesAsync();
                    }

                    // 6. Xóa tài khoản Users:
                    _db.Users.Remove(user);
                    await _db.SaveChangesAsync();

                    await transaction.CommitAsync();
                });

                var successMsg = "Đã xóa người dùng và toàn bộ dữ liệu liên quan!";
                if (IsAjax()) return Json(new { success = true, message = successMsg });
                TempData["Success"] = successMsg;
                TempData["SuccessMessage"] = successMsg;
                return RedirectToAction(nameof(Users));
            }
            catch (Exception ex)
            {
                var errorMsg = "Lỗi khi xóa người dùng: " + ex.Message;
                if (IsAjax()) return Json(new { success = false, message = errorMsg });
                TempData["Error"] = errorMsg;
                TempData["ErrorMessage"] = errorMsg;
                return RedirectToAction(nameof(Users));
            }
        }

        private static string HashPassword(string password)
        {
            if (string.IsNullOrEmpty(password)) return string.Empty;
            using var sha = SHA256.Create();
            var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(password));
            return BitConverter.ToString(bytes).Replace("-", "").ToLower();
        }

        // ========================================================
        // QUẢN LÝ DANH MỤC (PURE MVC - CHUẨN MYSTORE)
        // ========================================================

        // GET: /Admin/Categories
        public IActionResult Categories()
        {
            var categories = _db.Categories.AsNoTracking().ToList();
            return View(categories);
        }

        // GET: /Admin/CreateCategory
        public IActionResult CreateCategory()
        {
            return View(new Category());
        }

        // POST: /Admin/CreateCategory
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                if (string.IsNullOrWhiteSpace(category.Icon)) category.Icon = "fas fa-tag";
                _db.Categories.Add(category);
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Thêm danh mục mới thành công!";
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        // GET: /Admin/EditCategory/5
        public async Task<IActionResult> EditCategory(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        // POST: /Admin/EditCategory/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(Category category)
        {
            if (ModelState.IsValid)
            {
                var existing = await _db.Categories.FindAsync(category.Id);
                if (existing == null) return NotFound();
                existing.Name = category.Name.Trim();
                existing.Icon = string.IsNullOrWhiteSpace(category.Icon) ? "fas fa-tag" : category.Icon.Trim();
                existing.Description = category.Description?.Trim();
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = "Cập nhật danh mục thành công!";
                return RedirectToAction("Categories");
            }
            return View(category);
        }

        // GET: /Admin/DeleteCategory/5
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var cat = await _db.Categories
                .Include(c => c.Products)
                .FirstOrDefaultAsync(c => c.Id == id);
            if (cat == null) return NotFound();
            return View(cat);
        }

        // POST: /Admin/DeleteCategory/5
        [HttpPost, ActionName("DeleteCategory")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategoryConfirmed(int id)
        {
            var cat = await _db.Categories.FindAsync(id);
            if (cat == null) return NotFound();

            var prodCount = _db.Products.Count(p => p.CategoryId == id && !p.Deleted);
            if (prodCount > 0)
            {
                TempData["ErrorMessage"] = $"Không thể xóa! Danh mục này hiện đang có {prodCount} sản phẩm liên kết.";
                return RedirectToAction("Categories");
            }

            _db.Categories.Remove(cat);
            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = "Xóa danh mục thành công!";
            return RedirectToAction("Categories");
        }

        // GET: /Admin/Orders
        public IActionResult Orders()
        {
            var orders = _db.Orders.Include(o => o.OrderItems).AsNoTracking()
                .OrderByDescending(o => o.OrderDate)
                .ToList();
            return View(orders);
        }

        // GET: /Admin/OrderDetails/5
        public IActionResult OrderDetails(int id)
        {
            var order = _db.Orders.IgnoreQueryFilters()
                .Include(o => o.OrderItems)
                    .ThenInclude(i => i.Product!)
                        .ThenInclude(p => p.Seller)
                .AsNoTracking()
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng #" + id;
                return RedirectToAction(nameof(Orders));
            }

            return View(order);
        }

        // POST: /Admin/UpdateOrderStatus
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string newStatus)
        {
            var order = await _db.Orders.FindAsync(orderId);
            if (order != null)
            {
                order.Status = newStatus;
                await _db.SaveChangesAsync();
                TempData["SuccessMessage"] = $"Đã cập nhật trạng thái đơn hàng #{orderId} sang '{newStatus}'.";
            }
            return RedirectToAction("OrderDetails", new { id = orderId });
        }

        // GET: /Admin/DeleteOrder/5
        public IActionResult DeleteOrder(int id)
        {
            var order = _db.Orders
                .Include(o => o.OrderItems)
                .AsNoTracking()
                .FirstOrDefault(o => o.Id == id);

            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng #" + id;
                return RedirectToAction(nameof(Orders));
            }

            return View(order);
        }

        // POST: /Admin/DeleteOrder/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult DeleteOrder(int id, IFormCollection form)
        {
            var order = _db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == id);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng cần xóa.";
                return RedirectToAction(nameof(Orders));
            }

            _db.Orders.Remove(order);
            _db.SaveChanges();

            TempData["SuccessMessage"] = $"Đã xóa đơn hàng #{id} thành công!";
            return RedirectToAction(nameof(Orders));
        }

        // GET: /Admin/Chat (Bảng danh sách tin nhắn & Hỗ trợ khách hàng dạng Table phẳng MVC)
        public async Task<IActionResult> Chat(int? userId)
        {
            // 1. Xác định toàn bộ danh sách Admin ID
            var adminUsers = await _db.Users
                .Where(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn" || u.Email.ToLower().Contains("admin"))
                .ToListAsync();
            var adminIds = adminUsers.Select(u => u.Id).Distinct().ToList();
            if (!adminIds.Contains(1)) adminIds.Add(1);
            int primaryAdminId = adminIds.FirstOrDefault();
            if (primaryAdminId <= 0) primaryAdminId = 1;

            // 2. Lấy danh sách thành viên phục vụ dropdown tra cứu (loại trừ tài khoản Admin)
            var usersList = await _db.Users
                .Where(u => !adminIds.Contains(u.Id))
                .OrderBy(u => u.FullName ?? u.Email)
                .Select(u => new AdminUserLookupVM
                {
                    Id = u.Id,
                    FullName = !string.IsNullOrWhiteSpace(u.FullName) ? u.FullName : (u.Email ?? "Thành viên #" + u.Id),
                    Phone = !string.IsNullOrWhiteSpace(u.Phone) ? u.Phone : "Chưa có SĐT"
                })
                .ToListAsync();

            ViewBag.Users = usersList;
            ViewBag.SelectedUserId = userId;
            ViewBag.AdminId = primaryAdminId;
            ViewBag.AdminIds = adminIds;

            // 3. TRUY VẤN LỌC CỨNG: BẮT BUỘC một trong hai bên phải là tài khoản Admin
            var query = _db.Conversations
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .Include(c => c.Messages)
                .Where(c => adminIds.Contains(c.BuyerId) || adminIds.Contains(c.SellerId));

            if (userId.HasValue && userId.Value > 0)
            {
                query = query.Where(c => (c.BuyerId == userId.Value && adminIds.Contains(c.SellerId))
                                      || (c.SellerId == userId.Value && adminIds.Contains(c.BuyerId)));
            }

            var conversations = await query
                .OrderByDescending(c => c.UpdatedAt)
                .ToListAsync();

            return View(conversations);
        }

        // GET: /Admin/ChatWithUser?userId=5 (Mở hoặc tạo kênh chat riêng với 1 user)
        public async Task<IActionResult> ChatWithUser(int userId)
        {
            if (userId <= 0) return RedirectToAction(nameof(Chat));

            var adminUsers = await _db.Users
                .Where(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn" || u.Email.ToLower().Contains("admin"))
                .ToListAsync();
            var adminIds = adminUsers.Select(u => u.Id).Distinct().ToList();
            if (!adminIds.Contains(1)) adminIds.Add(1);
            int primaryAdminId = adminIds.FirstOrDefault();
            if (primaryAdminId <= 0) primaryAdminId = 1;

            var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ProductId == null &&
                ((c.BuyerId == userId && adminIds.Contains(c.SellerId)) || (adminIds.Contains(c.BuyerId) && c.SellerId == userId)));

            if (conv == null)
            {
                conv = new Conversation
                {
                    ProductId = null,
                    BuyerId = userId,
                    SellerId = primaryAdminId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Conversations.Add(conv);
                await _db.SaveChangesAsync();
            }

            return RedirectToAction(nameof(ChatDetails), new { id = conv.Id });
        }

        // GET: /Admin/ChatDetails/5 (Xem & Trả lời chi tiết)
        public async Task<IActionResult> ChatDetails(int id)
        {
            var adminUsers = await _db.Users
                .Where(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn" || u.Email.ToLower().Contains("admin"))
                .ToListAsync();
            var adminIds = adminUsers.Select(u => u.Id).Distinct().ToList();
            if (!adminIds.Contains(1)) adminIds.Add(1);
            int primaryAdminId = adminIds.FirstOrDefault();
            if (primaryAdminId <= 0) primaryAdminId = 1;

            var conv = await _db.Conversations
                .Include(c => c.Product)
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (conv == null || (!adminIds.Contains(conv.BuyerId) && !adminIds.Contains(conv.SellerId)))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền truy cập cuộc trò chuyện riêng tư của người dùng.";
                return RedirectToAction(nameof(Chat));
            }

            ViewBag.AdminId = primaryAdminId;
            ViewBag.AdminIds = adminIds;
            var messages = await _db.ChatMessages
                .Where(m => m.ConversationId == conv.Id || (conv.ProductId.HasValue && m.ProductId == conv.ProductId.Value))
                .OrderBy(m => m.SentAt)
                .ToListAsync();

            ViewBag.Messages = messages;
            return View(conv);
        }

        // POST: /Admin/DeleteConversation/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConversation(int id)
        {
            var adminUsers = await _db.Users
                .Where(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn" || u.Email.ToLower().Contains("admin"))
                .ToListAsync();
            var adminIds = adminUsers.Select(u => u.Id).Distinct().ToList();
            if (!adminIds.Contains(1)) adminIds.Add(1);

            var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.Id == id);
            if (conv == null || (!adminIds.Contains(conv.BuyerId) && !adminIds.Contains(conv.SellerId)))
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xóa cuộc trò chuyện này.";
                return RedirectToAction(nameof(Chat));
            }

            var msgs = await _db.ChatMessages
                .Where(m => m.ConversationId == id || (conv.ProductId.HasValue && m.ProductId == conv.ProductId.Value))
                .ToListAsync();
            _db.ChatMessages.RemoveRange(msgs);
            _db.Conversations.Remove(conv);
            await _db.SaveChangesAsync();

            TempData["SuccessMessage"] = $"Đã xóa phiên hỗ trợ #{id} thành công!";
            return RedirectToAction(nameof(Chat));
        }

        // POST: /Admin/BroadcastMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> BroadcastMessage(string content)
        {
            if (string.IsNullOrWhiteSpace(content))
            {
                TempData["ErrorMessage"] = "Nội dung thông báo không được để trống!";
                return RedirectToAction(nameof(Chat));
            }

            var admin = await _db.Users.FirstOrDefaultAsync(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn");
            int adminId = admin?.Id ?? 1;
            string adminAvatar = admin?.Avatar ?? "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80";

            var users = await _db.Users
                .Where(u => u.Id != adminId)
                .ToListAsync();

            if (!users.Any())
            {
                TempData["ErrorMessage"] = "Không tìm thấy người dùng nào trên hệ thống!";
                return RedirectToAction(nameof(Chat));
            }

            int count = 0;
            var now = DateTime.Now;

            foreach (var user in users)
            {
                var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ProductId == null && 
                    ((c.BuyerId == user.Id && c.SellerId == adminId) || (c.BuyerId == adminId && c.SellerId == user.Id)));

                if (conv == null)
                {
                    conv = new Conversation
                    {
                        ProductId = null,
                        BuyerId = user.Id,
                        SellerId = adminId,
                        CreatedAt = now,
                        UpdatedAt = now
                    };
                    _db.Conversations.Add(conv);
                    await _db.SaveChangesAsync();
                }

                var msg = new ChatMessage
                {
                    ConversationId = conv.Id,
                    ProductId = null,
                    SenderId = adminId,
                    SenderName = "Ban Quản Trị ThriftShop",
                    SenderAvatar = adminAvatar,
                    ReceiverId = user.Id,
                    ReceiverName = user.FullName ?? user.Email,
                    ReceiverAvatar = user.Avatar,
                    Message = $"[THÔNG BÁO TỪ BAN QUẢN TRỊ]: {content.Trim()}",
                    SentAt = now
                };
                _db.ChatMessages.Add(msg);
                conv.UpdatedAt = now;
                count++;
            }

            await _db.SaveChangesAsync();
            TempData["SuccessMessage"] = $"Đã phát thông báo tổng tới {count} thành viên trên toàn sàn thành công!";
            return RedirectToAction(nameof(Chat));
        }

        // POST: /Admin/SendAdminMessage
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> SendAdminMessage(int conversationId, string message)
        {
            if (conversationId <= 0 || string.IsNullOrWhiteSpace(message))
            {
                TempData["ErrorMessage"] = "Nội dung tin nhắn không hợp lệ!";
                return RedirectToAction(nameof(ChatDetails), new { id = conversationId });
            }

            var adminUsers = await _db.Users
                .Where(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn" || u.Email.ToLower().Contains("admin"))
                .ToListAsync();
            var adminIds = adminUsers.Select(u => u.Id).Distinct().ToList();
            if (!adminIds.Contains(1)) adminIds.Add(1);
            int primaryAdminId = adminIds.FirstOrDefault();
            if (primaryAdminId <= 0) primaryAdminId = 1;

            var conv = await _db.Conversations
                .Include(c => c.Buyer)
                .Include(c => c.Seller)
                .FirstOrDefaultAsync(c => c.Id == conversationId);

            if (conv == null || (!adminIds.Contains(conv.BuyerId) && !adminIds.Contains(conv.SellerId)))
            {
                TempData["ErrorMessage"] = "Không tìm thấy cuộc trò chuyện hoặc bạn không có quyền gửi tin nhắn vào hội thoại này!";
                return RedirectToAction(nameof(Chat));
            }

            var admin = adminUsers.FirstOrDefault(u => u.Id == primaryAdminId) ?? adminUsers.FirstOrDefault();
            string adminAvatar = admin?.Avatar ?? "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80";

            int receiverId = adminIds.Contains(conv.BuyerId) ? conv.SellerId : conv.BuyerId;
            var receiverUser = (conv.BuyerId == receiverId) ? conv.Buyer : conv.Seller;

            var chatMsg = new ChatMessage
            {
                ConversationId = conv.Id,
                ProductId = conv.ProductId,
                SenderId = primaryAdminId,
                SenderName = "Ban Quản Trị ThriftShop",
                SenderAvatar = adminAvatar,
                ReceiverId = receiverId,
                ReceiverName = receiverUser?.FullName ?? receiverUser?.Email ?? "Người dùng",
                ReceiverAvatar = receiverUser?.Avatar,
                Message = message.Trim(),
                SentAt = DateTime.Now
            };

            try
            {
                _db.ChatMessages.Add(chatMsg);
                conv.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();

                TempData["SuccessMessage"] = "Đã gửi phản hồi thành công!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = "Lỗi khi gửi phản hồi: " + (ex.InnerException?.Message ?? ex.Message);
            }
            return RedirectToAction(nameof(ChatDetails), new { id = conversationId });
        }
    }
}
