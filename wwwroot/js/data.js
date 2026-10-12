/**
 * data.js - Quản lý dữ liệu Mock Data & LocalStorage
 * ThriftShop - Nền tảng Trao đổi & Bán đồ cũ Secondhand
 */

// Dữ liệu mẫu Danh mục
const DEFAULT_CATEGORIES = [
  { id: 1, name: "Đồ điện tử", icon: "fas fa-laptop", description: "Điện thoại, tai nghe, máy tính, máy ảnh cũ thanh lý" },
  { id: 2, name: "Đồ gia dụng", icon: "fas fa-couch", description: "Bàn ghế, quạt, đèn, đồ dùng gia đình thanh lý" },
  { id: 3, name: "Văn phòng phẩm", icon: "fas fa-book", description: "Sách cũ, giáo trình, truyện tranh, bút vở thanh lý" },
  { id: 4, name: "Thời trang", icon: "fas fa-tshirt", description: "Quần áo vintage, áo khoác, giày dép, túi xách secondhand" },
  { id: 5, name: "Khác", icon: "fas fa-shapes", description: "Nhạc cụ, đồ decor, phụ kiện, đồ sưu tầm độc lạ" }
];

// Dữ liệu mẫu Sản phẩm đồ cũ đang mở bán (Đã được Admin duyệt)
const DEFAULT_PRODUCTS = [
  {
    id: 101,
    name: "Áo khoác Jean Levis Vintage 1990s lót lông",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 420000,
    originalPrice: 1650000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 9,
    sellerName: "Chung Tấn Thành",
    sellerEmail: "thanh@thriftshop.vn",
    sellerPhone: "0908765432",
    sellerAddress: "Quận 1, TP. Hồ Chí Minh",
    sellerAvatar: "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80",
      "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80",
      "https://images.unsplash.com/photo-1520639888713-7851133b1ed0?w=600&q=80"
    ],
    description: "Áo jean Levis nguyên bản form boxy chuẩn vintage thập niên 90. Chất vải denim dày dặn, có bạc màu tự nhiên rất phong cách. Lớp lót lông cừu ấm áp không bị rách.",
    conditionDetails: "Khuy áo nguyên bản, hơi sờn nhẹ gấu tay áo (chất vintage), không rách, đã giặt sấy thơm tho sạch sẽ.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-01 10:30"
  },
  {
    id: 102,
    name: "Tai nghe chống ồn Sony WH-1000XM4 màu Bạc",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 3600000,
    originalPrice: 6990000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 11,
    sellerName: "Đặng Thuỳ Dương",
    sellerEmail: "duong.dang@thriftshop.vn",
    sellerPhone: "0918765432",
    sellerAddress: "Quận Phú Nhuận, TP.HCM",
    sellerAvatar: "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80",
    images: [
      "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80",
      "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=600&q=80",
      "https://images.unsplash.com/photo-1484704849700-f032a568e944?w=600&q=80"
    ],
    description: "Tai nghe chống ồn chủ động đỉnh cao Sony WH-1000XM4. Mua chính hãng Sony Store Việt Nam, ít dùng do chuyển sang dùng in-ear. Pin còn nghe liên tục trên 28 tiếng chống ồn.",
    conditionDetails: "Đệm tai và headband còn nguyên vẹn không tróc da. Đầy đủ phụ kiện: hộp cứng bảo vệ, cáp Type-C, jack 3.5mm và adapter máy bay zin.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-02 14:15"
  },
  {
    id: 103,
    name: "Giày Sneaker Nike Air Jordan 1 Retro High OG (Size 42)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 1850000,
    originalPrice: 4200000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1552346154-21d32810aba3?w=600&q=80",
    description: "Giày Jordan 1 phối màu kinh điển, mua order từ Nhật. Đi giữ gìn, đế còn nguyên vân ngôi sao, chỉ nhăn nhẹ ở phần mũi giày toebox theo thời gian.",
    conditionDetails: "Còn box thay thế, lót giày zin, đã vệ sinh và phủ nano chống nước chuyên sâu.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-03 09:00"
  },
  {
    id: 104,
    name: "Máy ảnh Film Canon AE-1 kèm Lens 50mm f/1.8",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 2450000,
    originalPrice: 4500000,
    condition: "88% - Hoạt động hoàn hảo",
    conditionPercent: 88,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1526170375885-4d8ecf77b99f?w=600&q=80",
    description: "Máy cơ huyền thoại Canon AE-1 cho các bạn bắt đầu chơi film. Cơ tốc chuẩn xác, đo sáng nhạy, buồng film sạch sẽ kín sáng không lọt sáng.",
    conditionDetails: "Kính lens trong veo không mốc rễ tre, body trầy xước nhẹ góc dưới do thời gian, tặng kèm pin mới và dây đeo vintage.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-04 16:45"
  },
  {
    id: 105,
    name: "Bộ trọn bộ truyện tranh Doraemon Đại Tuyển Tập (45 Tập)",
    categoryId: 3,
    categoryName: "Văn phòng phẩm",
    price: 680000,
    originalPrice: 1350000,
    condition: "96% - Sưu tầm đẹp",
    conditionPercent: 96,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80",
    description: "Bộ truyện tranh Doraemon trọn bộ 45 tập NXB Kim Đồng bản bìa gập dày. Giữ trong tủ kính chống ẩm, giấy vàng nhẹ tự nhiên, không rách trang.",
    conditionDetails: "Đầy đủ 45 tập từ tập 1 đến 45, không mất bìa, không vẽ bậy, thích hợp cho người sưu tầm kỷ niệm.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-05 11:20"
  },
  {
    id: 106,
    name: "Đàn Guitar Acoustic Yamaha FG800 gỗ thịt Solid Top",
    categoryId: 5,
    categoryName: "Khác",
    price: 2900000,
    originalPrice: 5800000,
    condition: "93% - Rất tốt",
    conditionPercent: 93,
    sellerId: 2,
    sellerName: "Trần Nguyễn Anh Khoa",
    sellerEmail: "user@thriftshop.vn",
    sellerPhone: "0889771935",
    sellerAddress: "Quận 1, TP. Hồ Chí Minh",
    sellerAvatar: "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1510915361894-db8b60106cb1?w=600&q=80",
    description: "Đàn guitar chính hãng Yamaha acoustic dòng FG nổi tiếng với mặt top gỗ vân sam nguyên tấm (Solid Spruce). Âm thanh vang ấm, bass dày, action êm tay không đau ngón.",
    conditionDetails: "Vừa thay bộ dây Elixir xịn 12-53, cần thẳng tắp, tặng kèm bao da 3 lớp chống sốc và capo hợp kim nhôm.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-06 08:30"
  },
  {
    id: 107,
    name: "Đèn bàn làm việc Vintage phong cách Bauhaus chao đồng",
    categoryId: 2,
    categoryName: "Đồ gia dụng",
    price: 350000,
    originalPrice: 850000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 15,
    sellerName: "Lê Mỹ Dung",
    sellerPhone: "0908112233",
    sellerAddress: "Quận 1, TP.HCM",
    stock: 2,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600&q=80",
    description: "Đèn bàn học kiêm decor góc làm việc thiết kế retro phong cách Đức cổ. Chao đèn kim loại màu xanh olive phối thân kim loại đồng giả cổ.",
    conditionDetails: "Dây điện và công tắc bập bênh hoạt động an toàn, tặng kèm bóng đèn LED Edison ánh sáng ấm 4W tiết kiệm điện.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-07 15:10"
  },
  {
    id: 108,
    name: "Áo sơ mi Flannel Caro Retro Unisex Size L",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 180000,
    originalPrice: 520000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 2,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1596755094514-f87e34085b2c?w=600&q=80",
    description: "Áo sơ mi dạ caro flannel hàng si tuyển chọn từ Nhật. Chất vải mềm mịn, giữ ấm nhẹ, phối layer với áo thun trắng cực ngầu.",
    conditionDetails: "Không xù lông, khuy cài đủ không sứt chỉ, form oversize trẻ trung năng động.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-08 13:00"
  },
  {
    id: 109,
    name: "Bàn phím cơ không dây NuPhy Air75 Low-Profile Switch Gateron",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 1950000,
    originalPrice: 3200000,
    condition: "97% - Như mới",
    conditionPercent: 97,
    sellerId: 11,
    sellerName: "Đặng Thuỳ Dương",
    sellerPhone: "0918765432",
    sellerAddress: "Quận Phú Nhuận, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1587829741301-dc798b83add3?w=600&q=80",
    description: "Bàn phím cơ siêu mỏng mượt mà dành cho Mac và Windows. Kết nối 3 chế độ: Bluetooth 5.0, 2.4Ghz và cắm dây. Switch bấm êm ái.",
    conditionDetails: "Hàng mua đập hộp được 2 tháng ít dùng, pin trâu, keycap PBT không bóng, đủ fullbox phụ kiện zin.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-09 10:00"
  },
  {
    id: 110,
    name: "Giày Boots da nam cổ lửng Dr. Martens 1460 (Size 41)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 1750000,
    originalPrice: 4800000,
    condition: "89% - Bền đẹp",
    conditionPercent: 89,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1520639888713-7851133b1ed0?w=600&q=80",
    description: "Đôi boots da bò thật 8 lỗ xỏ kinh điển của thương hiệu Dr. Martens nước Anh. Đế AirWair êm ái chống trơn trượt mài mòn cực tốt.",
    conditionDetails: "Da đã break-in mềm chân, xước nhẹ mũi giày đánh xi đen là bóng loáng, chỉ vàng đặc trưng còn nguyên.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-10 17:30"
  },
  {
    id: 111,
    name: "Sách cũ: Nhà Giả Kim & Nghìn Lẻ Một Đêm (Combo 2 cuốn)",
    categoryId: 3,
    categoryName: "Văn phòng phẩm",
    price: 110000,
    originalPrice: 280000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1495446815901-a7297e633e8d?w=600&q=80",
    description: "Bộ 2 tác phẩm văn học kinh điển thế giới bìa cứng đẹp. Sách đọc xong cất kệ cẩn thận, không quăn góc, không gạch chân chữ.",
    conditionDetails: "Sách sạch sẽ như mới, giấy ngà chống lóa mắt, thích hợp cho độc giả yêu sách văn học.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 09:15"
  },
  {
    id: 112,
    name: "Loa Bluetooth di động Marshall Emberton Black & Brass",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 2150000,
    originalPrice: 3890000,
    condition: "94% - Rất mới",
    conditionPercent: 94,
    sellerId: 14,
    sellerName: "Vũ Đình Trọng",
    sellerPhone: "0966554433",
    sellerAddress: "Quận Gò Vấp, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1545454675-3531b543be5d?w=600&q=80",
    description: "Loa bluetooth nhỏ gọn thiết kế rock cổ điển với núm đồng đa hướng. Âm thanh đa hướng 360 độ True Stereophonic, kháng nước chuẩn IPX7.",
    conditionDetails: "Lưới thép mặt trước không móp méo, pin còn rất tốt nghe liên tục hơn 15 tiếng, kèm cáp sạc Type-C.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 15:40"
  },
  {
    id: 113,
    name: "Áo Khoác Bomber Da Lộn Vintage Nâu Bò (Size L)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 520000,
    originalPrice: 1450000,
    condition: "94% - Rất mới",
    conditionPercent: 94,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1551028719-00167b16eac5?w=600&q=80",
    description: "Áo khoác bomber da lộn lót dù ấm áp, phong cách retro cổ điển thập niên 90. Bo tay và gấu áo dệt kim chắc chắn không dão.",
    conditionDetails: "Chất da mềm mịn, màu nâu bò patina đều màu, khóa kéo kim loại YKK trơn tru không kẹt.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 16:00"
  },
  {
    id: 114,
    name: "Áo Măng Tô Dạ Dáng Dài Cổ Điển Hàn Quốc (Freesize)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 390000,
    originalPrice: 1100000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1539533018447-63fcce667823?w=600&q=80",
    description: "Áo dạ dáng dài màu be thanh lịch, 2 hàng khuy form suông chuẩn style vintage Ulzzang. Vải dạ ép dày dặn đứng form.",
    conditionDetails: "Không sờn chỉ, khuy sừng nguyên bản, đã giặt hấp thơm tho sạch sẽ.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 16:30"
  },
  {
    id: 115,
    name: "Túi Xách Da Thật Đeo Vai Retro Y2K Phong Cách Cổ Điển",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 340000,
    originalPrice: 850000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 15,
    sellerName: "Lê Mỹ Dung",
    sellerPhone: "0908112233",
    sellerAddress: "Quận 1, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1590874103328-eac38a683ce7?w=600&q=80",
    description: "Túi kẹp nách da bò thật form cứng cáp, quai đeo vai bản nhỏ retro. Đựng vừa điện thoại Pro Max, ví tiền và son môi.",
    conditionDetails: "Lớp lót vải satin sạch đẹp, khóa bấm kim loại mạ vàng đồng còn sáng bóng.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 17:00"
  },
  {
    id: 116,
    name: "Quần Jean Levi's 501 Original Ống Đứng 1995 (W31 L32)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 480000,
    originalPrice: 1800000,
    condition: "93% - Rất tốt",
    conditionPercent: 93,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1542272604-787c3835535d?w=600&q=80",
    description: "Dòng quần jean huyền thoại Levi's 501 cúc cài button-fly, vải denim 100% cotton dày 14oz không co giãn, giữ form cực đỉnh.",
    conditionDetails: "Màu wash xanh chàm tự nhiên, gấu quần nguyên bản chưa cắt line, tag da phía sau còn nguyên chữ.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 17:30"
  },
  {
    id: 117,
    name: "Giày Sneaker New Balance 550 White Green (Size 41)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 1350000,
    originalPrice: 2950000,
    condition: "93% - Rất tốt",
    conditionPercent: 93,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1595950653106-6c9ebd614d3a?w=600&q=80",
    description: "Mẫu sneaker bóng rổ vintage hot nhất của New Balance. Phối màu trắng phối xanh lá đậm thời thượng, da tổng hợp phối da lộn.",
    conditionDetails: "Đế giày mòn cực ít, da mũi giày chỉ gập nhẹ theo cử động bước đi, đã giặt hấp và xịt khử khuẩn UV.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 18:00"
  },
  {
    id: 118,
    name: "Giày Converse Chuck Taylor 70s Parchment Vintage (Size 42)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 750000,
    originalPrice: 1700000,
    condition: "91% - Tốt",
    conditionPercent: 91,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1607522370275-f14206abe5d3?w=600&q=80",
    description: "Bản Chuck 70s cổ thấp vải canvas dày dặn màu kem parchment ngà vintage. Đế cao su bóng retro êm ái hơn dòng Classic nhiều.",
    conditionDetails: "Heel logo đen còn nguyên chữ, đế chưa mòn gót, vải canvas sạch sẽ không ố vàng bẩn.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 18:30"
  },
  {
    id: 119,
    name: "Giày Loafer Da Bóng Vintage Penny Loafer (Size 40)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 620000,
    originalPrice: 1550000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 14,
    sellerName: "Vũ Đình Trọng",
    sellerPhone: "0966554433",
    sellerAddress: "Quận Gò Vấp, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1533867617858-e7b97e060509?w=600&q=80",
    description: "Giày tây lười penny loafer da bóng phong cách học viện Academia cổ điển. Dễ phối cùng quần tây, tất trắng và blazer.",
    conditionDetails: "Đế cao su đúc nguyên khối chống trơn, da được đánh xi bóng loáng không nứt nẻ.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 19:00"
  },
  {
    id: 120,
    name: "Giày Sneaker Adidas Samba OG White Black Gum (Size 41.5)",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 1450000,
    originalPrice: 2700000,
    condition: "94% - Rất mới",
    conditionPercent: 94,
    sellerId: 12,
    sellerName: "Nguyễn Tuấn Kiệt",
    sellerPhone: "0977889900",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1584735935682-2f2b69dff9d2?w=600&q=80",
    description: "Đôi giày quốc dân Samba OG mũi da lộn chữ T kinh điển kèm đế cao su gum màu nâu bám sàn. Đôi giày không bao giờ lỗi mốt.",
    conditionDetails: "Đi lướt vài lần chụp ảnh, đế còn nguyên gai cao su, lót giày in logo vàng rõ nét.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 19:30"
  },
  {
    id: 121,
    name: "Máy Chơi Game Nintendo Switch OLED White Fullbox",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 5100000,
    originalPrice: 8200000,
    condition: "98% - Như mới",
    conditionPercent: 98,
    sellerId: 11,
    sellerName: "Đặng Thuỳ Dương",
    sellerPhone: "0918765432",
    sellerAddress: "Quận Phú Nhuận, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1578301978693-85fa9c0320b9?w=600&q=80",
    description: "Máy Switch OLED màn hình tràn viền 7 inch sắc nét rực rỡ, màu trắng ngọc trai sang trọng. Máy zin nguyên bản chưa hack, online mượt mà.",
    conditionDetails: "Màn hình đã dán kính cường lực từ ngày đầu, Joycon không trôi analog, đủ dock TV, sạc zin và dây strap.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 20:00"
  },
  {
    id: 122,
    name: "Máy Đọc Sách Kindle Paperwhite 4 Chống Nước 8GB",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 1450000,
    originalPrice: 2800000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1592434134753-a70baf7979d5?w=600&q=80",
    description: "Máy đọc sách E-ink màn hình 300 ppi sắc nét như trang giấy thật, có đèn nền đọc ban đêm và kháng nước IPX8. Pin dùng cả tháng chỉ với 1 lần sạc.",
    conditionDetails: "Màn hình không điểm chết không trầy xước, tặng kèm cover bảo vệ da tự động đóng tắt màn hình.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 20:30"
  },
  {
    id: 123,
    name: "Máy Tính Bảng iPad Gen 9 10.2 inch 64GB Wifi Space Grey",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 4600000,
    originalPrice: 7990000,
    condition: "97% - Như mới",
    conditionPercent: 97,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1544244015-0df4b3ffc6b0?w=600&q=80",
    description: "iPad Gen 9 chip Apple A13 Bionic mạnh mẽ phục vụ học tập, xem phim, vẽ tranh hoặc đọc tài liệu. Màn hình Retina sắc nét kèm phím Home vật lý bền bỉ.",
    conditionDetails: "Pin zin 96% sạc ít lần, máy không cấn móp góc cạnh, iCloud chính chủ reset thoải mái tại chỗ.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 21:00"
  },
  {
    id: 124,
    name: "Bộ Truyện Tranh One Piece Tập 1 - 30 (Bản gốc NXB Kim Đồng)",
    categoryId: 3,
    categoryName: "Văn phòng phẩm",
    price: 450000,
    originalPrice: 950000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1589829085413-56de8ae18c73?w=600&q=80",
    description: "30 tập đầu tiên của hành trình Vua Hải Tặc One Piece (từ Làng Cối Xay Gió tới Đảo Trên Trời Skypiea). Giấy thơm phẳng phiu.",
    conditionDetails: "Đủ tập từ 1 đến 30, không rách trang, không ướt nước, bảo quản trong bọc nilon cẩn thận.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 21:30"
  },
  {
    id: 125,
    name: "Bộ 3 Cuốn Sách Lịch Sử Văn Minh Thế Giới (Bìa Cứng Ép Kim)",
    categoryId: 3,
    categoryName: "Văn phòng phẩm",
    price: 260000,
    originalPrice: 680000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 15,
    sellerName: "Lê Mỹ Dung",
    sellerPhone: "0908112233",
    sellerAddress: "Quận 1, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1512820790803-83ca734da794?w=600&q=80",
    description: "Bộ tuyển tập lịch sử văn minh nhân loại khổ lớn bìa cứng ép nhũ vàng sang trọng. Trình bày chi tiết với nhiều tranh minh họa lịch sử quý giá.",
    conditionDetails: "Sách như mới đập hộp, gáy sách chắc nịch không lung lay, ruột sách sạch không vết bút bi.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 22:00"
  },
  {
    id: 126,
    name: "Máy Pha Cà Phê Espresso DeLonghi Dedica EC685 Đỏ Kim Loại",
    categoryId: 2,
    categoryName: "Đồ gia dụng",
    price: 2850000,
    originalPrice: 6500000,
    condition: "94% - Rất tốt",
    conditionPercent: 94,
    sellerId: 14,
    sellerName: "Vũ Đình Trọng",
    sellerPhone: "0966554433",
    sellerAddress: "Quận Gò Vấp, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1517668808822-9ebb02f2a0e6?w=600&q=80",
    description: "Máy pha cà phê gia đình nhỏ gọn bề ngang chỉ 15cm áp suất bơm 15 bar tiêu chuẩn Ý. Cho lớp crema dày béo ngậy và vòi đánh sữa latte mượt mà.",
    conditionDetails: "Máy hoạt động hoàn hảo vừa tẩy cặn định kỳ, tặng kèm tay pha portafilter, tamper nén inox và ca đánh sữa 350ml.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-11 22:30"
  },
  {
    id: 127,
    name: "Cây Đèn Đứng Decor Phòng Khách Phong Cách Bắc Âu Scandinavian",
    categoryId: 2,
    categoryName: "Đồ gia dụng",
    price: 420000,
    originalPrice: 950000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 15,
    sellerName: "Lê Mỹ Dung",
    sellerPhone: "0908112233",
    sellerAddress: "Quận 1, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1513506003901-1e6a229e2d15?w=600&q=80",
    description: "Đèn sàn uốn cong decor góc sofa hoặc bàn đọc sách. Thân kim loại sơn tĩnh điện đen mờ phối đế đá cẩm thạch đầm chắc chắn.",
    conditionDetails: "Có công tắc đạp chân tiện lợi, bóng đèn LED 3 chế độ màu (vàng, trung tính, trắng) tiết kiệm điện.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 23:00"
  },
  {
    id: 128,
    name: "Bộ Ấm Chén Gốm Mộc Men Rạn Cổ Điển Kèm Khay Gỗ Trúc",
    categoryId: 2,
    categoryName: "Đồ gia dụng",
    price: 280000,
    originalPrice: 650000,
    condition: "98% - Như mới",
    conditionPercent: 98,
    sellerId: 10,
    sellerName: "Hoàng Minh Trí",
    sellerPhone: "0903124567",
    sellerAddress: "Quận 3, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1578749556568-bc2c40e68b61?w=600&q=80",
    description: "Bộ trà gốm mộc vuốt tay gồm 1 ấm trà, 6 chén và 1 khay gỗ tre tự nhiên thoát nước. Nước men rạn xanh ngọc mang đậm phong vị thiền trà.",
    conditionDetails: "Hàng trưng bày tủ kính chưa qua sử dụng, không mẻ miệng chén, quai ấm bọc đồng chắc chắn.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-11 23:30"
  },
  {
    id: 129,
    name: "Đàn Classic Yamaha C40 Cho Người Mới Bắt Đầu (Dây Nilon)",
    categoryId: 5,
    categoryName: "Khác",
    price: 1650000,
    originalPrice: 3100000,
    condition: "92% - Tốt",
    conditionPercent: 92,
    sellerId: 14,
    sellerName: "Vũ Đình Trọng",
    sellerPhone: "0966554433",
    sellerAddress: "Quận Gò Vấp, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1550291652-6ea9114a47b1?w=600&q=80",
    description: "Cây đàn cổ điển huyền thoại được các trường nhạc khuyên dùng. Dây nilon mềm mại êm tay, âm thanh tròn trịa chuẩn cao độ.",
    conditionDetails: "Cần đàn thẳng tắp, phím đàn không mòn, vừa thay bộ dây D'Addario EJ27N mới tinh, tặng kèm bao da.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-12 00:00"
  },
  {
    id: 130,
    name: "Đàn Kalimba 17 Phím Gỗ Mahogany Kèm Búa Chỉnh Âm",
    categoryId: 5,
    categoryName: "Khác",
    price: 1900000,
    originalPrice: 450000,
    condition: "97% - Như mới",
    conditionPercent: 97,
    sellerId: 13,
    sellerName: "Phạm Thảo Vy",
    sellerPhone: "0934567891",
    sellerAddress: "Quận 10, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1520523839898-50712825e617?w=600&q=80",
    description: "Đàn ngón tay Kalimba 17 nốt kim loại khắc sẵn chữ số nốt nhạc, thân đàn gỗ gụ Mahogany nguyên khối vang trong veo.",
    conditionDetails: "Phím sáng bóng không rỉ sét, âm chuẩn tone C, tặng kèm hộp chống sốc, búa chỉnh âm và sách hướng dẫn tự học.",
    featured: false,
    status: "approved",
    createdAt: "2026-10-12 00:30"
  },
  {
    id: 131,
    name: "Micro Thu Âm Audio-Technica AT2020 USB+ Kèm Giá Đỡ Chống Rung",
    categoryId: 5,
    categoryName: "Khác",
    price: 1850000,
    originalPrice: 3600000,
    condition: "95% - Rất mới",
    conditionPercent: 95,
    sellerId: 11,
    sellerName: "Đặng Thuỳ Dương",
    sellerPhone: "0918765432",
    sellerAddress: "Quận Phú Nhuận, TP.HCM",
    stock: 1,
    soldCount: 0,
    image: "https://images.unsplash.com/photo-1590602847861-f357a9332bbc?w=600&q=80",
    description: "Microphone condenser thu âm chuyên nghiệp cho podcast, livestream, thu giọng hát acoustic. Cắm cổng USB nhận ngay không cần soundcard.",
    conditionDetails: "Màng lọc âm sạch sẽ, cổng jack tai nghe kiểm âm 3.5mm hoạt động hoàn hảo, tặng kèm shockmount và chân đế bàn kim loại.",
    featured: true,
    status: "approved",
    createdAt: "2026-10-12 01:00"
  }
];

