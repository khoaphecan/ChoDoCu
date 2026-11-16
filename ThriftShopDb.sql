-- =======================================================================================
-- ThriftShop Database Setup & Initial Seed Script
-- Nền tảng Trao Đổi & Bán Đồ Cũ Secondhand (C2C E-Commerce)
-- Công nghệ: ASP.NET Core MVC 8.0 + Entity Framework Core + SQL Server
-- Tài khoản Quản trị mặc định: admin@thriftshop.vn / Mật khẩu: admin
-- Tài khoản Người dùng mẫu: nguyenvana@gmail.com / tranthib@gmail.com (Mật khẩu: 123456)
-- =======================================================================================

IF NOT EXISTS (SELECT * FROM sys.databases WHERE name = N'ThriftShopDB')
BEGIN
    CREATE DATABASE [ThriftShopDB];
END
GO

USE [ThriftShopDB];
GO

-- 1. BẢNG CATEGORIES (Danh mục sản phẩm đồ cũ)
IF OBJECT_ID(N'[dbo].[Categories]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Categories] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(100) NOT NULL,
        [Icon] NVARCHAR(50) NOT NULL CONSTRAINT [DF_Categories_Icon] DEFAULT ('fas fa-tag'),
        [Description] NVARCHAR(255) NULL,
        CONSTRAINT [PK_Categories] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 2. BẢNG USERS (Người dùng, người bán C2C và Admin)
IF OBJECT_ID(N'[dbo].[Users]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Users] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [FullName] NVARCHAR(100) NOT NULL,
        [Email] NVARCHAR(150) NOT NULL,
        [PasswordHash] NVARCHAR(MAX) NOT NULL,
        [Phone] NVARCHAR(20) NULL,
        [Address] NVARCHAR(255) NULL,
        [Avatar] NVARCHAR(MAX) NULL,
        [Bio] NVARCHAR(500) NULL,
        [Role] NVARCHAR(20) NOT NULL CONSTRAINT [DF_Users_Role] DEFAULT ('customer'),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_Users_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_Users] PRIMARY KEY CLUSTERED ([Id] ASC)
    );
END
GO

-- 3. BẢNG PENDINGPOSTS (Hàng đợi tin đăng bán đồ cũ chờ Admin duyệt)
IF OBJECT_ID(N'[dbo].[PendingPosts]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[PendingPosts] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(150) NOT NULL,
        [CategoryId] INT NOT NULL,
        [CategoryName] NVARCHAR(100) NULL,
        [Price] DECIMAL(18, 2) NOT NULL,
        [OriginalPrice] DECIMAL(18, 2) NOT NULL,
        [Description] NVARCHAR(MAX) NOT NULL,
        [Image] NVARCHAR(MAX) NOT NULL,
        [ImagesJson] NVARCHAR(MAX) NULL,
        [VideosJson] NVARCHAR(MAX) NULL,
        [Stock] INT NOT NULL CONSTRAINT [DF_PendingPosts_Stock] DEFAULT ((1)),
        [SellerId] INT NOT NULL,
        [SellerName] NVARCHAR(100) NOT NULL,
        [SellerPhone] NVARCHAR(20) NOT NULL,
        [SellerEmail] NVARCHAR(150) NULL,
        [SellerAddress] NVARCHAR(255) NOT NULL,
        [SellerAvatar] NVARCHAR(255) NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_PendingPosts_Status] DEFAULT ('pending'),
        [IsSold] BIT NOT NULL CONSTRAINT [DF_PendingPosts_IsSold] DEFAULT ((0)),
        [RejectReason] NVARCHAR(255) NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_PendingPosts_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME2 NULL,
        [Deleted] BIT NOT NULL CONSTRAINT [DF_PendingPosts_Deleted] DEFAULT ((0)),
        CONSTRAINT [PK_PendingPosts] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_PendingPosts_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]),
        CONSTRAINT [FK_PendingPosts_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [dbo].[Users] ([Id])
    );
END
GO

