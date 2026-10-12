/**
 * main.js - Common UI & Event Handlers for Client Pages
 * Chủ đề 30: Website trao đổi / Bán đồ cũ (Thrift Shop / Secondhand)
 */

// Hàm mở / đóng dropdown menu người dùng khi click (CHỈ MỞ KHI CLICK, KHÔNG MỞ KHI HOVER)
window.toggleUserDropdown = function(event) {
  if (event) {
    if (typeof event.preventDefault === "function") event.preventDefault();
    if (typeof event.stopPropagation === "function") event.stopPropagation();
  }
  const container = document.getElementById("userDropdownContainer") || document.querySelector(".user-dropdown");
  if (!container) return;
  const menu = document.getElementById("userDropdownMenu") || container.querySelector(".dropdown-menu");
  
  const isOpen = container.classList.contains("show") || container.classList.contains("open") || (menu && menu.classList.contains("show"));
  
  // Đóng tất cả dropdown khác nếu có
  document.querySelectorAll(".user-dropdown, .dropdown").forEach(el => el.classList.remove("show", "open"));
  document.querySelectorAll(".dropdown-menu").forEach(el => {
    el.classList.remove("show", "open");
    el.style.display = "";
  });

  if (!isOpen) {
    container.classList.add("show", "open");
    if (menu) {
      menu.classList.add("show", "open");
      menu.style.display = "block";
    }
  }
};

$(document).ready(function() {
  updateCartBadge();
  renderUserStatus();
  initHeaderSearch();

  // Bắt sự kiện click mở / đóng dropdown menu người dùng
  $(document).on("click", "#userDropdownBtn, .user-auth-btn, .user-dropdown > a", function(e) {
    window.toggleUserDropdown(e);
  });

  // Đóng khi click ra bên ngoài
  $(document).on("click", function(e) {
    if (!e.target.closest("#userDropdownContainer") && !e.target.closest(".user-dropdown")) {
      document.querySelectorAll(".user-dropdown, .dropdown").forEach(el => el.classList.remove("show", "open"));
      document.querySelectorAll(".dropdown-menu").forEach(el => {
        el.classList.remove("show", "open");
        el.style.display = "";
      });
    }
  });

  // Khi click vào các mục bên trong dropdown-menu
  $(document).on("click", "#userDropdownMenu, .dropdown-menu", function(e) {
    if (e.target.closest(".dropdown-item")) {
      document.querySelectorAll(".user-dropdown, .dropdown").forEach(el => el.classList.remove("show", "open"));
      document.querySelectorAll(".dropdown-menu").forEach(el => {
        el.classList.remove("show", "open");
        el.style.display = "";
      });
    } else {
      e.stopPropagation();
    }
  });

  // Lắng nghe sự kiện cập nhật giỏ hàng hoặc người dùng hoặc chat
  window.addEventListener("cartUpdated", updateCartBadge);
  window.addEventListener("userChanged", renderUserStatus);
  window.addEventListener("chatsUpdated", updateCartBadge);
});

// Cập nhật số lượng liên hệ / tin nhắn chưa đọc trên Header
function updateCartBadge() {
  const count = (typeof ThriftDB !== 'undefined' && ThriftDB.getUnreadChatCount) ? ThriftDB.getUnreadChatCount() : 2;
  const badges = $(".cart-count, #cartCount, #unreadChatCount, .badge-cart");
  if (badges.length) {
    badges.text(count > 0 ? count : 0);
  }
}

// Kiểm tra trạng thái đã đăng nhập hay chưa
window.checkUserLoggedIn = function() {
  try {
    if (typeof ThriftDB !== 'undefined' && typeof ThriftDB.isLoggedIn === 'function') {
      return ThriftDB.isLoggedIn();
    }
    const u = localStorage.getItem("thrift_current_user");
    return !!(u && u !== "null" && u !== "undefined");
  } catch (e) {
    return false;
  }
};

