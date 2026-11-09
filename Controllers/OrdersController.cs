using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    public class OrdersController : Controller
    {
        private readonly ThriftDbContext _db;

        public OrdersController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Orders
        public IActionResult Index(string? search, string? status)
        {
            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();

            if (!userId.HasValue)
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để xem danh sách đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Orders" });
            }

            var query = _db.Orders.Include(o => o.OrderItems).AsNoTracking();

            if (!isAdmin)
            {
                query = query.Where(o => o.UserId == userId.Value);
            }

            if (!string.IsNullOrWhiteSpace(status) && status != "all")
            {
                query = query.Where(o => o.Status.ToLower() == status.Trim().ToLower());
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(o => o.CustName.ToLower().Contains(q) || o.CustPhone.Contains(q));
            }

            var orders = query.OrderByDescending(o => o.OrderDate).ToList();
            return View(orders);
        }

        // GET: /Orders/GetMyOrders
        // Trả về JSON mảng OrderSummaryDto cho frontend fetch
        [HttpGet]
        public IActionResult GetMyOrders()
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                var isAdmin = HttpContext.Session.IsAdmin();

                if (!userId.HasValue)
                {
                    return Json(new List<OrderSummaryDto>());
                }

                var query = _db.Orders.Include(o => o.OrderItems).AsNoTracking();
                if (!isAdmin)
                {
                    query = query.Where(o => o.UserId == userId.Value);
                }

                var rawOrders = query.OrderByDescending(o => o.OrderDate).ToList();

                var dtoList = rawOrders.Select(o => new OrderSummaryDto
                {
                    Id = o.OrderCode,
                    OrderDate = o.OrderDate.ToString("dd/MM/yyyy HH:mm"),
                    OrderStatus = (o.Status ?? "pending").ToLower(),
                    CustomerName = o.CustName,
                    CustomerPhone = o.CustPhone,
                    ShippingAddress = o.CustAddress,
                    TotalAmount = o.TotalAmount,
                    Items = o.OrderItems.Select(oi => new OrderItemDto
                    {
                        ProductId = oi.ProductId,
                        ProductName = oi.ProductName,
                        Image = oi.Image,
                        Price = oi.Price,
                        Quantity = oi.Quantity
                    }).ToList()
                }).ToList();

                return Json(dtoList);
            }
            catch (Exception ex)
            {
                return Json(new { error = ex.Message });
            }
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
            return View("Index", new List<Order> { order });
        }
    }
}
