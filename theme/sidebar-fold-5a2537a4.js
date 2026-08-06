// Sidebar fold state: per-session persistence, plus per-section defaults.
//
// mdBook persists nothing about the sidebar except its scroll offset, and each
// page is a full document load, so the fold tree resets on every navigation --
// toc.js re-expands the ancestors of the current page and nothing else. This
// records the tree's shape whenever it changes and restores it on the next
// page, so a section the reader opened stays open as they move around.
//
// State lives in sessionStorage: it survives navigation within a tab and dies
// with it. A new tab starts from the defaults below.
//
// Setting or clearing the 'expanded' class is sufficient on its own: chrome.css
// hides folded children via .chapter li:not(.expanded) > ol and rotates the
// chevron off the same class.
(function sidebarFold() {
    // Defaults for the first page loaded in a tab, matched as a substring
    // against sidebar link hrefs. Directory names tend to outlive display
    // labels, so they make the more durable handle. Note that toc.js rewrites
    // hrefs relative to the current page, so these must stay prefix-free -- a
    // leading "./" or "/" would not match on nested pages.
    //
    // 'aspnet/' does not match 'aspnet-core/' (the character after "aspnet" is
    // "-", not "/"), so the two sections cannot be confused for one another.
    //
    // Expanding is one level deep: the section opens to show its groups, but
    // those groups stay shut. The section is large enough that expanding it
    // whole buries the rest of the book below the fold.
    const EXPAND_ON_LOAD = ['aspnet-core/'];
    const COLLAPSE_ON_LOAD = ['aspnet/'];

    // toc.js expands every ancestor of the current page before this runs. When
    // true, a COLLAPSE_ON_LOAD section is folded shut even if it contains the
    // page being viewed -- which is what "completely collapsed on load" means
    // literally, at the cost of the sidebar no longer showing where you are.
    // Set to false to leave the active section alone. First load only.
    const COLLAPSE_EVEN_WHEN_ACTIVE = true;

    // Whether restoring saved state may re-open a section to show the current
    // page. Only matters if the reader folds a section shut and then navigates
    // into it by some route other than the sidebar -- the next/previous links,
    // or a link in the page body.
    const REVEAL_ACTIVE_PAGE = true;

    const STATE_KEY = 'sidebar-fold-state';

    // Every node that can fold, in document order. Position is the identity
    // used in storage: the sidebar markup is one constant string emitted into
    // every page, so a node's index is stable across navigation. It is not
    // stable across edits to SUMMARY.md, hence the size check in restore().
    //
    // The pagetoc tree that toc.js builds for the current page is made of
    // li.header-item, and it hangs off a div rather than a direct child ol, so
    // neither the filter nor the count below can see it.
    function foldables(chapter) {
        return Array.from(chapter.querySelectorAll('li.chapter-item'))
            .filter(li => li.querySelector(':scope > ol.section') !== null);
    }

    function save(nodes, total) {
        try {
            sessionStorage.setItem(STATE_KEY, JSON.stringify({
                total: total,
                folds: nodes.map(li => li.classList.contains('expanded') ? '1' : '0').join(''),
            }));
        } catch {
            // Storage unavailable (file:// on some browsers, or storage
            // blocked). Nothing to do: state stops persisting and every page
            // falls back to the defaults, which is the old behaviour.
        }
    }

    // Returns true if saved state was applied. Anything unparseable or sized
    // against a different SUMMARY.md is discarded rather than mapped onto the
    // wrong nodes -- an edit that preserves both counts could still restore
    // crooked, but the state is ephemeral and a new tab clears it.
    function restore(nodes, total) {
        let saved;

        try {
            saved = JSON.parse(sessionStorage.getItem(STATE_KEY));
        } catch {
            return false;
        }

        if (saved === null
            || typeof saved !== 'object'
            || saved.total !== total
            || typeof saved.folds !== 'string'
            || saved.folds.length !== nodes.length) {
            return false;
        }

        nodes.forEach((li, i) => li.classList.toggle('expanded', saved.folds[i] === '1'));
        return true;
    }

    function revealActive(chapter) {
        const active = chapter.querySelector('a.active');

        if (active === null) {
            return;
        }

        for (let li = active.closest('li.chapter-item'); li !== null; li = li.parentElement.closest('li.chapter-item')) {
            li.classList.add('expanded');
        }
    }

    function applyDefaults(chapter) {
        const topLevel = Array.from(chapter.children).filter(el => el.matches('li.chapter-item'));

        function sectionFor(marker) {
            const section = topLevel.find(
                li => li.querySelector('a[href*="' + marker + '"]') !== null);

            if (section === undefined) {
                console.warn('sidebar-fold: no top-level section matched "' + marker + '"');
            }

            return section;
        }

        for (const marker of EXPAND_ON_LOAD) {
            const section = sectionFor(marker);

            if (section === undefined) {
                continue;
            }

            section.classList.add('expanded');

            // Every group nested inside it closes, except the branch holding
            // the current page -- revealing a section only to hide where you
            // are in it would defeat the point of expanding it.
            section.querySelectorAll('li.chapter-item')
                .forEach(li => {
                    if (li.querySelector('a.active') === null) {
                        li.classList.remove('expanded');
                    }
                });
        }

        for (const marker of COLLAPSE_ON_LOAD) {
            const section = sectionFor(marker);

            if (section === undefined) {
                continue;
            }

            if (!COLLAPSE_EVEN_WHEN_ACTIVE && section.querySelector('a.active') !== null) {
                continue;
            }

            section.classList.remove('expanded');
            section.querySelectorAll('li.chapter-item')
                .forEach(li => li.classList.remove('expanded'));
        }
    }

    document.addEventListener('DOMContentLoaded', function () {
        const chapter = document.querySelector('#mdbook-sidebar ol.chapter');

        if (chapter === null) {
            return;
        }

        const nodes = foldables(chapter);
        const total = chapter.querySelectorAll('li.chapter-item').length;

        if (restore(nodes, total)) {
            if (REVEAL_ACTIVE_PAGE) {
                revealActive(chapter);
            }
        } else {
            applyDefaults(chapter);
        }

        save(nodes, total);

        // Record the tree whenever the reader folds something. Listening on the
        // chevrons rather than observing the subtree is deliberate: toc.js
        // rewrites classes inside the sidebar on every scroll event to track
        // the current heading, which a MutationObserver could not tell apart
        // from a fold without filtering it back out. These handlers are
        // registered after the ones toc.js installs in connectedCallback, so
        // the class has already been toggled by the time they run.
        //
        // The pagetoc tree has chevrons of its own; they hang off li.header-item
        // and are excluded here.
        chapter.querySelectorAll('.chapter-fold-toggle').forEach(toggle => {
            if (toggle.closest('li') === null || !toggle.closest('li').matches('li.chapter-item')) {
                return;
            }

            toggle.addEventListener('click', () => save(nodes, total));
        });
    });
})();
