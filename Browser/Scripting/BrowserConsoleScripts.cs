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

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";
}
