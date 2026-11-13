using System.Text.Json;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Filters;
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

        // Helper tìm hoặc tạo Conversation (Chặn tự tạo hội thoại giữa 1 user với chính họ)
        protected async Task<Conversation?> GetOrCreateConversationAsync(int? productId, int buyerId, int sellerId)
        {
            if (buyerId == sellerId)
            {
                return null;
            }

            Conversation? conv = null;
            if (productId.HasValue && productId.Value > 0)
            {
                conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ProductId == productId.Value &&
                    ((c.BuyerId == buyerId && c.SellerId == sellerId) || (c.BuyerId == sellerId && c.SellerId == buyerId)));
            }
            else
            {
                conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ProductId == null &&
                    ((c.BuyerId == buyerId && c.SellerId == sellerId) || (c.BuyerId == sellerId && c.SellerId == buyerId)));
            }

            if (conv == null)
            {
                conv = new Conversation
                {
                    ProductId = (productId.HasValue && productId.Value > 0) ? productId.Value : null,
                    BuyerId = buyerId,
                    SellerId = sellerId,
                    CreatedAt = DateTime.Now,
                    UpdatedAt = DateTime.Now
                };
                _db.Conversations.Add(conv);
                await _db.SaveChangesAsync();
            }
            return conv;
        }

        // POST: /Cart/ClearChat or /Chat/ClearChat (Xóa chat 1 phía chuẩn)
        [HttpPost]
        [UserAuthorize]
        public async Task<IActionResult> ClearChat(int? conversationId, int? productId)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue) return Json(new { success = false, message = "Chưa đăng nhập" });

                Conversation? conv = null;
                if (conversationId.HasValue && conversationId.Value > 0)
                {
                    conv = await _db.Conversations.FindAsync(conversationId.Value);
                }
                else if (productId.HasValue && productId.Value > 0)
                {
                    conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ProductId == productId.Value && 
                        (c.BuyerId == userId.Value || c.SellerId == userId.Value));
                    if (conv == null)
                    {
                        var prod = await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == productId.Value);
                        int sellerId = prod?.SellerId ?? 1;
                        int buyerId = (sellerId == userId.Value) ? 2 : userId.Value;
                        conv = await GetOrCreateConversationAsync(productId.Value, buyerId, sellerId);
                    }
                }

                if (conv == null)
                {
                    return Json(new { success = false, message = "Không tìm thấy cuộc trò chuyện để xóa!" });
                }

                if (userId.Value == conv.BuyerId)
                {
                    conv.BuyerClearedAt = DateTime.Now;
                }
                else if (userId.Value == conv.SellerId)
                {
                    conv.SellerClearedAt = DateTime.Now;
                }
                else
                {
                    conv.BuyerClearedAt = DateTime.Now;
                }

                await _db.SaveChangesAsync();
                return Json(new { success = true, message = "Đã xóa lịch sử trò chuyện ở phía bạn thành công." });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi xóa đoạn chat: " + ex.Message });
            }
        }

        // GET: /Cart/StartChat?productId=123
        [HttpGet]
        public IActionResult StartChat(int productId)
        {
            return RedirectToAction("StartChat", "Chat", new { productId });
        }

        // GET: /Cart (Trang Giỏ hàng & Trao đổi Chat với người bán hoặc CSKH)
        [UserAuthorize]
        public async Task<IActionResult> Index(int? productId, int? conversationId)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                var idClaim = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (int.TryParse(idClaim, out int uid))
                {
                    HttpContext.Session.SetInt32(ThriftShop.Common.SessionExtensions.SessionIdKey, uid);
                    var u = await _db.Users.FindAsync(uid);
                    if (u != null) HttpContext.Session.SetUser(u);
                }
            }

            if (!HttpContext.Session.IsLoggedIn())
            {
                string returnUrl = conversationId.HasValue ? $"/Cart?conversationId={conversationId.Value}" :
                    (productId.HasValue && productId.Value > 0 ? $"/Cart?productId={productId.Value}" : "/Cart");
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }

            int currentUserId = HttpContext.Session.GetUserId() ?? 0;
            Conversation? conv = null;
            Product? product = null;
            string? activePartnerName = null;
            string? activePartnerAvatar = null;
            string? activePartnerRole = null;
            bool isSupportChat = false;

            var adminUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn");
            int adminId = adminUser?.Id ?? 1;

            if (conversationId.HasValue && conversationId.Value > 0)
            {
                conv = await _db.Conversations.Include(c => c.Product).FirstOrDefaultAsync(c => c.Id == conversationId.Value);
                if (conv != null)
                {
                    // Chặn hội thoại tự chat hoặc Admin tự mở kênh CSKH của chính mình
                    if (conv.BuyerId == conv.SellerId || (conv.ProductId == null && (currentUserId == adminId || HttpContext.Session.IsAdmin())))
                    {
                        conv = null;
                        conversationId = null;
                        isSupportChat = false;
                    }
                    else if (conv.ProductId == null)
                    {
                        isSupportChat = true;
                        activePartnerName = "Ban Quản Trị ThriftShop (Hỗ trợ CSKH)";
                        activePartnerAvatar = adminUser?.Avatar ?? "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80";
                        activePartnerRole = "Quản trị viên";
                    }
                    else
                    {
                        productId = conv.ProductId;
                    }
                }
            }

            if (!isSupportChat && productId.HasValue && productId.Value > 0)
            {
                product = await _db.Products.IgnoreQueryFilters()
                    .Include(p => p.Seller)
                    .AsNoTracking()
                    .FirstOrDefaultAsync(p => p.Id == productId.Value);

                if (product != null && product.Seller != null)
                {
                    if (!string.IsNullOrWhiteSpace(product.Seller.FullName)) product.SellerName = product.Seller.FullName;
                    if (!string.IsNullOrWhiteSpace(product.Seller.Avatar)) product.SellerAvatar = product.Seller.Avatar;
                }
                else if (product == null)
                {
                    var post = await _db.PendingPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == productId.Value);
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

                if (product != null)
                {
                    if (conv == null && currentUserId > 0)
                    {
                        if (currentUserId == product.SellerId && !conversationId.HasValue)
                        {
                            TempData["InfoMessage"] = "Đây là sản phẩm do chính bạn đăng bán.";
                            return RedirectToAction("Details", "Products", new { id = productId.Value });
                        }

                        int sId = product.SellerId;
                        conv = await GetOrCreateConversationAsync(productId.Value, currentUserId, sId);
                    }

                    bool isUserSeller = (currentUserId != 0 && currentUserId == product.SellerId);
                    if (isUserSeller)
                    {
                        var latestBuyerMsg = await _db.ChatMessages.AsNoTracking()
                            .Where(m => m.ProductId == product.Id && m.SenderId != currentUserId)
                            .OrderByDescending(m => m.SentAt)
                            .FirstOrDefaultAsync();
                        if (latestBuyerMsg != null)
                        {
                            var buyerUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == latestBuyerMsg.SenderId);
                            activePartnerName = !string.IsNullOrWhiteSpace(buyerUser?.FullName)
                                ? buyerUser.FullName
                                : (!string.IsNullOrWhiteSpace(latestBuyerMsg.SenderName) ? latestBuyerMsg.SenderName : (!string.IsNullOrWhiteSpace(buyerUser?.Email) ? buyerUser.Email : "Khách quan tâm"));
                            activePartnerAvatar = buyerUser?.Avatar ?? latestBuyerMsg.SenderAvatar;
                        }
                        else
                        {
                            activePartnerName = "Khách quan tâm";
                        }
                    }
                    else
                    {
                        var sellerUser = product.Seller ?? await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == product.SellerId);
                        activePartnerName = !string.IsNullOrWhiteSpace(sellerUser?.FullName)
                            ? sellerUser.FullName
                            : (!string.IsNullOrWhiteSpace(product.SellerName) && product.SellerName != "Người bán" ? product.SellerName : (!string.IsNullOrWhiteSpace(sellerUser?.Email) ? sellerUser.Email : "Chủ bài đăng"));
                        activePartnerAvatar = sellerUser?.Avatar ?? product.SellerAvatar;
                    }
                }
            }

            ViewBag.ActiveConversationId = conv?.Id;
            ViewBag.IsSupportChat = isSupportChat;
            ViewBag.ActivePartnerName = activePartnerName;
            ViewBag.ActivePartnerAvatar = activePartnerAvatar;
            ViewBag.ActivePartnerRole = activePartnerRole;
            ViewBag.ActiveProductId = productId;
            ViewBag.ActiveProduct = product;
            return View();
        }

        // GET: /Cart/GetMessages?productId=... or conversationId=...
        [HttpGet]
        [UserAuthorize]
        public async Task<IActionResult> GetMessages(int? productId, int? conversationId)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                int currentUserId = userId ?? 0;
                bool isAdmin = HttpContext.Session.IsAdmin();

                Conversation? conv = null;
                if (conversationId.HasValue && conversationId.Value > 0)
                {
                    conv = await _db.Conversations.FindAsync(conversationId.Value);
                }
                else if (productId.HasValue && productId.Value > 0)
                {
                    conv = await _db.Conversations.FirstOrDefaultAsync(c => c.ProductId == productId.Value && 
                        (c.BuyerId == currentUserId || c.SellerId == currentUserId));
                }

                DateTime? clearedAt = null;
                if (conv != null && !isAdmin)
                {
                    if (currentUserId == conv.BuyerId) clearedAt = conv.BuyerClearedAt;
                    else if (currentUserId == conv.SellerId) clearedAt = conv.SellerClearedAt;
                }

                var query = _db.ChatMessages.AsNoTracking().AsQueryable();

                if (conv != null)
                {
                    if (conv.ProductId.HasValue)
                    {
                        query = query.Where(m => m.ConversationId == conv.Id || 
                            (m.ConversationId == null && m.ProductId == conv.ProductId.Value &&
                             ((m.SenderId == conv.BuyerId && m.ReceiverId == conv.SellerId) || (m.SenderId == conv.SellerId && m.ReceiverId == conv.BuyerId))));
                    }
                    else
                    {
                        query = query.Where(m => m.ConversationId == conv.Id || 
                            (m.ProductId == null && ((m.SenderId == conv.BuyerId && m.ReceiverId == conv.SellerId) || (m.SenderId == conv.SellerId && m.ReceiverId == conv.BuyerId))));
                    }
                }
                else if (productId.HasValue && productId.Value > 0)
                {
                    query = query.Where(m => m.ProductId == productId.Value);
                }
                else
                {
                    return Json(new { success = true, messages = Array.Empty<object>() });
                }

                if (clearedAt.HasValue)
                {
                    query = query.Where(m => m.SentAt > clearedAt.Value);
                }

                var messages = await query
                    .OrderBy(m => m.SentAt)
                    .Select(m => new
                    {
                        id = m.Id,
                        conversationId = m.ConversationId,
                        productId = m.ProductId,
                        senderId = m.SenderId,
                        senderName = m.SenderName,
                        senderAvatar = m.SenderAvatar,
                        receiverId = m.ReceiverId,
                        receiverName = m.ReceiverName,
                        receiverAvatar = m.ReceiverAvatar,
                        message = m.Message,
                        imageUrl = m.ImageUrl,
                        sentAt = m.SentAt.ToString("HH:mm"),
                        sentAtFull = m.SentAt.ToString("dd/MM/yyyy HH:mm")
                    })
                    .ToListAsync();

                return Json(new { 
                    success = true, 
                    conversationId = conv?.Id,
                    isSupport = (conv != null && conv.ProductId == null),
                    messages 
                });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Lỗi khi tải lịch sử tin nhắn: " + ex.Message, messages = Array.Empty<object>() });
            }
        }

        // GET: /Cart/GetConversations
        [HttpGet]
        [UserAuthorize]
        public async Task<IActionResult> GetConversations()
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue)
                {
                    return Json(new { success = false, message = "Chưa đăng nhập", conversations = Array.Empty<object>() });
                }

                int myUserId = userId.Value;
                bool isAdmin = HttpContext.Session.IsAdmin();
                var adminUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Role.ToLower() == "admin" || u.Email.ToLower() == "admin@thriftshop.vn");
                int adminId = adminUser?.Id ?? 1;

                // 1. Tự động đồng bộ các tin nhắn cũ chưa gắn ConversationId
                var unlinkedMessages = await _db.ChatMessages
                    .Where(m => m.ConversationId == null && (m.SenderId == myUserId || m.ReceiverId == myUserId) && m.SenderId != m.ReceiverId)
                    .ToListAsync();
                if (unlinkedMessages.Any())
                {
                    foreach (var group in unlinkedMessages.GroupBy(m => new { m.ProductId, OtherId = (m.SenderId == myUserId ? m.ReceiverId : m.SenderId) }))
                    {
                        int pId = group.Key.ProductId ?? 0;
                        int otherId = group.Key.OtherId;
                        if (otherId == myUserId) continue; // Skip self chat

                        var prod = pId > 0 ? await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == pId) : null;
                        int sellerId = prod?.SellerId ?? (otherId == myUserId ? 1 : otherId);
                        int buyerId = (sellerId == myUserId) ? otherId : myUserId;
                        if (buyerId == sellerId) continue;

                        var conv = await GetOrCreateConversationAsync(pId > 0 ? pId : (int?)null, buyerId, sellerId);
                        if (conv != null)
                        {
                            foreach (var m in group)
                            {
                                m.ConversationId = conv.Id;
                            }
                        }
                    }
                    await _db.SaveChangesAsync();
                }

                // 2. Lấy tất cả Conversations của user, LOẠI BỎ HỘI THOẠI TỰ CHAT (BuyerId == SellerId)
                var conversations = await _db.Conversations
                    .Include(c => c.Product)
                    .Include(c => c.Buyer)
                    .Include(c => c.Seller)
                    .Where(c => (c.BuyerId == myUserId || c.SellerId == myUserId) && c.BuyerId != c.SellerId)
                    .OrderByDescending(c => c.UpdatedAt)
                    .ToListAsync();

                var convList = new List<object>();

                foreach (var c in conversations)
                {
                    bool isMeSeller = (myUserId == c.SellerId);
                    int partnerId = isMeSeller ? c.BuyerId : c.SellerId;

                    bool isSupport = (c.ProductId == null);
                    // Nếu là Admin và đây là CSKH chat nhưng partnerId cũng là Admin -> bỏ qua
                    if (isAdmin && isSupport && (partnerId == adminId || partnerId == myUserId))
                    {
                        continue;
                    }

                    var partnerUser = isMeSeller ? c.Buyer : c.Seller;
                    if (partnerUser == null)
                    {
                        partnerUser = await _db.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == partnerId);
                    }

                    DateTime? clearedAt = isMeSeller ? c.SellerClearedAt : c.BuyerClearedAt;
                    if (isAdmin) clearedAt = null;

                    // Ẩn hẳn cuộc trò chuyện khỏi danh sách bên trái nếu đã xóa và chưa có tin nhắn nào mới hơn
                    if (!isAdmin && clearedAt != null)
                    {
                        bool hasNewerMessage = await _db.ChatMessages
                            .AnyAsync(m => (m.ConversationId == c.Id || (c.ProductId.HasValue && m.ProductId == c.ProductId.Value)) &&
                                           m.SentAt > clearedAt.Value);
                        if (!hasNewerMessage)
                        {
                            continue;
                        }
                    }

                    var lastMsg = await _db.ChatMessages
                        .AsNoTracking()
                        .Where(m => (m.ConversationId == c.Id || (c.ProductId.HasValue && m.ProductId == c.ProductId.Value)) &&
                                    (clearedAt == null || m.SentAt > clearedAt.Value))
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefaultAsync();

                    string partnerName;
                    string? partnerAvatar;
                    string partnerRole;
                    string partnerPhone = partnerUser?.Phone ?? "";

                    if (isSupport)
                    {
                        bool isOtherAdmin = partnerUser?.Role?.ToLower() == "admin" || partnerUser?.Email?.ToLower() == "admin@thriftshop.vn";
                        if (isOtherAdmin)
                        {
                            partnerName = "Ban Quản Trị ThriftShop (Hỗ trợ CSKH)";
                            partnerAvatar = partnerUser?.Avatar ?? "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80";
                            partnerRole = "Quản trị viên";
                        }
                        else
                        {
                            partnerName = partnerUser?.FullName ?? partnerUser?.Email ?? "Người dùng";
                            partnerAvatar = partnerUser?.Avatar;
                            partnerRole = "Thành viên";
                        }
                    }
                    else
                    {
                        partnerName = !string.IsNullOrWhiteSpace(partnerUser?.FullName)
                            ? partnerUser.FullName
                            : (!string.IsNullOrWhiteSpace(partnerUser?.Email) ? partnerUser.Email : (isMeSeller ? "Khách quan tâm" : "Chủ bài đăng"));
                        partnerAvatar = partnerUser?.Avatar;
                        partnerRole = partnerUser?.Role?.ToLower() == "admin" ? "Quản trị viên" : "Thành viên";
                    }

                    object? prodObj = null;
                    if (c.ProductId.HasValue)
                    {
                        var prod = c.Product ?? await _db.Products.IgnoreQueryFilters().FirstOrDefaultAsync(p => p.Id == c.ProductId.Value);
                        if (prod != null)
                        {
                            bool isSold = prod.IsSold || (prod.Status != null && prod.Status.ToLower() == "sold");
                            bool isDel = prod.Deleted || (prod.Status != null && prod.Status.ToLower() == "deleted");
                            prodObj = new
                            {
                                id = prod.Id,
                                name = prod.Name,
                                price = prod.Price,
                                image = prod.Image,
                                isSold = isSold,
                                isDeleted = isDel,
                                deleted = isDel,
                                status = prod.Status ?? (isDel ? "deleted" : (isSold ? "sold" : "approved")),
                                statusText = (isSold || isDel) ? "[Đã bán]" : "[Đang mở bán]"
                            };
                        }
                    }

                    convList.Add(new
                    {
                        id = "conv-" + c.Id,
                        conversationId = c.Id,
                        productId = c.ProductId ?? 0,
                        isSupport = isSupport,
                        buyerId = c.BuyerId,
                        sellerId = c.SellerId,
                        partnerId = partnerId,
                        partnerName = partnerName,
                        partnerAvatar = partnerAvatar,
                        partnerRole = partnerRole,
                        partnerPhone = partnerPhone,
                        product = prodObj,
                        lastMessage = lastMsg?.Message ?? "",
                        lastTime = lastMsg?.SentAt.ToString("HH:mm") ?? "",
                        unread = false
                    });
                }

                return Json(new { success = true, conversations = convList });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message, conversations = Array.Empty<object>() });
            }
        }

        #region Chat Media Helper
        protected static string ProcessAndSaveBase64Images(string text, out string? firstImageUrl)
        {
            firstImageUrl = null;
            if (string.IsNullOrEmpty(text) || !text.Contains("data:image/")) return text;

            try
            {
                var chatFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "chat");
                if (!Directory.Exists(chatFolder))
                {
                    Directory.CreateDirectory(chatFolder);
                }

                var pattern = @"data:image\/(?<type>[a-zA-Z0-9]+);base64,(?<data>[A-Za-z0-9+/=]+)";
                var matches = Regex.Matches(text, pattern);

                foreach (Match match in matches)
                {
                    try
                    {
                        var type = match.Groups["type"].Value.ToLower();
                        if (type == "jpeg") type = "jpg";
                        var base64Data = match.Groups["data"].Value;
                        var bytes = Convert.FromBase64String(base64Data);

                        var fileName = $"chat_{DateTime.Now.Ticks}_{Guid.NewGuid():N}.{type}";
                        var filePath = Path.Combine(chatFolder, fileName);
                        System.IO.File.WriteAllBytes(filePath, bytes);

                        var relativeUrl = $"/images/chat/{fileName}";
                        if (firstImageUrl == null) firstImageUrl = relativeUrl;
                        text = text.Replace(match.Value, relativeUrl);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("Lỗi decode ảnh base64 chat: " + ex.Message);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine("Lỗi ProcessAndSaveBase64Images: " + ex.Message);
            }

            return text;
        }
        #endregion

        // POST: /Cart/SendMessage
        [HttpPost]
        [UserAuthorize]
        public async Task<IActionResult> SendMessage(int? productId, string? message, int? receiverId = null, int? conversationId = null, IFormFile? imageFile = null, IFormFile? file = null)
        {
            try
            {
                var userId = HttpContext.Session.GetUserId();
                if (!userId.HasValue) return Json(new { success = false, message = "Chưa đăng nhập" });
                int senderId = userId.Value;

                var userName = HttpContext.Session.GetUserName() ?? "Thành viên";
                var userAvatar = HttpContext.Session.GetUserAvatar();

                string? uploadedImageUrl = null;
                var fileToUpload = imageFile ?? file;
                if (fileToUpload != null && fileToUpload.Length > 0)
                {
                    try
                    {
                        var chatFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images", "chat");
                        if (!Directory.Exists(chatFolder))
                        {
                            Directory.CreateDirectory(chatFolder);
                        }

                        var ext = Path.GetExtension(fileToUpload.FileName).ToLower();
                        if (string.IsNullOrWhiteSpace(ext)) ext = ".jpg";
                        var fileName = $"chat_{DateTime.Now.Ticks}_{Guid.NewGuid():N}{ext}";
                        var filePath = Path.Combine(chatFolder, fileName);

                        using (var stream = new FileStream(filePath, FileMode.Create))
                        {
                            await fileToUpload.CopyToAsync(stream);
                        }
                        uploadedImageUrl = $"/images/chat/{fileName}";
                    }
                    catch (Exception fileEx)
                    {
                        Console.WriteLine("Lỗi lưu file ảnh chat: " + fileEx.Message);
                    }
                }

                // Xử lý và trích xuất Base64 Image nếu có trong message
                string? extractedImageUrl = null;
                if (!string.IsNullOrEmpty(message))
                {
                    message = ProcessAndSaveBase64Images(message, out extractedImageUrl);
                }

                if (!string.IsNullOrEmpty(uploadedImageUrl))
                {
                    if (string.IsNullOrWhiteSpace(message))
                    {
                        message = "[IMG]: " + uploadedImageUrl;
                    }
                    else
                    {
                        message = message.Trim() + "\n[IMG]: " + uploadedImageUrl;
                    }
                }

                if (string.IsNullOrWhiteSpace(message) && string.IsNullOrEmpty(uploadedImageUrl) && string.IsNullOrEmpty(extractedImageUrl))
                {
                    return Json(new { success = false, message = "Nội dung tin nhắn không được để trống!" });
                }

                if (string.IsNullOrWhiteSpace(message))
                {
                    message = !string.IsNullOrEmpty(uploadedImageUrl) ? "[IMG]: " + uploadedImageUrl : "[Hình ảnh]";
                }

                var currentUser = await _db.Users.FindAsync(senderId);
                if (currentUser != null && !string.IsNullOrWhiteSpace(currentUser.FullName))
                {
                    userName = currentUser.FullName;
                    userAvatar = currentUser.Avatar ?? userAvatar;
                }

                Conversation? conv = null;
                if (conversationId.HasValue && conversationId.Value > 0)
                {
                    conv = await _db.Conversations.FindAsync(conversationId.Value);
                }

                Product? product = null;
                int pId = productId ?? (conv?.ProductId ?? 0);
                if (pId > 0)
                {
                    product = await _db.Products.IgnoreQueryFilters().Include(p => p.Seller).FirstOrDefaultAsync(p => p.Id == pId);
                }

                int sellerId = product?.SellerId ?? 0;
                string sellerName = !string.IsNullOrWhiteSpace(product?.Seller?.FullName)
                    ? product.Seller.FullName
                    : (!string.IsNullOrWhiteSpace(product?.SellerName) ? product.SellerName : "Chủ bài đăng");
                string? sellerAvatar = product?.Seller?.Avatar ?? product?.SellerAvatar;

                int targetReceiverId = 0;

                if (conv != null && conv.ProductId == null)
                {
                    // Kênh hỗ trợ CSKH Admin
                    targetReceiverId = (conv.BuyerId == senderId) ? conv.SellerId : conv.BuyerId;
                }
                else if (receiverId.HasValue && receiverId.Value > 0 && receiverId.Value != senderId)
                {
                    targetReceiverId = receiverId.Value;
                }
                else if (sellerId != 0 && sellerId != senderId)
                {
                    targetReceiverId = sellerId;
                }
                else
                {
                    var prevMsg = await _db.ChatMessages
                        .Where(m => (m.ConversationId == (conv != null ? conv.Id : -1) || (pId > 0 && m.ProductId == pId)) && m.SenderId != senderId)
                        .OrderByDescending(m => m.SentAt)
                        .FirstOrDefaultAsync();
                    if (prevMsg != null)
                    {
                        targetReceiverId = prevMsg.SenderId;
                    }
                    else
                    {
                        var prevRecv = await _db.ChatMessages
                            .Where(m => (m.ConversationId == (conv != null ? conv.Id : -1) || (pId > 0 && m.ProductId == pId)) && m.ReceiverId != senderId)
                            .OrderByDescending(m => m.SentAt)
                            .FirstOrDefaultAsync();
                        targetReceiverId = prevRecv?.ReceiverId ?? (sellerId != senderId ? sellerId : 1);
                    }
                }

                int? validProductId = product != null ? product.Id : null;
                if (!validProductId.HasValue && conv?.ProductId != null)
                {
                    bool exists = await _db.Products.AnyAsync(p => p.Id == conv.ProductId.Value);
                    if (exists) validProductId = conv.ProductId.Value;
                }

                if (conv == null)
                {
                    int buyerId = (targetReceiverId == sellerId) ? senderId : targetReceiverId;
                    conv = await GetOrCreateConversationAsync(validProductId, buyerId, sellerId > 0 ? sellerId : targetReceiverId);
                }

                if (conv != null)
                {
                    conv.ProductId = validProductId;
                }

                string targetReceiverName = "Thành viên";
                string? targetReceiverAvatar = null;

                var targetUser = await _db.Users.FindAsync(targetReceiverId);
                if (targetUser != null)
                {
                    targetReceiverName = !string.IsNullOrWhiteSpace(targetUser.FullName) ? targetUser.FullName : (targetUser.Email ?? "Thành viên");
                    targetReceiverAvatar = targetUser.Avatar;
                }

                var chatMsg = new ChatMessage
                {
                    ConversationId = conv?.Id,
                    ProductId = validProductId,
                    SenderId = senderId,
                    SenderName = userName,
                    SenderAvatar = userAvatar,
                    ReceiverId = targetReceiverId,
                    ReceiverName = targetReceiverName,
                    ReceiverAvatar = targetReceiverAvatar,
                    Message = message.Trim(),
                    ImageUrl = uploadedImageUrl ?? extractedImageUrl,
                    SentAt = DateTime.Now,
                    IsRead = false
                };

                if (conv != null) conv.UpdatedAt = DateTime.Now;

                _db.ChatMessages.Add(chatMsg);
                await _db.SaveChangesAsync();

                return Json(new
                {
                    success = true,
                    conversationId = conv?.Id,
                    message = "Gửi tin nhắn thành công!",
                    data = new
                    {
                        id = chatMsg.Id,
                        conversationId = conv?.Id,
                        senderId = chatMsg.SenderId,
                        senderName = chatMsg.SenderName,
                        senderAvatar = chatMsg.SenderAvatar,
                        message = chatMsg.Message,
                        imageUrl = chatMsg.ImageUrl,
                        sentAt = chatMsg.SentAt.ToString("HH:mm")
                    }
                });
            }
            catch (Exception ex)
            {
                var errorMsg = ex.InnerException?.Message ?? ex.Message;
                return Json(new { success = false, message = "Lỗi máy chủ khi gửi tin nhắn: " + errorMsg });
            }
        }

        // POST: /Cart/DeleteChat?productId=...
        [HttpPost]
        [UserAuthorize]
        public async Task<IActionResult> DeleteChat(int? productId)
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
                    var msgs = await _db.ChatMessages
                        .Where(m => m.ProductId == productId.Value && (m.SenderId == userId.Value || m.ReceiverId == userId.Value))
                        .ToListAsync();

                    if (msgs.Any())
                    {
                        _db.ChatMessages.RemoveRange(msgs);
                        await _db.SaveChangesAsync();
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
        [UserAuthorize]
        public IActionResult Checkout(int? productId)
        {
            if (!HttpContext.Session.IsLoggedIn())
            {
                string returnUrl = productId.HasValue && productId.Value > 0
                    ? $"/Cart/Checkout?productId={productId.Value}"
                    : "/Cart/Checkout";
                TempData["ErrorMessage"] = "Vui lòng đăng nhập để tiến hành thanh toán đơn hàng!";
                return RedirectToAction("Login", "Account", new { returnUrl = returnUrl });
            }

            var userId = HttpContext.Session.GetUserId();
            var user = userId.HasValue ? _db.Users.Find(userId.Value) : null;

            if (productId.HasValue && productId.Value > 0)
            {
                var prod = _db.Products.AsNoTracking().FirstOrDefault(p => p.Id == productId.Value && !p.Deleted);
                if (prod != null)
                {
                    var directItem = new
                    {
                        id = prod.Id,
                        name = prod.Name,
                        price = prod.Price,
                        img = prod.Image,
                        quantity = 1
                    };
                    ViewBag.DirectItemJson = JsonSerializer.Serialize(directItem);
                }
            }

            return View(user);
        }

        // POST: /Cart/Checkout ([FromForm] CheckoutRequestDto)
        [HttpPost]
        [UserAuthorize]
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

                    // Cập nhật trạng thái sản phẩm đã bán trong mô hình đồ cũ C2C
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
                    else
                    {
                        var linkedPost = _db.PendingPosts.FirstOrDefault(p => p.Id == prod.Id);
                        if (linkedPost != null)
                        {
                            linkedPost.IsSold = true;
                            linkedPost.Status = "sold";
                        }
                    }

                    var item = new OrderItem
                    {
                        ProductId = prod.Id,
                        ProductName = prod.Name,
                        Image = prod.DisplayImage,
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
        [UserAuthorize]
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
