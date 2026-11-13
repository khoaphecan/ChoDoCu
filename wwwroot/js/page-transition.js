/* =========================================
   CAMERA917 - PAGE TRANSITION
========================================= */

(function () {
    function restorePageVisibility() {
        if (document.body) {
            document.body.classList.remove("page-exit");
            document.body.classList.add("page-loaded");
            document.body.style.opacity = "1";
        }
    }

    // Khôi phục ngay lập tức
    restorePageVisibility();

    document.addEventListener("DOMContentLoaded", function () {
        restorePageVisibility();

        /* Phục hồi giao diện khi nhấn Back / Forward trên trình duyệt */
        window.addEventListener("pageshow", function (event) {
            restorePageVisibility();
        });

        /* ================================
           HIỆU ỨNG KHI BẤM LINK
        ================================= */
        document.querySelectorAll("a").forEach(function (link) {
            link.addEventListener("click", function (event) {
                const href = this.getAttribute("href");

                /* BỎ QUA HOÀN TOÀN: link menu User, dropdown, header, nav, tab mới, script */
                if (
                    !href ||
                    href.startsWith("#") ||
                    href.startsWith("http") ||
                    href.startsWith("mailto:") ||
                    href.startsWith("tel:") ||
                    href.startsWith("javascript:") ||
                    this.target === "_blank" ||
                    this.classList.contains("no-transition") ||
                    this.classList.contains("dropdown-item") ||
                    this.classList.contains("dropdown-toggle") ||
                    this.hasAttribute("data-toggle") ||
                    this.closest(".dropdown") ||
                    this.closest(".dropdown-menu") ||
                    this.closest("header") ||
                    this.closest("nav")
                ) {
                    return;
                }

                /* Ngăn chuyển trang tức thì */
                event.preventDefault();

                /* Chạy hiệu ứng thoát nhẹ nhàng */
                document.body.classList.remove("page-loaded");
                document.body.classList.add("page-exit");

                /* Chuyển trang sau 180ms */
                setTimeout(function () {
                    window.location.href = href;
                }, 180);

                /* Bảo hiểm chống kẹt trắng trang: tự động hồi phục nếu sau 1.5s chưa chuyển */
                setTimeout(function () {
                    restorePageVisibility();
                }, 1500);
            });
        });
    });
})();
