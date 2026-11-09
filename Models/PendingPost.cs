using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Tin đăng bán đồ cũ người dùng gửi lên chờ Admin duyệt (Cốt lõi Đề tài 30)
    /// ĐÃ BỎ HOÀN TOÀN các cột: Condition, ConditionPercent, ConditionDetails
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

        [StringLength(100)]
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

        [Required(ErrorMessage = "Vui lòng nhập mô tả chi tiết, tình trạng và khuyết điểm")]
        [Display(Name = "Mô Tả Sản Phẩm & Tình Trạng")]
        public string Description { get; set; } = string.Empty;

        [Display(Name = "Hình Ảnh Thật")]
        public string Image { get; set; } = string.Empty;

        /// <summary>
        /// Chuỗi JSON lưu danh sách ảnh phụ thực tế (nvarchar(max))
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? ImagesJson { get; set; }

        /// <summary>
        /// Chuỗi JSON lưu danh sách video thực tế (nvarchar(max))
        /// </summary>
        [Column(TypeName = "nvarchar(max)")]
        public string? VideosJson { get; set; }

        /// <summary>
        /// Helper get/set mảng ảnh phụ thông qua serialize/deserialize ImagesJson
        /// </summary>
        [NotMapped]
        public string[] Images
        {
            get
            {
                if (string.IsNullOrWhiteSpace(ImagesJson)) return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(ImagesJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                ImagesJson = value != null && value.Length > 0 ? JsonSerializer.Serialize(value) : null;
            }
        }

        /// <summary>
        /// Helper get/set mảng video thông qua serialize/deserialize VideosJson
        /// </summary>
        [NotMapped]
        public string[] Videos
        {
            get
            {
                if (string.IsNullOrWhiteSpace(VideosJson)) return Array.Empty<string>();
                try
                {
                    return JsonSerializer.Deserialize<string[]>(VideosJson) ?? Array.Empty<string>();
                }
                catch
                {
                    return Array.Empty<string>();
                }
            }
            set
            {
                VideosJson = value != null && value.Length > 0 ? JsonSerializer.Serialize(value) : null;
            }
        }

        public int Stock { get; set; } = 1;

        [Required]
        public int SellerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người bán")]
        [Display(Name = "Họ Tên Người Bán")]
        [StringLength(100)]
        public string SellerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [Display(Name = "Số Điện Thoại")]
        [StringLength(20)]
        public string SellerPhone { get; set; } = string.Empty;

        [StringLength(150)]
        public string? SellerEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập khu vực")]
        [Display(Name = "Khu Vực")]
        [StringLength(255)]
        public string SellerAddress { get; set; } = string.Empty;

        [StringLength(255)]
        public string? SellerAvatar { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "pending"; // 'pending' | 'approved' | 'rejected' | 'sold'

        public bool IsSold { get; set; } = false;

        [StringLength(255)]
        public string? RejectReason { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public string CreatedAtString => CreatedAt.ToString("dd/MM/yyyy HH:mm");

        public string? UpdatedAt { get; set; }

        // Navigation properties (EF Core)
        public virtual Category? Category { get; set; }
        public virtual User? Seller { get; set; }
    }
}
