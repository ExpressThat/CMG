namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string ShowGifAccessibilityEvidence(
        string? selector,
        string? keystroke,
        bool focusEvidence,
        bool accessibleNames,
        bool highContrast) =>
        $$"""
        (() => {
          document.querySelectorAll('[data-cmg-gif-a11y]').forEach(node => node.remove());
          const add = (kind, css, text = '') => {
            const node = document.createElement('div');
            node.dataset.cmgGifA11y = kind;
            node.style.cssText = 'all:initial;position:fixed;z-index:2147483645;pointer-events:none;box-sizing:border-box;' + css;
            node.textContent = text;
            document.documentElement.appendChild(node);
            return node;
          };
          const selector = {{JsonString(selector ?? string.Empty)}} || null;
          const target = (selector ? document.querySelector(selector) : null) || document.activeElement;
          const rect = target?.getBoundingClientRect?.();
          const visible = rect && rect.width > 0 && rect.height > 0;
          if ({{focusEvidence.ToString().ToLowerInvariant()}} && visible && target === document.activeElement) {
            const ring = add('focus', `left:${rect.left - 5}px;top:${rect.top - 5}px;width:${rect.width + 10}px;height:${rect.height + 10}px;border:4px solid {{(highContrast ? "#ffea00" : "#0ea5e9")}};border-radius:6px;box-shadow:0 0 0 2px #111,0 0 0 7px rgba(255,255,255,.85);`);
          }
          if ({{accessibleNames.ToString().ToLowerInvariant()}} && visible) {
            const explicitRole = target.getAttribute('role');
            const tag = target.tagName.toLowerCase();
            const role = explicitRole || ({button:'button',a:'link',select:'combobox',textarea:'textbox'}[tag]) || (tag === 'input' ? (target.type === 'checkbox' ? 'checkbox' : target.type === 'radio' ? 'radio' : 'textbox') : tag);
            const labelled = target.getAttribute('aria-labelledby')?.split(/\s+/).map(id => document.getElementById(id)?.textContent || '').join(' ').trim();
            const name = (target.getAttribute('aria-label') || labelled || target.getAttribute('alt') || target.getAttribute('title') || target.innerText || '').trim().replace(/\s+/g, ' ').slice(0, 100);
            const label = add('name', `left:${Math.max(8, Math.min(rect.left, innerWidth - 300))}px;top:${Math.max(8, rect.top - 42)}px;max-width:292px;padding:7px 10px;background:#111;color:#fff;border:2px solid {{(highContrast ? "#ffea00" : "#38bdf8")}};border-radius:5px;font:700 13px/1.3 system-ui,sans-serif;box-shadow:0 4px 12px rgba(0,0,0,.35);`, name ? `${role}: ${name}` : role);
          }
          const key = {{JsonString(keystroke ?? string.Empty)}};
          if (key) add('key', 'left:50%;bottom:20px;transform:translateX(-50%);padding:9px 14px;background:#111;color:#fff;border:3px solid #ffea00;border-radius:6px;font:700 16px/1.2 ui-monospace,monospace;box-shadow:0 5px 16px rgba(0,0,0,.4);', key);
          return true;
        })()
        """;

    public static string RemoveGifAccessibilityEvidence() =>
        "(() => { document.querySelectorAll('[data-cmg-gif-a11y]').forEach(node => node.remove()); return true; })()";
}
