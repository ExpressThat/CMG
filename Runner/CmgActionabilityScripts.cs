namespace CMG.Runner;

public static class CmgActionabilityScripts
{
    public static string WaitForActionable(string selector, CmgNode action)
    {
        var timeout = action.Options.TryGetValue("timeout", out var timeoutValue) ? timeoutValue : "5000";
        return $"evaluate \"{Escape(BuildScript(selector, timeout))}\"";
    }

    private static string BuildScript(string selector, string timeout) =>
        $$"""
        new Promise((resolve, reject) => {
          const deadline = Date.now() + {{timeout}};
          let last = null;
          const check = () => {
            const element = document.querySelector({{QuoteJs(selector)}});
            if (!element) {
              if (Date.now() > deadline) reject(new Error('No element matched selector {{EscapeMessage(selector)}}'));
              else setTimeout(check, 50);
              return;
            }
            const rect = element.getBoundingClientRect();
            const style = getComputedStyle(element);
            const visible = rect.width > 0 && rect.height > 0 && style.visibility !== 'hidden' && style.display !== 'none';
            const enabled = !element.matches(':disabled,[aria-disabled="true"]');
            const stable = last && last.x === rect.x && last.y === rect.y && last.width === rect.width && last.height === rect.height;
            last = { x: rect.x, y: rect.y, width: rect.width, height: rect.height };
            if (visible && enabled && stable) { resolve(true); return; }
            if (Date.now() > deadline) reject(new Error('Element {{EscapeMessage(selector)}} was not actionable before timeout'));
            else setTimeout(check, 50);
          };
          check();
        })
        """;

    private static string QuoteJs(string value) =>
        $"'{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("'", "\\'", StringComparison.Ordinal)}'";

    private static string EscapeMessage(string value) => value.Replace("'", "\\'", StringComparison.Ordinal);

    private static string Escape(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal);
}
