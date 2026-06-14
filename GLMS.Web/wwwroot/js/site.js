document.addEventListener('DOMContentLoaded', function () {
    document.querySelectorAll('.alert-success').forEach(el => {
        setTimeout(() => bootstrap.Alert.getOrCreateInstance(el).close(), 4000);
    });
    const path = window.location.pathname.split('/')[1].toLowerCase();
    document.querySelectorAll('.nav-link').forEach(link => {
        const href = (link.getAttribute('href') || '').toLowerCase();
        if (path && href.includes(path)) link.classList.add('active');
    });
});
