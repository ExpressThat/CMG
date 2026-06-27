namespace CMG.Browser;

public sealed partial class FirefoxBiDiClient
{
    private static readonly Dictionary<string, CoverageOptions> FirefoxCoverageOptions = [];
    private static readonly object FirefoxCoverageLock = new();

    public void StartCoverage(string remoteDebuggingUrl, CoverageOptions options)
    {
        lock (FirefoxCoverageLock)
        {
            FirefoxCoverageOptions[Key(remoteDebuggingUrl)] = options;
        }
    }

    public string StopCoverage(string remoteDebuggingUrl)
    {
        var options = TakeFirefoxCoverageOptions(remoteDebuggingUrl);
        if (!options.JavaScript && !options.Css)
        {
            return """{"js":[],"css":[]}""";
        }

        return Evaluate(remoteDebuggingUrl, BuildFirefoxCoverageExpression(options));
    }

    private static CoverageOptions TakeFirefoxCoverageOptions(string remoteDebuggingUrl)
    {
        lock (FirefoxCoverageLock)
        {
            var key = Key(remoteDebuggingUrl);
            var options = FirefoxCoverageOptions.GetValueOrDefault(key) ?? new CoverageOptions(true, true);
            FirefoxCoverageOptions.Remove(key);
            return options;
        }
    }

    private static string BuildFirefoxCoverageExpression(CoverageOptions options) =>
        $$"""
        (() => {
          const js = {{options.JavaScript.ToString().ToLowerInvariant()}} ? Array.from(document.scripts).map((script, index) => {
            const text = script.src ? '' : (script.textContent || '');
            return { scriptId: String(index + 1), url: script.src || location.href, text, functions: [], ranges: text ? [{ startOffset: 0, endOffset: text.length, count: 1 }] : [] };
          }) : [];
          const css = {{options.Css.ToString().ToLowerInvariant()}} ? Array.from(document.styleSheets).map((sheet, index) => {
            let ruleCount = 0;
            try { ruleCount = sheet.cssRules ? sheet.cssRules.length : 0; } catch { ruleCount = 0; }
            return { styleSheetId: String(index + 1), url: sheet.href || location.href, ranges: ruleCount ? [{ startOffset: 0, endOffset: ruleCount, count: 1 }] : [] };
          }) : [];
          return JSON.stringify({ js, css });
        })()
        """;
}