-- 4. BẢNG PRODUCTS (Sản phẩm đồ cũ đang mở bán sau khi duyệt)
IF OBJECT_ID(N'[dbo].[Products]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Products] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [Name] NVARCHAR(150) NOT NULL,
        [CategoryId] INT NOT NULL,
        [CategoryName] NVARCHAR(100) NULL,
        [Price] DECIMAL(18, 2) NOT NULL,
        [OriginalPrice] DECIMAL(18, 2) NOT NULL,
        [Description] NVARCHAR(MAX) NULL,
        [Image] NVARCHAR(MAX) NOT NULL,
        [ImagesJson] NVARCHAR(MAX) NULL,
        [VideosJson] NVARCHAR(MAX) NULL,
        [Stock] INT NOT NULL CONSTRAINT [DF_Products_Stock] DEFAULT ((1)),
        [SoldCount] INT NOT NULL CONSTRAINT [DF_Products_SoldCount] DEFAULT ((0)),
        [SellerId] INT NOT NULL,
        [SellerName] NVARCHAR(100) NULL,
        [SellerPhone] NVARCHAR(20) NULL,
        [SellerEmail] NVARCHAR(150) NULL,
        [SellerAddress] NVARCHAR(255) NULL,
        [SellerAvatar] NVARCHAR(255) NULL,
        [Featured] BIT NOT NULL CONSTRAINT [DF_Products_Featured] DEFAULT ((0)),
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_Products_Status] DEFAULT ('approved'),
        [IsSold] BIT NOT NULL CONSTRAINT [DF_Products_IsSold] DEFAULT ((0)),
        [Deleted] BIT NOT NULL CONSTRAINT [DF_Products_Deleted] DEFAULT ((0)),
        [PendingPostId] INT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_Products_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_Products] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Products_Categories_CategoryId] FOREIGN KEY ([CategoryId]) REFERENCES [dbo].[Categories] ([Id]),
        CONSTRAINT [FK_Products_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [dbo].[Users] ([Id])
    );
END
GO

-- 5. BẢNG CONVERSATIONS (Cuộc hội thoại chat giữa Người mua và Người bán / Hỗ trợ Admin)
IF OBJECT_ID(N'[dbo].[Conversations]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Conversations] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ProductId] INT NULL,
        [BuyerId] INT NOT NULL,
        [SellerId] INT NOT NULL,
        [BuyerClearedAt] DATETIME2 NULL,
        [SellerClearedAt] DATETIME2 NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_Conversations_CreatedAt] DEFAULT (GETDATE()),
        [UpdatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_Conversations_UpdatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_Conversations] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Conversations_Users_BuyerId] FOREIGN KEY ([BuyerId]) REFERENCES [dbo].[Users] ([Id]),
        CONSTRAINT [FK_Conversations_Users_SellerId] FOREIGN KEY ([SellerId]) REFERENCES [dbo].[Users] ([Id]),
        CONSTRAINT [FK_Conversations_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE SET NULL
    );
END
GO

-- 6. BẢNG CHATMESSAGES (Tin nhắn trao đổi trong cuộc hội thoại)
IF OBJECT_ID(N'[dbo].[ChatMessages]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[ChatMessages] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [ProductId] INT NULL,
        [SenderId] INT NOT NULL,
        [SenderName] NVARCHAR(100) NOT NULL,
        [SenderAvatar] NVARCHAR(MAX) NULL,
        [ReceiverId] INT NOT NULL,
        [ReceiverName] NVARCHAR(100) NOT NULL,
        [ReceiverAvatar] NVARCHAR(MAX) NULL,
        [Message] NVARCHAR(MAX) NULL,
        [SentAt] DATETIME2 NOT NULL CONSTRAINT [DF_ChatMessages_SentAt] DEFAULT (GETDATE()),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_ChatMessages_CreatedAt] DEFAULT (GETDATE()),
        [IsRead] BIT NOT NULL CONSTRAINT [DF_ChatMessages_IsRead] DEFAULT ((0)),
        [ConversationId] INT NULL,
        [ImageUrl] NVARCHAR(MAX) NULL,
        CONSTRAINT [PK_ChatMessages] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_ChatMessages_Conversations_ConversationId] FOREIGN KEY ([ConversationId]) REFERENCES [dbo].[Conversations] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_ChatMessages_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
    );
