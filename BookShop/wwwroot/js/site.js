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

    // 3. Searchable Dropdowns (Tom Select)
    initSearchableSelects();

    // 4. Fast Navbar Live Search
    initNavbarFastSearch();
});

// 3. Searchable Dropdowns (Tom Select)
function initSearchableSelects() {
    if (typeof TomSelect === 'undefined') return;

    const isRtl = document.documentElement.getAttribute('dir') === 'rtl';
    const defaultPlaceholder = isRtl ? 'ابحث أو اختر...' : 'Type to search...';

    document.querySelectorAll('select.searchable-select').forEach(function (el) {
        if (el.tomselect) return; // already initialized

        const placeholder = el.getAttribute('data-placeholder') || (el.options.length > 0 && el.options[0].value === "" ? el.options[0].text : defaultPlaceholder);

        try {
            new TomSelect(el, {
                create: false,
                placeholder: placeholder,
                allowEmptyOption: true,
                maxOptions: 300,
                sortField: [{ field: '$order' }, { field: '$score' }],
                plugins: ['clear_button']
            });
        } catch (e) {
            console.warn('TomSelect init error on', el, e);
        }
    });
}

// 4. Fast Navbar Live Search
function initNavbarFastSearch() {
    const searchInput = document.getElementById('navbarSearchInput');
    const resultsContainer = document.getElementById('navbarSearchResults');
    const itemsContainer = document.getElementById('navbarSearchItems');
    const searchForm = document.getElementById('navbarSearchForm');

    if (!searchInput || !resultsContainer || !itemsContainer) return;

    let debounceTimer = null;
    const isRtl = document.documentElement.getAttribute('dir') === 'rtl';

    function closeResults() {
        resultsContainer.style.display = 'none';
        itemsContainer.innerHTML = '';
    }

    // Close on click outside
    document.addEventListener('click', function (e) {
        if (!searchForm.contains(e.target)) {
            closeResults();
        }
    });

    // Close on escape key
    searchInput.addEventListener('keydown', function (e) {
        if (e.key === 'Escape') {
            closeResults();
        }
    });

    searchInput.addEventListener('input', function () {
        const query = this.value.trim();
        clearTimeout(debounceTimer);

        if (query.length < 1) {
            closeResults();
            return;
        }

        debounceTimer = setTimeout(async function () {
            try {
                const response = await fetch(`/Home/QuickSearch?q=${encodeURIComponent(query)}`);
                if (!response.ok) return;

                const data = await response.json();
                const books = data.results || [];

                if (books.length === 0) {
                    itemsContainer.innerHTML = `
                        <div class="p-3 text-center text-muted small">
                            <i class="bi bi-search d-block fs-5 mb-1" style="color: var(--c-cafe-light);"></i>
                            ${isRtl ? 'لا توجد نتائج مطابقة لبحثك' : 'No matching books found'}
                        </div>
                        <a href="/Home/Shop?searchString=${encodeURIComponent(query)}" class="dropdown-item text-center text-primary py-2 border-top small fw-semibold">
                            ${isRtl ? 'البحث في كامل الكتالوج &larr;' : 'Search entire catalog &rarr;'}
                        </a>
                    `;
                    resultsContainer.style.display = 'block';
                    return;
                }

                let html = '';
                books.forEach(function (b) {
                    const coverUrl = b.coverImageUrl || '/images/default-book-cover.svg';
                    const coverHtml = `<img src="${coverUrl}" alt="" class="rounded-1 shadow-sm flex-shrink-0" style="width: 38px; height: 52px; object-fit: contain;" onerror="if(this.src.indexOf('default-book-cover.svg')===-1){this.src='/images/default-book-cover.svg';}" />`;

                    html += `
                        <a href="/Home/Details/${b.id}" class="dropdown-item p-2 d-flex align-items-center gap-2 rounded-2 text-decoration-none">
                            ${coverHtml}
                            <div class="flex-grow-1 min-w-0">
                                <div class="font-serif fw-semibold text-dark text-truncate small">${b.title}</div>
                                <div class="text-muted text-truncate" style="font-size: 0.76rem;">${b.author}</div>
                                <div class="d-flex align-items-center gap-1 mt-1">
                                    ${b.category ? `<span class="badge bg-light text-dark border py-0 px-1" style="font-size: 0.68rem;">${b.category}</span>` : ''}
                                    <span class="fw-bold ${isRtl ? 'me-auto' : 'ms-auto'} font-serif" style="color: var(--c-espresso); font-size: 0.82rem;">${b.effectivePrice}</span>
                                </div>
                            </div>
                        </a>
                    `;
                });

                html += `
                    <div class="border-top mt-1 pt-1">
                        <a href="/Home/Shop?searchString=${encodeURIComponent(query)}" class="dropdown-item text-center text-primary py-2 small fw-semibold">
                            ${isRtl ? `عرض كافة النتائج لـ "${query}" &larr;` : `View all results for "${query}" &rarr;`}
                        </a>
                    </div>
                `;

                itemsContainer.innerHTML = html;
                resultsContainer.style.display = 'block';
            } catch (err) {
                console.error('QuickSearch error:', err);
            }
        }, 200);
    });

    // Reopen on focus if text is present
    searchInput.addEventListener('focus', function () {
        if (this.value.trim().length >= 1 && itemsContainer.children.length > 0) {
            resultsContainer.style.display = 'block';
        }
    });
}
