(function () {
    window.typerOpenProfileEdit = function (mode) {
        var id = mode === 'player' ? 'profile-edit-player' : 'profile-edit-team';
        var cb = document.getElementById(id);
        if (cb) cb.checked = true;
    };

    document.addEventListener('change', function (e) {
        if (!e.target.classList.contains('profile-edit-toggle')) return;
        if (!e.target.checked) return;
        document.querySelectorAll('.profile-edit-toggle').forEach(function (cb) {
            if (cb !== e.target) cb.checked = false;
        });
    });

    document.addEventListener('click', function (e) {
        if (e.target.closest('.selection-picker__team, .player-picker-card')) {
            document.querySelectorAll('.profile-edit-toggle').forEach(function (cb) {
                cb.checked = false;
            });
        }
    });
})();
