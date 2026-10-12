using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;

namespace ThriftShop.Controllers
{
    public class CartController : Controller
    {
        private readonly ThriftDbContext _db;

        public CartController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: /Cart (Hộp thư Liên hệ & Chat trao đổi theo sản phẩm - Bắt buộc đăng nhập)
        public IActionResult Index(int? productId)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để mở hộp thư Liên hệ & Chat!";
                return RedirectToAction("Login", "Account", new { returnUrl = productId.HasValue ? $"/Cart?productId={productId}" : "/Cart" });
            }

            ViewBag.ActiveProductId = productId;
            if (productId.HasValue)
            {
                var prod = _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == productId.Value);
                if (prod != null)
                {
                    if (prod.SellerId > 0)
                    {
                        var seller = _db.Users.AsNoTracking().FirstOrDefault(u => u.Id == prod.SellerId);
                        if (seller != null)
                        {
                            if (string.IsNullOrEmpty(prod.SellerName)) prod.SellerName = seller.Name;
                            if (string.IsNullOrEmpty(prod.SellerAvatar) || prod.SellerAvatar.Contains("ui-avatars.com")) prod.SellerAvatar = seller.Avatar;
                            if (!string.IsNullOrEmpty(seller.Phone)) prod.SellerPhone = seller.Phone;
                        }
                        if (string.IsNullOrEmpty(prod.SellerAvatar) || prod.SellerAvatar.Contains("ui-avatars.com"))
                        {
                            prod.SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80";
                        }
                    }
                    ViewBag.ActiveProduct = prod;
                }
            }

            return View();
        }

        // POST: /Cart/SendMessage (Gửi tin nhắn chat)
        [HttpPost]
        public IActionResult SendMessage(int productId, string message)
        {
            var userId = HttpContext.Session.GetUserId();
            var userName = HttpContext.Session.GetUserName() ?? "Bạn";
            var userAvatar = HttpContext.Session.GetUserAvatar();

            if (!userId.HasValue || string.IsNullOrWhiteSpace(message))
            {
                return Json(new { success = false, message = "Dữ liệu không hợp lệ hoặc chưa đăng nhập!" });
            }

            var product = _db.Products.Find(productId);
            var receiverId = product != null ? product.SellerId : 0;
            var receiverName = product != null ? product.SellerName : "Người bán";

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

        // POST: /Cart/DeleteChat (Xóa lịch sử trao đổi của sản phẩm)
        [HttpPost]
        public IActionResult DeleteChat(int? productId)
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

        // POST: /Cart/Checkout (Xử lý lưu đơn hàng vào CSDL SQL Server)
        [HttpPost]
        public IActionResult Checkout(string custName, string custPhone, string? custEmail, string custAddress, string? paymentMethod, string? notes, string itemsJson)
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                return Json(new { success = false, message = "Vui lòng đăng nhập để đặt hàng!" });
            }

            if (string.IsNullOrWhiteSpace(custName) || string.IsNullOrWhiteSpace(custPhone) || string.IsNullOrWhiteSpace(custAddress))
            {
                return Json(new { success = false, message = "Vui lòng điền đầy đủ họ tên, số điện thoại và địa chỉ nhận hàng!" });
            }

            // Issue 4: Kiểm tra và từ chối giỏ hàng rỗng hoặc JSON lỗi
            if (string.IsNullOrWhiteSpace(itemsJson))
            {
                return Json(new { success = false, message = "Giỏ hàng rỗng! Vui lòng chọn sản phẩm trước khi thanh toán." });
            }

            System.Text.Json.JsonDocument doc;
            try
            {
                doc = System.Text.Json.JsonDocument.Parse(itemsJson);
            }
            catch
            {
                return Json(new { success = false, message = "Dữ liệu giỏ hàng không đúng định dạng!" });
            }

            using (doc)
            {
                if (doc.RootElement.ValueKind != System.Text.Json.JsonValueKind.Array || doc.RootElement.GetArrayLength() == 0)
                {
                    return Json(new { success = false, message = "Giỏ hàng rỗng! Vui lòng chọn ít nhất 1 món đồ." });
                }

                // Issue 6: Sinh mã đơn hàng duy nhất có kiểm tra trùng lặp
                string orderId;
                do
                {
                    orderId = $"DH{DateTime.Now:yyMMdd}{Random.Shared.Next(1000, 9999)}";
                } while (_db.Orders.Any(o => o.Id == orderId));

                var order = new Order
                {
                    Id = orderId,
                    CreatedAt = DateTime.Now,
                    OrderDate = DateTime.Now.ToString("dd/MM/yyyy HH:mm"),
                    CustomerId = userId.Value,
                    CustomerName = custName.Trim(),
                    CustomerPhone = custPhone.Trim(),
                    CustomerEmail = custEmail?.Trim() ?? "",
                    ShippingAddress = custAddress.Trim(),
                    PaymentMethod = paymentMethod ?? "COD - Thanh toán khi nhận hàng",
                    // Issue 2: Không tự động đánh dấu đã thanh toán khi chưa đối soát ngân hàng
                    PaymentStatus = paymentMethod != null && paymentMethod.Contains("VietQR") ? "Chờ chuyển khoản (QR)" : "Chưa thanh toán (COD)",
                    OrderStatus = "pending",
                    OrderStatusText = "Chờ duyệt đơn",
                    ItemsJson = itemsJson,
                    Notes = notes?.Trim() ?? "Đồng kiểm khi nhận hàng"
                };

                decimal calculatedTotal = 0;

                // Issue 2 & 3: Tính toán đơn giá trực tiếp từ CSDL và kiểm soát tồn kho
                foreach (var elem in doc.RootElement.EnumerateArray())
                {
                    int pId = elem.TryGetProperty("id", out var idProp) ? idProp.GetInt32() : (elem.TryGetProperty("productId", out var pidProp) ? pidProp.GetInt32() : 0);
                    int qty = elem.TryGetProperty("quantity", out var qProp) ? qProp.GetInt32() : 1;
                    if (qty <= 0) qty = 1;

                    var prod = _db.Products.FirstOrDefault(p => p.Id == pId && !p.Deleted);
                    if (prod == null)
                    {
                        return Json(new { success = false, message = $"Món đồ (mã #{pId}) không còn tồn tại hoặc đã bị gỡ khỏi sàn!" });
                    }

                    // Issue 3: Kiểm tra tồn kho & trạng thái đã bán
                    if (prod.IsSold || prod.Stock < qty)
                    {
                        return Json(new { success = false, message = $"Món đồ '{prod.Name}' đã hết hàng hoặc không đủ số lượng tồn kho (còn lại: {prod.Stock})!" });
                    }

                    // Tính giá theo CSDL (chống gian lận client sửa giá)
                    decimal lineTotal = prod.Price * qty;
                    calculatedTotal += lineTotal;

                    // Giảm tồn kho món đồ
                    prod.Stock -= qty;
                    if (prod.Stock <= 0)
                    {
                        prod.Stock = 0;
                        prod.IsSold = true;
                        prod.Status = "sold";

                        // Nếu liên kết với PendingPost thì đồng bộ trạng thái đã bán
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
                        OrderId = order.Id,
                        ProductId = prod.Id,
                        ProductName = prod.Name,
                        Image = prod.Image,
                        Condition = prod.Condition ?? "95% - Rất mới",
                        Price = prod.Price, // Giá chuẩn từ SQL Server
                        Quantity = qty
                    };
                    order.Items.Add(item);
                }

                if (order.Items.Count == 0)
                {
                    return Json(new { success = false, message = "Không có sản phẩm hợp lệ trong giỏ hàng!" });
                }

                order.Subtotal = calculatedTotal;
                order.ShippingFee = 0;
                order.TotalAmount = calculatedTotal;

                _db.Orders.Add(order);
                _db.SaveChanges();

                return Json(new { success = true, orderId = order.Id });
            }
        }

        // GET: /Cart/OrderSuccess (Trang thông báo đặt hàng thành công - Issue 7)
        public IActionResult OrderSuccess(string? orderId)
        {
            if (string.IsNullOrWhiteSpace(orderId))
            {
                return RedirectToAction("Index", "Orders");
            }

            var order = _db.Orders.Include(o => o.Items).FirstOrDefault(o => o.Id == orderId);
            if (order == null)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin đơn hàng này!";
                return RedirectToAction("Index", "Orders");
            }

            // Kiểm tra phân quyền: Chỉ chủ đơn hàng (hoặc Admin) mới xem được
            var userId = HttpContext.Session.GetUserId();
            var isAdmin = HttpContext.Session.IsAdmin();
            if (!isAdmin && userId.HasValue && order.CustomerId != userId.Value)
            {
                TempData["ErrorMessage"] = "Bạn không có quyền xem thông tin đơn hàng này!";
                return RedirectToAction("Index", "Orders");
            }

            ViewBag.OrderId = order.Id;
            return View(order);
        }
    }
}
