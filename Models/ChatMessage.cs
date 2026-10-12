using System.ComponentModel.DataAnnotations;

namespace ThriftShop.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int ProductId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverAvatar { get; set; }
        
        [Required]
        public string Message { get; set; } = string.Empty;
        public DateTime SentAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public virtual Product? Product { get; set; }
    }
}
