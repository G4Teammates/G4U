window.setTimeout(function () {
    $(".alert").fadeTo(500, 0).slideUp(500, function () {
        $(this).remove();
    });
}, 5000);

// Please see documentation at https://learn.microsoft.com/aspnet/core/client-side/bundling-and-minification
// for details on configuring this project to bundle and minify static web assets.

// Write your JavaScript code.


// Sticky nav trên top
document.addEventListener('DOMContentLoaded', function () {
    const header = document.querySelector('header');
    const stickyClass = 'sticky';
    const scrollOffset = 0;
    let isSticky = false;

    window.addEventListener('scroll', function () {
        // Sử dụng requestAnimationFrame để tối ưu hóa hiệu ứng mượt mà khi cuộn
        requestAnimationFrame(() => {
            if (window.scrollY > scrollOffset && !isSticky) {
                header.classList.add(stickyClass);
                isSticky = true;
            } else if (window.scrollY <= scrollOffset && isSticky) {
                header.classList.remove(stickyClass);
                isSticky = false;
            }
        });
    });
});


/*Header*/




/*Scroll To Top*/
document.addEventListener('DOMContentLoaded', function () {
    const scrollToTopBtn = document.getElementById('scrollToTopBtn');

    // Xử lý khi nhấp vào nút
    scrollToTopBtn.addEventListener('click', function () {
        // Cuộn mượt mà lên đầu trang
        window.scrollTo({
            top: 0,
            behavior: 'smooth',
        });
    });
});




// Owl carousel
$(document).ready(function () {

    $('.owl-carousel').owlCarousel({
        center: true,
        loop: true,
        margin: 10,
        nav: true,
        autoplay: true,
        autoplayTimeout: 3000,
        autoplayHoverPause: true,
        smartSpeed: 1500,
        responsive: {
            0: {
                items: 1
            },
            600: {
                items: 1
            },
            1000: {
                items: 3
            }
        }
    });

    //function showDetailModal(element) {
    //    var url = $(element).data('url');
    //    console.log("URL: " + url);

    //    $.ajax({
    //        url: url,
    //        type: 'GET',
    //        success: function (response) {
    //            $('#modalContainer').html(response);
    //            $('#modalOrderDetail').modal('show');
    //        },
    //        error: function () {
    //            alert('Có lỗi xảy ra khi tải nội dung modal.');
    //        }
    //    });
    //}

    //function showEditModal(element) {
    //    var url = $(element).data('url');
    //    console.log("URL: " + url);

    //    $.ajax({
    //        url: url,
    //        type: 'GET',
    //        success: function (response) {
    //            $('#modalContainer').html(response);
    //            $('#editModalAccount').modal('show');
    //        },
    //        error: function () {
    //            alert('Có lỗi xảy ra khi tải nội dung modal.');
    //        }
    //    });
    //}

    //window.showDetailModal = showDetailModal;  // Gán hàm vào window
    //window.showEditModal = showEditModal;      // Gán hàm vào window

})

//Scroll Other Product ở giữa màn hình khi được chọn
document.querySelectorAll('.lisotherproduct_name a').forEach(anchor => {
    anchor.addEventListener('click', function (e) {
        e.preventDefault(); // Ngăn chặn hành động mặc định

        // Lấy ID của phần được liên kết
        const targetId = this.getAttribute('href');
        const targetElement = document.querySelector(targetId);

        // Cuộn đến phần đó
        if (targetElement) {
            // Tính toán offset để cuộn đến giữa
            const rect = targetElement.getBoundingClientRect();
            const offset = rect.top + window.scrollY - (window.innerHeight / 2 - rect.height / 2);
            window.scrollTo({
                top: offset,
                behavior: 'smooth' // Cuộn mượt mà
            });
        }
    });
});

//$(document).ready(function () {
    
//});