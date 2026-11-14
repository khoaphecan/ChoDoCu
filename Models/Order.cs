using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThriftShop.Models
{
    /// <summary>
    /// Entity Đơn hàng (Order)
    /// </summary>
    public class Order
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên từ 2 đến 100 ký tự")]
        public string CustName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        [StringLength(20, ErrorMessage = "Số điện thoại tối đa 20 ký tự")]
        public string CustPhone { get; set; } = string.Empty;

        [StringLength(150, ErrorMessage = "Email tối đa 150 ký tự")]
        public string? CustEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ nhận hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ tối đa 255 ký tự")]
        public string CustAddress { get; set; } = string.Empty;

        [StringLength(100)]
        public string PaymentMethod { get; set; } = "COD - Thanh toán khi nhận hàng";

        [StringLength(500)]
        public string? Notes { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }

        [StringLength(30)]
        public string Status { get; set; } = "pending"; // 'pending' | 'processing' | 'shipping' | 'delivered' | 'cancelled'

        [Column(TypeName = "datetime2")]
        public DateTime OrderDate { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public int? UserId { get; set; }

        // Navigation properties (EF Core)
        public virtual User? User { get; set; }
        public virtual ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();

        // Thuộc tính Helper tương thích mã đơn 'DH000123' và các View cũ
        [NotMapped]
        public string OrderCode => $"DH{Id:D6}";

        [NotMapped]
        public string CustomerName
        {
            get => CustName;
            set => CustName = value;
        }

        [NotMapped]
        public string CustomerPhone
        {
            get => CustPhone;
            set => CustPhone = value;
        }

        [NotMapped]
        public string? CustomerEmail
        {
            get => CustEmail;
            set => CustEmail = value;
        }

        [NotMapped]
        public string ShippingAddress
        {
            get => CustAddress;
            set => CustAddress = value;
        }

        [NotMapped]
        public string ShippingMethod { get; set; } = "Giao hàng bưu tá / Đồng kiểm COD";

        [NotMapped]
        public string OrderStatus
        {
            get => Status;
            set => Status = value;
        }

        [NotMapped]
        public string OrderDateString => OrderDate.ToString("dd/MM/yyyy HH:mm");

        [NotMapped]
        public List<OrderItem> Items
        {
            get => OrderItems.ToList();
            set => OrderItems = value;
        }
    }

    /// <summary>
    /// Entity Chi tiết món đồ trong đơn hàng (OrderItem)
    /// ĐÃ BỎ HOÀN TOÀN CỘT Condition
    /// </summary>
    public class OrderItem
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int OrderId { get; set; }

        [Required]
        public int ProductId { get; set; }

        [Required]
        [StringLength(150)]
        public string ProductName { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }

        public int Quantity { get; set; } = 1;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [StringLength(255)]
        public string Image { get; set; } = string.Empty;

        // Navigation properties (EF Core)
        public virtual Order? Order { get; set; }
        public virtual Product? Product { get; set; }
    }
}
