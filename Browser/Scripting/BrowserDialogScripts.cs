namespace CMG.Browser.Scripting;

public static class BrowserDialogScripts
{
    public static string Install(string behavior, string promptText) =>
        $$"""
        (() => {
          window.__cmgDialogs = window.__cmgDialogs || [];
          window.__cmgDialogBehavior = {{Quote(behavior)}};
          window.__cmgDialogPromptText = {{Quote(promptText)}};
          window.alert = message => {
            window.__cmgDialogs.push({ type: 'alert', message: String(message ?? ''), accepted: true });
          };
          window.confirm = message => {
            const accepted = window.__cmgDialogBehavior !== 'dismiss';
            window.__cmgDialogs.push({ type: 'confirm', message: String(message ?? ''), accepted });
            return accepted;
          };
          window.prompt = (message, defaultValue = '') => {
            const accepted = window.__cmgDialogBehavior !== 'dismiss';
            const value = accepted ? (window.__cmgDialogPromptText || defaultValue || '') : null;
            window.__cmgDialogs.push({ type: 'prompt', message: String(message ?? ''), accepted, value });
            return value;
          };
          return true;
        })()
        """;

    public static string WaitForDialog(string pattern, int timeoutMilliseconds, string matchMode = "contains", bool ignoreCase = false) =>
        $$"""
        new Promise(resolve => {
          const pattern = {{Quote(pattern)}};
          const matchMode = {{Quote(matchMode)}};
          const ignoreCase = {{ignoreCase.ToString().ToLowerInvariant()}};
          const matchesText = {{TextMatcherScript()}};
          const deadline = Date.now() + {{timeoutMilliseconds}};
          const poll = () => {
            const hit = (window.__cmgDialogs || []).find(dialog => matchesText(dialog.message, pattern, matchMode, ignoreCase));
            if (hit) {
              resolve(JSON.stringify({ success: true, value: hit }));
              return;
            }

            if (Date.now() >= deadline) {
              resolve(JSON.stringify({ success: false, error: `Timed out waiting for dialog ${pattern}` }));
              return;
            }

            setTimeout(poll, 50);
          };
          poll();
        })
        """;

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string TextMatcherScript() =>
        """
        (value, pattern, mode, ignoreCase) => {
          if (!pattern) return true;
          const actual = ignoreCase ? String(value || '').toLowerCase() : String(value || '');
          const expected = ignoreCase ? String(pattern || '').toLowerCase() : String(pattern || '');
          if (mode === 'exact') return actual === expected;
          if (mode === 'regex') return new RegExp(String(pattern || ''), ignoreCase ? 'i' : '').test(String(value || ''));
          return actual.includes(expected);
        }
        """;
}
