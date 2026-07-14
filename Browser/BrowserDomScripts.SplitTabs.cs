namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string ShowGifSplitTabLabel(string prefix, bool active) =>
        $$"""
        (() => {
          document.getElementById('__cmg_split_tab_label')?.remove();
          const label = document.createElement('div');
          label.id = '__cmg_split_tab_label';
          label.setAttribute('popover', 'manual');
          label.style.cssText = 'all:initial;position:fixed;top:12px;left:12px;max-width:70vw;padding:7px 10px;border:2px solid #111827;border-radius:5px;background:{{(active ? "#fbbf24" : "#ffffff")}};color:#111827;box-shadow:0 2px 8px #0005;font:600 13px/1.2 system-ui;white-space:nowrap;overflow:hidden;text-overflow:ellipsis;z-index:2147483646;pointer-events:none;';
          label.textContent = {{JsonString(prefix)}} + (document.title ? ` | ${document.title}` : '');
          document.documentElement.appendChild(label);
          label.showPopover?.();
          return true;
        })()
        """;

    public static string RemoveGifSplitTabLabel() =>
        "(() => { const node=document.getElementById('__cmg_split_tab_label'); if(node?.matches?.(':popover-open')) node.hidePopover(); node?.remove(); return true; })()";
}
