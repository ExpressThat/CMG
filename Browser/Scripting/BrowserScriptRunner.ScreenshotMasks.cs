using CMG.Runner;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static bool HasScreenshotMask(BrowserScriptAction action) =>
        action.Options.TryGetValue("mask", out var mask) && !string.IsNullOrWhiteSpace(mask);

    private static IReadOnlyList<string> AddTemporaryScreenshotMasks(
        BrowserScriptAction action,
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient)
    {
        if (!HasScreenshotMask(action))
        {
            return [];
        }

        var color = action.Options.TryGetValue("maskColor", out var rawColor) && !string.IsNullOrWhiteSpace(rawColor)
            ? rawColor
            : "#ff00ff";
        var ids = new List<string>();
        foreach (var locator in action.Options["mask"].Split(';', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            foreach (var expression in CmgLocator.PrefixExpressions(locator, action.LineNumber))
            {
                automationClient.Evaluate(remoteDebuggingUrl, expression);
            }

            var selector = CmgLocator.Resolve(locator, action.LineNumber).Selector;
            var id = $"cmg-mask-{Guid.NewGuid():N}";
            automationClient.Evaluate(remoteDebuggingUrl, BuildScreenshotMaskExpression(selector, color, id, locator));
            ids.Add(id);
        }

        return ids;
    }

    private static string BuildScreenshotMaskExpression(string selector, string color, string id, string locator) =>
        "(() => { " +
        $"const element = window.__cmgQuery?.({QuoteScriptString(selector)}) ?? document.querySelector({QuoteScriptString(selector)}); " +
        $"if (!element) throw new Error({QuoteScriptString($"No element matched screenshot mask selector {locator}")}); " +
        "const rect = element.getBoundingClientRect(); " +
        "const overlay = document.createElement('div'); " +
        $"overlay.setAttribute('data-cmg-screenshot-mask', {QuoteScriptString(id)}); " +
        "Object.assign(overlay.style, { position: 'absolute', " +
        "left: `${rect.left + window.scrollX}px`, top: `${rect.top + window.scrollY}px`, " +
        "width: `${rect.width}px`, height: `${rect.height}px`, " +
        $"background: {QuoteScriptString(color)}, zIndex: '2147483647', pointerEvents: 'none' }}); " +
        "document.documentElement.appendChild(overlay); return true; })()";

    private static void RemoveTemporaryScreenshotArtifacts(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string? styleId,
        IReadOnlyList<string> maskIds)
    {
        if (styleId is not null)
        {
            automationClient.Evaluate(remoteDebuggingUrl, $"document.querySelector('[data-cmg-screenshot-style=\"{styleId}\"]')?.remove(); true");
        }

        if (maskIds.Count > 0)
        {
            var selector = string.Join(",", maskIds.Select(id => $"[data-cmg-screenshot-mask=\"{id}\"]"));
            automationClient.Evaluate(remoteDebuggingUrl, $"document.querySelectorAll('{selector}').forEach(e => e.remove()); true");
        }
    }

}
