namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string ShowGifWaitProgress(int elapsedMilliseconds, int totalMilliseconds) =>
        $$"""
        (() => {
          let panel = document.getElementById('__cmg_wait_progress');
          if (!panel) {
            panel = document.createElement('div');
            panel.id = '__cmg_wait_progress'; panel.setAttribute('popover','manual');
            panel.style.cssText = 'all:initial;position:fixed;left:50%;bottom:18px;transform:translateX(-50%);width:min(360px,calc(100vw - 32px));padding:10px 12px;background:#111827;color:#fff;border-radius:6px;box-shadow:0 10px 28px rgba(0,0,0,.24);font:600 13px/1.4 system-ui,sans-serif;z-index:2147483646;pointer-events:none;box-sizing:border-box';
            panel.innerHTML = '<div id="__cmg_wait_label"></div><div style="height:5px;margin-top:7px;background:#374151;border-radius:9px;overflow:hidden"><div id="__cmg_wait_fill" style="height:100%;background:#60a5fa"></div></div>';
            document.documentElement.appendChild(panel);
          }
          const elapsed={{elapsedMilliseconds}}, total={{totalMilliseconds}}, percent=Math.min(100,Math.round(elapsed/total*100));
          document.getElementById('__cmg_wait_label').textContent=`Waiting ${(total/1000).toFixed(total%1000?1:0)}s · ${percent}%`;
          document.getElementById('__cmg_wait_fill').style.width=percent+'%';
          if (typeof panel.showPopover==='function' && !panel.matches(':popover-open')) panel.showPopover();
          return true;
        })()
        """;

    public static string RemoveGifWaitProgress() =>
        "(() => { const node=document.getElementById('__cmg_wait_progress'); if(node?.matches?.(':popover-open'))node.hidePopover(); node?.remove(); return true; })()";
}
