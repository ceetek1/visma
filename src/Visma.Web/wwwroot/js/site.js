// VISMA: small client-side behaviours shared by every page.

// Live clock in the top bar (front-desk staff glance at it when logging times).
(function () {
    const el = document.getElementById('liveClock');
    if (!el) return;
    const fmt = new Intl.DateTimeFormat(undefined, { weekday: 'short', hour: '2-digit', minute: '2-digit' });
    const tick = () => {
        const now = new Date();
        el.textContent = fmt.format(now);
        el.setAttribute('datetime', now.toISOString());
    };
    tick();
    setInterval(tick, 15000);
})();

// Light / dark theme toggle. The initial theme is applied in <head> (see _Layout.cshtml).
(function () {
    const btn = document.getElementById('themeToggle');
    if (!btn) return;
    const root = document.documentElement;
    const label = btn.querySelector('span');
    const icon = btn.querySelector('i');

    const render = () => {
        const dark = root.getAttribute('data-bs-theme') === 'dark';
        label.textContent = dark ? 'Light mode' : 'Dark mode';
        icon.className = 'bi ' + (dark ? 'bi-sun' : 'bi-moon-stars');
    };

    btn.addEventListener('click', () => {
        const next = root.getAttribute('data-bs-theme') === 'dark' ? 'light' : 'dark';
        root.setAttribute('data-bs-theme', next);
        try { localStorage.setItem('visma-theme', next); } catch (e) { /* private mode: ignore */ }
        render();
    });
    render();
})();
