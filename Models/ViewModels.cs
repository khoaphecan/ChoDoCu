namespace ThriftShop.Models
{
    public class HomeViewModel
    {
        public List<Product> Products { get; set; } = new();
        public List<Category> Categories { get; set; } = new();
        public int TotalCount { get; set; }
        public int CurrentPage { get; set; } = 1;
        public int TotalPages { get; set; } = 1;
        public string? Search { get; set; }
        public int[]? CatFilter { get; set; }
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public string? Sort { get; set; } = "newest";
    }

    public class ProductDetailViewModel
    {
        public Product Product { get; set; } = new();
        public List<Product> RelatedProducts { get; set; } = new();
        public User? Seller { get; set; }
        public List<string> Images { get; set; } = new();
        public List<string> Videos { get; set; } = new();
        public bool IsFavorite { get; set; }
    }

    public class AdminDashboardViewModel
    {
        public int PendingCount { get; set; }
        public int ProductsCount { get; set; }
        public int UsersCount { get; set; }
        public List<PendingPost> RecentPending { get; set; } = new();
        public List<Product> RecentProducts { get; set; } = new();
    }

    public class SellerProfileViewModel
    {
        public User User { get; set; } = new();
        public List<Product> ActiveProducts { get; set; } = new();
    }

    public class ChatThreadViewModel
    {
        public Product Product { get; set; } = new();
        public User OtherUser { get; set; } = new();
        public List<ChatMessage> Messages { get; set; } = new();
    }
}