// Dữ liệu mẫu TIN ĐĂNG BÁN ĐỒ CŨ DO NGƯỜI DÙNG GỬI LÊN (Chờ Admin duyệt hoặc đã duyệt/từ chối)
const DEFAULT_PENDING_POSTS = [
  {
    id: 201,
    name: "Máy chơi game Sony PlayStation 4 Slim 500GB kèm 2 tay cầm",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 3200000,
    originalPrice: 7500000,
    condition: "93% - Rất tốt",
    conditionPercent: 93,
    sellerId: 2,
    sellerName: "Trần Nhật Hào (Bạn)",
    sellerPhone: "0909123456",
    sellerAddress: "123 Nguyễn Huệ, Quận 1, TP.HCM",
    stock: 1,
    image: "https://images.unsplash.com/photo-1507457379470-08b800bebc67?w=600&q=80",
    description: "Máy PS4 Slim 500GB mua chính hãng Sony VN, đã nâng cấp firmware mới nhất. Ít chơi do đi làm bận, máy chạy êm mát không kêu to.",
    conditionDetails: "Kèm 2 tay cầm DualShock 4 không trôi cần analog, cáp nguồn, cáp HDMI, tặng kèm đĩa game God of War.",
    status: "pending", // 'pending' | 'approved' | 'rejected'
    rejectReason: "",
    createdAt: "2026-10-12 08:30"
  },
  {
    id: 202,
    name: "Bàn phím cơ không dây Keychron K2 V2 nhôm Hot-swap RGB",
    categoryId: 1,
    categoryName: "Đồ điện tử",
    price: 1150000,
    originalPrice: 2150000,
    condition: "97% - Như mới",
    conditionPercent: 97,
    sellerId: 20,
    sellerName: "Lê Quốc Bảo",
    sellerPhone: "0912345678",
    sellerAddress: "Quận Tân Bình, TP.HCM",
    stock: 1,
    image: "https://images.unsplash.com/photo-1595225476474-87563907a212?w=600&q=80",
    description: "Keychron K2 V2 bản viền nhôm cao cấp, switch Gateron Brown gõ nảy đầm tay. Dùng được cả Bluetooth và cắm dây.",
    conditionDetails: "Fullbox đầy đủ phụ kiện keycap cho cả Mac/Windows, cáp Type-C, gắp phím.",
    status: "pending",
    rejectReason: "",
    createdAt: "2026-10-12 09:10"
  },
  {
    id: 203,
    name: "Túi đeo chéo da bò mộc Handmade Secondhand",
    categoryId: 4,
    categoryName: "Thời trang",
    price: 390000,
    originalPrice: 950000,
    condition: "90% - Tốt",
    conditionPercent: 90,
    sellerId: 21,
    sellerName: "Ngô Mai Phương",
    sellerPhone: "0988776655",
    sellerAddress: "Quận 7, TP.HCM",
    stock: 1,
    image: "https://images.unsplash.com/photo-1548036328-c9fa89d128fa?w=600&q=80",
    description: "Túi da bò thật may tay thủ công đựng vừa iPad 11 inch và sổ tay. Màu nâu sáp lên màu patina vintage rất đẹp theo thời gian.",
    conditionDetails: "Khóa đồng sáng bóng, quai đeo êm chắc chắn, không trầy rách sâu.",
    status: "pending",
    rejectReason: "",
    createdAt: "2026-10-11 20:15"
  },
  {
    id: 204,
    name: "Bộ 10 đĩa than Vinyl Nhạc Jazz Cổ Điển US",
    categoryId: 5,
    categoryName: "Khác",
    price: 1200000,
    originalPrice: 2800000,
    condition: "85% - Khá",
    conditionPercent: 85,
    sellerId: 22,
    sellerName: "Phan Văn Hưng",
    sellerPhone: "0933221100",
    sellerAddress: "Quận Bình Thạnh, TP.HCM",
    stock: 1,
    image: "https://images.unsplash.com/photo-1539185441755-769473a23570?w=600&q=80",
    description: "Bộ sưu tập 10 đĩa than nhạc Jazz thập niên 60-70 xách tay từ Mỹ gồm Miles Davis, John Coltrane, Bill Evans.",
    conditionDetails: "Vỏ giấy hơi ố theo thời gian, bề mặt đĩa sạch, nghe mượt mà có tiếng lách tách đặc trưng của đĩa than.",
    status: "pending",
    rejectReason: "",
    createdAt: "2026-10-11 18:40"
  }
];