// Chuyển sang hộp thoại Chat với người bán của sản phẩm
window.openChatWithSeller = function(productId) {
  const isHtml = window.location.pathname.endsWith('.html');
  const loginUrl = isHtml ? 'login.html' : '/Account/Login';
  const pId = parseInt(productId);
  const chatUrl = isHtml ? `cart.html?productId=${pId}` : `/Cart?productId=${pId}`;

  // KIỂM TRA ĐĂNG NHẬP: Khi chưa đăng nhập thì lập tức chuyển hướng đến trang đăng nhập ngay
  if (!window.checkUserLoggedIn()) {
    window.location.href = `${loginUrl}?returnUrl=${encodeURIComponent(chatUrl)}`;
    return;
  }

  if (typeof ThriftDB !== 'undefined') {
    const prod = ThriftDB.getProductById(pId);
    if (prod && ThriftDB.startOrGetChatForProduct) {
      ThriftDB.startOrGetChatForProduct(prod);
    }
  }
  window.location.href = chatUrl;
};
window.handleChatWithSeller = window.openChatWithSeller;

// Hàm kiểm tra đăng nhập khi bấm vào các tính năng yêu cầu tài khoản
window.handleCheckAuthNav = function(event, targetUrl) {
  if (!window.checkUserLoggedIn()) {
    if (event) {
      if (typeof event.preventDefault === "function") event.preventDefault();
      if (typeof event.stopPropagation === "function") event.stopPropagation();
    }
    const isHtml = window.location.pathname.endsWith('.html');
    const loginUrl = isHtml ? 'login.html' : '/Account/Login';
    window.location.href = `${loginUrl}?returnUrl=${encodeURIComponent(targetUrl)}`;
    return false;
  }
  return true;
};

