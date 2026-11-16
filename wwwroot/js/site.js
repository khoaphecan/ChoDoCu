// ThriftShop - Global Site Scripts
// Đồng bộ nút Yêu thích / Wishlist với SQL Database
async function toggleFavorite(event, productId, btn) {
    if (event) {
        if (typeof event.preventDefault === "function") event.preventDefault();
        if (typeof event.stopPropagation === "function") event.stopPropagation();
    }
    try {
        const res = await fetch(`/Products/ToggleFavorite?productId=${productId}`, {
            method: 'POST'
        });
        const data = await res.json();
        if (data.requireLogin) {
            alert(data.message || 'Vui lòng đăng nhập để sử dụng tính năng này!');
            window.location.href = '/Account/Login?returnUrl=' + encodeURIComponent(window.location.pathname + window.location.search);
            return;
        }
        if (data.success) {
            const isFav = (typeof data.isFavorited !== 'undefined') ? data.isFavorited : data.isFav;
            if (typeof ThriftDB !== 'undefined' && typeof ThriftDB.toggleFavorite === 'function') {
                const localIsFav = ThriftDB.isFavorite ? ThriftDB.isFavorite(productId) : false;
                if (localIsFav !== isFav) ThriftDB.toggleFavorite(productId);
            }

            // Cập nhật giao diện nút tim tròn trên Card hoặc nút Yêu thích ở trang Details
            document.querySelectorAll(`[data-fav-id="${productId}"]`).forEach(b => {
                const icon = b.querySelector('i, svg');
                if (b.classList.contains('btn-fav-detail')) {
                    b.innerHTML = isFav ? '❤️ Đã yêu thích' : '♡ Yêu thích';
                    b.classList.toggle('active', isFav);
                    if (isFav) {
                        b.style.borderColor = '#ef4444';
                        b.style.backgroundColor = '#fef2f2';
                        b.style.color = '#ef4444';
                    } else {
                        b.style.borderColor = '#e4e4e7';
                        b.style.backgroundColor = '#ffffff';
                        b.style.color = '#71717a';
                    }
                } else if (icon) {
                    icon.className = isFav ? 'fas fa-heart text-danger' : 'far fa-heart';
                    b.classList.toggle('active', isFav);
                    b.title = isFav ? 'Bỏ yêu thích' : 'Yêu thích món đồ';
                }
            });

            if (typeof showToast === 'function') {
                showToast(isFav ? 'Đã thêm món đồ vào Tin đã yêu thích ❤️' : 'Đã bỏ món đồ khỏi Tin đã yêu thích', isFav ? 'success' : 'info');
            }
        }
    } catch (err) {
        console.error('Lỗi khi cập nhật yêu thích:', err);
    }
}

window.toggleFavorite = toggleFavorite;
window.toggleProductFavorite = function(productId, event, btn) {
    const targetBtn = btn || (event ? event.currentTarget : null) || document.querySelector(`[data-fav-id="${productId}"]`);
    return toggleFavorite(event, productId, targetBtn);
};