// Dữ liệu mẫu Danh sách Đơn hàng (Dùng cho cả Khách hàng và Admin)
const DEFAULT_ORDERS = [
  {
    id: "DH100234",
    orderDate: "2026-10-10 14:20",
    customerId: 2,
    customerName: "Trần Nhật Hào",
    customerPhone: "0909123456",
    customerEmail: "user@gmail.com",
    shippingAddress: "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
    paymentMethod: "COD - Thanh toán khi nhận hàng",
    paymentStatus: "Chưa thanh toán",
    orderStatus: "delivered", // 'pending' | 'processing' | 'shipping' | 'delivered' | 'cancelled'
    orderStatusText: "Đã giao hàng thành công",
    items: [
      {
        productId: 101,
        productName: "Áo khoác Jean Levis Vintage 1990s lót lông",
        image: "https://images.unsplash.com/photo-1576995853123-5a10305d93c0?w=600&q=80",
        condition: "92% - Tốt",
        price: 420000,
        quantity: 1,
        subtotal: 420000
      }
    ],
    subtotal: 420000,
    shippingFee: 30000,
    totalAmount: 450000,
    notes: "Giao giờ hành chính, gọi trước 15 phút."
  },
  {
    id: "DH100235",
    orderDate: "2026-10-11 16:45",
    customerId: 2,
    customerName: "Trần Nhật Hào",
    customerPhone: "0909123456",
    customerEmail: "user@gmail.com",
    shippingAddress: "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
    paymentMethod: "Chuyển khoản Ngân hàng / MoMo",
    paymentStatus: "Đã thanh toán",
    orderStatus: "shipping",
    orderStatusText: "Đang giao hàng",
    items: [
      {
        productId: 105,
        productName: "Bộ trọn bộ truyện tranh Doraemon Đại Tuyển Tập (45 Tập)",
        image: "https://images.unsplash.com/photo-1544716278-ca5e3f4abd8c?w=600&q=80",
        condition: "96% - Sưu tầm đẹp",
        price: 680000,
        quantity: 1,
        subtotal: 680000
      }
    ],
    subtotal: 680000,
    shippingFee: 30000,
    totalAmount: 710000,
    notes: "Bọc chống sốc kỹ giúp mình vì là sách sưu tầm."
  },
  {
    id: "DH100236",
    orderDate: "2026-10-12 08:15",
    customerId: 2,
    customerName: "Trần Nhật Hào",
    customerPhone: "0909123456",
    customerEmail: "user@gmail.com",
    shippingAddress: "123 Nguyễn Huệ, Phường Bến Nghé, Quận 1, TP.HCM",
    paymentMethod: "COD - Thanh toán khi nhận hàng",
    paymentStatus: "Chưa thanh toán",
    orderStatus: "pending",
    orderStatusText: "Chờ duyệt đơn",
    items: [
      {
        productId: 107,
        productName: "Đèn bàn làm việc Vintage phong cách Bauhaus chao đồng",
        image: "https://images.unsplash.com/photo-1507473885765-e6ed057f782c?w=600&q=80",
        condition: "90% - Tốt",
        price: 350000,
        quantity: 1,
        subtotal: 350000
      }
    ],
    subtotal: 350000,
    shippingFee: 30000,
    totalAmount: 380000,
    notes: ""
  },
  {
    id: "DH100237",
    orderDate: "2026-10-12 09:30",
    customerId: 3,
    customerName: "Nguyễn Thị Mai Lan",
    customerPhone: "0912987654",
    customerEmail: "mailan@gmail.com",
    shippingAddress: "45 Lê Lợi, Phường Bến Nghé, Quận 1, TP.HCM",
    paymentMethod: "Chuyển khoản Ngân hàng / MoMo",
    paymentStatus: "Đã thanh toán",
    orderStatus: "processing",
    orderStatusText: "Đang đóng gói xử lý",
    items: [
      {
        productId: 102,
        productName: "Tai nghe chống ồn Sony WH-1000XM4 màu Bạc",
        image: "https://images.unsplash.com/photo-1546435770-a3e426bf472b?w=600&q=80",
        condition: "95% - Rất mới",
        price: 3600000,
        quantity: 1,
        subtotal: 3600000
      }
    ],
    subtotal: 3600000,
    shippingFee: 0,
    totalAmount: 3600000,
    notes: "Đơn hàng giá trị cao, yêu cầu đồng kiểm."
  }
];

