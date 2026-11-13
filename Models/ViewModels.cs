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

    public class SellerProfileViewModel
    {
        public User User { get; set; } = new();
        public List<Product> ActiveProducts { get; set; } = new();
    }
}
