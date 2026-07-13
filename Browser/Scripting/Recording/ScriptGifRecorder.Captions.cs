namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    public bool CaptureCaptionTimeline(BrowserScriptAction action)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended) return false;
        var hasTimeline = action.Options.ContainsKey("duration") || action.Options.ContainsKey("captionDuration") ||
            action.Options.ContainsKey("fadeIn") || action.Options.ContainsKey("fadeOut");
        if (!hasTimeline) return false;

        var duration = CaptionDuration(action);
        var fadeIn = CaptionTime(action, "fadeIn", 0);
        var fadeOut = CaptionTime(action, "fadeOut", 0);
        if (fadeIn > 0)
        {
            SetCaptionOpacity(0.35);
            CaptureHoldFrame(Math.Max(1, fadeIn / 2));
            SetCaptionOpacity(1);
            CaptureHoldFrame(Math.Max(1, fadeIn - fadeIn / 2));
        }
        else
        {
            SetCaptionOpacity(1);
        }

        CaptureHoldFrame(duration);
        if (fadeOut > 0)
        {
            SetCaptionOpacity(0.5);
            CaptureHoldFrame(Math.Max(1, fadeOut / 2));
            SetCaptionOpacity(0);
            CaptureHoldFrame(Math.Max(1, fadeOut - fadeOut / 2));
        }
        devToolsClient.Evaluate(remoteDebuggingUrl, BrowserDomScripts.RemoveMessageBar());
        return true;
    }

    public bool CaptureFailureCaption(BrowserScriptAction action, string reason)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended || IsFalse(action, "failureCaptions")) return false;
        var message = $"FAILED: {action.Name} (line {action.LineNumber})\n{Trim(reason, 220)}";
        devToolsClient.ShowMessageBar(remoteDebuggingUrl, message,
            new BrowserCaptionOptions(CaptionStyle.BugReport, CaptionPosition.Bottom, CaptionSeverity.Error));
        CaptureHoldFrame(Math.Min(700, options.HoldOnFailureMilliseconds));
        return true;
    }

    public void CaptureAssertionCaption(BrowserScriptAction action, IReadOnlyList<string> output)
    {
        if (remoteDebuggingUrl is null || IsCaptureSuspended || IsFalse(action, "assertionCaptions")) return;
        var expected = Sensitive(action) ? "[masked]" : ExpectedValue(action);
        var actual = output.LastOrDefault()?.StartsWith("EXPECT_EVAL ", StringComparison.Ordinal) is true
            ? output[^1].Split(' ').Last() : expected;
        var message = $"PASS: {action.Name}\nExpected: {Trim(expected, 100)}\nActual: {Trim(actual, 120)}";
        devToolsClient.ShowMessageBar(remoteDebuggingUrl, message,
            new BrowserCaptionOptions(CaptionStyle.Qa, CaptionPosition.Bottom, CaptionSeverity.Success));
    }

    private int CaptionDuration(BrowserScriptAction action)
    {
        var value = action.Options.GetValueOrDefault("captionDuration") ?? action.Options.GetValueOrDefault("duration");
        return value is null ? options.HoldAfterAssertionMilliseconds : ScriptPointerMotionOptions.ParseDuration(value, $"{action.Name} option duration=");
    }

    private static int CaptionTime(BrowserScriptAction action, string name, int fallback) =>
        action.Options.TryGetValue(name, out var value)
            ? ScriptPointerMotionOptions.ParseDuration(value, $"{action.Name} option {name}=") : fallback;

    private void SetCaptionOpacity(double opacity) =>
        devToolsClient.Evaluate(remoteDebuggingUrl!, BrowserDomScripts.SetMessageBarOpacity(opacity));

    private static bool IsFalse(BrowserScriptAction action, string name) =>
        action.Options.GetValueOrDefault(name)?.Equals("false", StringComparison.OrdinalIgnoreCase) is true;

    private static bool Sensitive(BrowserScriptAction action) =>
        action.Arguments.Any(value => value.Contains("password", StringComparison.OrdinalIgnoreCase) ||
            value.Contains("token", StringComparison.OrdinalIgnoreCase) || value.Contains("secret", StringComparison.OrdinalIgnoreCase));

    private static string ExpectedValue(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("equals", out var equals) || action.Options.TryGetValue("eq", out equals)) return equals;
        if (action.Options.TryGetValue("contains", out var contains)) return contains;
        if (action.Arguments.Count >= 2) return action.Arguments[^1];
        var name = action.Name.ToLowerInvariant();
        if (name.Contains("visible")) return name.Contains("notvisible") ? "not visible" : "visible";
        if (name.Contains("hidden")) return name.Contains("nothidden") ? "not hidden" : "hidden";
        if (name.Contains("enabled")) return name.Contains("notenabled") ? "not enabled" : "enabled";
        if (name.Contains("disabled")) return name.Contains("notdisabled") ? "not disabled" : "disabled";
        return name.Contains("eval") ? "truthy" : "pass";
    }

    private static string Trim(string value, int length) => value.Length <= length ? value : value[..length] + "...";
}
