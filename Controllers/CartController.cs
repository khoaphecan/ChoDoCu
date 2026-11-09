using System.Text.Json;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;
using ThriftShop.Models.DTOs;

namespace ThriftShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ThriftDbContext _db;

        public CartController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Cart (Trang Giỏ hàng & Trao đổi Chat với người bán)
        public IActionResult Index(int? productId)
        {
            var prodId = productId ?? 1;
            var product = _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == prodId);
            if (product == null)
            {
                var post = _db.PendingPosts.AsNoTracking().FirstOrDefault(p => p.Id == prodId);
                if (post != null)
                {
                    product = new Product
                    {
                        Id = post.Id,
                        Name = post.Name,
                        CategoryId = post.CategoryId,
                        CategoryName = post.CategoryName,
                        Price = post.Price,
                        OriginalPrice = post.OriginalPrice,
                        Description = post.Description,
                        Image = post.Image,
                        ImagesJson = post.ImagesJson,
                        VideosJson = post.VideosJson,
                        SellerId = post.SellerId,
                        SellerName = post.SellerName,
                        SellerPhone = post.SellerPhone,
                        SellerEmail = post.SellerEmail,
                        SellerAddress = post.SellerAddress,
                        SellerAvatar = post.SellerAvatar,
                        Status = post.Status,
                        IsSold = post.IsSold
                    };
                }
            }

            ViewBag.ActiveProductId = prodId;
            ViewBag.ActiveProduct = product;
            return View();
        }

        // POST: /Cart/SendMessage?productId=...&message=...
        [HttpPost]
        public IActionResult SendMessage(int productId, string message)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                var userName = HttpContext.Session.GetUserName() ?? "Người mua";
                var userAvatar = HttpContext.Session.GetUserAvatar();

                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để gửi tin nhắn trao đổi!" });
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    return Json(new { success = false, message = "Nội dung tin nhắn không được để trống!" });
                }

                var product = _db.Products.Find(productId);
                int receiverId = product?.SellerId ?? 1;
                string receiverName = product?.SellerName ?? "Người bán";

                var chatMsg = new ChatMessage
                {
                    ProductId = productId,
                    SenderId = userId.Value,
                    SenderName = userName,
                    SenderAvatar = userAvatar,
                    ReceiverId = receiverId,
                    ReceiverName = receiverName,
                    Message = message.Trim(),
                    SentAt = DateTime.Now
                };

                _db.ChatMessages.Add(chatMsg);
                _db.SaveChanges();

                return Json(new { success = true, msg = chatMsg });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi gửi tin nhắn: " + ex.Message });
            }
        }

        // POST: /Cart/DeleteChat?productId=...
        [HttpPost]
        public IActionResult DeleteChat(int? productId)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập!" });
                }

                if (productId.HasValue && productId.Value > 0)
                {
                    var msgs = _db.ChatMessages
                        .Where(m => m.ProductId == productId.Value && (m.SenderId == userId.Value || m.ReceiverId == userId.Value))
                        .ToList();

                    if (msgs.Any())
                    {
                        _db.ChatMessages.RemoveRange(msgs);
                        _db.SaveChanges();
                    }
                }

                return Json(new { success = true, message = "Đã xóa đoạn chat thành công!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa đoạn chat: " + ex.Message });
            }
        }

        // GET: /Cart/Checkout (Trang thanh toán đơn hàng)
        public IActionResult Checkout()
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiến hành thanh toán đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = "/Cart/Checkout" });
            }

            var userId = HttpContext.Session.GetUserId();
            var user = userId.HasValue ? _db.Users.Find(userId.Value) : null;
            return View(user);
        }

        // POST: /Cart/Checkout ([FromForm] CheckoutRequestDto)
        [HttpPost]
        public IActionResult Checkout([FromForm] CheckoutRequestDto dto)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Vui lòng đăng nhập để tiến hành đặt hàng!" });
                }

                if (!ModelState.IsValid)
                {
                    var firstError = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage;
                    return Json(new { success = false, message = firstError ?? "Thông tin nhận hàng không hợp lệ!" });
                }

                if (string.IsNullOrWhiteSpace(dto.ItemsJson) || dto.ItemsJson.Trim() == "[]")
                {
                    return Json(new { success = false, message = "Giỏ hàng của bạn đang trống! Vui lòng chọn món đồ trước khi đặt." });
                }

                using var doc = JsonDocument.Parse(dto.ItemsJson);
                var itemsArray = doc.RootElement;
                if (itemsArray.ValueKind != JsonValueKind.Array || itemsArray.GetArrayLength() == 0)
                {
                    return Json(new { success = false, message = "Danh sách sản phẩm giỏ hàng không hợp lệ!" });
                }

                var order = new Order
                {
                    CustName = dto.CustName.Trim(),
                    CustPhone = dto.CustPhone.Trim(),
                    CustEmail = dto.CustEmail?.Trim() ?? string.Empty,
                    CustAddress = dto.CustAddress.Trim(),
                    PaymentMethod = dto.PaymentMethod ?? "COD - Thanh toán khi nhận hàng",
                    Status = "pending",
                    OrderDate = DateTime.Now,
                    UserId = userId.Value,
                    Notes = dto.Notes?.Trim() ?? "Đồng kiểm ngoại hình khi nhận hàng"
                };

                decimal calculatedTotal = 0;

                foreach (var elem in itemsArray.EnumerateArray())
                {
                    int pId = elem.TryGetProperty("id", out var idProp) ? idProp.GetInt32()
                            : (elem.TryGetProperty("productId", out var pidProp) ? pidProp.GetInt32() : 0);
                    int qty = elem.TryGetProperty("quantity", out var qProp) ? qProp.GetInt32() : 1;
                    if (qty <= 0) qty = 1;

                    var prod = _db.Products.FirstOrDefault(p => p.Id == pId && !p.Deleted);
                    if (prod == null)
                    {
                        return Json(new { success = false, message = $"Món đồ (mã #{pId}) không còn tồn tại hoặc đã bị gỡ khỏi sàn!" });
                    }

                    if (prod.IsSold || prod.Stock < qty)
                    {
                        return Json(new { success = false, message = $"Món đồ '{prod.Name}' đã hết hàng hoặc không đủ tồn kho (còn: {prod.Stock})!" });
                    }

                    decimal lineTotal = prod.Price * qty;
                    calculatedTotal += lineTotal;

                    // Cập nhật tồn kho sản phẩm
                    prod.Stock -= qty;
                    if (prod.Stock <= 0)
                    {
                        prod.Stock = 0;
                        prod.IsSold = true;
                        prod.Status = "sold";

                        // Đồng bộ trạng thái đã bán sang PendingPost nếu có liên kết
                        if (prod.PendingPostId.HasValue)
                        {
                            var linkedPost = _db.PendingPosts.FirstOrDefault(p => p.Id == prod.PendingPostId.Value);
                            if (linkedPost != null)
                            {
                                linkedPost.IsSold = true;
                                linkedPost.Status = "sold";
                            }
                        }
                    }

                    var item = new OrderItem
                    {
                        ProductId = prod.Id,
                        ProductName = prod.Name,
                        Image = prod.Image,
                        Price = prod.Price,
                        Quantity = qty,
                        Subtotal = lineTotal
                    };
                    order.OrderItems.Add(item);
                }

                if (order.OrderItems.Count == 0)
                {
                    return Json(new { success = false, message = "Không có sản phẩm hợp lệ trong giỏ hàng!" });
                }

                order.TotalAmount = calculatedTotal > 0 ? calculatedTotal : dto.TotalAmount;

                _db.Orders.Add(order);
                _db.SaveChanges();

                return Json(new { success = true, orderId = order.OrderCode });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi máy chủ khi tạo đơn hàng: " + ex.Message });
            }
        }

        // GET: /Cart/OrderSuccess?orderId=DH000123
        public IActionResult OrderSuccess(string? orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return RedirectToAction("Index", "Orders");
            }

            int numericId = 0;
            if (orderId.StartsWith("DH", StringComparison.OrdinalIgnoreCase))
            {
                int.TryParse(orderId.Substring(2), out numericId);
            }
            else
            {
                int.TryParse(orderId, out numericId);
            }

            var order = _db.Orders.Include(o => o.OrderItems).FirstOrDefault(o => o.Id == numericId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng này!";
                return RedirectToAction("Index", "Orders");
            }

            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();
            if (!isAdmin && userId.HasValue && order.UserId != userId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem thông tin đơn hàng này!";
                return RedirectToAction("Index", "Orders");
            }

            ViewBag.OrderId = order.OrderCode;
            return View(order);
        }
    }
}
