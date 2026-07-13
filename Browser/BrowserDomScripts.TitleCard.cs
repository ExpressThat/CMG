namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string ShowTitleCard(string text, string kind) =>
        $$"""
        (() => {
          document.getElementById('__cmg_title_card')?.remove();
          const card = document.createElement('div');
          card.id = '__cmg_title_card';
          card.setAttribute('popover', 'manual');
          card.setAttribute('role', 'presentation');
          card.style.cssText = 'all:initial;position:fixed;inset:0;z-index:2147483647;margin:0;border:0;padding:48px;box-sizing:border-box;background:#111827;color:#fff;display:flex;flex-direction:column;align-items:center;justify-content:center;text-align:center;font-family:system-ui,-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;';
          const label = document.createElement('div');
          label.textContent = {{JsonString(kind.ToUpperInvariant())}};
          label.style.cssText = 'margin:0 0 18px;color:#93c5fd;font:700 12px/1.2 ui-monospace,SFMono-Regular,Menlo,Consolas,monospace;letter-spacing:2px;text-transform:uppercase;';
          const title = document.createElement('div');
          title.textContent = {{JsonString(text)}};
          title.style.cssText = 'max-width:900px;color:#fff;font:700 34px/1.25 system-ui,-apple-system,BlinkMacSystemFont,"Segoe UI",sans-serif;overflow-wrap:anywhere;';
          card.append(label, title);
          document.documentElement.appendChild(card);
          if (typeof card.showPopover === 'function') card.showPopover();
          return true;
        })()
        """;

    public static string RemoveTitleCard() =>
        "(() => { const card = document.getElementById('__cmg_title_card'); if (card?.matches?.(':popover-open')) card.hidePopover(); card?.remove(); return true; })()";
}
