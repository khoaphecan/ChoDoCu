using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Filters;
using ThriftShop.Models;

namespace ThriftShop.Controllers
{
    [UserAuthorize]
    public class ChatController : Controller
    {
        private readonly ThriftDbContext _db;

        public ChatController(ThriftDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public IActionResult Index(int? conversationId = null, int? productId = null)
        {
            if (conversationId.HasValue && conversationId.Value > 0)
            {
                return Redirect($"/Cart?conversationId={conversationId.Value}");
            }
            if (productId.HasValue && productId.Value > 0)
            {
                return RedirectToAction(nameof(StartChat), new { productId = productId.Value });
            }
            return Redirect("/Cart");
        }

        // GET: /Chat/StartChat?productId=123
        [HttpGet]
        public async Task<IActionResult> StartChat(int productId)
        {
            var currentUserId = HttpContext.Session.GetUserId();
            if (!currentUserId.HasValue)
            {
                var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int uid))
                {
                    currentUserId = uid;
                    HttpContext.Session.SetInt32(ThriftShop.Common.SessionExtensions.SessionIdKey, uid);
                }
            }

            if (!currentUserId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = $"/Chat/StartChat?productId={productId}" });
            }

            var product = await _db.Products.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
            int sellerId = product?.SellerId ?? 0;

            if (product == null)
            {
                var pending = await _db.PendingPosts.IgnoreQueryFilters().AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId);
                if (pending != null)
                {
                    sellerId = pending.SellerId;
                }
            }

            if (sellerId <= 0)
            {
                TempData["ErrorMessage"] = "Không tìm thấy thông tin món đồ hoặc người bán!";
                return RedirectToAction("Index", "Home");
            }

            // Nếu người dùng bấm Chat với chính món đồ của mình
            if (currentUserId.Value == sellerId)
            {
                TempData["InfoMessage"] = "Đây là sản phẩm do chính bạn đăng bán.";
                var referer = Request.Headers["Referer"].ToString();
                if (!string.IsNullOrWhiteSpace(referer) && Uri.IsWellFormedUriString(referer, UriKind.RelativeOrAbsolute))
                {
                    return Redirect(referer);
                }
                return RedirectToAction("Details", "Products", new { id = productId });
            }

            // Tìm cuộc hội thoại sẵn có giữa currentUserId và sellerId gắn với productId
            var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ProductId == productId &&
                ((c.BuyerId == currentUserId.Value && c.SellerId == sellerId) || (c.BuyerId == sellerId && c.SellerId == currentUserId.Value)));

            if (conv == null)
            {
                conv = await _db.Conversations.FirstOrDefaultAsync(c =>
                    (c.BuyerId == currentUserId.Value && c.SellerId == sellerId) || (c.BuyerId == sellerId && c.SellerId == currentUserId.Value));
            }

            if (conv == null)
            {
                conv = new Conversation
                {
                    ProductId = productId,
                    BuyerId = currentUserId.Value,
                    SellerId = sellerId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Conversations.Add(conv);
                await _db.SaveChangesAsync();
            }
            else if (conv.ProductId != productId)
            {
                conv.ProductId = productId;
                conv.UpdatedAt = DateTime.Now;
                await _db.SaveChangesAsync();
            }

            return Redirect($"/Cart?conversationId={conv.Id}");
        }

        // GET: /Chat/StartSupportChat (Kết nối hỗ trợ trực tiếp với Admin)
        [HttpGet]
        public async Task<IActionResult> StartSupportChat()
        {
            var userId = HttpContext.Session.GetUserId();
            if (!userId.HasValue)
            {
                var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int uid))
                {
                    userId = uid;
                    HttpContext.Session.SetInt32(ThriftShop.Common.SessionExtensions.SessionIdKey, uid);
                }
            }

            if (!userId.HasValue)
            {
                return RedirectToAction("Login", "Account", new { returnUrl = "/Chat/StartSupportChat" });
            }

            var admin = await _db.Users.AsNoTracking()
                .FirstOrDefaultAsync(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn");
            int adminId = admin?.Id ?? 1;

            // CHẶN ADMIN TỰ CHAT VỚI CHÍNH MÌNH
            if (userId.Value == adminId || HttpContext.Session.IsAdmin() || User.IsInRole("admin") || User.IsInRole("Admin"))
            {
                TempData["InfoMessage"] = "Bạn đang đăng nhập bằng tài khoản Quản trị viên.";
                return Redirect("/Admin/Chat");
            }

            var conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ProductId == null &&
                ((c.BuyerId == userId.Value && c.SellerId == adminId) || (c.BuyerId == adminId && c.SellerId == userId.Value)));

            if (conv == null)
            {
                conv = new Conversation
                {
                    ProductId = null,
                    BuyerId = userId.Value,
                    SellerId = adminId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Conversations.Add(conv);
                await _db.SaveChangesAsync();
            }

            return Redirect($"/Cart?conversationId={conv.Id}");
        }
    }
}
