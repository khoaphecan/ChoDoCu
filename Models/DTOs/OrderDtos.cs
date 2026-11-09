using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ThriftShop.Models.DTOs
{
    /// <summary>
    /// Thông tin 1 món đồ trong đơn hàng (Đã bỏ hoàn toàn condition)
    /// </summary>
    public class OrderItemDto
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("image")]
        public string Image { get; set; } = string.Empty;

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 1;

        [JsonPropertyName("subtotal")]
        public decimal Subtotal => Price * Quantity;
    }

    /// <summary>
    /// Payload đặt hàng gửi lên từ Checkout.cshtml
    /// </summary>
    public class CheckoutRequestDto
    {
        [Required(ErrorMessage = "Vui lòng nhập họ và tên người nhận hàng")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên người nhận từ 2 đến 100 ký tự")]
        [JsonPropertyName("custName")]
        public string CustName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng nhập số điện thoại nhận hàng")]
        [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
        [RegularExpression(@"^(\+84|0)\d{9,10}$", ErrorMessage = "Số điện thoại nhận hàng không hợp lệ")]
        [JsonPropertyName("custPhone")]
        public string CustPhone { get; set; } = string.Empty;

        [EmailAddress(ErrorMessage = "Email không hợp lệ")]
        [JsonPropertyName("custEmail")]
        public string? CustEmail { get; set; }

        [Required(ErrorMessage = "Vui lòng nhập địa chỉ chi tiết nhận hàng")]
        [StringLength(255, ErrorMessage = "Địa chỉ nhận hàng tối đa 255 ký tự")]
        [JsonPropertyName("custAddress")]
        public string CustAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "Vui lòng chọn phương thức thanh toán")]
        [JsonPropertyName("paymentMethod")]
        public string PaymentMethod { get; set; } = "COD - Thanh toán khi nhận hàng";

        [JsonPropertyName("notes")]
        public string? Notes { get; set; } = "Đồng kiểm ngoại hình khi nhận hàng";

        [Required(ErrorMessage = "Giỏ hàng không có sản phẩm nào")]
        [JsonPropertyName("itemsJson")]
        public string ItemsJson { get; set; } = "[]";

        [Required(ErrorMessage = "Vui lòng cung cấp tổng tiền thanh toán")]
        [Range(1000, 1000000000, ErrorMessage = "Tổng tiền thanh toán không hợp lệ")]
        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }
    }

    /// <summary>
    /// Response tóm tắt đơn hàng cho trang Orders/Index.cshtml
    /// </summary>
    public class OrderSummaryDto
    {
        [JsonPropertyName("id")]
        public string Id { get; set; } = string.Empty; // Format dạng 'DH000123'

        [JsonPropertyName("orderDate")]
        public string OrderDate { get; set; } = DateTime.Now.ToString("dd/MM/yyyy HH:mm");

        [JsonPropertyName("orderStatus")]
        public string OrderStatus { get; set; } = "pending"; // 'pending' | 'processing' | 'shipping' | 'delivered' | 'cancelled'

        [JsonPropertyName("customerName")]
        public string CustomerName { get; set; } = string.Empty;

        [JsonPropertyName("customerPhone")]
        public string CustomerPhone { get; set; } = string.Empty;

        [JsonPropertyName("shippingAddress")]
        public string ShippingAddress { get; set; } = string.Empty;

        [JsonPropertyName("totalAmount")]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("items")]
        public List<OrderItemDto> Items { get; set; } = new();
    }
}
