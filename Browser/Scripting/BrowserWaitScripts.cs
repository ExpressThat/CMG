namespace CMG.Browser.Scripting;

public static class BrowserWaitScripts
{
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
