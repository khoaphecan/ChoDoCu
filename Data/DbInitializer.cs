using Microsoft.EntityFrameworkCore;
using ThriftShop.Models;

namespace ThriftShop.Data
{
    public static class DbInitializer
    {
        public static void Initialize(ThriftDbContext context)
        {
            context.Database.EnsureCreated();

            // Đảm bảo cấu trúc bảng Orders & Conversations nếu CSDL đã tồn tại từ trước
            try
            {
                context.Database.ExecuteSqlRaw(@"
                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'PendingPosts')
                    BEGIN
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'PendingPosts' AND COLUMN_NAME = 'UpdatedAt' AND DATA_TYPE = 'nvarchar')
                        BEGIN
                            ALTER TABLE PendingPosts ALTER COLUMN UpdatedAt DATETIME2 NULL;
                        END
                    END

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Users')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'CreatedAt')
                        BEGIN
                            ALTER TABLE Users ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                        END
                    END

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Products')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Products' AND COLUMN_NAME = 'CreatedAt')
                        BEGIN
                            ALTER TABLE Products ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                        END
                    END

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Favorites')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Favorites' AND COLUMN_NAME = 'CreatedAt')
                        BEGIN
                            ALTER TABLE Favorites ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                        END
                    END

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Orders')
                    BEGIN
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Orders' AND COLUMN_NAME = 'CreatedAt')
                        BEGIN
                            ALTER TABLE Orders ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                        END
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'Conversations')
                    BEGIN
                        CREATE TABLE Conversations (
                            Id INT IDENTITY(1,1) PRIMARY KEY,
                            ProductId INT NULL,
                            BuyerId INT NOT NULL,
                            SellerId INT NOT NULL,
                            BuyerClearedAt DATETIME2 NULL,
                            SellerClearedAt DATETIME2 NULL,
                            CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE(),
                            UpdatedAt DATETIME2 NOT NULL DEFAULT GETDATE()
                        );
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatMessages' AND COLUMN_NAME = 'ConversationId')
                    BEGIN
                        ALTER TABLE ChatMessages ADD ConversationId INT NULL;
                    END

                    IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatMessages' AND COLUMN_NAME = 'CreatedAt')
                    BEGIN
                        ALTER TABLE ChatMessages ADD CreatedAt DATETIME2 NOT NULL DEFAULT GETDATE();
                    END

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatMessages' AND COLUMN_NAME = 'ProductId' AND IS_NULLABLE = 'NO')
                    BEGIN
                        ALTER TABLE ChatMessages ALTER COLUMN ProductId INT NULL;
                    END

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'Users' AND COLUMN_NAME = 'Avatar' AND CHARACTER_MAXIMUM_LENGTH <> -1)
                    BEGIN
                        ALTER TABLE Users ALTER COLUMN Avatar NVARCHAR(MAX) NULL;
                    END

                    IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.TABLES WHERE TABLE_NAME = 'ChatMessages')
                    BEGIN
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatMessages' AND COLUMN_NAME = 'Message' AND CHARACTER_MAXIMUM_LENGTH <> -1)
                        BEGIN
                            ALTER TABLE ChatMessages ALTER COLUMN Message NVARCHAR(MAX) NULL;
                        END
                        IF NOT EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatMessages' AND COLUMN_NAME = 'ImageUrl')
                        BEGIN
                            ALTER TABLE ChatMessages ADD ImageUrl NVARCHAR(MAX) NULL;
                        END
                        ELSE
                        BEGIN
                            ALTER TABLE ChatMessages ALTER COLUMN ImageUrl NVARCHAR(MAX) NULL;
                        END
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatMessages' AND COLUMN_NAME = 'SenderAvatar' AND CHARACTER_MAXIMUM_LENGTH <> -1)
                        BEGIN
                            ALTER TABLE ChatMessages ALTER COLUMN SenderAvatar NVARCHAR(MAX) NULL;
                        END
                        IF EXISTS (SELECT * FROM INFORMATION_SCHEMA.COLUMNS WHERE TABLE_NAME = 'ChatMessages' AND COLUMN_NAME = 'ReceiverAvatar' AND CHARACTER_MAXIMUM_LENGTH <> -1)
                        BEGIN
                            ALTER TABLE ChatMessages ALTER COLUMN ReceiverAvatar NVARCHAR(MAX) NULL;
                        END
                    END

                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Conversations_Users_BuyerId')
                    BEGIN
                        ALTER TABLE Conversations ADD CONSTRAINT FK_Conversations_Users_BuyerId FOREIGN KEY (BuyerId) REFERENCES Users(Id);
                    END

                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_Conversations_Users_SellerId')
                    BEGIN
                        ALTER TABLE Conversations ADD CONSTRAINT FK_Conversations_Users_SellerId FOREIGN KEY (SellerId) REFERENCES Users(Id);
                    END

                    IF NOT EXISTS (SELECT * FROM sys.foreign_keys WHERE name = 'FK_ChatMessages_Conversations_ConversationId')
                    BEGIN
                        ALTER TABLE ChatMessages ADD CONSTRAINT FK_ChatMessages_Conversations_ConversationId FOREIGN KEY (ConversationId) REFERENCES Conversations(Id) ON DELETE CASCADE;
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

            // 2. SEED USERS (Chỉ giữ / tạo duy nhất tài khoản Quản trị viên mặc định nếu CSDL trống)
            if (!context.Users.Any())
            {
                var admin = new User
                {
                    FullName = "Ban Quản Trị ThriftShop",
                    Email = "admin@thriftshop.vn",
                    PasswordHash = "admin",
                    Phone = "19006868",
                    Address = "TP. Hồ Chí Minh",
                    Avatar = "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80",
                    Bio = "Ban Quản Trị & Kiểm Duyệt Sàn Đồ Cũ ThriftShop",
                    Role = "admin"
                };
                context.Users.Add(admin);
                context.SaveChanges();
            }
        }
    }
}
