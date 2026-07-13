namespace CMG.Browser.Scripting.Recording;

public sealed record GifAccessibilityOptions(
    bool ShowKeystrokes = false,
    bool FocusEvidence = false,
    bool AccessibleNames = false,
    bool HighContrast = false,
    bool ContrastWarnings = false,
    bool ShowMouseButtons = false)
{
    public static GifAccessibilityOptions FromOptions(
        IReadOnlyDictionary<string, string> options,
        string source)
    {
        var preset = ParseBoolean(options.GetValueOrDefault("accessibilityEvidence"), false, "accessibilityEvidence", source);
        return new GifAccessibilityOptions(
            ParseBoolean(options.GetValueOrDefault("showKeystrokes"), preset, "showKeystrokes", source),
            ParseBoolean(options.GetValueOrDefault("focusEvidence"), preset, "focusEvidence", source),
            ParseBoolean(options.GetValueOrDefault("accessibleNames"), preset, "accessibleNames", source),
            ParseBoolean(options.GetValueOrDefault("highContrast"), preset, "highContrast", source),
            ParseBoolean(options.GetValueOrDefault("contrastWarnings"), preset, "contrastWarnings", source),
            ParseBoolean(options.GetValueOrDefault("showMouseButtons"), false, "showMouseButtons", source));
    }

    private static bool ParseBoolean(string? value, bool fallback, string option, string source) =>
        value?.Trim().ToLowerInvariant() switch
        {
            null => fallback,
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{source} {option}= must be true or false.")
        };
}
