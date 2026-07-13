namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string AutoPositionMessageBar(string selector) =>
        $$"""
        (() => {
          const bar = document.getElementById('__cmg_message_bar');
          const target = {{Query(selector)}};
          if (!bar || !target) return false;
          const rect = target.getBoundingClientRect();
          const placeAtBottom = rect.top + (rect.height / 2) < window.innerHeight / 2;
          bar.style.left = '50%';
          bar.style.right = 'auto';
          bar.style.transform = 'translateX(-50%)';
          bar.style.top = placeAtBottom ? 'auto' : '12px';
          bar.style.bottom = placeAtBottom ? '16px' : 'auto';
          return placeAtBottom ? 'bottom' : 'top';
        })()
        """;
}
