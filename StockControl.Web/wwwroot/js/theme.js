window.themeInterop = {
    get: function () {
        return localStorage.getItem('theme');
    },
    set: function (value) {
        localStorage.setItem('theme', value);
        document.documentElement.setAttribute('data-theme', value);
    }
};
