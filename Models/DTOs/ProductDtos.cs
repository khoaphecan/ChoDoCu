using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Payload đăng bán món đồ cũ người dùng gửi lên từ PostItem.cshtml
    /// ĐÃ BỎ HOÀN TOÀN CÁC TRƯỜNG Condition, ConditionPercent, ConditionDetails
    /// (Toàn bộ mô tả tình trạng & khuyết điểm đã gộp vào Description)
    /// </summary>
    public class PostItemRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập tên món đồ cần thanh lý")]
        [StringLength(150, MinimumLength = 3, ErrorMessage = "Tên món đồ từ 3 đến 150 ký tự")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục món đồ")]
        [Range(1, int.MaxValue, ErrorMessage = "Danh mục không hợp lệ")]
        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá thanh lý mong muốn")]
        [Range(1000, 1000000000, ErrorMessage = "Giá thanh lý phải từ 1.000 ₫ đến 1.000.000.000 ₫")]
        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [Range(0, 1000000000, ErrorMessage = "Giá gốc không hợp lệ")]
        [JsonPropertyName("originalPrice")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khu vực giao dịch / địa chỉ xem đồ")]
        [StringLength(255, ErrorMessage = "Địa chỉ khu vực tối đa 255 ký tự")]
        [JsonPropertyName("sellerAddress")]
        public string SellerAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại hoặc Zalo liên hệ")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại Việt Nam không hợp lệ")]
        [JsonPropertyName("sellerPhone")]
        public string SellerPhone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng cung cấp ít nhất 1 ảnh đại diện chính của món đồ")]
        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("images")]
        public string[] Images { get; set; } = Array.Empty<string>();

        [JsonPropertyName("videos")]
        public string[] Videos { get; set; } = Array.Empty<string>();

        [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết, tình trạng sử dụng và khuyết điểm món đồ")]
        [MinLength(10, ErrorMessage = "Mô tả chi tiết phải có tối thiểu 10 ký tự")]
        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;
    }

    /// <summary>
    /// Response trả về sau khi người dùng đăng bán thành công
    /// </summary>
    public class PostItemResponseDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending";

        [JsonPropertyName("message")]
        public string Message { get; set; } = "Tin đăng đã được gửi và chuyển sang trạng thái chờ Admin duyệt!";
    }

    /// <summary>
    /// DTO hiển thị danh sách và modal kiểm duyệt tin đồ cũ cho Admin (ApproveProducts.cshtml)
    /// Đảm bảo serialize ra JSON ở dạng camelCase, đồng bộ 100% với frontend JS
    /// </summary>
    public class AdminProductViewDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("categoryId")]
        public int CategoryId { get; set; }

        [JsonPropertyName("categoryName")]
        public string CategoryName { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("originalPrice")]
        public decimal OriginalPrice { get; set; }

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("imagesJson")]
        public string? ImagesJson { get; set; }

        [JsonPropertyName("videosJson")]
        public string? VideosJson { get; set; }

        [JsonPropertyName("stock")]
        public int Stock { get; set; } = 1;

        [JsonPropertyName("sellerId")]
        public int SellerId { get; set; }

        [JsonPropertyName("sellerName")]
        public string SellerName { get; set; } = string.Empty;

        [JsonPropertyName("sellerPhone")]
        public string SellerPhone { get; set; } = string.Empty;

        [JsonPropertyName("sellerEmail")]
        public string? SellerEmail { get; set; }

        [JsonPropertyName("sellerAddress")]
        public string SellerAddress { get; set; } = string.Empty;

        [JsonPropertyName("sellerAvatar")]
        public string? SellerAvatar { get; set; }

        [JsonPropertyName("status")]
        public string Status { get; set; } = "pending"; // 'pending' | 'approved' | 'rejected' | 'sold'

        [JsonPropertyName("isSold")]
        public bool IsSold { get; set; } = false;

        [JsonPropertyName("rejectReason")]
        public string? RejectReason { get; set; }

        [JsonPropertyName("createdAt")]
        public string CreatedAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
    }
}
