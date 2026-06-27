using CMG.Browser;

namespace CMG.Browser.Scripting;

public static class BrowserWaitScripts
{
    public static string SelectorState(string selector) =>
        $$"""
        (() => {
          const element = {{BrowserDomScripts.Query(selector)}};
          if (!element) return JSON.stringify({ attached: false, visible: false });
          const rect = element.getBoundingClientRect();
          const style = getComputedStyle(element);
          const visible = rect.width > 0 && rect.height > 0 &&
            style.visibility !== 'hidden' && style.display !== 'none' &&
            Number(style.opacity || '1') !== 0;
          return JSON.stringify({ attached: true, visible });
        })()
        """;

    public static string Function(string expression, int timeoutMilliseconds) =>
        $$"""
        new Promise((resolve) => {
          const deadline = Date.now() + {{timeoutMilliseconds}};
          const poll = () => {
            let value;
            try {
              value = ({{expression}});
            } catch (error) {
              resolve(JSON.stringify({ success: false, error: error?.message || String(error) }));
              return;
            }

            if (value) {
              resolve(JSON.stringify({ success: true, value: String(value) }));
              return;
            }

            if (Date.now() >= deadline) {
              resolve(JSON.stringify({ success: false, error: 'waitForFunction did not become truthy within {{timeoutMilliseconds}}ms.' }));
              return;
            }

            setTimeout(poll, 50);
          };
          poll();
        })
        """;
}
