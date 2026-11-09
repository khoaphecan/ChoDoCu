using System.ComponentModel.DataAnnotations;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Danh mục món đồ cũ (Category)
    /// </summary>
    public class Category
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập tên danh mục")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Tên danh mục từ 2 đến 100 ký tự")]
        [Display(Name = "Tên Danh Mục")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập icon Font Awesome")]
        [StringLength(50, ErrorMessage = "Icon tối đa 50 ký tự")]
        [Display(Name = "Biểu Tượng (Icon)")]
        public string Icon { get; set; } = "fas fa-tag";

        [StringLength(255, ErrorMessage = "Mô tả danh mục tối đa 255 ký tự")]
        [Display(Name = "Mô Tả Danh Mục")]
        public string? Description { get; set; }

        // Navigation property
        public virtual ICollection<Product> Products { get; set; } = new List<Product>();
    }
}
