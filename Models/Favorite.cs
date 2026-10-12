using System.ComponentModel.DataAnnotations;

namespace ThriftShop.Models
{
    public class Favorite
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int ProductId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public virtual Product? Product { get; set; }
        public virtual User? User { get; set; }
    }
}
