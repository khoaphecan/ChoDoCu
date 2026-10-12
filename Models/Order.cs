using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThriftShop.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public string? OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string Image { get; set; } = string.Empty;
        public string Condition { get; set; } = string.Empty;

        [Column(TypeName = "decimal(18,2)")]
        public decimal Price { get; set; }
        public int Quantity { get; set; } = 1;

        [NotMapped]
        public decimal Subtotal => Price * Quantity;
    }

    /// <summary>
    /// Model Đơn hàng (Order)
    /// </summary>
    public class Order
    {
        [Key]
        public string Id { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string OrderDate { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");
        public int CustomerId { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập họ tên người nhận")]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại")]
        public string CustomerPhone { get; set; } = string.Empty;

        public string CustomerEmail { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ giao hàng")]
        public string ShippingAddress { get; set; } = string.Empty;

        public string PaymentMethod { get; set; } = "COD - Thanh toán khi nhận hàng";
        public string PaymentStatus { get; set; } = "Chưa thanh toán";
        public string OrderStatus { get; set; } = "pending"; // 'pending' | 'processing' | 'shipping' | 'delivered' | 'cancelled'
        public string OrderStatusText { get; set; } = "Chờ duyệt đơn";

        public string? ItemsJson { get; set; }

        public virtual List<OrderItem> Items { get; set; } = new();

        [Column(TypeName = "decimal(18,2)")]
        public decimal Subtotal { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal ShippingFee { get; set; } = 0;

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; }
        public string? Notes { get; set; }
    }
}
