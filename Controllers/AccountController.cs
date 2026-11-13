using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Filters;
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
        public async Task<IActionResult> ApiLogin([FromBody] LoginRequestDto dto)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    return BadRequest(new { message = firstError ?? "Dữ liệu đăng nhập không hợp lệ!" });
                }

                var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == dto.Email.Trim().ToLower());
                if (user == null || !VerifyPassword(dto.Password, user.PasswordHash))
                {
                    return BadRequest(new { message = "Email hoặc mật khẩu không chính xác!" });
                }

                // 1. Ký phát hành Cookie Authentication (Persistent Session 14 ngày)
                var claims = new List<Claim>
                {
                    new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                    new Claim(ClaimTypes.Name, user.FullName ?? user.Name ?? "Thành viên"),
                    new Claim(ClaimTypes.Email, user.Email),
                    new Claim(ClaimTypes.Role, user.Role ?? "customer"),
                    new Claim("Avatar", user.Avatar ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150")
                };
                var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                var authProperties = new AuthenticationProperties
                {
                    IsPersistent = true,
                    ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                };
                await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

                // 2. Thiết lập Session đăng nhập để tương thích ngược
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
            if (User.Identity?.IsAuthenticated == true || HttpContext.Session.IsLoggedIn())
            {
                bool isAdmin = User.IsInRole("admin") || HttpContext.Session.IsAdmin();
                if (isAdmin)
                    return RedirectToAction("Index", "Admin");
                return Redirect(returnUrl ?? "/");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View(new LoginViewModel());
        }

        // POST: /Account/Login
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email.ToLower() == model.Email.Trim().ToLower());
            if (user == null || !VerifyPassword(model.Password, user.PasswordHash))
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác!");
                return View(model);
            }

            // 1. Tạo danh sách Claims định danh người dùng
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.FullName ?? user.Name ?? "Thành viên"),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role ?? "customer"),
                new Claim("Avatar", user.Avatar ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150")
            };
            var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);

            // 2. Cấu hình Cookie Authentication (Persistent Session 14 ngày)
            var authProperties = new AuthenticationProperties
            {
                IsPersistent = true, // Duy trì phiên qua restart server/đóng trình duyệt
                ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
            };

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, authProperties);

            // 3. Đồng bộ gán Session cũ để tương thích ngược với code cũ
            HttpContext.Session.SetUser(user);
            TempData["SuccessMessage"] = $"Đăng nhập thành công! Chào mừng {user.FullName}";

            // 4. Chuyển hướng an toàn
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
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.ClearUser();
            HttpContext.Session.Clear();
            Response.Cookies.Delete(".AspNetCore.Session");
            Response.Cookies.Delete(".ThriftShop.Auth");
            TempData["SuccessMessage"] = "Đã đăng xuất thành công.";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/AccessDenied
        public IActionResult AccessDenied()
        {
            TempData["ErrorMessage"] = "Bạn không có quyền truy cập vào chức năng này!";
            return RedirectToAction("Index", "Home");
        }

        // GET: /Account/Profile
        [UserAuthorize]
        public IActionResult Profile()
        {
            int? userId = null;
            var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (int.TryParse(nameIdClaim, out int parsedId))
            {
                userId = parsedId;
            }
            else
            {
                userId = HttpContext.Session.GetUserId();
            }

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
        [UserAuthorize]
        public async Task<IActionResult> UpdateProfile([FromForm] UpdateProfileDto dto, IFormFile? avatarFile)
        {
            try
            {
                int? userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    var nameIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                    if (int.TryParse(nameIdClaim, out int parsedId))
                    {
                        userId = parsedId;
                    }
                }

                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để thực hiện chức năng này!" });
                }

                var user = await _db.Users.FindAsync(userId.Value);
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

                // Xử lý lưu ảnh đại diện:
                var file = avatarFile ?? dto.AvatarFile ?? (Request.HasFormContentType && Request.Form.Files.Count > 0 ? (Request.Form.Files["avatarFile"] ?? Request.Form.Files["avatar"]) : null);
                if (file != null && file.Length > 0)
                {
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    var ext = Path.GetExtension(file.FileName);
                    if (string.IsNullOrEmpty(ext)) ext = ".jpg";
                    var uniqueFileName = $"avatar_{user.Id}_{DateTime.Now.Ticks}{ext}";
                    var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                    using (var fileStream = new FileStream(filePath, FileMode.Create))
                    {
                        await file.CopyToAsync(fileStream);
                    }

                    user.Avatar = $"/images/avatars/{uniqueFileName}";
                }
                else if (!string.IsNullOrWhiteSpace(dto.Avatar) && dto.Avatar.StartsWith("data:image/"))
                {
                    try
                    {
                        var commaIndex = dto.Avatar.IndexOf(',');
                        if (commaIndex > 0)
                        {
                            var header = dto.Avatar.Substring(0, commaIndex);
                            var base64Data = dto.Avatar.Substring(commaIndex + 1);

                            string ext = ".jpg";
                            if (header.Contains("png")) ext = ".png";
                            else if (header.Contains("webp")) ext = ".webp";
                            else if (header.Contains("gif")) ext = ".gif";

                            var imageBytes = Convert.FromBase64String(base64Data);

                            var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "avatars");
                            if (!Directory.Exists(uploadsFolder))
                                Directory.CreateDirectory(uploadsFolder);

                            var uniqueFileName = $"avatar_{user.Id}_{DateTime.Now.Ticks}{ext}";
                            var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                            await System.IO.File.WriteAllBytesAsync(filePath, imageBytes);

                            user.Avatar = $"/images/avatars/{uniqueFileName}";
                        }
                        else
                        {
                            user.Avatar = dto.Avatar.Trim();
                        }
                    }
                    catch
                    {
                        user.Avatar = dto.Avatar.Trim();
                    }
                }
                else if (!string.IsNullOrWhiteSpace(dto.Avatar))
                {
                    user.Avatar = dto.Avatar.Trim();
                }

                await _db.SaveChangesAsync();

                // 1. Cập nhật Session
                HttpContext.Session.SetUser(user);

                // 2. Cập nhật Claims trong Cookie Authentication để Navbar đổi ngay lập tức
                try
                {
                    var claims = new List<Claim>
                    {
                        new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                        new Claim(ClaimTypes.Name, user.FullName ?? user.Name ?? "Thành viên"),
                        new Claim(ClaimTypes.Email, user.Email),
                        new Claim(ClaimTypes.Role, user.Role ?? "customer"),
                        new Claim("Avatar", user.Avatar ?? "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150")
                    };
                    var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
                    var claimsPrincipal = new ClaimsPrincipal(claimsIdentity);
                    await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, claimsPrincipal, new AuthenticationProperties
                    {
                        IsPersistent = true,
                        ExpiresUtc = DateTimeOffset.UtcNow.AddDays(14)
                    });
                }
                catch { }

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
                var detailError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                return Json(new { success = false, message = "Lỗi khi cập nhật hồ sơ: " + detailError });
            }
        }

        // POST: /Account/ChangePassword ([FromForm] ChangePasswordDto)
        [HttpPost]
        [UserAuthorize]
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

        // GET: /Account/PublicProfile/5 hoặc /Account/PublicProfile?id=5
        [HttpGet]
        public async Task<IActionResult> PublicProfile(int id)
        {
            if (id <= 0)
            {
                int curUserId = HttpContext.Session.GetUserId() ?? 0;
                if (curUserId > 0)
                {
                    id = curUserId;
                }
                else
                {
                    return NotFound("Không tìm thấy thông tin người bán.");
                }
            }

            var seller = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == id);
            if (seller == null)
            {
                return NotFound("Người dùng không tồn tại hoặc đã bị xóa.");
            }

            var activeProducts = await _db.Products
                .Include(p => p.Category)
                .AsNoTracking()
                .Where(p => p.SellerId == id && !p.Deleted && !p.IsSold && p.Status == "approved")
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var viewModel = new SellerProfileViewModel
            {
                User = seller,
                ActiveProducts = activeProducts
            };

            ViewBag.Seller = seller;
            ViewBag.UserId = seller.Id.ToString();

            return View("SellerProfile", viewModel);
        }

        // GET: /Account/SellerProfile?userId=...
        public async Task<IActionResult> SellerProfile(string? userId, int? id)
        {
            int targetId = id ?? 0;
            if (targetId <= 0 && !string.IsNullOrWhiteSpace(userId) && int.TryParse(userId, out int parsed))
            {
                targetId = parsed;
            }

            if (targetId <= 0 && !string.IsNullOrWhiteSpace(userId))
            {
                var user = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u =>
                    u.Email.ToLower() == userId.ToLower() ||
                    u.FullName.ToLower() == userId.ToLower());
                if (user != null)
                {
                    targetId = user.Id;
                }
            }

            if (targetId <= 0)
            {
                targetId = HttpContext.Session.GetUserId() ?? 0;
            }

            if (targetId <= 0)
            {
                return RedirectToAction("Login", new { returnUrl = Request.Path + Request.QueryString });
            }

            return await PublicProfile(targetId);
        }

        #endregion
    }
}