END
GO

-- 7. BẢNG ORDERS (Đơn hàng người mua đặt trực tiếp)
IF OBJECT_ID(N'[dbo].[Orders]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Orders] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [CustName] NVARCHAR(100) NOT NULL,
        [CustPhone] NVARCHAR(20) NOT NULL,
        [CustEmail] NVARCHAR(150) NULL,
        [CustAddress] NVARCHAR(255) NOT NULL,
        [PaymentMethod] NVARCHAR(100) NOT NULL CONSTRAINT [DF_Orders_PaymentMethod] DEFAULT (N'COD - Thanh toán khi nhận hàng'),
        [Notes] NVARCHAR(500) NULL,
        [TotalAmount] DECIMAL(18, 2) NOT NULL,
        [Status] NVARCHAR(30) NOT NULL CONSTRAINT [DF_Orders_Status] DEFAULT ('pending'),
        [OrderDate] DATETIME2 NOT NULL CONSTRAINT [DF_Orders_OrderDate] DEFAULT (GETDATE()),
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_Orders_CreatedAt] DEFAULT (GETDATE()),
        [UserId] INT NULL,
        CONSTRAINT [PK_Orders] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Orders_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE SET NULL
    );
END
GO

-- 8. BẢNG ORDERITEMS (Chi tiết các mặt hàng trong đơn)
IF OBJECT_ID(N'[dbo].[OrderItems]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[OrderItems] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [OrderId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [ProductName] NVARCHAR(150) NOT NULL,
        [Price] DECIMAL(18, 2) NOT NULL,
        [Quantity] INT NOT NULL CONSTRAINT [DF_OrderItems_Quantity] DEFAULT ((1)),
        [Subtotal] DECIMAL(18, 2) NOT NULL,
        [Image] NVARCHAR(255) NOT NULL,
        CONSTRAINT [PK_OrderItems] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_OrderItems_Orders_OrderId] FOREIGN KEY ([OrderId]) REFERENCES [dbo].[Orders] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_OrderItems_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id])
    );
END
GO

-- 9. BẢNG FAVORITES (Món đồ người dùng lưu yêu thích)
IF OBJECT_ID(N'[dbo].[Favorites]', N'U') IS NULL
BEGIN
    CREATE TABLE [dbo].[Favorites] (
        [Id] INT IDENTITY(1,1) NOT NULL,
        [UserId] INT NOT NULL,
        [ProductId] INT NOT NULL,
        [CreatedAt] DATETIME2 NOT NULL CONSTRAINT [DF_Favorites_CreatedAt] DEFAULT (GETDATE()),
        CONSTRAINT [PK_Favorites] PRIMARY KEY CLUSTERED ([Id] ASC),
        CONSTRAINT [FK_Favorites_Users_UserId] FOREIGN KEY ([UserId]) REFERENCES [dbo].[Users] ([Id]) ON DELETE CASCADE,
        CONSTRAINT [FK_Favorites_Products_ProductId] FOREIGN KEY ([ProductId]) REFERENCES [dbo].[Products] ([Id]) ON DELETE CASCADE
    );
END
GO

-- =======================================================================================
-- DỮ LIỆU KHỞI TẠO CHUẨN (SEED DATA)
-- TẤT CẢ GIÁ TRỊ NGÀY GIỜ ĐỀU SỬ DỤNG ĐỊNH DẠNG CHUẨN ISO 8601 HOẶC GETDATE()
-- =======================================================================================

