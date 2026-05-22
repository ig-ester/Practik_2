
$(document).ready(function () {
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('.wrapper').toggleClass('active');
    });

    $(document).on('click', function (e) {
        if ($(window).width() <= 992) {
            if (!$(e.target).closest('#sidebar, #sidebarCollapse').length) {
                $('#sidebar').removeClass('active');
                $('.wrapper').removeClass('active');
            }
        }
    });

    const currentLocation = location.href;
    const menuItems = document.querySelectorAll('.nav-link');
    menuItems.forEach(item => {
        if (item.href === currentLocation) {
            item.classList.add('active');
        }
    });
});