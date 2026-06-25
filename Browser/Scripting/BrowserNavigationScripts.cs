namespace CMG.Browser.Scripting;

public static class BrowserNavigationScripts
{
    public static string History(string direction, int timeoutMilliseconds) =>
        $$"""
        new Promise((resolve, reject) => {
          const start = location.href;
          const deadline = Date.now() + {{timeoutMilliseconds}};
          const poll = () => {
            if (location.href !== start) {
              resolve(location.href);
              return;
            }

            if (Date.now() >= deadline) {
              reject(new Error('History {{direction}} did not change URL within {{timeoutMilliseconds}}ms. Last URL: ' + location.href));
              return;
            }

            setTimeout(poll, 50);
          };
          history.{{direction}}();
          poll();
        })
        """;

    public static string WaitForUrl(string expected, int timeoutMilliseconds) =>
        $$"""
        new Promise((resolve, reject) => {
          const expected = {{Quote(expected)}};
          const deadline = Date.now() + {{timeoutMilliseconds}};
          const poll = () => {
            if (location.href.includes(expected)) {
              resolve(location.href);
              return;
            }

            if (Date.now() >= deadline) {
              reject(new Error(`URL did not match ${expected} within {{timeoutMilliseconds}}ms. Last URL: ${location.href}`));
              return;
            }

            setTimeout(poll, 50);
          };
          poll();
        })
        """;

    public static string ExpectUrl(string expected) =>
        $"(() => {{ if (!location.href.includes({Quote(expected)})) throw new Error(`Expected URL to contain {expected}, got ${{location.href}}`); return location.href; }})()";

    public static string ExpectTitle(string expected) =>
        $"(() => {{ if (!document.title.includes({Quote(expected)})) throw new Error(`Expected title to contain {expected}, got ${{document.title}}`); return document.title; }})()";

    public static string WaitForLoadState(string state, int timeoutMilliseconds) =>
        $$"""
        new Promise((resolve, reject) => {
          const expected = {{Quote(state)}};
          const deadline = Date.now() + {{timeoutMilliseconds}};
          const isReady = () => expected === 'load'
            ? document.readyState === 'complete'
            : document.readyState === expected;
          const poll = () => {
            if (isReady()) {
              resolve(document.readyState);
              return;
            }

            if (Date.now() >= deadline) {
              reject(new Error(`Load state ${expected} was not reached within {{timeoutMilliseconds}}ms. Last state: ${document.readyState}`));
              return;
            }

            setTimeout(poll, 50);
          };
          poll();
        })
        """;

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
