using CMG.Browser.Scripting;

namespace CMG.Browser;

public enum CaptionStyle
{
    Subtle,
    Teaching,
    Qa,
    BugReport,
    Compact
}

public enum CaptionPosition
{
    Top,
    Bottom,
    Left,
    Right,
    Auto
}

public enum CaptionSeverity
{
    Info,
    Success,
    Warning,
    Error
}

public sealed record BrowserCaptionOptions(
    CaptionStyle Style = CaptionStyle.Subtle,
    CaptionPosition Position = CaptionPosition.Top,
    CaptionSeverity Severity = CaptionSeverity.Info)
{
    public static readonly BrowserCaptionOptions Default = new();

    public BrowserCaptionOptions WithAction(BrowserScriptAction action)
    {
        var style = action.Options.TryGetValue("captionStyle", out var rawStyle) ||
            action.Options.TryGetValue("style", out rawStyle)
            ? ParseStyle(rawStyle, action.Name)
            : Style;
        var position = action.Options.TryGetValue("captionPosition", out var rawPosition) ||
            action.Options.TryGetValue("position", out rawPosition)
            ? ParsePosition(rawPosition, action.Name)
            : Position;
        var severity = action.Options.TryGetValue("captionSeverity", out var rawSeverity) ||
            action.Options.TryGetValue("severity", out rawSeverity)
            ? ParseSeverity(rawSeverity, action.Name)
            : Severity;
        return new(style, position, severity);
    }

    public static BrowserCaptionOptions FromOptions(IReadOnlyDictionary<string, string> options, string source) =>
        new(
            options.TryGetValue("captionStyle", out var style) || options.TryGetValue("style", out style)
                ? ParseStyle(style, source) : CaptionStyle.Subtle,
            options.TryGetValue("captionPosition", out var position) || options.TryGetValue("position", out position)
                ? ParsePosition(position, source) : CaptionPosition.Top,
            options.TryGetValue("captionSeverity", out var severity) || options.TryGetValue("severity", out severity)
                ? ParseSeverity(severity, source) : CaptionSeverity.Info);

    public IReadOnlyDictionary<string, string> ToRecordingDefaults() =>
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["captionStyle"] = StyleValue(Style),
            ["captionPosition"] = PositionValue(Position),
            ["captionSeverity"] = SeverityValue(Severity)
        };

    private static CaptionStyle ParseStyle(string value, string source) =>
        value.Trim().ToLowerInvariant() switch
        {
            "subtle" => CaptionStyle.Subtle,
            "teaching" or "teach" => CaptionStyle.Teaching,
            "qa" or "qa-evidence" or "evidence" => CaptionStyle.Qa,
            "bug" or "bug-report" or "bugreport" => CaptionStyle.BugReport,
            "compact" => CaptionStyle.Compact,
            _ => throw new ScriptExecutionException($"{source} option captionStyle= must be one of: subtle, teaching, qa, bug-report, compact.")
        };

    private static CaptionPosition ParsePosition(string value, string source) =>
        value.Trim().ToLowerInvariant() switch
        {
            "top" => CaptionPosition.Top,
            "bottom" => CaptionPosition.Bottom,
            "left" => CaptionPosition.Left,
            "right" => CaptionPosition.Right,
            "auto" => CaptionPosition.Auto,
            _ => throw new ScriptExecutionException($"{source} option captionPosition= must be one of: top, bottom, left, right, auto.")
        };

    private static CaptionSeverity ParseSeverity(string value, string source) =>
        value.Trim().ToLowerInvariant() switch
        {
            "info" => CaptionSeverity.Info,
            "success" or "pass" => CaptionSeverity.Success,
            "warning" or "warn" => CaptionSeverity.Warning,
            "error" or "fail" => CaptionSeverity.Error,
            _ => throw new ScriptExecutionException($"{source} option captionSeverity= must be one of: info, success, warning, error.")
        };

    public static string StyleValues => "subtle, teaching, qa, bug-report, compact";

    public static string PositionValues => "top, bottom, left, right, auto";

    public static string SeverityValues => "info, success, warning, error";

    private static string StyleValue(CaptionStyle style) =>
        style is CaptionStyle.BugReport ? "bug-report" : style.ToString().ToLowerInvariant();

    private static string PositionValue(CaptionPosition position) =>
        position.ToString().ToLowerInvariant();

    private static string SeverityValue(CaptionSeverity severity) =>
        severity.ToString().ToLowerInvariant();
}
