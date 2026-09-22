// CEHR Health — site scripts.
// Kept intentionally small in Phase 2; cart/quantity helpers are added in later phases.
(function () {
    "use strict";

    // Enable Bootstrap tooltips if any are present.
    document.addEventListener("DOMContentLoaded", function () {
        var triggers = [].slice.call(document.querySelectorAll('[data-bs-toggle="tooltip"]'));
        triggers.forEach(function (el) {
            // eslint-disable-next-line no-undef
            new bootstrap.Tooltip(el);
        });
    });
})();
