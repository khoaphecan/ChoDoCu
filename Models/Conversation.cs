using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ThriftShop.Models
{
    public class Conversation
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        public int? ProductId { get; set; }

        [Required]
        public int BuyerId { get; set; }

        [Required]
        public int SellerId { get; set; }

        public DateTime? BuyerClearedAt { get; set; }
        public DateTime? SellerClearedAt { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation
        public virtual Product? Product { get; set; }
        public virtual User? Buyer { get; set; }
        public virtual User? Seller { get; set; }
        public virtual ICollection<ChatMessage> Messages { get; set; } = new List<ChatMessage>();
    }
}
