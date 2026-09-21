// BookShop JavaScript
document.addEventListener("DOMContentLoaded", function () {
    // 1. Password visibility toggle (Click to toggle OR Press & Hold to temporarily peek)
    document.querySelectorAll(".password-toggle").forEach(function (toggle) {
        const targetId = toggle.getAttribute("data-target");
        const targetInput = targetId ? document.getElementById(targetId) : null;
        const icon = toggle.querySelector("i");

        if (!targetInput) return;

        let holdTimeout = null;
        let isHolding = false;

        function setVisibility(show) {
            targetInput.type = show ? "text" : "password";
            if (icon) {
                if (show) {
                    icon.classList.remove("bi-eye");
                    icon.classList.add("bi-eye-slash");
                } else {
                    icon.classList.remove("bi-eye-slash");
                    icon.classList.add("bi-eye");
                }
            }
        }

        // Click to toggle visibility
        toggle.addEventListener("click", function (e) {
            e.preventDefault();
            if (isHolding) {
                isHolding = false;
                return;
            }
            const isCurrentlyPassword = targetInput.type === "password";
            setVisibility(isCurrentlyPassword);
        });

        // Press & hold to temporarily unhide while entering
        function startHold() {
            holdTimeout = setTimeout(function () {
                isHolding = true;
                setVisibility(true);
            }, 180);
        }

        function endHold() {
            if (holdTimeout) {
                clearTimeout(holdTimeout);
                holdTimeout = null;
            }
            if (isHolding) {
                setVisibility(false);
                setTimeout(function () {
                    isHolding = false;
                }, 50);
            }
        }

        // Mouse events
        toggle.addEventListener("mousedown", startHold);
        toggle.addEventListener("mouseup", endHold);
        toggle.addEventListener("mouseleave", endHold);

        // Touch events for mobile
        toggle.addEventListener("touchstart", startHold, { passive: true });
        toggle.addEventListener("touchend", endHold);
        toggle.addEventListener("touchcancel", endHold);
    });

    // 2. Smooth Scroll-Reveal Animation Controller for Bookstore Pages
    if ('IntersectionObserver' in window) {
        const observerOptions = {
            root: null,
            rootMargin: '0px 0px -40px 0px',
            threshold: 0.12
        };

        const revealObserver = new IntersectionObserver(function (entries, observer) {
            entries.forEach(function (entry) {
                if (entry.isIntersecting) {
                    entry.target.classList.add('is-visible');
                    observer.unobserve(entry.target);
                }
            });
        }, observerOptions);

        document.querySelectorAll('.reveal-on-scroll').forEach(function (el) {
            revealObserver.observe(el);
        });
    } else {
        // Fallback for older browsers
        document.querySelectorAll('.reveal-on-scroll').forEach(function (el) {
            el.classList.add('is-visible');
        });
    }
});
