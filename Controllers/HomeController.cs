using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using ThriftShop.Common;
using ThriftShop.Data;
using ThriftShop.Models;

namespace ThriftShop.Controllers
{
    public class HomeController : Controller
    {
        private readonly ThriftDbContext _db;

        public HomeController(ThriftDbContext db)
        {
            _db = db;
        }

        // GET: / (Trang chủ - Hiển thị sản phẩm đồ cũ, lọc danh mục, khoảng giá, sắp xếp, phân trang 9 sp bằng LINQ)
        public IActionResult Index(string? search, int[]? catFilter, decimal? minPrice, decimal? maxPrice, string? sort, int page = 1)
        {
            const int pageSize = 9;
            if (page < 1) page = 1;

            // 1. LINQ: Lấy sản phẩm đang mở bán (Đã duyệt, chưa bán, chưa xoá)
            var query = _db.Products.AsNoTracking()
                .Where(p => !p.Deleted && !p.IsSold && p.Status == "approved");

            // 2. LINQ: Lọc theo danh mục
            if (catFilter != null && catFilter.Length > 0)
            {
                query = query.Where(p => catFilter.Contains(p.CategoryId));
            }

            // 3. LINQ: Lọc theo khoảng giá
            if (minPrice.HasValue && minPrice.Value > 0)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue && maxPrice.Value > 0)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 4. LINQ: Tìm kiếm từ khoá (CHỈ SO KHỚP CHÍNH XÁC TRÊN TÊN SẢN PHẨM Name)
            if (!string.IsNullOrWhiteSpace(search))
            {
                string kw = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(kw));
            }

            // 5. LINQ: Sắp xếp
            query = sort switch
            {
                "price-asc" => query.OrderBy(p => p.Price),
                "price-desc" => query.OrderByDescending(p => p.Price),
                _ => query.OrderByDescending(p => p.Id) // Mặc định: Mới nhất
            };

            // 6. LINQ: Tính tổng số lượng và phân trang
            var totalCount = query.Count();
            var totalPages = (int)Math.Ceiling(totalCount / (double)pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page > totalPages) page = totalPages;

            var products = query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var categories = _db.Categories.AsNoTracking().OrderBy(c => c.Id).ToList();
            ViewBag.Categories = categories;

            // Truy vấn danh sách ProductId đã thích nếu user đã đăng nhập
            int currentUserId = 0;
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value
                            ?? HttpContext.Session.GetInt32("UserId")?.ToString()
                            ?? HttpContext.Session.GetUserId()?.ToString();
            int.TryParse(userIdStr, out currentUserId);

            var favoriteProductIds = new List<int>();
            if (currentUserId > 0)
            {
                favoriteProductIds = _db.Favorites.AsNoTracking()
                    .Where(f => f.UserId == currentUserId)
                    .Select(f => f.ProductId)
                    .ToList();
            }
            ViewBag.FavoriteProductIds = favoriteProductIds;

            var viewModel = new HomeViewModel
            {
                Products = products,
                Categories = categories,
                TotalCount = totalCount,
                CurrentPage = page,
                TotalPages = totalPages,
                Search = search,
                CatFilter = catFilter,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Sort = sort ?? "newest",
                FavoriteProductIds = favoriteProductIds
            };

            return View(viewModel);
        }

        // GET: /api/categories (API đồng bộ danh mục động từ Database cho Client-side JS)
        [HttpGet("/api/categories")]
        public IActionResult GetCategories()
        {
            var categories = _db.Categories.AsNoTracking().OrderBy(c => c.Id)
                .Select(c => new
                {
                    id = c.Id,
                    name = c.Name,
                    icon = c.Icon,
                    description = c.Description
                })
                .ToList();
            return Json(categories);
        }

        // GET: /Home/Warranty (Chính sách bảo đảm đồ cũ uy tín)
        public IActionResult Warranty()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