// Dữ liệu mẫu Cuộc trò chuyện / Liên hệ theo sản phẩm (Mặc định để trống)
const DEFAULT_CHATS = [];

// Dữ liệu mẫu Tài khoản người dùng (2 tài khoản chuẩn và các người bán tiêu biểu)
const DEFAULT_USERS = [
  {
    id: 1,
    name: "Ban Quản Trị (Admin)",
    email: "admin@thriftshop.vn",
    password: "admin",
    phone: "0901234567",
    role: "admin",
    address: "Văn phòng BQT ThriftShop, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80",
    bio: "Ban Quản Trị hệ thống kiểm duyệt và hỗ trợ sàn giao dịch đồ cũ ThriftShop."
  },
  {
    id: 2,
    name: "Trần Nguyễn Anh Khoa",
    email: "user@thriftshop.vn",
    password: "user",
    phone: "0889771935",
    role: "customer",
    address: "Quận 1, TP. Hồ Chí Minh",
    avatar: "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
    bio: "Chuyên thanh lý đồ công nghệ & gia dụng cũ còn tốt, hỗ trợ giao dịch trực tiếp."
  },
  {
    id: 3,
    name: "Chung Tấn Thành",
    email: "thanh@thriftshop.vn",
    password: "user",
    phone: "0908765432",
    role: "customer",
    address: "Quận 1, TP. Hồ Chí Minh",
    avatar: "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
    bio: "Đam mê đồ công nghệ & thiết bị điện tử, máy ảnh cũ. Món đồ nào thanh lý cũng được vệ sinh và test kỹ càng."
  },
  {
    id: 4,
    name: "Đặng Thuỳ Dương",
    email: "duong.dang@thriftshop.vn",
    password: "user",
    phone: "0918765432",
    role: "customer",
    address: "Quận Phú Nhuận, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80",
    bio: "Thanh lý đồ công nghệ chính hãng mua dùng lướt ít lần còn nguyên hộp."
  },
  {
    id: 5,
    name: "Hoàng Minh Trí",
    email: "tri.hoang@thriftshop.vn",
    password: "user",
    phone: "0903124567",
    role: "customer",
    address: "Quận 3, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80",
    bio: "Tín đồ thời trang vintage và phụ kiện cổ điển thập niên 90."
  },
  {
    id: 12,
    name: "Nguyễn Tuấn Kiệt",
    email: "kiet.nguyen@thriftshop.vn",
    password: "user",
    phone: "0977889900",
    role: "customer",
    address: "Quận Bình Thạnh, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&q=80",
    bio: "Sưu tầm giày sneaker và quần áo streetwear secondhand chính hãng."
  },
  {
    id: 13,
    name: "Phạm Thảo Vy",
    email: "vy.pham@thriftshop.vn",
    password: "user",
    phone: "0934567891",
    role: "customer",
    address: "Quận 10, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&q=80",
    bio: "Yêu thích sách và truyện tranh vintage, giữ gìn sách cẩn thận."
  },
  {
    id: 14,
    name: "Vũ Đình Trọng",
    email: "trong.vu@thriftshop.vn",
    password: "user",
    phone: "0966554433",
    role: "customer",
    address: "Quận Gò Vấp, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=150&q=80",
    bio: "Đam mê âm nhạc và nhạc cụ acoustic chính hãng."
  },
  {
    id: 15,
    name: "Lê Mỹ Dung",
    email: "dung.le@thriftshop.vn",
    password: "user",
    phone: "0908112233",
    role: "customer",
    address: "Quận 1, TP.HCM",
    avatar: "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&q=80",
    bio: "Sưu tầm đồ trang trí và phụ kiện decor phong cách hoài cổ."
  }
];

