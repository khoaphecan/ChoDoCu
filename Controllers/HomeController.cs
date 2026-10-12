using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
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

            // 1. LINQ: Lấy sản phẩm đang mở bán (Đã duyệt, chưa bán, chưa xoá, còn tồn kho)
            var query = _db.Products.AsNoTracking()
                .Where(p => p.Status == "approved" && !p.IsSold && !p.Deleted && p.Stock > 0);

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

            // 4. LINQ: Tìm kiếm từ khoá
            if (!string.IsNullOrWhiteSpace(search))
            {
                var q = search.Trim().ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(q) 
                                      || (p.Description != null && p.Description.ToLower().Contains(q))
                                      || p.CategoryName.ToLower().Contains(q));
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

            var categories = _db.Categories.AsNoTracking().ToList();

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
                Sort = sort ?? "newest"
            };

            return View(viewModel);
        }

        // GET: /Home/Warranty (Chính sách bảo đảm đồ cũ uy tín)
        public IActionResult Warranty()
        {
            return View();
        }

        // GET: /Home/Barter (Chuyển hướng sang Danh mục đồ cũ)
        public IActionResult Barter()
        {
            return RedirectToAction("Index", "Products");
        }

        // GET: /Home/Repair (Dịch vụ phục chế / spa đồ cũ)
        public IActionResult Repair()
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
