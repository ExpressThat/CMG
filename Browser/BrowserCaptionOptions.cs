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

public enum CaptionSize
{
    Normal,
    Large,
    ExtraLarge
}

public sealed record BrowserCaptionOptions(
    CaptionStyle Style = CaptionStyle.Subtle,
    CaptionPosition Position = CaptionPosition.Top,
    CaptionSeverity Severity = CaptionSeverity.Info,
    CaptionSize Size = CaptionSize.Normal,
    bool Markdown = false,
    bool AutoCaptions = false,
    string? CaptionTemplate = null)
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
        var size = action.Options.TryGetValue("captionSize", out var rawSize)
            ? ParseSize(rawSize, action.Name) : Size;
        var markdown = action.Options.TryGetValue("captionFormat", out var format)
            ? ParseFormat(format, action.Name) : Markdown;
        return new(style, position, severity, size, markdown, AutoCaptions, CaptionTemplate);
    }

    public static BrowserCaptionOptions FromOptions(IReadOnlyDictionary<string, string> options, string source) =>
        new(
            options.TryGetValue("captionStyle", out var style) || options.TryGetValue("style", out style)
                ? ParseStyle(style, source) : CaptionStyle.Subtle,
            options.TryGetValue("captionPosition", out var position) || options.TryGetValue("position", out position)
                ? ParsePosition(position, source) : CaptionPosition.Top,
            options.TryGetValue("captionSeverity", out var severity) || options.TryGetValue("severity", out severity)
                ? ParseSeverity(severity, source) : CaptionSeverity.Info,
            options.TryGetValue("captionSize", out var size) ? ParseSize(size, source) : CaptionSize.Normal,
            options.TryGetValue("captionFormat", out var format) && ParseFormat(format, source),
            options.TryGetValue("autoCaptions", out var automatic) && ParseBoolean(automatic, source, "autoCaptions"),
            options.GetValueOrDefault("captionTemplate"));

    public IReadOnlyDictionary<string, string> ToRecordingDefaults()
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            ["captionStyle"] = StyleValue(Style),
            ["captionPosition"] = PositionValue(Position),
            ["captionSeverity"] = SeverityValue(Severity),
            ["captionSize"] = SizeValue(Size),
            ["captionFormat"] = Markdown ? "markdown" : "plain",
            ["autoCaptions"] = AutoCaptions ? "true" : "false"
        };
        if (CaptionTemplate is not null) values["captionTemplate"] = CaptionTemplate;
        return values;
    }

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

    private static CaptionSize ParseSize(string value, string source) =>
        value.Trim().ToLowerInvariant() switch
        {
            "normal" or "default" => CaptionSize.Normal,
            "large" => CaptionSize.Large,
            "x-large" or "xlarge" or "extra-large" => CaptionSize.ExtraLarge,
            _ => throw new ScriptExecutionException($"{source} option captionSize= must be normal, large, or x-large.")
        };

    private static bool ParseFormat(string value, string source) => value.Trim().ToLowerInvariant() switch
    {
        "markdown" or "formatted" => true,
        "plain" or "text" => false,
        _ => throw new ScriptExecutionException($"{source} option captionFormat= must be markdown or plain.")
    };

    private static bool ParseBoolean(string value, string source, string name) => value.Trim().ToLowerInvariant() switch
    {
        "true" or "yes" or "on" or "1" => true,
        "false" or "no" or "off" or "0" => false,
        _ => throw new ScriptExecutionException($"{source} option {name}= must be true or false.")
    };

    public static string StyleValues => "subtle, teaching, qa, bug-report, compact";

    public static string PositionValues => "top, bottom, left, right, auto";

    public static string SeverityValues => "info, success, warning, error";

    public static string SizeValues => "normal, large, x-large";

    private static string StyleValue(CaptionStyle style) =>
        style is CaptionStyle.BugReport ? "bug-report" : style.ToString().ToLowerInvariant();

    private static string PositionValue(CaptionPosition position) =>
        position.ToString().ToLowerInvariant();

    private static string SeverityValue(CaptionSeverity severity) =>
        severity.ToString().ToLowerInvariant();

    private static string SizeValue(CaptionSize size) =>
        size is CaptionSize.ExtraLarge ? "x-large" : size.ToString().ToLowerInvariant();
}