// KHỞI TẠO LOCALSTORAGE
function initThriftData() {
  const savedCats = JSON.parse(localStorage.getItem("thrift_categories") || "[]");
  // Kiểm tra nếu danh mục chưa đúng 5 danh mục chuẩn thì thiết lập lại và đồng bộ sản phẩm
  if (!savedCats.length || savedCats.length !== 5 || savedCats[0].name !== "Đồ điện tử") {
    localStorage.setItem("thrift_categories", JSON.stringify(DEFAULT_CATEGORIES));
    localStorage.setItem("thrift_products", JSON.stringify(DEFAULT_PRODUCTS));
    localStorage.setItem("thrift_pending_posts", JSON.stringify(DEFAULT_PENDING_POSTS));
  }

  // Tự động hợp nhất sản phẩm mặc định và đồng bộ chính xác ảnh đại diện người bán
  const existingProds = JSON.parse(localStorage.getItem("thrift_products") || "[]");
  if (!existingProds.length) {
    localStorage.setItem("thrift_products", JSON.stringify(DEFAULT_PRODUCTS));
  } else {
    let prodsChanged = false;
    const existingIds = new Set(existingProds.map(p => p.id));
    DEFAULT_PRODUCTS.forEach(dp => {
      if (!existingIds.has(dp.id)) {
        existingProds.push(dp);
        prodsChanged = true;
      }
    });
    // Đồng bộ chuẩn hóa avatar và tên người bán cho các sản phẩm mẫu
    existingProds.forEach(p => {
      const def = DEFAULT_PRODUCTS.find(d => d.id === p.id);
      if (def && (!p.sellerAvatar || p.sellerAvatar !== def.sellerAvatar || p.sellerName !== def.sellerName)) {
        p.sellerAvatar = def.sellerAvatar;
        p.sellerName = def.sellerName;
        p.sellerId = def.sellerId;
        p.sellerPhone = def.sellerPhone;
        prodsChanged = true;
      }
    });
    if (prodsChanged) {
      localStorage.setItem("thrift_products", JSON.stringify(existingProds));
    }
  }

  // Đồng bộ tin chờ duyệt nếu chưa có
  const existingPending = JSON.parse(localStorage.getItem("thrift_pending_posts") || "[]");
  if (!existingPending.length) {
    localStorage.setItem("thrift_pending_posts", JSON.stringify(DEFAULT_PENDING_POSTS));
  }

  if (!localStorage.getItem("thrift_orders")) {
    localStorage.setItem("thrift_orders", JSON.stringify(DEFAULT_ORDERS));
  }
  // Đồng bộ danh sách người dùng mẫu (Đảm bảo avatar đúng cho từng seller, tránh bị gán nhầm)
  const existingUsers = JSON.parse(localStorage.getItem("thrift_users") || "[]");
  if (!existingUsers.length) {
    localStorage.setItem("thrift_users", JSON.stringify(DEFAULT_USERS));
  } else {
    const userMap = new Map();
    DEFAULT_USERS.forEach(u => userMap.set(u.id, u));
    existingUsers.forEach(u => {
      const def = userMap.get(u.id);
      if (def) {
        userMap.set(u.id, { ...def, ...u, avatar: def.avatar });
      } else {
        userMap.set(u.id, u);
      }
    });
    localStorage.setItem("thrift_users", JSON.stringify(Array.from(userMap.values())));
  }

  if (!localStorage.getItem("thrift_cart")) {
    localStorage.setItem("thrift_cart", JSON.stringify([]));
  }

  // Khởi tạo danh sách Liên hệ / Chat: Xóa sạch lịch sử chat mặc định
  if (!sessionStorage.getItem("thrift_chat_cleared_v2")) {
    localStorage.setItem("thrift_chats", JSON.stringify([]));
    sessionStorage.setItem("thrift_chat_cleared_v2", "true");
  } else if (!localStorage.getItem("thrift_chats")) {
    localStorage.setItem("thrift_chats", JSON.stringify([]));
  }

  // MẶC ĐỊNH KHI VÀO WEBSITE: CHƯA ĐĂNG NHẬP
  if (!sessionStorage.getItem("thrift_session_active")) {
    localStorage.removeItem("thrift_current_user");
  }
}

