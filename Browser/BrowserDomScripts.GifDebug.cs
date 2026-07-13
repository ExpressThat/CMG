namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string ShowGifDebugEvidence(
        string? action,
        int line,
        string? context,
        string? selector,
        ElementPoint? pointer,
        bool showScroll) =>
        $$"""
        (() => {
          document.querySelectorAll('[data-cmg-gif-debug]').forEach(node => node.remove());
          const add = (kind, css) => {
            const node = document.createElement('div');
            node.dataset.cmgGifDebug = kind;
            node.style.cssText = 'all:initial;position:fixed;z-index:2147483644;pointer-events:none;box-sizing:border-box;' + css;
            document.documentElement.appendChild(node);
            return node;
          };
          const selector = {{JsonString(selector ?? string.Empty)}};
          if (selector) {
            let target = null;
            try { target = document.querySelector(selector); } catch {}
            const rect = target?.getBoundingClientRect?.();
            if (rect && rect.width > 0 && rect.height > 0) {
              add('target', `left:${rect.left - 2}px;top:${rect.top - 2}px;width:${rect.width + 4}px;height:${rect.height + 4}px;border:2px dashed #22d3ee;border-radius:4px;box-shadow:0 0 0 1px #111;`);
            }
          }
          const rows = [];
          const action = {{JsonString(action ?? string.Empty)}};
          const context = {{JsonString(context ?? string.Empty)}};
          if (action) rows.push(`Action  ${action}  line {{line}}`);
          if (context) rows.push(`Scope   ${context}`);
          {{(pointer is null ? string.Empty : $"rows.push('Pointer {Invariant(Math.Round(pointer.X, 1))}, {Invariant(Math.Round(pointer.Y, 1))}');")}}
          if ({{showScroll.ToString().ToLowerInvariant()}}) rows.push(`Scroll  ${Math.round(scrollX)}, ${Math.round(scrollY)}`);
          if (rows.length) {
            const panel = add('panel', 'right:12px;top:12px;max-width:min(420px,calc(100vw - 24px));padding:9px 12px;background:rgba(8,15,28,.94);color:#e5f7ff;border:2px solid #22d3ee;border-radius:5px;font:600 12px/1.5 ui-monospace,monospace;white-space:pre-wrap;overflow-wrap:anywhere;box-shadow:0 6px 18px rgba(0,0,0,.35);');
            panel.textContent = rows.join('\n');
          }
          return true;
        })()
        """;

    public static string RemoveGifDebugEvidence() =>
        "(() => { document.querySelectorAll('[data-cmg-gif-debug]').forEach(node => node.remove()); return true; })()";
}
