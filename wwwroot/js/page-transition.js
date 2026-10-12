/* =========================================
   CAMERA917 - PAGE TRANSITION
========================================= */

document.addEventListener("DOMContentLoaded", function () {

    /* ================================
       HIỆU ỨNG KHI TRANG VỪA MỞ
    ================================= */

    requestAnimationFrame(function () {

        document.body.classList.add("page-loaded");

    });


    /* ================================
       HIỆU ỨNG KHI BẤM LINK
    ================================= */

    document.querySelectorAll("a").forEach(function (link) {

        link.addEventListener("click", function (event) {

            const href = this.getAttribute("href");


            /* Không xử lý link đặc biệt */

            if (
                !href ||
                href.startsWith("#") ||
                href.startsWith("http") ||
                href.startsWith("mailto:") ||
                href.startsWith("tel:")
            ) {
                return;
            }


            /* Nếu mở tab mới thì bỏ qua */

            if (this.target === "_blank") {
                return;
            }


            /* Ngăn chuyển trang ngay */

            event.preventDefault();


            /* Chạy hiệu ứng thoát */

            document.body.classList.remove("page-loaded");

            document.body.classList.add("page-exit");


            /* Chờ animation xong rồi chuyển trang */

            setTimeout(function () {

                window.location.href = href;

            }, 350);

        });

    });

});