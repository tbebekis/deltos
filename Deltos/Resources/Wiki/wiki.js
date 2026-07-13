
(function () {
    function q(sel, root) { return (root || document).querySelector(sel) }
    function qa(sel, root) { return Array.prototype.slice.call((root || document).querySelectorAll(sel)) }

    function setTheme(mode) {
        var html = document.documentElement;
        html.setAttribute('data-theme', mode);
        try { localStorage.setItem('wiki.theme', mode); } catch (e) { }
        var link = q('#hljs-theme');
        if (link) {
            if (mode === 'dark') { link.href = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github-dark.min.css'; }
            else if (mode === 'light') { link.href = 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css'; }
            else {
                var prefersDark = window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
                link.href = prefersDark ? 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github-dark.min.css' : 'https://cdnjs.cloudflare.com/ajax/libs/highlight.js/11.9.0/styles/github.min.css';
            }
        }
        qa('.theme-btn').forEach(function (b) { b.classList.remove('active') });
        var active = q('.theme-btn[data-mode="' + mode + '"]');
        if (active) active.classList.add('active');
    }

    (function () {
        var stored = null; try { stored = localStorage.getItem('wiki.theme'); } catch (e) { }
        var mode = stored || 'auto'; setTheme(mode);
        qa('.theme-btn').forEach(function (btn) {
            btn.addEventListener('click', function () {
                var m = btn.getAttribute('data-mode') || 'auto'; setTheme(m);
            });
        });
        if (window.matchMedia) {
            try {
                window.matchMedia('(prefers-color-scheme: dark)').addEventListener('change', function () {
                    if ((localStorage.getItem('wiki.theme') || 'auto') === 'auto') setTheme('auto');
                });
            } catch (e) { }
        }
    })();

    function setActivePanel(panelName) {
        qa('.wiki-nav-tab').forEach(function (item) { item.classList.remove('active'); });
        qa('.wiki-nav-panel').forEach(function (item) { item.classList.remove('active'); });
        var tab = q('.wiki-nav-tab[data-panel="' + panelName + '"]');
        var panel = q('.wiki-nav-panel[data-panel="' + panelName + '"]');
        if (tab) tab.classList.add('active');
        if (panel) panel.classList.add('active');
        return panel;
    }

    function setActiveGroup(panel, groupKey, itemKey) {
        if (!panel || !groupKey) return;
        var group = q('.wiki-nav-group[data-target="' + groupKey + '"]', panel);
        var list = q('.wiki-nav-item-list[data-group="' + groupKey + '"]', panel);
        if (!group || !list) return;
        qa('.wiki-nav-group', panel).forEach(function (item) { item.classList.remove('active'); });
        qa('.wiki-nav-item-list', panel).forEach(function (item) { item.classList.remove('active'); });
        group.classList.add('active');
        list.classList.add('active');
        selectNavItem(list, itemKey);
    }

    function selectNavItem(list, itemKey) {
        if (!list) return;
        qa('li', list).forEach(function (item) { item.classList.remove('active'); });
        var item = itemKey ? q('li[data-item="' + itemKey + '"]', list) : null;
        if (!item) item = q('li', list);
        if (item) item.classList.add('active');
    }

    function getSelectedItemKey(panel) {
        var item = q('.wiki-nav-item-list.active li.active', panel);
        return item ? item.getAttribute('data-item') : '';
    }

    function getSelectedItemLink(panel) {
        return q('.wiki-nav-item-list.active li.active a', panel);
    }

    function updateNavQuery(panelName, groupKey, itemKey) {
        if (!window.history || !window.URLSearchParams) return;
        var params = new URLSearchParams(window.location.search);
        if (panelName) params.set('tab', panelName);
        if (groupKey) params.set('group', groupKey);
        if (itemKey) params.set('item', itemKey); else params.delete('item');
        var query = params.toString();
        var url = window.location.pathname + (query ? '?' + query : '') + window.location.hash;
        window.history.replaceState(null, '', url);
    }

    qa('.wiki-nav-tab').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var panelName = btn.getAttribute('data-panel');
            var panel = setActivePanel(panelName);
            var group = q('.wiki-nav-group.active', panel);
            updateNavQuery(panelName, group ? group.getAttribute('data-target') : '', getSelectedItemKey(panel));
        });
    });

    qa('.wiki-nav-group').forEach(function (btn) {
        btn.addEventListener('click', function () {
            var panel = btn.closest('.wiki-nav-panel');
            var target = btn.getAttribute('data-target');
            if (!panel || !target) return;
            setActiveGroup(panel, target, '');
            var link = getSelectedItemLink(panel);
            if (link && link.href) {
                window.location.href = link.href;
            } else {
                updateNavQuery(panel.getAttribute('data-panel'), target, getSelectedItemKey(panel));
            }
        });
    });

    qa('.wiki-nav-item-link').forEach(function (link) {
        link.addEventListener('click', function () {
            var panel = link.closest('.wiki-nav-panel');
            var list = link.closest('.wiki-nav-item-list');
            var itemKey = link.getAttribute('data-item');
            if (!panel || !list || !itemKey) return;
            selectNavItem(list, itemKey);
            updateNavQuery(panel.getAttribute('data-panel'), list.getAttribute('data-group'), itemKey);
        });
    });

    (function restoreNavState() {
        if (!window.URLSearchParams) return;
        var params = new URLSearchParams(window.location.search);
        var panelName = params.get('tab');
        var groupKey = params.get('group');
        var itemKey = params.get('item');
        if (!panelName && groupKey) {
            panelName = groupKey.indexOf('tags-') === 0 ? 'tags' : 'categories';
        }
        if (!panelName) return;
        var panel = setActivePanel(panelName);
        if (groupKey) setActiveGroup(panel, groupKey, itemKey);
    })();

    window.__filter = function (input, id) {
        var panel = input.closest('.wiki-nav-panel');
        if (!panel) return;
        var qv = (input.value || '').toLowerCase();
        qa('.wiki-nav-group', panel).forEach(function (item) {
            var t = (item.textContent || '').toLowerCase();
            item.style.display = t.indexOf(qv) >= 0 ? '' : 'none';
        });
    };

    qa('.quick-filter').forEach(function (input) {
        input.addEventListener('input', function () { window.__filter(input); });
    });

    function initHL() { qa('pre code').forEach(function (block) { try { window.hljs.highlightElement(block); } catch (e) { } }); }
    if (window.hljs) { initHL(); } else { document.addEventListener('DOMContentLoaded', initHL); }

    function norm(t) {
        if (!t) return '';
        try { return t.normalize('NFD').replace(/[\u0300-\u036f]/g, '').toLowerCase(); } catch (e) { return (t + '').toLowerCase(); }
    }

    var fuse = null, data = null, loaded = false;
    function loadIndex() {
        if (loaded) return;
        loaded = true;
        fetch('/search-index.json').then(function (r) { return r.json() }).then(function (arr) {
            data = arr.map(function (d) {
                var id = d.Id || d.id || '';
                var title = d.Title || d.title || '';
                var aliases = d.Aliases || d.aliases || [];
                var tags = d.Tags || d.tags || [];
                var body = d.Body || d.body || '';
                var url = d.Url || d.url || '';
                var category = d.Category || d.category || '';
                return {
                    Id: id, Title: title, Aliases: aliases, Tags: tags, Body: body, Url: url, Category: category,
                    _t: norm(title), _b: norm(body), _a: aliases.map(norm), _g: tags.map(norm)
                };
            });
            fuse = new Fuse(data, {
                includeMatches: true,
                threshold: 0.33,
                minMatchCharLength: 2,
                keys: [
                    { name: '_t', weight: 0.6 },
                    { name: '_a', weight: 0.18 },
                    { name: '_g', weight: 0.14 },
                    { name: '_b', weight: 0.08 }
                ]
            });
        }).catch(function (e) { console.error('Search index load failed', e) });
    }

    var input = q('#search-input'); var box = q('#search-results');
    if (input) {
        input.addEventListener('focus', function () { loadIndex(); if (box) box.classList.add('open'); });
        input.addEventListener('blur', function () { setTimeout(function () { if (box) box.classList.remove('open'); }, 150); });
        var tId = 0;
        input.addEventListener('input', function () {
            if (!fuse) { loadIndex(); }
            var val = input.value.trim();
            if (!val) { if (box) { box.innerHTML = ''; } return; }
            clearTimeout(tId);
            tId = setTimeout(function () {
                if (!fuse) { return; }
                var qn = norm(val);
                var res = fuse.search(qn, { limit: 50 });
                box.innerHTML = res.map(renderHit).join('');
                box.classList.add('open');
            }, 120);
        });
    }

    function escapeHtml(s) {
        var map = { '&': '&amp;', '<': '&lt;', '>': '&gt;', '"': '&quot;', "'": '&#39;' };
        return String(s || '').replace(/[&<>"']/g, function (c) { return map[c]; });
    }
    function escReg(s) { return (s + '').replace(/[.*+?^${}()|[\]\\]/g, '\\$&'); }
    function hlSnippet(text, term, radius) {
        if (!text) return '';
        var low = text.toLowerCase(); var lowTerm = term.toLowerCase();
        var i = low.indexOf(lowTerm);
        if (i < 0) { return escapeHtml(text.slice(0, 200)) + (text.length > 200 ? '…' : ''); }
        var start = Math.max(0, i - 120); var end = Math.min(text.length, i + term.length + 120);
        var pre = start > 0 ? '…' : ''; var post = end < text.length ? '…' : '';
        var raw = text.substring(start, end);
        var re = new RegExp(escReg(term), 'ig');
        return pre + escapeHtml(raw).replace(re, function (m) { return '<b>' + escapeHtml(m) + '</b>'; }) + post;
    }
    function renderHit(hit) {
        var d = hit.item;
        var sn = hlSnippet(d.Body, input.value, 140);
        return '<div class="hit"><a href="' + d.Url + '">' + escapeHtml(d.Title) + '</a><div class="snip">' + sn + '</div></div>';
    }

    document.addEventListener('DOMContentLoaded', function () {
        var burger = document.querySelector('.burger');
        var sidebar = document.querySelector('.sidebar');
        var overlay = document.querySelector('.drawer-overlay');
        if (!overlay) { overlay = document.createElement('div'); overlay.className = 'drawer-overlay'; document.body.appendChild(overlay); }
        function openDrawer() { if (sidebar) { sidebar.classList.add('open'); } if (overlay) { overlay.classList.add('visible'); } }
        function closeDrawer() { if (sidebar) { sidebar.classList.remove('open'); } if (overlay) { overlay.classList.remove('visible'); } }
        if (burger) { burger.addEventListener('click', function (e) { e.preventDefault(); openDrawer(); }); }
        if (overlay) { overlay.addEventListener('click', closeDrawer); }
        document.addEventListener('keydown', function (e) { if (e.key === 'Escape') closeDrawer(); });
    });

})();