// Cập nhật trạng thái Đăng nhập / Hồ sơ người dùng trên Header
function renderUserStatus() {
  const user = ThriftDB.getCurrentUser();
  const container = $("#user-header-auth");
  if (!container.length) return;

  const isHtml = window.location.pathname.endsWith('.html');
  const postLink = isHtml ? 'post-item.html' : '/Products/PostItem';
  const myListingsLink = isHtml ? 'my-listings.html' : '/Products/MyListings';
  const favoritesLink = isHtml ? 'favorites.html' : '/Products/Favorites';
  const profileLink = isHtml ? 'profile.html' : '/Account/Profile';
  const sellerProfileLink = isHtml ? `seller-profile.html?userId=${encodeURIComponent(user ? (user.id || user.email) : '')}` : `/Account/SellerProfile?userId=${encodeURIComponent(user ? (user.id || user.email) : '')}`;
  const adminUrl = isHtml ? 'admin/index.html' : '/Admin';
  const loginUrl = isHtml ? 'login.html' : '/Account/Login';
  const registerUrl = isHtml ? 'register.html' : '/Account/Register';

  if (user) {
    let adminLink = '';
    // CHỈ HIỂN THỊ LINK QUẢN TRỊ ADMIN CHO TÀI KHOẢN ADMIN
    if (user.role === 'admin' || user.email === 'admin@thriftshop.vn') {
      adminLink = `
        <a class="dropdown-item text-warning font-weight-bold" href="${adminUrl}">
          <i class="fas fa-user-shield mr-2"></i>Trang Quản Trị (Admin)
        </a>
        <div class="dropdown-divider"></div>
      `;
    }

    container.html(`
      <div class="dropdown user-dropdown" id="userDropdownContainer">
        <a class="user-auth-btn" id="userDropdownBtn" href="javascript:void(0)" onclick="toggleUserDropdown(event)" role="button" aria-haspopup="true" aria-expanded="false" title="${user.name}">
          ${user.avatar ? `<img src="${user.avatar}" alt="${user.name}" style="width: 22px; height: 22px; border-radius: 50%; object-fit: cover; border: 1.5px solid #22c55e;">` : `<i class="fas fa-user-circle fa-lg text-success" style="font-size: 16px;"></i>`}
          <span class="user-display-name">${user.name}</span>
          <i class="fas fa-chevron-down" style="font-size: 10px; opacity: 0.7; margin-left: 2px;"></i>
        </a>
        <div class="dropdown-menu dropdown-menu-right mt-2" id="userDropdownMenu">
          <a class="dropdown-header user-profile-header-link" href="${sellerProfileLink}" style="display: block; text-decoration: none; padding: 10px 14px; border-radius: 8px; background: rgba(255, 255, 255, 0.05); margin-bottom: 6px; transition: all 0.2s; cursor: pointer;" title="Xem trang cá nhân & bài đăng bán của tôi">
            <div style="display: flex; justify-content: space-between; align-items: center; margin-bottom: 2px;">
              <small class="text-muted" style="font-size: 11px; text-transform: uppercase; font-weight: 600; letter-spacing: 0.5px;">Tài khoản đăng nhập:</small>
              <span style="font-size: 11px; color: #22c55e; font-weight: 600; display: inline-flex; align-items: center; gap: 4px;">
                Trang cá nhân <i class="fas fa-arrow-right" style="font-size: 9px;"></i>
              </span>
            </div>
            <strong class="text-white" style="font-size: 13.5px; word-break: break-all; display: block;">${user.email}</strong>
          </a>
          <div class="dropdown-divider"></div>
          ${adminLink}
          <a class="dropdown-item" href="${postLink}">
            <i class="fas fa-plus-circle text-warning mr-2"></i>Đăng bán đồ cũ mới
          </a>
          <a class="dropdown-item" href="${myListingsLink}">
            <i class="fas fa-tags text-info mr-2"></i>Tin đăng bán của tôi
          </a>
          <a class="dropdown-item" href="${favoritesLink}">
            <i class="fas fa-heart text-danger mr-2"></i>Tin đã yêu thích
          </a>
          <a class="dropdown-item" href="${profileLink}">
            <i class="fas fa-user-cog text-secondary mr-2"></i>Cài đặt tài khoản
          </a>
          <div class="dropdown-divider"></div>
          <a class="dropdown-item text-muted" href="javascript:void(0)" onclick="handleLogout()">
            <i class="fas fa-sign-out-alt mr-2"></i>Đăng xuất
          </a>
        </div>
      </div>
    `);
  } else {
    container.html(`
      <div class="d-flex align-items-center">
        <a href="${loginUrl}" class="btn btn-link text-white font-weight-bold p-0 mr-3 text-decoration-none">
          <i class="fas fa-sign-in-alt mr-1"></i>Đăng nhập
        </a>
        <a href="${registerUrl}" class="btn btn-sm btn-outline-success font-weight-bold">
          <i class="fas fa-user-plus mr-1"></i>Đăng ký
        </a>
      </div>
    `);
  }
}

