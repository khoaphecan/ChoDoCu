using System.Security.Claims;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Filters;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    [UserAuthorize]
    public class OrdersController : Controller
    {
        private readonly ThriftDbContext _db;

        public OrdersController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Orders/MyOrders
        [HttpGet]
        public async Task<IActionResult> MyOrders()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                var idClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int uid))
                {
                    userId = uid;
                }
            }

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem đơn hàng của bạn!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Orders/MyOrders" });
            }

            int currentUserId = userId.Value;
            bool isAdmin = HttpContext.Session.IsAdmin() || User.IsInRole("admin") || (User.FindFirst(ClaimTypes.Email)?.Value == "admin@thriftshop.vn");

            var query = _db.Orders.IgnoreQueryFilters()
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.Product)
                .AsQueryable();

            if (!isAdmin)
            {
                query = query.Where(o => o.UserId == currentUserId);
            }

            var orders = await query
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            return View("MyOrders", orders);
        }

        // GET: /Orders hoặc /Orders/Index
        [HttpGet]
        public async Task<IActionResult> Index()
        {
            return await MyOrders();
        }

        // GET: /Orders/Details/DH000123
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
                return RedirectToAction(nameof(Index));
            }

            int numericId = 0;
            if (id.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
                int.TryParse(id.Substring(2), out numericId);
            else
                int.TryParse(id, out numericId);

            var order = _db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == numericId);
            if (order == null || (!isAdmin && order.UserId != userId.Value))
            {
                TempData["ErrorMessage"] = "Không tìm thấy đơn hàng hoặc bạn không có quyền xem đơn hàng này!";
                return RedirectToAction(nameof(Index));
            }

            ViewBag.OrderId = order.OrderCode;
            return View("MyOrders", new List<Order> { order });
        }
    }
}
