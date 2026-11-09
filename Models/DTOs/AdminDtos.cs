using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Payload thêm/sửa danh mục đồ cũ từ Admin/Categories.cshtml
    /// </summary>
    public class SaveCategoryDto
    {
        [JsonPropertyName("id")]
        public int Id { get; set; } = 0;

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục đồ cũ")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 100 ký tự")]
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập icon Font Awesome (VD: fas fa-tag)")]
        [StringLength(50, ErrorMessage = "Icon tối đa 50 ký tự")]
        [JsonPropertyName("icon")]
        public string Icon { get; set; } = "fas fa-tag";

        [StringLength(255, ErrorMessage = "Mô tả danh mục tối đa 255 ký tự")]
        [JsonPropertyName("description")]
        public string? Description { get; set; }
    }

    /// <summary>
    /// Payload từ chối tin đăng bán đồ cũ từ Admin/ApproveProducts.cshtml
    /// </summary>
    public class RejectPostDto
    {
        [Required(ErrorMessage = "Vui lòng cung cấp mã tin đăng cần từ chối")]
        [Range(1, int.MaxValue, ErrorMessage = "Mã tin đăng không hợp lệ")]
        [JsonPropertyName("id")]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập lý do từ chối tin đăng bán")]
        [StringLength(255, MinimumLength = 3, ErrorMessage = "Lý do từ chối từ 3 đến 255 ký tự")]
        [JsonPropertyName("reason")]
        public string Reason { get; set; } = string.Empty;
    }
}
