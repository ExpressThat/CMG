namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string AddGifRedaction(
        string selector,
        string id,
        string style,
        string color,
        string replacement,
        int padding) =>
        $$"""
        (() => {
          const element = window.__cmgQuery?.({{JsonString(selector)}}) ?? document.querySelector({{JsonString(selector)}});
          if (!element) return false;
          return window.__cmgAddGifRedaction(element, {{JsonString(id)}}, {{JsonString(style)}}, {{JsonString(color)}}, {{JsonString(replacement)}}, {{padding}});
        })()
        """;

    public static string PrepareGifRedactions() =>
        """
        (() => {
          document.querySelectorAll('[data-cmg-gif-redaction]').forEach(node => { if (node.matches?.(':popover-open')) node.hidePopover(); node.remove(); });
          window.__cmgAddGifRedaction = (element, id, style, color, replacement, padding) => {
            const rect = element.getBoundingClientRect();
            if (rect.width <= 0 || rect.height <= 0) return false;
            const overlay = document.createElement('div');
            overlay.setAttribute('data-cmg-gif-redaction', id);
            overlay.setAttribute('popover', 'manual');
            overlay.style.cssText = `all:initial;position:fixed;box-sizing:border-box;left:${rect.left-padding}px;top:${rect.top-padding}px;width:${rect.width+padding*2}px;height:${rect.height+padding*2}px;z-index:2147483644;pointer-events:none;margin:0;`;
            if (style === 'blur') {
              overlay.style.background = 'rgba(17,24,39,.16)';
              overlay.style.backdropFilter = 'blur(10px)';
            } else {
              overlay.style.background = color;
            }
            if (style === 'replacement') {
              overlay.textContent = replacement;
              overlay.style.color = '#fff';
              overlay.style.font = '600 12px/1.2 system-ui,sans-serif';
              overlay.style.display = 'flex';
              overlay.style.alignItems = 'center';
              overlay.style.justifyContent = 'center';
              overlay.style.overflow = 'hidden';
            }
            document.documentElement.appendChild(overlay);
            overlay.showPopover?.();
            return true;
          };
          return true;
        })()
        """;

    public static string AddAutomaticGifRedactions(string mode) =>
        $$"""
        (() => {
          const mode = {{JsonString(mode)}};
          let index = 0;
          const add = element => window.__cmgAddGifRedaction?.(element, `auto-${++index}`, 'solid', '#111827', '[redacted]', 2);
          document.querySelectorAll('input[type="password"]').forEach(add);
          const patterns = [];
          if (['sensitive', 'privacy'].includes(mode)) {
            patterns.push(/(?:bearer\s+[a-z0-9._~+/=-]{12,}|\beyJ[a-zA-Z0-9_-]{10,}\.[a-zA-Z0-9_-]{10,}|\b(?:sk|pk|ghp|github_pat|xox[baprs])[-_][a-zA-Z0-9_-]{12,})/i);
          }
          if (['emails', 'privacy'].includes(mode)) {
            patterns.push(/\b[a-z0-9.!#$%&'*+/=?^_`{|}~-]+@[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?(?:\.[a-z0-9](?:[a-z0-9-]{0,61}[a-z0-9])?)+\b/i);
          }
          if (['payment', 'privacy'].includes(mode)) {
            patterns.push(/(?:\d[ -]*?){13,19}/);
          }
          if (patterns.length) {
            const matches = value => patterns.some(pattern => pattern.test(value));
            document.querySelectorAll('input,textarea,[data-token],[data-secret]').forEach(element => {
              const value = element.value ?? element.textContent ?? '';
              if (matches(value)) add(element);
            });
            const walker = document.createTreeWalker(document.body ?? document.documentElement, NodeFilter.SHOW_TEXT);
            const elements = new Set();
            while (walker.nextNode()) if (matches(walker.currentNode.nodeValue ?? '')) elements.add(walker.currentNode.parentElement);
            elements.forEach(element => element && add(element));
          }
          return index;
        })()
        """;

    public static string EnforceGifRedactionSafety() =>
        """
        (() => {
          const masks = [...document.querySelectorAll('[data-cmg-gif-redaction]')].map(e => e.getBoundingClientRect());
          const unsafe = [...document.querySelectorAll('input[type="password"]')].filter(element => {
            const rect = element.getBoundingClientRect();
            const style = getComputedStyle(element);
            if (rect.width <= 0 || rect.height <= 0 || style.visibility === 'hidden' || style.display === 'none') return false;
            return !masks.some(mask => mask.left <= rect.left && mask.top <= rect.top && mask.right >= rect.right && mask.bottom >= rect.bottom);
          });
          if (unsafe.length) throw new Error(`GIF redaction safety blocked capture: ${unsafe.length} visible password field(s) are not masked.`);
          return true;
        })()
        """;

    public static string PromoteGifEvidence() =>
        "(() => { const failed=[]; for (const id of ['__cmg_message_bar','__cmg_virtual_cursor','__cmg_cursor_pulse']) { const e=document.getElementById(id); if(!e || typeof e.showPopover!=='function') continue; try { if(!e.matches(':popover-open')) e.showPopover(); if(!e.matches(':popover-open')) failed.push(id); } catch { failed.push(id); } } return JSON.stringify({failed}); })()";

    public static string RemoveGifRedactions() =>
        "(() => { document.querySelectorAll('[data-cmg-gif-redaction]').forEach(node => { if(node.matches?.(':popover-open')) node.hidePopover(); node.remove(); }); delete window.__cmgAddGifRedaction; return true; })()";
}
