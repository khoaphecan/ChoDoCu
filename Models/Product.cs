using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Sản phẩm đồ cũ (Secondhand Product)
    /// ĐÃ BỎ HOÀN TOÀN các cột: Condition, ConditionPercent, ConditionDetails
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
        [StringLength(100)]
        public string CategoryName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập giá thanh lý")]
        [Range(1000, 1000000000, ErrorMessage = "Giá thanh lý từ 1.000 ₫ trở lên")]
        [Display(Name = "Giá Thanh Lý (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập giá gốc")]
        [Range(0, 1000000000, ErrorMessage = "Giá gốc từ 0 ₫ trở lên")]
        [Display(Name = "Giá Mua Mới Ban Đầu (₫)")]
        [Column(TypeName = "decimal(18,2)")]
        public decimal OriginalPrice { get; set; }

        [Display(Name = "Mô Tả Sản Phẩm & Tình Trạng Khuyết Điểm")]
        public string? Description { get; set; }

        [Display(Name = "Hình Ảnh Đại Diện")]
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

        [Range(0, 100, ErrorMessage = "Số lượng tồn từ 0 trở lên")]
        [Display(Name = "Số Lượng Tồn Kho")]
        public int Stock { get; set; } = 1;

        public int SoldCount { get; set; } = 0;

        [Required]
        [Display(Name = "Mã Người Bán")]
        public int SellerId { get; set; }

        [Display(Name = "Người Bán")]
        [StringLength(100)]
        public string SellerName { get; set; } = string.Empty;

        [Display(Name = "SĐT Người Bán")]
        [StringLength(20)]
        public string SellerPhone { get; set; } = string.Empty;

        [StringLength(150)]
        public string? SellerEmail { get; set; }

        [Display(Name = "Địa Chỉ Người Bán")]
        [StringLength(255)]
        public string SellerAddress { get; set; } = string.Empty;

        [StringLength(255)]
        public string? SellerAvatar { get; set; }

        public bool Featured { get; set; } = false;

        [StringLength(30)]
        public string Status { get; set; } = "approved"; // 'approved' | 'pending' | 'sold' | 'rejected'

        public bool IsSold { get; set; } = false;

        public bool Deleted { get; set; } = false;

        public int? PendingPostId { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [NotMapped]
        public string CreatedAtString => CreatedAt.ToString("dd/MM/yyyy HH:mm");

        // Navigation properties (EF Core)
        public virtual Category? Category { get; set; }
        public virtual User? Seller { get; set; }
    }
}
