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

-- 2. SEED TÀI KHOẢN NGƯỜI DÙNG (Admin)
IF NOT EXISTS (SELECT * FROM [dbo].[Users] WHERE [Email] = 'admin@thriftshop.vn')
BEGIN
    SET IDENTITY_INSERT [dbo].[Users] ON;
    INSERT INTO [dbo].[Users] ([Id], [FullName], [Email], [PasswordHash], [Phone], [Address], [Avatar], [Bio], [Role], [CreatedAt]) VALUES
    (1, N'Ban Quản Trị ThriftShop', 'admin@thriftshop.vn', 'admin', '19006868', N'TP. Hồ Chí Minh', '/images/avatars/avatar_1_639301642064879045.webp', N'Ban Quản Trị & Kiểm Duyệt Sàn Đồ Cũ ThriftShop', 'admin', '2026-11-13T08:00:00');
    SET IDENTITY_INSERT [dbo].[Users] OFF;
END
GO

-- 3. BẢNG PENDINGPOSTS (Khởi tạo rỗng, người dùng đăng tin bán đồ cũ)
-- 4. BẢNG PRODUCTS (Khởi tạo rỗng, hiển thị sau khi Quản trị viên duyệt tin)
-- 5. BẢNG CONVERSATIONS (Khởi tạo rỗng, tạo khi Người mua nhắn tin cho Người bán)
-- 6. BẢNG CHATMESSAGES (Khởi tạo rỗng)
-- 7. BẢNG ORDERS & ORDERITEMS (Khởi tạo rỗng)
-- 8. BẢNG FAVORITES (Khởi tạo rỗng)

-- RESEED IDENTITY COUNTERS ĐẢM BẢO DỮ LIỆU TỰ ĐỘNG TĂNG BẮT ĐẦU CHÍNH XÁC
DBCC CHECKIDENT ('Categories', RESEED, 5);
DBCC CHECKIDENT ('Users', RESEED, 1);
DBCC CHECKIDENT ('PendingPosts', RESEED, 0);
DBCC CHECKIDENT ('Products', RESEED, 0);
DBCC CHECKIDENT ('Conversations', RESEED, 0);
DBCC CHECKIDENT ('ChatMessages', RESEED, 0);
DBCC CHECKIDENT ('Orders', RESEED, 0);
DBCC CHECKIDENT ('OrderItems', RESEED, 0);
DBCC CHECKIDENT ('Favorites', RESEED, 0);
GO

PRINT '=======================================================================';
PRINT ' ThriftShopDB Database schema & seed data initialized successfully!';
PRINT ' All DateTime2 columns strictly configured with ISO standard seed data.';
PRINT '=======================================================================';
GO
