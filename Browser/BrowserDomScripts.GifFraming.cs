namespace CMG.Browser;

public static partial class BrowserDomScripts
{
    public static string StabilizeGifTarget(string selector, int safeArea, int timeoutMilliseconds) =>
        $$"""
        (async () => {
          const element = window.__cmgQuery?.({{JsonString(selector)}}) ?? document.querySelector({{JsonString(selector)}});
          if (!element) return false;
          const safe = {{safeArea}};
          let rect = element.getBoundingClientRect();
          const cx = Math.max(0, Math.min(innerWidth - 1, rect.left + rect.width / 2));
          const cy = Math.max(0, Math.min(innerHeight - 1, rect.top + rect.height / 2));
          const blocker = document.elementsFromPoint(cx, cy).find(node => node !== element && !element.contains(node) && !node.contains(element) && ['fixed','sticky'].includes(getComputedStyle(node).position));
          if (blocker) {
            const blocked = blocker.getBoundingClientRect();
            if (blocked.bottom > rect.top && blocked.top < rect.bottom) scrollBy(0, rect.top - blocked.bottom - safe);
          }
          rect = element.getBoundingClientRect();
          const dx = rect.left < safe ? rect.left - safe : rect.right > innerWidth - safe ? rect.right - innerWidth + safe : 0;
          const dy = rect.top < safe ? rect.top - safe : rect.bottom > innerHeight - safe ? rect.bottom - innerHeight + safe : 0;
          if (dx || dy) scrollBy(dx, dy);
          if ({{timeoutMilliseconds}} <= 0) return true;
          const deadline = performance.now() + {{timeoutMilliseconds}};
          let previous = element.getBoundingClientRect();
          let stable = 0;
          while (performance.now() < deadline && stable < 2) {
            await new Promise(requestAnimationFrame);
            const current = element.getBoundingClientRect();
            const settled = Math.abs(current.x - previous.x) < .5 && Math.abs(current.y - previous.y) < .5 && Math.abs(current.width - previous.width) < .5 && Math.abs(current.height - previous.height) < .5;
            stable = settled ? stable + 1 : 0;
            previous = current;
          }
          return true;
        })()
        """;
}
