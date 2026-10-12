using System.ComponentModel.DataAnnotations;

namespace ThriftShop.Models
{
    /// <summary>
    /// Model Danh mục món đồ cũ (Category)
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, ErrorMessage = "Tên danh mục không quá 100 ký tự")]
        [Display(Name = "Tên Danh Mục")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập icon Font Awesome")]
        [Display(Name = "Biểu Tượng (Icon)")]
        public string Icon { get; set; } = "fas fa-tag";

        [Display(Name = "Mô Tả Danh Mục")]
        public string? Description { get; set; }
    }
}
