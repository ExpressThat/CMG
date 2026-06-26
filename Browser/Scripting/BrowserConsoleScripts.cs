namespace CMG.Browser.Scripting;

public static class BrowserConsoleScripts
{
    public static string Install() =>
        """
        (() => {
          if (window.__cmgConsoleInstalled) return true;
          window.__cmgConsoleInstalled = true;
          window.__cmgConsole = [];
          for (const level of ['log', 'info', 'warn', 'error']) {
            const original = console[level].bind(console);
            console[level] = (...args) => {
              window.__cmgConsole.push({ level, text: args.map(String).join(' ') });
              original(...args);
            };
          }
          return true;
        })()
        """;

    public static string WaitFor(string text, string level, int timeout) =>
        $$"""
        new Promise((resolve, reject) => {
          const expected = {{Quote(text)}};
          const level = {{Quote(level)}};
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            const entries = window.__cmgConsole || [];
            const match = entries.find(entry =>
              entry.text.includes(expected) && (!level || entry.level === level));
            if (match) { resolve(`${match.level}: ${match.text}`); return; }
            if (Date.now() >= deadline) {
              reject(new Error(`Console message '${expected}' was not seen within {{timeout}}ms.`));
              return;
            }
            setTimeout(poll, 50);
          };
          poll();
        })
        """;

    public static string ExpectNone(string text, string level, int timeout) =>
        $$"""
        new Promise((resolve, reject) => {
          const expected = {{Quote(text)}};
          const level = {{Quote(level)}};
          const matches = entry =>
            (!expected || entry.text.includes(expected)) &&
            (!level || entry.level === level);
          const failure = () => (window.__cmgConsole || []).find(matches);
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            const hit = failure();
            if (hit) {
              reject(new Error(`Unexpected console ${hit.level}: ${hit.text}`));
              return;
            }
            if (Date.now() >= deadline) { resolve('none'); return; }
            setTimeout(poll, 50);
          };
          poll();
        })
        """;

    public static string InstallPageErrors() =>
        """
        (() => {
          if (window.__cmgPageErrorsInstalled) return true;
          window.__cmgPageErrorsInstalled = true;
          window.__cmgPageErrors = [];
          window.addEventListener('error', event => {
            window.__cmgPageErrors.push({
              type: 'error',
              text: event.message || String(event.error || ''),
              source: event.filename || '',
              line: event.lineno || 0,
              column: event.colno || 0
            });
          });
          window.addEventListener('unhandledrejection', event => {
            window.__cmgPageErrors.push({
              type: 'unhandledrejection',
              text: String(event.reason?.message || event.reason || ''),
              source: '',
              line: 0,
              column: 0
            });
          });
          return true;
        })()
        """;

    public static string WaitForPageError(string text, int timeout) =>
        $$"""
        new Promise((resolve, reject) => {
          const expected = {{Quote(text)}};
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            const entries = window.__cmgPageErrors || [];
            const match = entries.find(entry => entry.text.includes(expected));
            if (match) {
              resolve(`${match.type}: ${match.text}`);
              return;
            }

            if (Date.now() >= deadline) {
              reject(new Error(`Page error '${expected}' was not seen within {{timeout}}ms.`));
              return;
            }

            setTimeout(poll, 50);
          };
          poll();
        })
        """;

    public static string ExpectNoPageError(string text, int timeout) =>
        $$"""
        new Promise((resolve, reject) => {
          const expected = {{Quote(text)}};
          const matches = entry => !expected || entry.text.includes(expected);
          const failure = () => (window.__cmgPageErrors || []).find(matches);
          const deadline = Date.now() + {{timeout}};
          const poll = () => {
            const hit = failure();
            if (hit) {
              reject(new Error(`Unexpected page ${hit.type}: ${hit.text}`));
              return;
            }

            if (Date.now() >= deadline) {
              resolve('none');
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
