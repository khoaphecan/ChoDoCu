/**
 * admin.js - Shared scripts for Admin Dashboard & Management Pages
 */

$(document).ready(function() {
  updateAdminPendingBadge();
  checkAdminAuth();
});

// Cập nhật số lượng tin đăng đồ cũ đang chờ duyệt trên Sidebar
function updateAdminPendingBadge() {
  const posts = ThriftDB.getPendingPosts();
  const pendingCount = posts.filter(p => p.status === "pending").length;
  const badge = $("#admin-pending-badge");
  if (badge.length) {
    badge.text(pendingCount);
    if (pendingCount > 0) {
      badge.show();
    } else {
      badge.text("0");
    }
  }

  const topbarBadge = $("#admin-topbar-pending-alert");
  if (topbarBadge.length) {
    topbarBadge.text(pendingCount);
  }
}

// Kiểm tra nghiêm ngặt quyền Admin: CHỈ TÀI KHOẢN ADMIN MỚI ĐƯỢC TRUY CẬP, CÒN LẠI CẤM VÀO
function checkAdminAuth() {
  const user = (typeof ThriftDB !== 'undefined' && ThriftDB.getCurrentUser) ? ThriftDB.getCurrentUser() : JSON.parse(localStorage.getItem('thrift_current_user') || 'null');
  const isHtml = window.location.pathname.endsWith('.html');
  const loginUrl = isHtml ? '../login.html' : '/Account/Login';
  const homeUrl = isHtml ? '../index.html' : '/';
  const currentPath = window.location.pathname + window.location.search;

  // 1. Chưa đăng nhập -> Chặn ngay và chuyển hướng sang trang đăng nhập
  if (!user) {
    alert("Vui lòng đăng nhập tài khoản Ban Quản Trị (admin@thriftshop.vn) để truy cập trang quản trị!");
    window.location.replace(`${loginUrl}?returnUrl=${encodeURIComponent(currentPath)}`);
    return false;
  }

  // 2. Đã đăng nhập nhưng KHÔNG PHẢI TÀI KHOẢN ADMIN (ví dụ user thường) -> CẤM VÀO!
  if (user.role !== "admin" && user.email !== "admin@thriftshop.vn") {
    alert("CẢNH BÁO: Bạn không có quyền truy cập trang quản trị! Chỉ tài khoản Ban Quản Trị (admin@thriftshop.vn) mới được phép vào.");
    window.location.replace(homeUrl);
    return false;
  }

  return true;
}

// Render Sidebar chung cho trang Admin
function renderAdminSidebar(activeMenu) {
  const posts = ThriftDB.getPendingPosts();
  const pendingCount = posts.filter(p => p.status === "pending").length;

  return `
    <div class="admin-sidebar">
      <div class="sidebar-header">
        <a href="index.html" class="sidebar-brand">
          <i class="fas fa-recycle fa-lg"></i>
          <span>ThriftAdmin</span>
        </a>
      </div>
      <ul class="sidebar-menu">
        <li class="menu-header">Chức Năng Chính</li>
        <li class="${activeMenu === 'dashboard' ? 'active' : ''}">
          <a href="index.html">
            <i class="fas fa-tachometer-alt"></i>Bảng Điều Khiển
          </a>
        </li>
        <li class="${activeMenu === 'approve' ? 'active' : ''}">
          <a href="approve-products.html" class="text-warning">
            <i class="fas fa-clipboard-check"></i>Duyệt Tin Đăng
            <span class="badge badge-warning" id="admin-pending-badge">${pendingCount}</span>
          </a>
        </li>
        <li class="${activeMenu === 'products' ? 'active' : ''}">
          <a href="products.html">
            <i class="fas fa-boxes"></i>Quản Lý Tin Đăng
          </a>
        </li>
        <li class="${activeMenu === 'users' ? 'active' : ''}">
          <a href="users.html">
            <i class="fas fa-users-cog"></i>Quản Lý User
          </a>
        </li>

        <li class="menu-header">Hệ Thống</li>
        <li>
          <a href="../index.html" target="_blank" class="text-info">
            <i class="fas fa-external-link-alt"></i>Xem Trang Khách Hàng
          </a>
        </li>
        <li>
          <a href="javascript:void(0)" onclick="handleAdminLogout()">
            <i class="fas fa-sign-out-alt"></i>Đăng Xuất
          </a>
        </li>
      </ul>
    </div>
  `;
}

// Render Topbar chung cho trang Admin
function renderAdminTopbar(pageTitle) {
  const posts = ThriftDB.getPendingPosts();
  const pendingCount = posts.filter(p => p.status === "pending").length;

  return `
    <header class="admin-topbar">
      <div>
        <h5 class="mb-0 font-weight-bold text-dark">${pageTitle}</h5>
      </div>
      <div class="topbar-right">
        <a href="approve-products.html" class="btn btn-sm btn-outline-warning font-weight-bold mr-2">
          <i class="fas fa-bell mr-1"></i>Tin chờ duyệt: <span class="badge badge-danger">${pendingCount}</span>
        </a>
        <a href="../index.html" class="btn-client-preview mr-3">
          <i class="fas fa-store mr-1"></i>Trang Mua Sắm
        </a>
        <div class="admin-user-badge">
          <div class="admin-avatar">AD</div>
          <div>
            <div class="small font-weight-bold">Quản Trị Viên</div>
            <div class="text-muted" style="font-size: 11px;">admin@thriftshop.vn</div>
          </div>
        </div>
      </div>
    </header>
  `;
}

function handleAdminLogout() {
  if (confirm("Bạn muốn đăng xuất khỏi trang Quản trị viên?")) {
    if (typeof ThriftDB !== 'undefined' && ThriftDB.logout) {
      ThriftDB.logout();
    } else {
      localStorage.removeItem("thrift_current_user");
    }
    const isHtml = window.location.pathname.endsWith('.html');
    window.location.href = isHtml ? "../login.html" : "/Account/Login";
  }
}
