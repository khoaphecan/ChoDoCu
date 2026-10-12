using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThriftShop.Models
{
    /// <summary>
    /// Model Sản phẩm đồ cũ (Secondhand Product)
    /// </summary>
    public class Product
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên món đồ thanh lý")]
        [StringLength(150, ErrorMessage = "Tên sản phẩm tối đa 150 ký tự")]
        [Display(Name = "Tên Món Đồ")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn danh mục")]
        [Display(Name = "Mã Danh Mục")]
        public int CategoryId { get; set; }

        [Display(Name = "Tên Danh Mục")]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá thanh lý")]
        [Range(1000, 1000000000, ErrorMessage = "Giá thanh lý từ 1.000 ₫ trở lên")]
        [Display(Name = "Giá Thanh Lý (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá gốc")]
        [Range(1000, 1000000000, ErrorMessage = "Giá gốc từ 1.000 ₫ trở lên")]
        [Display(Name = "Giá Mua Mới Ban Đầu (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Required(ErrorMessage = "Vui lòng chọn tình trạng")]
        [Display(Name = "Độ Mới")]
        public string Condition { get; set; } = "95% - Rất tốt";

        public int ConditionPercent { get; set; } = 95;

        [Display(Name = "Ghi Chú Khuyết Điểm")]
        public string? ConditionDetails { get; set; }

        [Display(Name = "Mô Tả Sản Phẩm")]
        public string? Description { get; set; }

        [Display(Name = "Hình Ảnh")]
        public string Image { get; set; } = string.Empty;

        public string? ImagesJson { get; set; }

        public string? VideosJson { get; set; }

        [Range(0, 100, ErrorMessage = "Số lượng tồn từ 0 trở lên")]
        [Display(Name = "Số Lượng Tồn Kho")]
        public int Stock { get; set; } = 1;

        public int SoldCount { get; set; } = 0;

        public int SellerId { get; set; }

        [Display(Name = "Người Bán")]
        public string SellerName { get; set; } = string.Empty;

        [Display(Name = "SĐT Người Bán")]
        public string SellerPhone { get; set; } = string.Empty;

        public string? SellerEmail { get; set; }

        [Display(Name = "Địa Chỉ Người Bán")]
        public string SellerAddress { get; set; } = string.Empty;

        public string? SellerAvatar { get; set; }

        public bool Featured { get; set; } = false;

        public string Status { get; set; } = "approved"; // 'approved' | 'pending' | 'sold' | 'rejected'

        public bool IsSold { get; set; } = false;

        public bool Deleted { get; set; } = false;

        public int? PendingPostId { get; set; }

        public string CreatedAt { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        // Navigation property (EF)
        public virtual Category? Category { get; set; }
    }
}
