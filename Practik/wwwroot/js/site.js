$(document).ready(function () {
    // Сворачивание сайдбара на мобильных устройствах
    $('#sidebarCollapse').on('click', function () {
        $('#sidebar').toggleClass('active');
        $('.wrapper').toggleClass('active');
    });

    // Закрытие сайдбара при клике вне его (на мобильных)
    $(document).on('click', function (e) {
        if ($(window).width() <= 992) {
            if (!$(e.target).closest('#sidebar, #sidebarCollapse').length) {
                $('#sidebar').removeClass('active');
                $('.wrapper').removeClass('active');
            }
        }
    });

    // === АВТООТКРЫТИЕ ГРУППЫ МЕНЮ С АКТИВНОЙ СТРАНИЦЕЙ ===
    const currentLocation = location.href;
    const allMenuLinks = document.querySelectorAll('.nav-submenu .nav-link');

    allMenuLinks.forEach(link => {
        if (link.href === currentLocation) {
            link.classList.add('active');

            // Находим родительскую группу (collapse) и раскрываем её
            const parentCollapse = link.closest('.collapse');
            if (parentCollapse) {
                // Используем Bootstrap API для корректного раскрытия
                const bsCollapse = new bootstrap.Collapse(parentCollapse, {
                    toggle: false
                });
                bsCollapse.show();

                // Обновляем aria-expanded у кнопки-триггера
                const toggleBtn = document.querySelector(`[href="#${parentCollapse.id}"]`);
                if (toggleBtn) {
                    toggleBtn.setAttribute('aria-expanded', 'true');
                }
            }
        }
    });

    // === АВТОМАТИЧЕСКОЕ СВОРАЧИВАНИЕ ДРУГИХ ГРУПП ПРИ ОТКРЫТИИ НОВОЙ ===
    // Bootstrap делает это автоматически через data-bs-parent,
    // но добавим синхронизацию aria-expanded для корректной анимации стрелок
    document.querySelectorAll('.nav-group-toggle').forEach(toggle => {
        toggle.addEventListener('click', function () {
            // Закрываем все остальные группы
            document.querySelectorAll('.nav-group-toggle').forEach(otherToggle => {
                if (otherToggle !== this) {
                    otherToggle.setAttribute('aria-expanded', 'false');
                }
            });
        });
    });

    // Обновление aria-expanded при событиях Bootstrap collapse
    document.querySelectorAll('.collapse').forEach(collapseEl => {
        collapseEl.addEventListener('show.bs.collapse', function () {
            const toggle = document.querySelector(`[href="#${this.id}"]`);
            if (toggle) toggle.setAttribute('aria-expanded', 'true');
        });
        collapseEl.addEventListener('hide.bs.collapse', function () {
            const toggle = document.querySelector(`[href="#${this.id}"]`);
            if (toggle) toggle.setAttribute('aria-expanded', 'false');
        });
    });
});