// CÁC HÀM TIỆN ÍCH DỮ LIỆU
const ThriftDB = {
  // Định dạng tiền tệ VND
  formatCurrency(amount) {
    if (isNaN(amount)) return "0 ₫";
    return new Intl.NumberFormat("vi-VN", { style: "currency", currency: "VND" }).format(amount);
  },

  // Danh mục
  getCategories() {
    return JSON.parse(localStorage.getItem("thrift_categories")) || DEFAULT_CATEGORIES;
  },
  saveCategories(cats) {
    localStorage.setItem("thrift_categories", JSON.stringify(cats));
  },
  getCategoryById(id) {
    const list = this.getCategories();
    return list.find(c => c.id === parseInt(id));
  },

  // Sản phẩm sàn (Đã duyệt - tự động ẩn món đã bán)
  getProducts() {
    let prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    if (!prods.length) {
      localStorage.setItem("thrift_products", JSON.stringify(DEFAULT_PRODUCTS));
      prods = DEFAULT_PRODUCTS;
    }
    // Lọc bỏ những sản phẩm đã được đánh dấu bán hoặc có trạng thái sold
    return prods.filter(p => !p.isSold && p.status !== 'sold');
  },
  saveProducts(prods) {
    localStorage.setItem("thrift_products", JSON.stringify(prods));
  },
  getProductById(id) {
    const list = this.getProducts();
    return list.find(p => p.id === parseInt(id));
  },

  // Quản lý tin đồ cũ chờ duyệt
  getPendingPosts() {
    return JSON.parse(localStorage.getItem("thrift_pending_posts")) || DEFAULT_PENDING_POSTS;
  },
  savePendingPosts(posts) {
    localStorage.setItem("thrift_pending_posts", JSON.stringify(posts));
  },
  getPendingPostById(id) {
    const list = this.getPendingPosts();
    return list.find(p => p.id === parseInt(id));
  },

  // Duyệt tin đồ cũ (Chuyển sang bảng sản phẩm mở bán)
  approvePost(postId) {
    const posts = this.getPendingPosts();
    const postIdx = posts.findIndex(p => p.id === parseInt(postId));
    if (postIdx === -1) return false;

    const post = posts[postIdx];
    post.status = "approved";
    this.savePendingPosts(posts);

    // Thêm vào danh sách sản phẩm sàn
    const products = this.getProducts();
    const newProduct = {
      id: Date.now(), // ID mới
      name: post.name,
      categoryId: post.categoryId,
      categoryName: post.categoryName,
      price: post.price,
      originalPrice: post.originalPrice,
      condition: post.condition,
      conditionPercent: post.conditionPercent,
      sellerId: post.sellerId,
      sellerName: post.sellerName,
      sellerPhone: post.sellerPhone,
      sellerAddress: post.sellerAddress,
      stock: post.stock || 1,
      soldCount: 0,
      image: post.image || (post.images && post.images[0]) || "",
      images: (post.images && post.images.length) ? post.images : (post.image ? [post.image] : []),
      videos: (post.videos && post.videos.length) ? post.videos : [],
      description: post.description,
      conditionDetails: post.conditionDetails,
      featured: false,
      status: "approved",
      createdAt: new Date().toLocaleString("vi-VN")
    };
    products.unshift(newProduct);
    this.saveProducts(products);
    return true;
  },

  // Lấy avatar chuẩn riêng biệt theo tên hoặc ID người dùng/người bán (ảnh người thật, không dùng ui-avatars)
  getAvatarForName(name) {
    if (!name) return "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&q=80";
    const n = String(name).toLowerCase().trim();
    if (n === "3" || n === "9" || n.includes("thành") || n.includes("thanh")) return "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80"; // Chung Tấn Thành (Nam)
    if (n === "4" || n === "11" || n.includes("dương") || n.includes("duong")) return "https://images.unsplash.com/photo-1494790108377-be9c29b29330?w=150&q=80"; // Đặng Thuỳ Dương (Nữ)
    if (n === "5" || n === "10" || n.includes("trí") || n.includes("tri")) return "https://images.unsplash.com/photo-1507003211169-0a1dd7228f2d?w=150&q=80"; // Hoàng Minh Trí (Nam)
    if (n === "6" || n === "12" || n.includes("kiệt") || n.includes("kiet")) return "https://images.unsplash.com/photo-1500648767791-00dcc994a43e?w=150&q=80"; // Nguyễn Tuấn Kiệt (Nam)
    if (n === "7" || n === "13" || n.includes("vy")) return "https://images.unsplash.com/photo-1544005313-94ddf0286df2?w=150&q=80"; // Phạm Thảo Vy (Nữ)
    if (n === "8" || n === "14" || n.includes("trọng") || n.includes("trong")) return "https://images.unsplash.com/photo-1506794778202-cad84cf45f1d?w=150&q=80"; // Vũ Đình Trọng (Nam)
    if (n === "9" || n === "15" || n.includes("dung")) return "https://images.unsplash.com/photo-1517841905240-472988babdf9?w=150&q=80"; // Lê Mỹ Dung (Nữ)
    if (n === "2" || n.includes("khoa") || n.includes("mẫu")) return "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80"; // Người Dùng Mẫu
    if (n === "1" || n.includes("admin") || n.includes("quản trị")) return "https://images.unsplash.com/photo-1472099645785-5658abf4ff4e?w=150&q=80"; // Ban Quản Trị
    return "https://images.unsplash.com/photo-1534528741775-53994a69daeb?w=150&q=80";
  },

  // Lấy thông tin người dùng / người bán theo ID, Email hoặc Tên
  getUserById(identifier) {
    if (!identifier) return null;
    const users = JSON.parse(localStorage.getItem("thrift_users")) || DEFAULT_USERS;
    const str = String(identifier).toLowerCase().trim();
    let user = users.find(u => String(u.id) === str || (u.email && u.email.toLowerCase() === str) || (u.name && u.name.toLowerCase() === str));
    if (user) {
      if (!user.avatar || user.avatar.includes('ui-avatars.com')) {
        user.avatar = this.getAvatarForName(user.name || user.id);
      }
      return user;
    }

    // Tìm trong DEFAULT_USERS nếu localStorage chưa đồng bộ
    user = DEFAULT_USERS.find(u => String(u.id) === str || (u.email && u.email.toLowerCase() === str) || (u.name && u.name.toLowerCase() === str));
    if (user) return user;

    // Nếu chưa có trong bảng user, tìm trong danh sách sản phẩm để trích xuất hồ sơ người bán
    const prods = (typeof this.getProducts === "function") ? this.getProducts() : (JSON.parse(localStorage.getItem("thrift_products")) || []);
    const p = prods.find(item => String(item.sellerId) === str || (item.sellerName && item.sellerName.toLowerCase() === str) || (item.sellerEmail && item.sellerEmail.toLowerCase() === str));
    if (p) {
      return {
        id: p.sellerId || identifier,
        name: p.sellerName || "Thành viên ThriftShop",
        email: p.sellerEmail || "user@thriftshop.vn",
        phone: p.sellerPhone || "0903124567",
        address: p.sellerAddress || "TP. Hồ Chí Minh",
        avatar: (!p.sellerAvatar || p.sellerAvatar.includes('ui-avatars.com')) ? this.getAvatarForName(p.sellerName || p.sellerId) : p.sellerAvatar,
        bio: "Thành viên mua bán & trao đổi đồ cũ uy tín trên sàn ThriftShop.",
        role: "customer"
      };
    }
    return {
      id: identifier,
      name: isNaN(identifier) ? identifier : ("Người bán #" + identifier),
      avatar: this.getAvatarForName(identifier),
      role: "customer"
    };
  },

  // Lấy các bài đăng ĐANG MỞ BÁN của một người dùng / người bán
  // TUÂN THỦ QUY TẮC BẮT BUỘC: Những thứ đã bán (isSold / status: 'sold') hay đã xoá (deleted) TUYỆT ĐỐI KHÔNG HIỆN!
  getSellerActiveProducts(identifier) {
    if (!identifier) return [];
    const prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    const str = String(identifier).toLowerCase().trim();

    return prods.filter(p => {
      // Khớp theo ID người bán, email người bán hoặc tên người bán
      const isMatch = (String(p.sellerId) === str) ||
                      (p.sellerEmail && p.sellerEmail.toLowerCase() === str) ||
                      (p.sellerName && p.sellerName.toLowerCase() === str) ||
                      (p.userId && String(p.userId) === str);

      // BẮT BUỘC: Không hiện đồ đã bán hay đã xoá
      const isSold = p.isSold === true || p.status === 'sold';
      const isDeleted = p.deleted === true;
      const isApproved = p.status === 'approved' || !p.status;

      return isMatch && !isSold && !isDeleted && isApproved;
    });
  },

  // Từ chối tin đồ cũ kèm lý do
  rejectPost(postId, reason) {
    const posts = this.getPendingPosts();
    const post = posts.find(p => p.id === parseInt(postId));
    if (!post) return false;
    post.status = "rejected";
    post.rejectReason = reason || "Thông tin món đồ chưa rõ ràng hoặc hình ảnh mờ.";
    this.savePendingPosts(posts);
    return true;
  },

  // Giỏ hàng
  getCart() {
    return JSON.parse(localStorage.getItem("thrift_cart")) || [];
  },
  saveCart(cart) {
    localStorage.setItem("thrift_cart", JSON.stringify(cart));
    // Phát event để cập nhật badge
    window.dispatchEvent(new Event("cartUpdated"));
  },
  getCartCount() {
    const cart = this.getCart();
    return cart.reduce((sum, item) => sum + item.quantity, 0);
  },
  addToCart(productId, quantity = 1) {
    const product = this.getProductById(productId);
    if (!product || product.stock <= 0) {
      return { success: false, message: "Sản phẩm đã hết hàng hoặc không tồn tại!" };
    }

    const cart = this.getCart();
    const existing = cart.find(i => i.productId === parseInt(productId));

    if (existing) {
      if (existing.quantity + quantity > product.stock) {
        return { success: false, message: `Số lượng món đồ thanh lý trong kho chỉ còn ${product.stock} món!` };
      }
      existing.quantity += quantity;
    } else {
      if (quantity > product.stock) {
        return { success: false, message: `Số lượng món đồ thanh lý trong kho chỉ còn ${product.stock} món!` };
      }
      cart.push({ productId: parseInt(productId), quantity: quantity });
    }

    this.saveCart(cart);
    return { success: true, message: "Đã thêm món đồ vào giỏ hàng thành công!" };
  },
  updateCartQuantity(productId, newQty) {
    const product = this.getProductById(productId);
    if (!product) return false;

    let cart = this.getCart();
    const item = cart.find(i => i.productId === parseInt(productId));
    if (!item) return false;

    if (newQty <= 0) {
      this.removeFromCart(productId);
      return true;
    }

    if (newQty > product.stock) {
      alert(`Món đồ cũ này chỉ có duy nhất ${product.stock} sản phẩm trong kho!`);
      item.quantity = product.stock;
    } else {
      item.quantity = newQty;
    }

    this.saveCart(cart);
    return true;
  },
  removeFromCart(productId) {
    let cart = this.getCart();
    cart = cart.filter(i => i.productId !== parseInt(productId));
    this.saveCart(cart);
    return true;
  },
  clearCart() {
    this.saveCart([]);
  },

  // Đơn hàng
  getOrders() {
    return JSON.parse(localStorage.getItem("thrift_orders")) || DEFAULT_ORDERS;
  },
  saveOrders(orders) {
    localStorage.setItem("thrift_orders", JSON.stringify(orders));
  },
  getOrderById(id) {
    const list = this.getOrders();
    return list.find(o => o.id === id);
  },
  createOrder(orderData) {
    const orders = this.getOrders();
    const newOrder = {
      id: "DH" + Math.floor(100000 + Math.random() * 900000),
      orderDate: new Date().toLocaleString("vi-VN"),
      orderStatus: "pending",
      orderStatusText: "Chờ duyệt đơn",
      ...orderData
    };
    orders.unshift(newOrder);
    this.saveOrders(orders);

    // Cập nhật giảm tồn kho các sản phẩm đã mua
    const products = this.getProducts();
    orderData.items.forEach(item => {
      const prod = products.find(p => p.id === item.productId);
      if (prod) {
        prod.stock = Math.max(0, prod.stock - item.quantity);
        prod.soldCount = (prod.soldCount || 0) + item.quantity;
      }
    });
    this.saveProducts(products);

    // Xoá giỏ hàng
    this.clearCart();
    return newOrder;
  },

  // Người dùng & Auth
  getUsers() {
    return JSON.parse(localStorage.getItem("thrift_users")) || DEFAULT_USERS;
  },
  saveUsers(users) {
    localStorage.setItem("thrift_users", JSON.stringify(users));
  },
  getCurrentUser() {
    try {
      const u = localStorage.getItem("thrift_current_user");
      return (u && u !== "null" && u !== "undefined") ? JSON.parse(u) : null;
    } catch (e) {
      return null;
    }
  },
  isLoggedIn() {
    return !!this.getCurrentUser();
  },
  setCurrentUser(user) {
    if (user) {
      localStorage.setItem("thrift_current_user", JSON.stringify(user));
    } else {
      localStorage.removeItem("thrift_current_user");
    }
    window.dispatchEvent(new Event("userChanged"));
  },
  login(email, password) {
    const users = this.getUsers();
    const user = users.find(u => u.email.toLowerCase() === email.toLowerCase() && u.password === password);
    if (user) {
      sessionStorage.setItem("thrift_session_active", "true");
      this.setCurrentUser(user);
      return { success: true, user: user };
    }
    return { success: false, message: "Sai email hoặc mật khẩu!" };
  },
  logout() {
    sessionStorage.removeItem("thrift_session_active");
    this.setCurrentUser(null);
  },
  register(userData) {
    const users = this.getUsers();
    if (users.some(u => u.email.toLowerCase() === userData.email.toLowerCase())) {
      return { success: false, message: "Email này đã được đăng ký tài khoản!" };
    }
    const newUser = {
      id: Date.now(),
      role: "customer",
      ...userData
    };
    users.push(newUser);
    this.saveUsers(users);
    return { success: true, user: newUser };
  },

  // === QUẢN LÝ TIN ĐÃ YÊU THÍCH (THEO SẢN PHẨM) ===
  getFavoritesKey() {
    const user = this.getCurrentUser();
    return user ? `thrift_favs_${user.id}` : 'thrift_favs_guest';
  },
  getFavorites() {
    return JSON.parse(localStorage.getItem(this.getFavoritesKey()) || "[]");
  },
  saveFavorites(favList) {
    localStorage.setItem(this.getFavoritesKey(), JSON.stringify(favList));
    window.dispatchEvent(new Event("favoritesChanged"));
  },
  isFavorite(productId) {
    const pId = parseInt(productId);
    return this.getFavorites().includes(pId);
  },
  toggleFavorite(productId) {
    const pId = parseInt(productId);
    let favs = this.getFavorites();
    const idx = favs.indexOf(pId);
    let isFav = false;
    if (idx >= 0) {
      favs.splice(idx, 1);
      isFav = false;
    } else {
      favs.push(pId);
      isFav = true;
    }
    this.saveFavorites(favs);
    return { success: true, isFav: isFav, count: favs.length };
  },
  getFavoriteProducts() {
    const favIds = this.getFavorites();
    const allProds = JSON.parse(localStorage.getItem("thrift_products")) || DEFAULT_PRODUCTS;
    return allProds.filter(p => favIds.includes(p.id));
  },

  // === THIẾT LẬP HỒ SƠ & ĐỔI MẬT KHẨU TÀI KHOẢN (CHUẨN ẢNH) ===
  updateCurrentUser(updatedData) {
    let current = this.getCurrentUser();
    if (!current) return { success: false, message: "Chưa đăng nhập!" };
    
    current = { ...current, ...updatedData };
    this.setCurrentUser(current);

    // Đồng bộ vào danh sách users
    let users = this.getUsers();
    const idx = users.findIndex(u => u.id === current.id || u.email.toLowerCase() === current.email.toLowerCase());
    if (idx >= 0) {
      users[idx] = { ...users[idx], ...updatedData };
      this.saveUsers(users);
    }
    return { success: true, user: current };
  },
  changePassword(oldPassword, newPassword) {
    let current = this.getCurrentUser();
    if (!current) return { success: false, message: "Chưa đăng nhập!" };

    if (current.password !== oldPassword) {
      return { success: false, message: "Mật khẩu hiện tại không chính xác!" };
    }

    current.password = newPassword;
    this.setCurrentUser(current);

    let users = this.getUsers();
    const idx = users.findIndex(u => u.id === current.id || u.email.toLowerCase() === current.email.toLowerCase());
    if (idx >= 0) {
      users[idx].password = newPassword;
      this.saveUsers(users);
    }
    return { success: true, message: "Đổi mật khẩu thành công!" };
  },

  // === QUẢN LÝ TIN ĐĂNG BÁN CỦA TÔI (CHỈNH SỬA DUYỆT LẠI, ĐÃ BÁN, XÓA HẲN) ===
  // 1. Đánh dấu ĐÃ BÁN (Ẩn khỏi sàn nhưng vẫn giữ trong mục Tin đăng của tôi)
  markPostSold(postId) {
    const id = parseInt(postId);
    const pending = this.getPendingPosts();
    const pPost = pending.find(p => p.id === id);
    if (pPost) {
      pPost.isSold = true;
      pPost.status = 'sold';
      this.savePendingPosts(pending);
    }

    // Ẩn món đồ này khỏi danh mục sản phẩm công khai
    let prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    prods = prods.filter(p => p.id !== id);
    this.saveProducts(prods);
    return true;
  },

  // 2. Xóa bài đăng vĩnh viễn (Xóa hẳn khỏi hệ thống)
  deletePostPermanently(postId) {
    const id = parseInt(postId);
    let pending = this.getPendingPosts();
    pending = pending.filter(p => p.id !== id);
    this.savePendingPosts(pending);

    let prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    prods = prods.filter(p => p.id !== id);
    this.saveProducts(prods);
    return true;
  },

  // 3. Chỉnh sửa tin đăng (YÊU CẦU BẮT BUỘC: Admin duyệt lại lần nữa nếu đã duyệt)
  updatePost(updatedData) {
    const id = parseInt(updatedData.id);
    let pending = this.getPendingPosts();
    const idx = pending.findIndex(p => p.id === id);
    const oldPost = (idx >= 0) ? pending[idx] : {};

    const updatedPost = {
      ...oldPost,
      ...updatedData,
      id: id,
      status: "pending", // BẮT BUỘC CHUYỂN VỀ CHỜ ADMIN DUYỆT LẠI
      rejectReason: null,
      updatedAt: new Date().toLocaleString("vi-VN")
    };

    if (idx >= 0) {
      pending[idx] = updatedPost;
    } else {
      pending.unshift(updatedPost);
    }
    this.savePendingPosts(pending);

    // Gỡ khỏi danh sách sản phẩm mở bán công khai cho đến khi Admin duyệt lại
    let prods = JSON.parse(localStorage.getItem("thrift_products")) || [];
    prods = prods.filter(p => p.id !== id);
    this.saveProducts(prods);

    return updatedPost;
  },

  // === QUẢN LÝ LIÊN HỆ & CHAT THEO SẢN PHẨM (CHUẨN ẢNH 3) ===
  getChats() {
    const raw = localStorage.getItem("thrift_chats");
    let chats = raw ? JSON.parse(raw) : DEFAULT_CHATS;
    let modified = false;

    // Tự động kiểm tra và đồng bộ lại ảnh đại diện chuẩn của người bán trong các cuộc trò chuyện (tránh cache nhầm avatar ui-avatars)
    chats.forEach(c => {
      let isAvatarBad = !c.sellerAvatar || 
                        c.sellerAvatar.includes('ui-avatars.com') || 
                        c.sellerAvatar.length < 10;
      if (isAvatarBad) {
        let correctAvatar = null;
        if (c.sellerName) {
          const u = this.getUserById(c.sellerName);
          if (u && u.avatar && !u.avatar.includes('ui-avatars.com')) correctAvatar = u.avatar;
          if (!correctAvatar) correctAvatar = this.getAvatarForName(c.sellerName);
        }
        if (!correctAvatar && c.sellerId) {
          const u = this.getUserById(c.sellerId);
          if (u && u.avatar && !u.avatar.includes('ui-avatars.com')) correctAvatar = u.avatar;
        }
        if (!correctAvatar && c.product) {
          const p = (typeof this.getProductById === 'function') ? this.getProductById(c.product.id) : null;
          if (p && p.sellerAvatar && !p.sellerAvatar.includes('ui-avatars.com')) correctAvatar = p.sellerAvatar;
        }
        if (!correctAvatar) {
          correctAvatar = this.getAvatarForName(c.sellerName || c.sellerId);
        }
        c.sellerAvatar = correctAvatar;
        modified = true;
      }
    });

    if (modified) {
      localStorage.setItem("thrift_chats", JSON.stringify(chats));
    }
    return chats;
  },
  deleteChat(chatId) {
    let chats = this.getChats();
    const target = chats.find(c => c.id === chatId);
    chats = chats.filter(c => c.id !== chatId);
    this.saveChats(chats);

    if (target && target.product && target.product.id) {
      try {
        fetch('/Cart/DeleteChat', {
          method: 'POST',
          headers: { 'Content-Type': 'application/x-www-form-urlencoded' },
          body: `productId=${encodeURIComponent(target.product.id)}`
        }).catch(() => {});
      } catch (e) {}
    }
    return true;
  },
  saveChats(chats) {
    localStorage.setItem("thrift_chats", JSON.stringify(chats));
    window.dispatchEvent(new Event("chatsUpdated"));
  },
  getChatById(chatId) {
    const chats = this.getChats();
    return chats.find(c => c.id === chatId);
  },
  getChatByProductId(productId) {
    const chats = this.getChats();
    return chats.find(c => c.product && c.product.id === parseInt(productId));
  },
  getUnreadChatCount() {
    const chats = this.getChats();
    return chats.filter(c => c.unread).length;
  },
  sendChatMessage(chatId, text, sender = "buyer") {
    const chats = this.getChats();
    const chat = chats.find(c => c.id === chatId);
    if (!chat) return null;
    const now = new Date();
    const timeStr = `${String(now.getHours()).padStart(2, '0')}:${String(now.getMinutes()).padStart(2, '0')}`;
    const newMsg = {
      sender: sender,
      text: text,
      time: timeStr
    };
    chat.messages.push(newMsg);
    chat.lastMessage = text;
    chat.lastTime = "Vừa xong";
    this.saveChats(chats);
    return newMsg;
  },
  startOrGetChatForProduct(product) {
    if (!product) return null;
    const chats = this.getChats();
    const prodIdStr = String(product.id || '');
    let chat = chats.find(c => c.product && String(c.product.id) === prodIdStr);
    const currentUser = (typeof this.getCurrentUser === 'function') ? this.getCurrentUser() : null;

    // Xác định đúng ảnh đại diện chuẩn của người bán (người muốn chat)
    let sellerAvatar = product.sellerAvatar;
    if (!sellerAvatar || sellerAvatar.includes('ui-avatars.com') || sellerAvatar.length < 10) {
      if (product.sellerName) {
        const u = this.getUserById(product.sellerName);
        if (u && u.avatar && !u.avatar.includes('ui-avatars.com')) sellerAvatar = u.avatar;
        if (!sellerAvatar) sellerAvatar = this.getAvatarForName(product.sellerName);
      }
      if (!sellerAvatar && product.sellerId) {
        const u = this.getUserById(product.sellerId);
        if (u && u.avatar && !u.avatar.includes('ui-avatars.com')) sellerAvatar = u.avatar;
      }
      if (!sellerAvatar) {
        sellerAvatar = this.getAvatarForName(product.sellerId || product.sellerName);
      }
    }

    if (!chat) {
      chat = {
        id: "chat-" + Date.now(),
        sellerId: product.sellerId || 99,
        sellerName: product.sellerName || "Người bán",
        sellerAvatar: sellerAvatar || "https://images.unsplash.com/photo-1539571696357-5a69c17a67c6?w=150&q=80",
        sellerPhone: product.sellerPhone || "0901234567",
        sellerEmail: product.sellerEmail || "",
        buyerId: currentUser ? currentUser.id : 2,
        buyerName: currentUser ? currentUser.name : "Bạn",
        buyerAvatar: currentUser ? (currentUser.avatar || "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80") : "https://images.unsplash.com/photo-1535713875002-d1d0cf377fde?w=150&q=80",
        isOnline: true,
        tab: "all",
        unread: false,
        product: {
          id: product.id,
          name: product.name,
          price: product.price,
          image: product.image,
          statusText: "Tin đang mở bán · Còn hàng"
        },
        lastMessage: "Tôi quan tâm đến món đồ này!",
        lastTime: "Vừa xong",
        messages: [
          { sender: "buyer", text: "Xin chào, món đồ này còn không bạn?", time: new Date().toLocaleTimeString('vi-VN', { hour: '2-digit', minute: '2-digit' }) }
        ]
      };
      chats.unshift(chat);
      this.saveChats(chats);
    } else {
      // Cập nhật lại thông tin người bán và vật muốn mua chuẩn xác
      if (product.sellerName) chat.sellerName = product.sellerName;
      if (sellerAvatar && !sellerAvatar.includes('ui-avatars.com')) chat.sellerAvatar = sellerAvatar;
      if (product.sellerPhone) chat.sellerPhone = product.sellerPhone;
      if (!chat.product) {
        chat.product = {
          id: product.id,
          name: product.name,
          price: product.price,
          image: product.image,
          statusText: "Tin đang mở bán · Còn hàng"
        };
      } else {
        if (product.name) chat.product.name = product.name;
        if (product.price) chat.product.price = product.price;
        if (product.image) chat.product.image = product.image;
      }
      this.saveChats(chats);
    }
    return chat;
  }
};

// Tự động khởi tạo dữ liệu
initThriftData();

