using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;

namespace ThriftShop.Controllers
{
    public class AccountController : Controller
    {
        private readonly ThriftDbContext _db;

        public AccountController(ThriftDbContext db)
        {
            _db = db;
        }

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

        // POST: /Account/Login (Xử lý đăng nhập Server-side qua EF & LINQ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Login(LoginViewModel model, string? returnUrl)
        {
            ViewBag.ReturnUrl = returnUrl;

            // Kiểm tra Data Annotation (Chuong 09 Data Validation)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // LINQ: Tìm người dùng trong Database SQL Server
            var user = _db.Users.FirstOrDefault(u => 
                u.Email.ToLower() == model.Email.Trim().ToLower() && 
                u.Password == model.Password);

            if (user == null)
            {
                ModelState.AddModelError(string.Empty, "Email hoặc mật khẩu không chính xác!");
                return View(model);
            }

            // Lưu thông tin vào Session Server-side
            HttpContext.Session.SetUser(user);
            TempData["SuccessMessage"] = $"Đăng nhập thành công! Chào mừng {user.Name}";

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

        // POST: /Account/Register (Xử lý đăng ký tài khoản qua EF & LINQ)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Register(RegisterViewModel model)
        {
            // Kiểm tra Data Annotation (Chuong 09 Data Validation)
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // LINQ: Kiểm tra trùng email
            var emailExists = _db.Users.Any(u => u.Email.ToLower() == model.Email.Trim().ToLower());
            if (emailExists)
            {
                ModelState.AddModelError("Email", "Địa chỉ email này đã được sử dụng!");
                return View(model);
            }

            // Thêm người dùng mới vào Database
            var newUser = new User
            {
                Name = model.Name.Trim(),
                Email = model.Email.Trim().ToLower(),
                Password = model.Password,
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

        // GET: /Account/Profile (Hồ sơ cá nhân & Đổi mật khẩu)
        public IActionResult Profile()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                return RedirectToAction("Login", new { returnUrl = "/Account/Profile" });
            }

            // LINQ: Lấy thông tin người dùng từ Database
            var user = _db.Users.Find(userId.Value);
            if (user == null)
            {
                HttpContext.Session.ClearUser();
                return RedirectToAction("Login");
            }

            return View(user);
        }

        // POST: /Account/UpdateProfile (Cập nhật hồ sơ & Avatar & Bio)
        [HttpPost]
        public IActionResult UpdateProfile(string name, string phone, string address, string? bio, string? avatar)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Chưa đăng nhập!" });
            }

            var user = _db.Users.Find(userId.Value);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản người dùng!" });
            }

            // Cập nhật thông tin vào SQL Server LocalDB
            user.Name = string.IsNullOrWhiteSpace(name) ? user.Name : name.Trim();
            user.Phone = phone?.Trim();
            user.Address = address?.Trim();
            user.Bio = bio?.Trim();
            if (!string.IsNullOrWhiteSpace(avatar))
            {
                user.Avatar = avatar.Trim();
            }

            _db.SaveChanges();

            // Đồng bộ lại dữ liệu trong Session Server-side
            HttpContext.Session.SetUser(user);
            TempData["SuccessMessage"] = "Cập nhật hồ sơ cá nhân thành công!";

            return Json(new { 
                success = true, 
                message = "Cập nhật hồ sơ cá nhân thành công!",
                user = new {
                    id = user.Id,
                    name = user.Name,
                    email = user.Email,
                    phone = user.Phone,
                    address = user.Address,
                    avatar = user.Avatar,
                    bio = user.Bio,
                    role = user.Role
                }
            });
        }

        // POST: /Account/ChangePassword (Đổi mật khẩu người dùng)
        [HttpPost]
        public IActionResult ChangePassword(string oldPassword, string newPassword, string confirmPassword)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Chưa đăng nhập!" });
            }

            var user = _db.Users.Find(userId.Value);
            if (user == null)
            {
                return Json(new { success = false, message = "Không tìm thấy tài khoản người dùng!" });
            }

            if (user.Password != oldPassword)
            {
                return Json(new { success = false, message = "Mật khẩu hiện tại không chính xác!" });
            }

            if (string.IsNullOrWhiteSpace(newPassword) || newPassword.Length < 6)
            {
                return Json(new { success = false, message = "Mật khẩu mới phải có tối thiểu 6 ký tự!" });
            }

            if (newPassword != confirmPassword)
            {
                return Json(new { success = false, message = "Mật khẩu xác nhận không khớp với mật khẩu mới!" });
            }

            user.Password = newPassword;
            _db.SaveChanges();

            // Cập nhật lại Session
            HttpContext.Session.SetUser(user);
            TempData["SuccessMessage"] = "Đổi mật khẩu tài khoản thành công!";

            return Json(new { success = true, message = "Đổi mật khẩu thành công!" });
        }

        // GET: /Account/SellerProfile?userId=... (Trang cá nhân công khai của người bán)
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

            // LINQ: Tìm user theo ID hoặc Email hoặc Tên
            User? seller = null;
            if (int.TryParse(userId, out int idVal))
            {
                seller = _db.Users.Find(idVal);
            }
            if (seller == null)
            {
                seller = _db.Users.FirstOrDefault(u => 
                    u.Email.ToLower() == userId.ToLower() || 
                    u.Name.ToLower() == userId.ToLower());
            }

            if (seller == null)
            {
                // Nếu chưa có trong bảng Users, tạo đối tượng hiển thị từ thông tin sản phẩm
                var sampleProd = _db.Products.FirstOrDefault(p => 
                    p.SellerId.ToString() == userId || 
                    p.SellerName.ToLower() == userId.ToLower());
                
                if (sampleProd != null)
                {
                    seller = new User
                    {
                        Id = sampleProd.SellerId,
                        Name = sampleProd.SellerName,
                        Email = sampleProd.SellerEmail ?? "seller@thriftshop.vn",
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
                        Name = userId,
                        Avatar = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80"
                    };
                }
            }

            // LINQ: Lấy danh sách sản phẩm ĐANG MỞ BÁN của người bán này
            // TUÂN THỦ: Tuyệt đối KHÔNG hiện đồ đã bán (isSold / status: 'sold') hay đã xoá (deleted)
            var activeProducts = _db.Products.AsNoTracking()
                .Where(p => (p.SellerId == seller.Id || 
                            (p.SellerEmail != null && p.SellerEmail.ToLower() == seller.Email.ToLower()) ||
                            p.SellerName.ToLower() == seller.Name.ToLower())
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
    }
}
