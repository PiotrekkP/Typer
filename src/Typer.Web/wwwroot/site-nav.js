(function () {
    function closeMobileNav() {
        var toggle = document.getElementById('site-nav-toggle');
        if (toggle) toggle.checked = false;
    }

    document.addEventListener('click', function (e) {
        if (e.target.closest('.site-header__drawer a, .site-header__drawer button[type="submit"]')) {
            closeMobileNav();
        }
    });

    window.addEventListener('popstate', closeMobileNav);
})();