// Hàm chuyển đổi trạng thái yêu thích món đồ (Dấu tim)
window.toggleProductFavorite = function(productId, event) {
  if (event) {
    if (typeof event.preventDefault === "function") event.preventDefault();
    if (typeof event.stopPropagation === "function") event.stopPropagation();
  }
  const isHtml = window.location.pathname.endsWith('.html');
  const loginUrl = isHtml ? 'login.html' : '/Account/Login';

  // KIỂM TRA ĐĂNG NHẬP: Chưa đăng nhập thì chuyển hướng ngay sang Login
  if (!window.checkUserLoggedIn()) {
    const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
    window.location.href = `${loginUrl}?returnUrl=${returnUrl}`;
    return false;
  }

  // Gửi cập nhật trực tiếp lên Backend ASP.NET Core SQL Server
  fetch('/Products/ToggleFavorite?productId=' + productId, { method: 'POST' })
    .then(r => r.json())
    .then(data => {
      if (data.requireLogin) {
        const returnUrl = encodeURIComponent(window.location.pathname + window.location.search);
        window.location.href = `${loginUrl}?returnUrl=${returnUrl}`;
        return;
      }
      if (data.success) {
        const isFav = data.isFav;
        if (typeof ThriftDB !== 'undefined') {
          const localIsFav = ThriftDB.isFavorite(productId);
          if (localIsFav !== isFav) ThriftDB.toggleFavorite(productId);
        }
        const btns = document.querySelectorAll(`[data-fav-id="${productId}"]`);
        btns.forEach(btn => {
          const icon = btn.querySelector('i');
          const span = btn.querySelector('span');
          if (isFav) {
            btn.classList.add('active');
            if (icon) icon.className = 'fas fa-heart text-danger mr-1';
            if (span) span.textContent = 'Đã thích';
          } else {
            btn.classList.remove('active');
            if (icon) icon.className = 'far fa-heart mr-1';
            if (span) span.textContent = 'Yêu thích';
          }
        });

        if (typeof showToast === 'function') {
          showToast(isFav ? 'Đã thêm món đồ vào Tin đã yêu thích ❤️' : 'Đã bỏ món đồ khỏi Tin đã yêu thích', isFav ? 'success' : 'info');
        }
      }
    })
    .catch(() => {
      // Fallback cục bộ nếu mất mạng
      if (typeof ThriftDB !== 'undefined') {
        const res = ThriftDB.toggleFavorite(productId);
        const btns = document.querySelectorAll(`[data-fav-id="${productId}"]`);
        btns.forEach(btn => {
          const icon = btn.querySelector('i');
          const span = btn.querySelector('span');
          if (res.isFav) {
            btn.classList.add('active');
            if (icon) icon.className = 'fas fa-heart text-danger mr-1';
            if (span) span.textContent = 'Đã thích';
          } else {
            btn.classList.remove('active');
            if (icon) icon.className = 'far fa-heart mr-1';
            if (span) span.textContent = 'Yêu thích';
          }
        });
      }
    });

  return true;
};

// Xử lý đăng xuất
function handleLogout() {
  if (confirm("Bạn có chắc chắn muốn đăng xuất tài khoản?")) {
    ThriftDB.logout();
    showToast("Đã đăng xuất tài khoản thành công!", "info");
    setTimeout(function() {
      window.location.reload();
    }, 600);
  }
}

// Xử lý tìm kiếm từ Header
function initHeaderSearch() {
  $("#header-search-form").on("submit", function(e) {
    e.preventDefault();
    const keyword = $("#header-search-input").val().trim();
    const categoryId = $("#header-search-category").val();
    let url = "products.html?";
    if (keyword) url += "search=" + encodeURIComponent(keyword) + "&";
    if (categoryId) url += "category=" + encodeURIComponent(categoryId);
    window.location.href = url;
  });

  // Load danh mục vào dropdown tìm kiếm header nếu có
  const catSelect = $("#header-search-category");
  if (catSelect.length) {
    const cats = ThriftDB.getCategories();
    cats.forEach(c => {
      catSelect.append(`<option value="${c.id}">${c.name}</option>`);
    });
  }
}

// Hiển thị thông báo Toast
function showToast(message, type = "success") {
  let toastContainer = $("#toast-container");
  if (!toastContainer.length) {
    $("body").append('<div id="toast-container"></div>');
    toastContainer = $("#toast-container");
  }

  let icon = "fa-check-circle";
  let bg = "#1b4332";
  if (type === "error" || type === "danger") {
    icon = "fa-exclamation-circle";
    bg = "#d90429";
  } else if (type === "warning") {
    icon = "fa-exclamation-triangle";
    bg = "#e76f51";
  } else if (type === "info") {
    icon = "fa-info-circle";
    bg = "#2a9d8f";
  }

  const toastId = "toast_" + Date.now();
  const html = `
    <div id="${toastId}" class="custom-toast" style="background-color: ${bg};">
      <i class="fas ${icon} fa-lg"></i>
      <div>${message}</div>
    </div>
  `;

  toastContainer.append(html);
  setTimeout(function() {
    $("#" + toastId).fadeOut(400, function() {
      $(this).remove();
    });
  }, 3000);
}

// Hàm thêm nhanh vào giỏ hàng từ thẻ sản phẩm
function handleAddToCart(productId, event) {
  if (event) event.preventDefault();
  const res = ThriftDB.addToCart(productId, 1);
  if (res.success) {
    showToast(res.message, "success");
  } else {
    showToast(res.message, "warning");
  }
}
