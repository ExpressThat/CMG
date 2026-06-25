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
          let quietSince = 0;
          let lastNetworkCount = -1;
          const networkCount = () => (window.__cmgRequests?.length || 0) +
            (window.__cmgResponses?.length || 0) + (window.__cmgRequestFailures?.length || 0);
          const isReady = () => {
            if (expected === 'load') return document.readyState === 'complete';
            if (expected === 'networkidle') return document.readyState === 'complete' && quietSince > 0 && Date.now() - quietSince >= 500;
            return document.readyState === expected;
          };
          const poll = () => {
            const count = networkCount();
            if (count !== lastNetworkCount) {
              lastNetworkCount = count;
              quietSince = Date.now();
            }

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

    public static string WaitForNavigation(string expectedUrl, string waitUntil, int timeoutMilliseconds) =>
        $$"""
        new Promise((resolve, reject) => {
          const expectedUrl = {{Quote(expectedUrl)}};
          const waitUntil = {{Quote(waitUntil)}};
          const deadline = Date.now() + {{timeoutMilliseconds}};
          let quietSince = 0;
          let lastNetworkCount = -1;
          const networkCount = () => (window.__cmgRequests?.length || 0) +
            (window.__cmgResponses?.length || 0) + (window.__cmgRequestFailures?.length || 0);
          const urlReady = () => !expectedUrl || location.href.includes(expectedUrl);
          const stateReady = () => {
            if (waitUntil === 'commit') return true;
            if (waitUntil === 'domcontentloaded') return document.readyState !== 'loading';
            if (waitUntil === 'networkidle') return document.readyState === 'complete' && quietSince > 0 && Date.now() - quietSince >= 500;
            return document.readyState === 'complete';
          };
          const poll = () => {
            const count = networkCount();
            if (count !== lastNetworkCount) {
              lastNetworkCount = count;
              quietSince = Date.now();
            }

            if (urlReady() && stateReady()) {
              resolve(JSON.stringify({ url: location.href, state: document.readyState, waitUntil }));
              return;
            }

            if (Date.now() >= deadline) {
              reject(new Error(`Navigation did not reach ${waitUntil} within {{timeoutMilliseconds}}ms. Last URL: ${location.href}; state: ${document.readyState}`));
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
