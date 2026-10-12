using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThriftShop.Models
{
    /// <summary>
    /// Model Tin đăng bán đồ cũ người dùng gửi lên (Cốt lõi Đề tài 30)
    /// </summary>
    public class PendingPost
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên món đồ")]
        [StringLength(150, ErrorMessage = "Tên món đồ tối đa 150 ký tự")]
        [Display(Name = "Tên Món Đồ")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        public int CategoryId { get; set; }

        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá bán mong muốn")]
        [Range(1000, 1000000000, ErrorMessage = "Giá thanh lý từ 1.000 ₫ trở lên")]
        [Display(Name = "Giá Thanh Lý (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá gốc")]
        [Display(Name = "Giá Mua Mới (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tình trạng")]
        [Display(Name = "Độ Mới")]
        public string Condition { get; set; } = "95% - Rất tốt";

        public int ConditionPercent { get; set; } = 95;

        [Display(Name = "Ghi Chú Khuyết Điểm")]
        public string ConditionDetails { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập mô tả")]
        [Display(Name = "Mô Tả Sản Phẩm")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Hình Ảnh Thật")]
        public string Image { get; set; } = string.Empty;

        public string? ImagesJson { get; set; }

        public string? VideosJson { get; set; }

        public int Stock { get; set; } = 1;

        public int SellerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người bán")]
        [Display(Name = "Họ Tên Người Bán")]
        public string SellerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số Điện Thoại")]
        public string SellerPhone { get; set; } = string.Empty;

        public string? SellerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khu vực")]
        [Display(Name = "Khu Vực")]
        public string SellerAddress { get; set; } = string.Empty;

        public string? SellerAvatar { get; set; }

        public string Status { get; set; } = "pending"; // 'pending' | 'approved' | 'rejected' | 'sold'

        public bool IsSold { get; set; } = false;

        public string? RejectReason { get; set; }

        public string CreatedAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        public string? UpdatedAt { get; set; }
    }
}
