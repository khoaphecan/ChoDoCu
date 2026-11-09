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
