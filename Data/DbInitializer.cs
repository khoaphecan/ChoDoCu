using Microsoft.EntityFrameworkCore;
using ThriftShop.Models;

namespace ThriftShop.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ThriftDbContext context)
        {
            context.Database.EnsureCreated();

            // Đảm bảo cập nhật bảng Orders nếu CSDL đã tồn tại từ trước (Giải quyết Issue 5 & 8)
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Orders')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedAt')
                        BEGIN
                            ALTER TABLE Orders ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                        END
                    END
                ");
            }
            catch { }

            // 1. SEED CATEGORIES (5 Danh mục chuẩn theo yêu cầu)
            if (!context.Categories.Any())
            {
                var categories = new List<Category>
                {
                    new() { Name = "Đồ điện tử", Icon = "fas fa-laptop", Description = "Điện thoại, tai nghe, máy tính, máy ảnh cũ thanh lý" },
                    new() { Name = "Đồ gia dụng", Icon = "fas fa-couch", Description = "Bàn ghế, quạt, đèn, đồ dùng gia đình thanh lý" },
                    new() { Name = "Văn phòng phẩm", Icon = "fas fa-book", Description = "Sách cũ, giáo trình, truyện tranh, bút vở thanh lý" },
                    new() { Name = "Thời trang", Icon = "fas fa-tshirt", Description = "Quần áo vintage, áo khoác, giày dép, túi xách secondhand" },
                    new() { Name = "Khác", Icon = "fas fa-shapes", Description = "Nhạc cụ, đồ decor, phụ kiện, đồ sưu tầm độc lạ" }
                };
                context.Categories.AddRange(categories);
                context.SaveChanges();
            }

            var catList = context.Categories.ToList();
            int catDienTu = catList.FirstOrDefault(c => c.Name == "Đồ điện tử")?.Id ?? 1;
            int catGiaDung = catList.FirstOrDefault(c => c.Name == "Đồ gia dụng")?.Id ?? 2;
            int catVPP = catList.FirstOrDefault(c => c.Name == "Văn phòng phẩm")?.Id ?? 3;
            int catThoiTrang = catList.FirstOrDefault(c => c.Name == "Thời trang")?.Id ?? 4;
            int catKhac = catList.FirstOrDefault(c => c.Name == "Khác")?.Id ?? 5;

            // 2. SEED USERS (Admin và Khách hàng)
            if (!context.Users.Any())
            {
                var users = new List<User>
                {
                    new()
                    {
                        Name = "Ban Quản Trị ThriftShop",
                        Email = "admin@thriftshop.vn",
                        Password = "admin",
                        Phone = "19006868",
                        Address = "TP. Hồ Chí Minh",
                        Avatar = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80",
                        Bio = "Ban Quản Trị & Kiểm Duyệt Sàn Đồ Cũ ThriftShop",
                        Role = "admin"
                    },
                    new()
                    {
                        Name = "Người Dùng Mẫu",
                        Email = "user@thriftshop.vn",
                        Password = "user",
                        Phone = "0901234567",
                        Address = "Quận 1, TP. Hồ Chí Minh",
                        Avatar = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                        Bio = "Thành viên tích cực mua bán & tái sử dụng đồ cũ.",
                        Role = "customer"
                    },
                    new()
                    {
                        Name = "Chung Tấn Thành",
                        Email = "thanh@thriftshop.vn",
                        Password = "user",
                        Phone = "0908765432",
                        Address = "Quận 1, TP. Hồ Chí Minh",
                        Avatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Bio = "Chuyên thanh lý đồ vintage, áo khoác, đồ denim sưu tầm.",
                        Role = "customer"
                    },
                    new()
                    {
                        Name = "Đặng Thuỳ Dương",
                        Email = "duong.dang@thriftshop.vn",
                        Password = "user",
                        Phone = "0918765432",
                        Address = "Quận Phú Nhuận, TP.HCM",
                        Avatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
                        Bio = "Thanh lý đồ công nghệ, đồ decor nhà cửa còn mới 95%.",
                        Role = "customer"
                    },
                    new()
                    {
                        Name = "Hoàng Minh Trí",
                        Email = "tri.hoang@thriftshop.vn",
                        Password = "user",
                        Phone = "0987654321",
                        Address = "Quận 3, TP.HCM",
                        Avatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Bio = "Đam mê sneaker secondhand và thời trang đường phố.",
                        Role = "customer"
                    }
                };
                context.Users.AddRange(users);
                context.SaveChanges();
            }

            var userList = context.Users.ToList();
            int userThanh = userList.FirstOrDefault(u => u.Name == "Chung Tấn Thành")?.Id ?? 3;
            int userDuong = userList.FirstOrDefault(u => u.Name == "Đặng Thuỳ Dương")?.Id ?? 4;
            int userTri = userList.FirstOrDefault(u => u.Name == "Hoàng Minh Trí")?.Id ?? 5;

            // 3. SEED PRODUCTS (Sản phẩm đồ cũ mở bán)
            if (!context.Products.Any())
            {
                var products = new List<Product>
                {
                    new()
                    {
                        Name = "Áo khoác Jean Levis Vintage 1990s lót lông",
                        CategoryId = catThoiTrang,
                        CategoryName = "Thời trang",
                        Price = 420000,
                        OriginalPrice = 1650000,
                        Condition = "92% - Tốt",
                        ConditionPercent = 92,
                        ConditionDetails = "Khuy áo nguyên bản, hơi sờn nhẹ gấu tay áo (chất vintage), không rách, đã giặt sấy thơm tho sạch sẽ.",
                        Description = "Áo jean Levis nguyên bản form boxy chuẩn vintage thập niên 90. Chất vải denim dày dặn, có bạc màu tự nhiên rất phong cách. Lớp lót lông cừu ấm áp không bị rách.",
                        Image = "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80\",\"https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80\",\"https://images.unsplash.com/photo-1520639888713-7851133b1ed0?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userThanh,
                        SellerName = "Chung Tấn Thành",
                        SellerPhone = "0908765432",
                        SellerEmail = "thanh@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Featured = true,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "01/10/2026 10:30"
                    },
                    new()
                    {
                        Name = "Tai nghe chống ồn Sony WH-1000XM4 màu Bạc",
                        CategoryId = catDienTu,
                        CategoryName = "Đồ điện tử",
                        Price = 3600000,
                        OriginalPrice = 6990000,
                        Condition = "95% - Rất mới",
                        ConditionPercent = 95,
                        ConditionDetails = "Đệm tai và headband còn nguyên vẹn không tróc da. Đầy đủ phụ kiện: hộp cứng bảo vệ, cáp Type-C, jack 3.5mm và adapter máy bay zin.",
                        Description = "Tai nghe chống ồn chủ động đỉnh cao Sony WH-1000XM4. Mua chính hãng Sony Store Việt Nam, ít dùng do chuyển sang dùng in-ear. Pin còn nghe liên tục trên 28 tiếng chống ồn.",
                        Image = "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80\",\"https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&q=80\",\"https://images.unsplash.com/photo-1484704849700-f032a568e944?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userDuong,
                        SellerName = "Đặng Thuỳ Dương",
                        SellerPhone = "0918765432",
                        SellerEmail = "duong.dang@thriftshop.vn",
                        SellerAddress = "Quận Phú Nhuận, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
                        Featured = true,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "02/10/2026 14:15"
                    },
                    new()
                    {
                        Name = "Giày Sneaker Nike Air Jordan 1 Retro High OG (Size 42)",
                        CategoryId = catThoiTrang,
                        CategoryName = "Thời trang",
                        Price = 1850000,
                        OriginalPrice = 4800000,
                        Condition = "90% - Tốt",
                        ConditionPercent = 90,
                        ConditionDetails = "Đế giày mòn nhẹ gót (đã dán sole bảo vệ lúc mới mua), da mũi giày có nếp nhăn tự nhiên, tặng kèm dây giày cam thay thế.",
                        Description = "Đôi Jordan 1 Retro bản High OG phối màu Chicago kinh điển. Giày chính hãng mua tại cửa hàng Nike Landmark 81, có box thay thế sạch sẽ.",
                        Image = "https://images.unsplash.com/photo-1552346154-21d32810aba3?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1552346154-21d32810aba3?w=600&q=80\",\"https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=600&q=80\",\"https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userTri,
                        SellerName = "Hoàng Minh Trí",
                        SellerPhone = "0987654321",
                        SellerEmail = "tri.hoang@thriftshop.vn",
                        SellerAddress = "Quận 3, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Featured = true,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "03/10/2026 09:00"
                    },
                    new()
                    {
                        Name = "Máy đọc sách Kindle Paperwhite Gen 4 (10th) 8GB",
                        CategoryId = catDienTu,
                        CategoryName = "Đồ điện tử",
                        Price = 1450000,
                        OriginalPrice = 2890000,
                        Condition = "94% - Rất mới",
                        ConditionPercent = 94,
                        ConditionDetails = "Màn hình E-ink không một vết xước, vỏ lưng cao su không chảy keo, pin dùng 3 tuần đọc mỗi ngày 1 tiếng.",
                        Description = "Kindle Paperwhite Gen 4 chống nước IPX8, màn hình 300ppi sắc nét như trang sách thật. Máy đã cài đặt sẵn từ điển Anh-Việt, hỗ trợ đọc file PRC, MOBI, EPUB, PDF.",
                        Image = "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80\",\"https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80\",\"https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userDuong,
                        SellerName = "Đặng Thuỳ Dương",
                        SellerPhone = "0918765432",
                        SellerEmail = "duong.dang@thriftshop.vn",
                        SellerAddress = "Quận Phú Nhuận, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
                        Featured = true,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "04/10/2026 11:20"
                    },
                    new()
                    {
                        Name = "Nồi chiên không dầu Philips HD9252 dung tích 4.1L",
                        CategoryId = catGiaDung,
                        CategoryName = "Đồ gia dụng",
                        Price = 850000,
                        OriginalPrice = 2200000,
                        Condition = "90% - Tốt",
                        ConditionPercent = 90,
                        ConditionDetails = "Khay chống dính còn rất tốt không bong tróc, bảng điều khiển cảm ứng nhạy bén, dây nguồn zin.",
                        Description = "Nồi chiên không dầu Philips công nghệ Rapid Air giảm 90% lượng dầu mỡ. Dung tích 4.1L đủ nướng gà nguyên con hoặc làm bánh cho cả gia đình.",
                        Image = "https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=600&q=80\",\"https://images.unsplash.com/photo-1556911220-e15b29be8c8f?w=600&q=80\",\"https://images.unsplash.com/photo-1540555700478-4be289fbecef?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userThanh,
                        SellerName = "Chung Tấn Thành",
                        SellerPhone = "0908765432",
                        SellerEmail = "thanh@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "05/10/2026 16:45"
                    },
                    new()
                    {
                        Name = "Combo trọn bộ 12 tập Manga Naruto Boxset sưu tầm",
                        CategoryId = catVPP,
                        CategoryName = "Văn phòng phẩm",
                        Price = 290000,
                        OriginalPrice = 650000,
                        Condition = "96% - Như mới",
                        ConditionPercent = 96,
                        ConditionDetails = "Sách bọc màng co nilon bảo quản cẩn thận, giấy không ố vàng, không quăn mép, đầy đủ bookmark tặng kèm.",
                        Description = "Trọn bộ truyện tranh Naruto chính hãng NXB Kim Đồng bản in chất lượng cao. Bộ sưu tập cá nhân ít đọc, giữ gìn kỹ càng trong tủ kính.",
                        Image = "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80\",\"https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80\",\"https://images.unsplash.com/photo-1524995997946-a1c2e315a42f?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userTri,
                        SellerName = "Hoàng Minh Trí",
                        SellerPhone = "0987654321",
                        SellerEmail = "tri.hoang@thriftshop.vn",
                        SellerAddress = "Quận 3, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "06/10/2026 08:30"
                    },
                    new()
                    {
                        Name = "Đèn bàn học chống cận Taotronics LED điều chỉnh 5 độ sáng",
                        CategoryId = catGiaDung,
                        CategoryName = "Đồ gia dụng",
                        Price = 320000,
                        OriginalPrice = 850000,
                        Condition = "93% - Rất tốt",
                        ConditionPercent = 93,
                        ConditionDetails = "Mọi chế độ sáng và cổng sạc USB tích hợp hoạt động bình thường, chân đế nặng chắc chắn không lỏng lẻo.",
                        Description = "Đèn LED Taotronics bảo vệ thị lực, có 5 tông màu ánh sáng từ vàng ấm đến trắng sáng và 7 mức cường độ sáng cảm ứng.",
                        Image = "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600&q=80\",\"https://images.unsplash.com/photo-1513506003901-1e6a229e2d15?w=600&q=80\",\"https://images.unsplash.com/photo-1584269600464-37b1b58a9fe7?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userThanh,
                        SellerName = "Chung Tấn Thành",
                        SellerPhone = "0908765432",
                        SellerEmail = "thanh@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "07/10/2026 13:10"
                    },
                    new()
                    {
                        Name = "Bút máy Parker Jotter London vỏ thép không gỉ",
                        CategoryId = catVPP,
                        CategoryName = "Văn phòng phẩm",
                        Price = 280000,
                        OriginalPrice = 680000,
                        Condition = "95% - Rất mới",
                        ConditionPercent = 95,
                        ConditionDetails = "Ngòi bút F trơn mượt không cào giấy, bơm mực piston chính hãng đi kèm, vỏ bút không trầy xước.",
                        Description = "Bút máy cao cấp Parker Jotter sản xuất tại Pháp. Thiết kế mũi tên đặc trưng, vỏ kim loại sáng bóng thanh lịch, phù hợp ký hợp đồng hoặc làm quà tặng.",
                        Image = "https://images.unsplash.com/photo-1583485088034-697b5bc54ccd?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1583485088034-697b5bc54ccd?w=600&q=80\",\"https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80\",\"https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userDuong,
                        SellerName = "Đặng Thuỳ Dương",
                        SellerPhone = "0918765432",
                        SellerEmail = "duong.dang@thriftshop.vn",
                        SellerAddress = "Quận Phú Nhuận, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "08/10/2026 15:00"
                    },
                    new()
                    {
                        Name = "Đàn Ukulele Concert Mahalo gỗ Mahogany tự nhiên",
                        CategoryId = catKhac,
                        CategoryName = "Khác",
                        Price = 390000,
                        OriginalPrice = 950000,
                        Condition = "94% - Rất tốt",
                        ConditionPercent = 94,
                        ConditionDetails = "Gỗ khô vang âm chuẩn, khóa đàn chắc chắn giữ dây tốt, dây Aquila chính hãng Ý, tặng bao da 3 lớp.",
                        Description = "Cây ukulele size Concert 23 inch dễ bấm cho người mới tập hoặc mang đi du lịch dã ngoại. Âm thanh ấm áp, mộc mạc và tươi vui.",
                        Image = "https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&q=80\",\"https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=600&q=80\",\"https://images.unsplash.com/photo-1465847899084-d164df4dedc6?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userTri,
                        SellerName = "Hoàng Minh Trí",
                        SellerPhone = "0987654321",
                        SellerEmail = "tri.hoang@thriftshop.vn",
                        SellerAddress = "Quận 3, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "09/10/2026 17:20"
                    },
                    new()
                    {
                        Name = "Mô hình Figure One Piece Luffy Gear 4 chính hãng Bandai",
                        CategoryId = catKhac,
                        CategoryName = "Khác",
                        Price = 450000,
                        OriginalPrice = 1100000,
                        Condition = "98% - Like New",
                        ConditionPercent = 98,
                        ConditionDetails = "Mô hình trưng tủ kính không bụi bẩn, không trầy sơn, đủ hộp và đế chống zin của hãng.",
                        Description = "Mô hình nhân vật Luffy trạng thái Gear 4 Snakeman siêu ngầu. Chi tiết sơn sắc nét, cơ bắp sống động, hiệu ứng khói chân thực.",
                        Image = "https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1566576912321-d58ddd7a6088?w=600&q=80\",\"https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&q=80\",\"https://images.unsplash.com/photo-1511671782779-c97d3d27a1d4?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userThanh,
                        SellerName = "Chung Tấn Thành",
                        SellerPhone = "0908765432",
                        SellerEmail = "thanh@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
                        Featured = false,
                        Status = "approved",
                        IsSold = false,
                        CreatedAt = "10/10/2026 12:00"
                    }
                };
                context.Products.AddRange(products);
                context.SaveChanges();
            }

            // 4. SEED PENDING POSTS (Tin chờ Admin duyệt)
            if (!context.PendingPosts.Any())
            {
                var pendingPosts = new List<PendingPost>
                {
                    new()
                    {
                        Name = "Bàn phím cơ không dây Keychron K2 v2 nhôm RGB",
                        CategoryId = catDienTu,
                        CategoryName = "Đồ điện tử",
                        Price = 1150000,
                        OriginalPrice = 2100000,
                        Condition = "95% - Rất tốt",
                        ConditionPercent = 95,
                        ConditionDetails = "Gateron Red Switch êm ái gõ văn phòng, pin kết nối bluetooth 3 thiết bị ngon lành, kèm cáp Type-C zin và keycap puller.",
                        Description = "Bàn phím cơ nhỏ gọn layout 75% cho cả Mac và Windows. Ít dùng do công ty cấp máy mới, còn nguyên tem bảo hành phongvu.",
                        Image = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80\",\"https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80\",\"https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = 2,
                        SellerName = "Người Dùng Mẫu",
                        SellerPhone = "0901234567",
                        SellerEmail = "user@thriftshop.vn",
                        SellerAddress = "Quận 1, TP. Hồ Chí Minh",
                        SellerAvatar = "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
                        Status = "pending",
                        CreatedAt = "12/10/2026 14:00"
                    },
                    new()
                    {
                        Name = "Áo Sweater Polo Ralph Lauren Vintage thêu ngựa nhỏ",
                        CategoryId = catThoiTrang,
                        CategoryName = "Thời trang",
                        Price = 350000,
                        OriginalPrice = 1200000,
                        Condition = "90% - Tốt",
                        ConditionPercent = 90,
                        ConditionDetails = "Chất len dệt kim mỏng nhẹ mặc mùa thu mát mẻ, bo chun tay áo còn co giãn tốt, không thủng rách.",
                        Description = "Sweater Ralph Lauren xách tay Nhật. Form oversize thoải mái unisex cả nam nữ đều mặc đẹp.",
                        Image = "https://images.unsplash.com/photo-1556905055-8f358a7a47b2?w=600&q=80",
                        ImagesJson = "[\"https://images.unsplash.com/photo-1556905055-8f358a7a47b2?w=600&q=80\",\"https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80\",\"https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80\"]",
                        Stock = 1,
                        SellerId = userTri,
                        SellerName = "Hoàng Minh Trí",
                        SellerPhone = "0987654321",
                        SellerEmail = "tri.hoang@thriftshop.vn",
                        SellerAddress = "Quận 3, TP.HCM",
                        SellerAvatar = "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
                        Status = "pending",
                        CreatedAt = "12/10/2026 15:30"
                    }
                };
                context.PendingPosts.AddRange(pendingPosts);
                context.SaveChanges();
            }

            // 5. SEED ORDERS
            if (!context.Orders.Any())
            {
                var sampleOrder = new Order
                {
                    Id = "DH100234",
                    CreatedAt = DateTime.Now.AddDays(-1),
                    OrderDate = "12/10/2026 10:15",
                    CustomerId = userList.FirstOrDefault(u => u.Email == "user@thriftshop.vn")?.Id ?? 2,
                    CustomerName = "Nguyễn Anh Khoa",
                    CustomerPhone = "0901234567",
                    CustomerEmail = "user@thriftshop.vn",
                    ShippingAddress = "Quận 1, TP. Hồ Chí Minh",
                    PaymentMethod = "COD - Thanh toán khi nhận hàng",
                    PaymentStatus = "Chưa thanh toán",
                    OrderStatus = "pending",
                    OrderStatusText = "Chờ duyệt đơn",
                    Subtotal = 450000,
                    ShippingFee = 0,
                    TotalAmount = 450000,
                    Notes = "Đồng kiểm ngoại hình khi nhận hàng",
                    Items = new List<OrderItem>
                    {
                        new()
                        {
                            ProductId = 1,
                            ProductName = "Bàn phím cơ DareU EK87 Tenkeyless Blue Switch cũ",
                            Image = "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80",
                            Condition = "95% - Rất tốt",
                            Price = 450000,
                            Quantity = 1
                        }
                    }
                };
                context.Orders.Add(sampleOrder);
                context.SaveChanges();
            }
        }
    }
}