-- 1. SEED 5 DANH MỤC ĐỒ CŨ MẶC ĐỊNH
IF NOT EXISTS (SELECT * FROM [dbo].[Categories])
BEGIN
    SET IDENTITY_INSERT [dbo].[Categories] ON;
    INSERT INTO [dbo].[Categories] ([Id], [Name], [Icon], [Description]) VALUES
    (1, N'Đồ điện tử', N'fas fa-laptop', N'Điện thoại, tai nghe, máy tính, máy ảnh cũ thanh lý'),
    (2, N'Đồ gia dụng', N'fas fa-couch', N'Bàn ghế, quạt, đèn, đồ dùng gia đình thanh lý'),
    (3, N'Văn phòng phẩm', N'fas fa-book', N'Sách cũ, giáo trình, truyện tranh, bút vở thanh lý'),
    (4, N'Thời trang', N'fas fa-tshirt', N'Quần áo vintage, áo khoác, giày dép, túi xách secondhand'),
    (5, N'Khác', N'fas fa-shapes', N'Nhạc cụ, đồ decor, phụ kiện, đồ sưu tầm độc lạ');
    SET IDENTITY_INSERT [dbo].[Categories] OFF;
END
GO

-- 2. SEED TÀI KHOẢN NGƯỜI DÙNG (Admin & Người bán/mua mẫu)
IF NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [Email] = 'admin@thriftshop.vn')
BEGIN
    SET IDENTITY_INSERT [dbo].[Users] ON;
    INSERT INTO [dbo].[Users] ([Id], [FullName], [Email], [PasswordHash], [Phone], [Address], [Avatar], [Bio], [Role], [CreatedAt]) VALUES
    (1, N'Ban Quản Trị ThriftShop', 'admin@thriftshop.vn', 'admin', '19006868', N'TP. Hồ Chí Minh', '/images/avatars/avatar_1_639301642064879045.webp', N'Ban Quản Trị & Kiểm Duyệt Sàn Đồ Cũ ThriftShop', 'admin', '2026-11-13T08:00:00'),
    (2, N'Nguyễn Văn A', 'nguyenvana@gmail.com', '123456', '0901234567', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', N'Người yêu đồ vintage & secondhand cổ điển', 'customer', '2026-11-13T08:30:00'),
    (3, N'Trần Thị B', 'tranthib@gmail.com', '123456', '0912345678', N'Quận 3, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', N'Thanh lý đồ gia dụng và thời trang secondhand', 'customer', '2026-11-13T09:00:00');
    SET IDENTITY_INSERT [dbo].[Users] OFF;
END
GO

-- 3. SEED BẢNG PENDINGPOSTS (Các tin đăng bán đã duyệt và chờ duyệt)
IF NOT EXISTS (SELECT * FROM [dbo].[PendingPosts])
BEGIN
    SET IDENTITY_INSERT [dbo].[PendingPosts] ON;
    INSERT INTO [dbo].[PendingPosts] ([Id], [Name], [CategoryId], [CategoryName], [Price], [OriginalPrice], [Description], [Image], [ImagesJson], [VideosJson], [Stock], [SellerId], [SellerName], [SellerPhone], [SellerEmail], [SellerAddress], [SellerAvatar], [Status], [IsSold], [RejectReason], [CreatedAt], [UpdatedAt], [Deleted]) VALUES
    (1, N'Máy ảnh Film Polaroid Impulse AF Auto Focus (1992)', 1, N'Đồ điện tử', 1850000.00, 3200000.00, N'Máy ảnh film lấy liền Polaroid Impulse AF cổ điển sản xuất tại Vương Quốc Anh năm 1992. Thấu kính Sonar Auto Focus cực nét, đèn flash đánh nhạy, cơ cuốn film mượt mà. Kèm hộp zin và dây đeo da.', 'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600', '["https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600","https://images.unsplash.com/photo-1502920917128-1aa500764cbd?w=600","https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=600"]', NULL, 1, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 'approved', 0, NULL, '2026-11-13T10:00:00', '2026-11-13T11:00:00', 0),
    (2, N'Áo khoác Denim Levi''s Vintage Trucker Jacket 70505', 4, N'Thời trang', 650000.00, 2100000.00, N'Áo khoác jeans Levi''s chính hãng form vintage thập niên 90. Chất vải denim dày dặn đã ngả màu wash tự nhiên cực đẹp, cúc đồng nguyên bản dập chìm logo Levi Strauss.', 'https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600', '["https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600","https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600","https://images.unsplash.com/photo-1548883354-7622d03aca27?w=600"]', NULL, 1, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 'approved', 0, NULL, '2026-11-13T10:30:00', '2026-11-13T11:30:00', 0),
    (3, N'Bàn làm việc gỗ sồi tự nhiên phong cách Bắc Âu (Scandinavian)', 2, N'Đồ gia dụng', 1200000.00, 3500000.00, N'Bàn làm việc gỗ sồi tự nhiên còn mới 95%, kích thước 120x60x75cm. Thiết kế tối giản 2 ngăn kéo tiện lợi, bề mặt phủ sơn mờ chống trầy xước, tháo lắp dễ dàng.', 'https://images.unsplash.com/photo-1518455027359-f3f8164ba6bd?w=600', '["https://images.unsplash.com/photo-1518455027359-f3f8164ba6bd?w=600","https://images.unsplash.com/photo-1538688525198-9b88f6f53126?w=600"]', NULL, 1, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 'approved', 0, NULL, '2026-11-13T11:00:00', '2026-11-13T12:00:00', 0),
    (4, N'Bàn phím cơ không dây Keychron K2 V2 khung nhôm Gateron Brown', 1, N'Đồ điện tử', 950000.00, 2250000.00, N'Bàn phím cơ Keychron K2 V2 Bluetooth kết nối cùng lúc 3 thiết bị (Mac/Win/iOS). Switch Gateron Brown gõ êm mượt, LED đơn trắng tinh tế, pin trâu 4000mAh. Kèm dây type-C bọc dù và keycap puller.', 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600', '["https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600","https://images.unsplash.com/photo-1618384887929-16ec33fab9ef?w=600"]', NULL, 1, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 'approved', 0, NULL, '2026-11-13T11:30:00', '2026-11-13T12:30:00', 0),
    (5, N'Combo Trọn Bộ Truyện Tranh Dragon Ball Siêu Cấp 20 Tập', 3, N'Văn phòng phẩm', 380000.00, 800000.00, N'Bộ truyện tranh Dragon Ball Super từ tập 1 đến tập 20 bản quyền Kim Đồng, giấy đẹp không quăn mép, bảo quản kỹ trong bọc chống ẩm.', 'https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=600', '["https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=600","https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600"]', NULL, 1, 3, N'Trần Thị B', '0912345678', 'tranthib@gmail.com', N'Quận 3, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', 'approved', 0, NULL, '2026-11-13T12:00:00', '2026-11-13T13:00:00', 0),
    (6, N'Đồng hồ cơ cổ điển Seiko 5 Automatic Vintage 1980s', 5, N'Khác', 1450000.00, 3500000.00, N'Đồng hồ cổ Seiko 5 cơ tự động lên cót sản xuất tại Nhật Bản thập niên 80. Mặt số chải tia màu vàng champange, dây thép đúc nguyên bản, hoạt động chính xác giữ giờ tốt.', 'https://images.unsplash.com/photo-1524805444758-089113d48a6d?w=600', '["https://images.unsplash.com/photo-1524805444758-089113d48a6d?w=600","https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?w=600"]', NULL, 1, 3, N'Trần Thị B', '0912345678', 'tranthib@gmail.com', N'Quận 3, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', 'approved', 0, NULL, '2026-11-13T12:30:00', '2026-11-13T13:30:00', 0),
    (7, N'Tai nghe kiểm âm Sony MDR-7506 chuyên phòng thu', 1, N'Đồ điện tử', 1350000.00, 2600000.00, N'Tai nghe studio huyền thoại Sony MDR-7506, dải âm trung thực, đệm tai còn mới tinh kèm jack chuyển 6.3mm mạ vàng.', 'https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600', '["https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600","https://images.unsplash.com/photo-1484704849700-f032a568e944?w=600"]', NULL, 1, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 'pending', 0, NULL, '2026-11-14T08:00:00', NULL, 0),
    (8, N'Ghế công thái học Ergonomic Sihoo M57 nguyên bản', 2, N'Đồ gia dụng', 1850000.00, 3800000.00, N'Ghế Sihoo M57 lưới toàn thân thoáng mát, đỡ thắt lưng tùy chỉnh 3 hướng, piston và chân nhôm sáng bóng chịu lực tốt.', 'https://images.unsplash.com/photo-1580481077195-c22ae2499d07?w=600', '["https://images.unsplash.com/photo-1580481077195-c22ae2499d07?w=600","https://images.unsplash.com/photo-1505797149-43b0069ec26b?w=600"]', NULL, 1, 3, N'Trần Thị B', '0912345678', 'tranthib@gmail.com', N'Quận 3, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', 'pending', 0, NULL, '2026-11-14T08:30:00', NULL, 0);
    SET IDENTITY_INSERT [dbo].[PendingPosts] OFF;
END
GO

-- 4. SEED BẢNG PRODUCTS (Các sản phẩm đã duyệt mở bán hiển thị ở Trang chủ & Cửa hàng)
IF NOT EXISTS (SELECT * FROM [dbo].[Products])
BEGIN
    SET IDENTITY_INSERT [dbo].[Products] ON;
    INSERT INTO [dbo].[Products] ([Id], [Name], [CategoryId], [CategoryName], [Price], [OriginalPrice], [Description], [Image], [ImagesJson], [VideosJson], [Stock], [SoldCount], [SellerId], [SellerName], [SellerPhone], [SellerEmail], [SellerAddress], [SellerAvatar], [Featured], [Status], [IsSold], [Deleted], [PendingPostId], [CreatedAt]) VALUES
    (1, N'Máy ảnh Film Polaroid Impulse AF Auto Focus (1992)', 1, N'Đồ điện tử', 1850000.00, 3200000.00, N'Máy ảnh film lấy liền Polaroid Impulse AF cổ điển sản xuất tại Vương Quốc Anh năm 1992. Thấu kính Sonar Auto Focus cực nét, đèn flash đánh nhạy, cơ cuốn film mượt mà. Kèm hộp zin và dây đeo da.', 'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600', '["https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600","https://images.unsplash.com/photo-1502920917128-1aa500764cbd?w=600","https://images.unsplash.com/photo-1516035069371-29a1b244cc32?w=600"]', NULL, 1, 0, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 1, 'approved', 0, 0, 1, '2026-11-13T11:00:00'),
    (2, N'Áo khoác Denim Levi''s Vintage Trucker Jacket 70505', 4, N'Thời trang', 650000.00, 2100000.00, N'Áo khoác jeans Levi''s chính hãng form vintage thập niên 90. Chất vải denim dày dặn đã ngả màu wash tự nhiên cực đẹp, cúc đồng nguyên bản dập chìm logo Levi Strauss.', 'https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600', '["https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600","https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600","https://images.unsplash.com/photo-1548883354-7622d03aca27?w=600"]', NULL, 1, 0, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 1, 'approved', 0, 0, 2, '2026-11-13T11:30:00'),
    (3, N'Bàn làm việc gỗ sồi tự nhiên phong cách Bắc Âu (Scandinavian)', 2, N'Đồ gia dụng', 1200000.00, 3500000.00, N'Bàn làm việc gỗ sồi tự nhiên còn mới 95%, kích thước 120x60x75cm. Thiết kế tối giản 2 ngăn kéo tiện lợi, bề mặt phủ sơn mờ chống trầy xước, tháo lắp dễ dàng.', 'https://images.unsplash.com/photo-1518455027359-f3f8164ba6bd?w=600', '["https://images.unsplash.com/photo-1518455027359-f3f8164ba6bd?w=600","https://images.unsplash.com/photo-1538688525198-9b88f6f53126?w=600"]', NULL, 1, 0, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 0, 'approved', 0, 0, 3, '2026-11-13T12:00:00'),
    (4, N'Bàn phím cơ không dây Keychron K2 V2 khung nhôm Gateron Brown', 1, N'Đồ điện tử', 950000.00, 2250000.00, N'Bàn phím cơ Keychron K2 V2 Bluetooth kết nối cùng lúc 3 thiết bị (Mac/Win/iOS). Switch Gateron Brown gõ êm mượt, LED đơn trắng tinh tế, pin trâu 4000mAh. Kèm dây type-C bọc dù và keycap puller.', 'https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600', '["https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600","https://images.unsplash.com/photo-1618384887929-16ec33fab9ef?w=600"]', NULL, 1, 0, 2, N'Nguyễn Văn A', '0901234567', 'nguyenvana@gmail.com', N'Quận 1, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 1, 'approved', 0, 0, 4, '2026-11-13T12:30:00'),
    (5, N'Combo Trọn Bộ Truyện Tranh Dragon Ball Siêu Cấp 20 Tập', 3, N'Văn phòng phẩm', 380000.00, 800000.00, N'Bộ truyện tranh Dragon Ball Super từ tập 1 đến tập 20 bản quyền Kim Đồng, giấy đẹp không quăn mép, bảo quản kỹ trong bọc chống ẩm.', 'https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=600', '["https://images.unsplash.com/photo-1607604276583-eef5d076aa5f?w=600","https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600"]', NULL, 1, 0, 3, N'Trần Thị B', '0912345678', 'tranthib@gmail.com', N'Quận 3, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', 1, 'approved', 0, 0, 5, '2026-11-13T13:00:00'),
    (6, N'Đồng hồ cơ cổ điển Seiko 5 Automatic Vintage 1980s', 5, N'Khác', 1450000.00, 3500000.00, N'Đồng hồ cổ Seiko 5 cơ tự động lên cót sản xuất tại Nhật Bản thập niên 80. Mặt số chải tia màu vàng champange, dây thép đúc nguyên bản, hoạt động chính xác giữ giờ tốt.', 'https://images.unsplash.com/photo-1524805444758-089113d48a6d?w=600', '["https://images.unsplash.com/photo-1524805444758-089113d48a6d?w=600","https://images.unsplash.com/photo-1522335789203-aabd1fc54bc9?w=600"]', NULL, 1, 0, 3, N'Trần Thị B', '0912345678', 'tranthib@gmail.com', N'Quận 3, TP. Hồ Chí Minh', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', 0, 'approved', 0, 0, 6, '2026-11-13T13:30:00');
    SET IDENTITY_INSERT [dbo].[Products] OFF;
END
GO

-- 5. SEED BẢNG CONVERSATIONS (Cuộc hội thoại mẫu giữa Người mua và Người bán)
IF NOT EXISTS (SELECT * FROM [dbo].[Conversations])
BEGIN
    SET IDENTITY_INSERT [dbo].[Conversations] ON;
    INSERT INTO [dbo].[Conversations] ([Id], [ProductId], [BuyerId], [SellerId], [BuyerClearedAt], [SellerClearedAt], [CreatedAt], [UpdatedAt]) VALUES
    (1, 1, 3, 2, NULL, NULL, '2026-11-13T14:00:00', '2026-11-13T14:15:00');
    SET IDENTITY_INSERT [dbo].[Conversations] OFF;
END
GO

-- 6. SEED BẢNG CHATMESSAGES (Tin nhắn trao đổi trong hội thoại mẫu)
IF NOT EXISTS (SELECT * FROM [dbo].[ChatMessages])
BEGIN
    SET IDENTITY_INSERT [dbo].[ChatMessages] ON;
    INSERT INTO [dbo].[ChatMessages] ([Id], [ProductId], [SenderId], [SenderName], [SenderAvatar], [ReceiverId], [ReceiverName], [ReceiverAvatar], [Message], [SentAt], [CreatedAt], [IsRead], [ConversationId], [ImageUrl]) VALUES
    (1, 1, 3, N'Trần Thị B', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', 2, N'Nguyễn Văn A', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', N'Chào bạn, máy ảnh Polaroid này ống kính còn trong và cơ cuốn film ổn định không bạn?', '2026-11-13T14:00:00', '2026-11-13T14:00:00', 1, 1, NULL),
    (2, 1, 2, N'Nguyễn Văn A', 'https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150', 3, N'Trần Thị B', 'https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150', N'Chào bạn! Thấu kính Sonar bắt nét rất chuẩn, cơ hoàn toàn êm ái nhé, bạn có thể kiểm tra trực tiếp khi nhận hàng ạ!', '2026-11-13T14:15:00', '2026-11-13T14:15:00', 1, 1, NULL);
    SET IDENTITY_INSERT [dbo].[ChatMessages] OFF;
END
GO

-- 7. SEED BẢNG ORDERS & ORDERITEMS (Đơn hàng mẫu kiểm tra chức năng đặt hàng)
IF NOT EXISTS (SELECT * FROM [dbo].[Orders])
BEGIN
    SET IDENTITY_INSERT [dbo].[Orders] ON;
    INSERT INTO [dbo].[Orders] ([Id], [CustName], [CustPhone], [CustEmail], [CustAddress], [PaymentMethod], [Notes], [TotalAmount], [Status], [OrderDate], [CreatedAt], [UserId]) VALUES
    (1, N'Trần Thị B', '0912345678', 'tranthib@gmail.com', N'123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM', N'COD - Thanh toán khi nhận hàng', N'Giao giờ hành chính giúp em', 1850000.00, 'pending', '2026-11-13T15:00:00', '2026-11-13T15:00:00', 3);
    SET IDENTITY_INSERT [dbo].[Orders] OFF;
END
GO

IF NOT EXISTS (SELECT * FROM [dbo].[OrderItems])
BEGIN
    SET IDENTITY_INSERT [dbo].[OrderItems] ON;
    INSERT INTO [dbo].[OrderItems] ([Id], [OrderId], [ProductId], [ProductName], [Price], [Quantity], [Subtotal], [Image]) VALUES
    (1, 1, 1, N'Máy ảnh Film Polaroid Impulse AF Auto Focus (1992)', 1850000.00, 1, 1850000.00, 'https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600');
    SET IDENTITY_INSERT [dbo].[OrderItems] OFF;
END
GO

-- 8. SEED BẢNG FAVORITES (Danh sách yêu thích mẫu)
IF NOT EXISTS (SELECT * FROM [dbo].[Favorites])
BEGIN
    SET IDENTITY_INSERT [dbo].[Favorites] ON;
    INSERT INTO [dbo].[Favorites] ([Id], [UserId], [ProductId], [CreatedAt]) VALUES
    (1, 3, 2, '2026-11-13T15:30:00'),
    (2, 2, 5, '2026-11-13T16:00:00');
    SET IDENTITY_INSERT [dbo].[Favorites] OFF;
END
GO

-- RESEED IDENTITY COUNTERS ĐẢM BẢO DỮ LIỆU TỰ ĐỘNG TĂNG BẮT ĐẦU CHÍNH XÁC
DBCC CHECKIDENT ('Categories', RESEED, 5);
DBCC CHECKIDENT ('Users', RESEED, 3);
DBCC CHECKIDENT ('PendingPosts', RESEED, 8);
DBCC CHECKIDENT ('Products', RESEED, 6);
DBCC CHECKIDENT ('Conversations', RESEED, 1);
DBCC CHECKIDENT ('ChatMessages', RESEED, 2);
DBCC CHECKIDENT ('Orders', RESEED, 1);
DBCC CHECKIDENT ('OrderItems', RESEED, 1);
DBCC CHECKIDENT ('Favorites', RESEED, 2);
GO

PRINT '=======================================================================';
PRINT ' ThriftShopDB Database schema & seed data initialized successfully!';
PRINT ' All DateTime2 columns strictly configured with ISO standard seed data.';
PRINT '=======================================================================';
GO
