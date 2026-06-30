using CMG.Browser.Scripting;

namespace CMG.Browser;

public enum PointerTheme
{
    Arrow,
    Hand,
    Dot,
    Ring,
    Branded,
    Touch
}

public enum PointerShadow
{
    None,
    Light,
    Medium,
    Strong
}

public sealed record PointerVisualOptions(
    PointerTheme Theme = PointerTheme.Arrow,
    string? Color = null,
    int? SizePixels = null,
    PointerShadow Shadow = PointerShadow.Medium)
{
    public static readonly PointerVisualOptions Default = new();

    public PointerVisualOptions WithAction(BrowserScriptAction action, bool touch)
    {
        var theme = ThemeFor(action, touch);
        var color = action.Options.TryGetValue("pointerColor", out var rawColor)
            ? ParseColor(rawColor, action.Name)
            : Color;
        var size = action.Options.TryGetValue("pointerSize", out var rawSize)
            ? ParseSize(rawSize, action.Name)
            : SizePixels;
        var shadow = action.Options.TryGetValue("pointerShadow", out var rawShadow)
            ? ParseShadow(rawShadow, action.Name)
            : Shadow;
        return new(theme, color, size, shadow);
    }

    public static PointerVisualOptions FromOptions(IReadOnlyDictionary<string, string> options, string source) =>
        new(
            options.TryGetValue("pointerTheme", out var theme) ? ParseTheme(theme, source) : PointerTheme.Arrow,
            options.TryGetValue("pointerColor", out var color) ? ParseColor(color, source) : null,
            options.TryGetValue("pointerSize", out var size) ? ParseSize(size, source) : null,
            options.TryGetValue("pointerShadow", out var shadow) ? ParseShadow(shadow, source) : PointerShadow.Medium);

    private PointerTheme ThemeFor(BrowserScriptAction action, bool touch)
    {
        if (action.Options.TryGetValue("pointerTheme", out var value))
        {
            return ParseTheme(value, action.Name);
        }

        return touch && Theme is PointerTheme.Arrow ? PointerTheme.Touch : Theme;
    }

    private static PointerTheme ParseTheme(string value, string source) =>
        value.Trim().ToLowerInvariant() switch
        {
            "arrow" or "system" or "system-arrow" => PointerTheme.Arrow,
            "hand" => PointerTheme.Hand,
            "dot" => PointerTheme.Dot,
            "ring" => PointerTheme.Ring,
            "brand" or "branded" => PointerTheme.Branded,
            "touch" => PointerTheme.Touch,
            _ => throw new ScriptExecutionException($"{source} option pointerTheme= must be one of: arrow, hand, dot, ring, branded, touch.")
        };

    private static string ParseColor(string value, string source)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new ScriptExecutionException($"{source} option pointerColor= cannot be empty.");
        }

        if (value.IndexOfAny([';', '{', '}', '"', '\'', '`', '<', '>']) >= 0)
        {
            throw new ScriptExecutionException($"{source} option pointerColor= must be a CSS color value without declarations.");
        }

        return value.Trim();
    }

    private static int? ParseSize(string value, string source)
    {
        if (string.IsNullOrWhiteSpace(value) || value.Equals("auto", StringComparison.OrdinalIgnoreCase))
        {
            return null;
        }

        return int.TryParse(value, out var size) && size is >= 8 and <= 96
            ? size
            : throw new ScriptExecutionException($"{source} option pointerSize= must be auto or a number from 8 to 96.");
    }

    private static PointerShadow ParseShadow(string value, string source) =>
        value.Trim().ToLowerInvariant() switch
        {
            "none" or "off" or "false" => PointerShadow.None,
            "light" => PointerShadow.Light,
            "medium" or "normal" => PointerShadow.Medium,
            "strong" or "heavy" => PointerShadow.Strong,
            _ => throw new ScriptExecutionException($"{source} option pointerShadow= must be one of: none, light, medium, strong.")
        };

    public static string ThemeValues => "arrow, hand, dot, ring, branded, touch";

    public static string ShadowValues => "none, light, medium, strong";
}
