using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThriftShop.Models
{
    public class ChatMessage
    {
        public int Id { get; set; }
        public int? ConversationId { get; set; }
        public int? ProductId { get; set; }
        public int SenderId { get; set; }
        public string SenderName { get; set; } = string.Empty;
        public string? SenderAvatar { get; set; }
        public int ReceiverId { get; set; }
        public string ReceiverName { get; set; } = string.Empty;
        public string? ReceiverAvatar { get; set; }
        
        public string Message { get; set; } = string.Empty;

        // Alias Content tương thích với yêu cầu hệ thống
        [NotMapped]
        public string Content
        {
            get => Message;
            set => Message = value ?? string.Empty;
        }

        public string? ImageUrl { get; set; }

        [Column(TypeName = "datetime2")]
        public DateTime SentAt { get; set; } = DateTime.Now;

        [Column(TypeName = "datetime2")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public bool IsRead { get; set; } = false;

        public virtual Conversation? Conversation { get; set; }
        public virtual Product? Product { get; set; }
    }
}
