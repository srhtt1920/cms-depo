// CMS tercih cookie yönetimi
// ProtectedBrowserStorage şifrelemesi yerine düz cookie kullanılır
// çünkü tema/dil ilk render'da (prerender) da okunması gerekir

window.cmsPrefs = {

    getCookie: function (name) {
        const match = document.cookie.match(new RegExp('(^| )' + name + '=([^;]+)'));
        return match ? decodeURIComponent(match[2]) : null;
    },

    setCookie: function (name, value, days) {
        const expires = new Date(Date.now() + days * 864e5).toUTCString();
        document.cookie = name + '=' + encodeURIComponent(value)
            + '; expires=' + expires
            + '; path=/; SameSite=Lax';
    },

    removeCookie: function (name) {
        document.cookie = name + '=; expires=Thu, 01 Jan 1970 00:00:00 GMT; path=/';
    },

    // Tüm tercihleri tek seferde oku
    getAll: function () {
        return {
            theme:     window.cmsPrefs.getCookie('cms-appTheme')     || 'light',
            language:  window.cmsPrefs.getCookie('cms-language')     || 'tr',
            menuTheme: window.cmsPrefs.getCookie('cms-menuTheme')    || 'dark',
            sizeMode:  window.cmsPrefs.getCookie('cms-sizeMode')     || 'default'
        };
    },

    // Tüm tercihleri kaydet
    saveAll: function (prefs) {
        const days = 365;
        window.cmsPrefs.setCookie('cms-appTheme',  prefs.theme,     days);
        window.cmsPrefs.setCookie('cms-language',  prefs.language,  days);
        window.cmsPrefs.setCookie('cms-menuTheme', prefs.menuTheme, days);
        window.cmsPrefs.setCookie('cms-sizeMode',  prefs.sizeMode,  days);
    },

    // HTML attribute'larını anında uygula (flash önleme)
    applyAll: function () {
        const p = window.cmsPrefs.getAll();
        document.documentElement.setAttribute('data-bs-theme',  p.theme);
        document.documentElement.setAttribute('data-menu-theme', p.menuTheme);
        document.documentElement.setAttribute('data-size-mode',  p.sizeMode);
        document.documentElement.setAttribute('lang',            p.language);
    }
};

// Sayfa yüklenir yüklenmez uygula — FOUC (Flash Of Unstyled Content) önle
window.cmsPrefs.applyAll();
