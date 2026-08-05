// Moves mdBook's generated "on this page" heading tree out of the sidebar and
// into a panel pinned to the top-right of the content area.
//
// mdBook's toc.js scans the page for h2-h6 on DOMContentLoaded and builds the
// tree, wrapped in div.on-this-page, inside the active sidebar item. This file
// is loaded after toc.js, so this listener is registered second and therefore
// runs second: the tree already exists by the time it fires.
//
// The tree is moved rather than rebuilt, which keeps mdBook's own anchor links,
// fold state and scroll-spy highlighting intact. Scroll-spy survives the move
// because toc.js looks up .header-in-summary and .current-header document-wide
// rather than scoping them to the sidebar.
//
// Below the breakpoint there is not enough room beside the content column, so
// the tree is put back in the sidebar instead of being hidden outright.
(function pageToc() {
    const BREAKPOINT = '(min-width: 1500px)';

    document.addEventListener('DOMContentLoaded', function () {
        const tree = document.querySelector('.on-this-page');

        if (tree === null) {
            return;
        }

        const panel = document.createElement('nav');
        panel.classList.add('pagetoc');
        panel.setAttribute('aria-label', 'On this page');

        const title = document.createElement('div');
        title.classList.add('pagetoc-title');
        title.textContent = 'On this page';
        panel.appendChild(title);
        document.body.appendChild(panel);

        // Remember where mdBook originally put the tree so it can go back.
        const home = document.createComment('on-this-page');
        tree.before(home);

        const wideEnough = window.matchMedia(BREAKPOINT);

        function place() {
            if (wideEnough.matches) {
                panel.appendChild(tree);
            } else {
                home.after(tree);
            }
        }

        place();
        wideEnough.addEventListener('change', place);
    });
})();
