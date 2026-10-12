using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;

namespace ThriftShop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ThriftDbContext _db;

        public OrdersController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Orders (Lịch sử đơn hàng của tôi / My Orders - Yêu cầu đăng nhập)
        public IActionResult Index(string? search, string? status)
        {
            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem danh sách đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Orders" });
            }

            var query = _db.Orders.Include(o => o.Items).AsNoTracking();

            // Nếu không phải Admin thì bắt buộc chỉ xem đơn hàng của chính mình (Issue 1)
            if (!isAdmin)
            {
                query = query.Where(o => o.CustomerId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                query = query.Where(o => o.OrderStatus == status);
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(o => o.Id.ToLower().Contains(q) || (o.CustomerName != null && o.CustomerName.ToLower().Contains(q)));
            }

            // Sắp xếp đơn hàng mới nhất theo DateTime CreatedAt chuẩn thời gian (Issue 5)
            var orders = query.OrderByDescending(o => o.CreatedAt).ToList();
            return View(orders);
        }

        // GET: /Orders/GetMyOrders (API JSON cho client-side - Yêu cầu đăng nhập)
        [HttpGet]
        public IActionResult GetMyOrders()
        {
            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();

            if (!userId.HasValue)
            {
                return Json(new List<Order>());
            }

            var query = _db.Orders.Include(o => o.Items).AsNoTracking();
            if (!isAdmin)
            {
                query = query.Where(o => o.CustomerId == userId.Value);
            }

            // Sắp xếp theo CreatedAt thời gian thực (Issue 5)
            var list = query.OrderByDescending(o => o.CreatedAt).ToList();
            return Json(list);
        }

        // GET: /Orders/Details/DH100234 (Chi tiết đơn hàng - Kiểm tra quyền sở hữu)
        public IActionResult Details(string? id)
        {
            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem chi tiết đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Orders/Details/{id}" });
            }

            if (string.IsNullOrWhiteSpace(id))
            {
                return RedirectToAction("Index");
            }

            var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == id);
            if (order == null || (!isAdmin && order.CustomerId != userId.Value))
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này!";
                return RedirectToAction("Index");
            }

            ViewBag.OrderId = id;
            return View("Index", new List<Order> { order });
        }
    }
}
