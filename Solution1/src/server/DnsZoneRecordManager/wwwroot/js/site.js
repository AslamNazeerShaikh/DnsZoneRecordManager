// DnsZoneRecordManager client behavior: runtime theme switch, toasts, filter auto-submit.
// Stack: jQuery + Bootstrap 5 (per brief default); vanilla where trivial.

(function () {
    "use strict";

    function currentTheme() {
        try {
            return localStorage.getItem("dns-theme") || document.documentElement.getAttribute("data-bs-theme") || "light";
        } catch (e) {
            return "light";
        }
    }

    function paintThemeIcon(theme) {
        var dark = theme === "dark";
        $("#theme-icon-sun").toggle(!dark);
        $("#theme-icon-moon").toggle(dark);
    }

    function applyTheme(theme) {
        document.documentElement.setAttribute("data-bs-theme", theme);
        try {
            localStorage.setItem("dns-theme", theme);
        } catch (e) { /* private mode: theme lasts this session */ }
        paintThemeIcon(theme);
    }

    $(function () {
        paintThemeIcon(currentTheme());

        $("#theme-toggle").on("click", function () {
            applyTheme(currentTheme() === "dark" ? "light" : "dark");
        });

        $(".toast").each(function (_, el) {
            bootstrap.Toast.getOrCreateInstance(el, { delay: 4000 }).show();
        });

        $(".dns-autosubmit").on("change", function () {
            $(this).closest("form").trigger("submit");
        });
    });
})();
