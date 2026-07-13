using SixLabors.ImageSharp;

namespace CMG.Browser.Scripting.Recording;

public enum GifGradientMode { Default, Smooth, Text }

public sealed record GifColorOptions(
    string? Background = null,
    GifGradientMode GradientMode = GifGradientMode.Default,
    bool HighContrastPalette = false)
{
    public GifColorOptions WithOptions(IReadOnlyDictionary<string, string> options, string source)
    {
        return this with
        {
            Background = options.TryGetValue("background", out var background) ? ParseBackground(background, source) : Background,
            GradientMode = options.TryGetValue("gradientMode", out var gradient) ? ParseGradientMode(gradient, source) : GradientMode,
            HighContrastPalette = ParseBoolean(options, "highContrastPalette", HighContrastPalette, source)
        };
    }

    public static GifColorOptions FromOptions(IReadOnlyDictionary<string, string> options, string source) =>
        new GifColorOptions().WithOptions(options, source);

    public static bool TryParse(
        string? background,
        string? gradientMode,
        bool highContrastPalette,
        out GifColorOptions options,
        out string? error)
    {
        var values = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        if (background is not null) values["background"] = background;
        if (gradientMode is not null) values["gradientMode"] = gradientMode;
        if (highContrastPalette) values["highContrastPalette"] = "true";
        try { options = FromOptions(values, "GIF"); error = null; return true; }
        catch (ScriptExecutionException exception) { options = new(); error = exception.Message; return false; }
    }

    public Color? ParsedBackground => Background is null ? null : Color.Parse(Background);

    public static string GradientModeValues => "smooth, text";

    private static string? ParseBackground(string value, string source)
    {
        var trimmed = value.Trim();
        if (trimmed.Equals("transparent", StringComparison.OrdinalIgnoreCase) ||
            trimmed.Equals("none", StringComparison.OrdinalIgnoreCase)) return null;
        if (!Color.TryParse(trimmed, out _))
            throw new ScriptExecutionException($"{source} option background= must be a named, hex, rgb, rgba, hsl, or hsla color.");
        return trimmed;
    }

    private static GifGradientMode ParseGradientMode(string value, string source) => value.Trim().ToLowerInvariant() switch
    {
        "smooth" => GifGradientMode.Smooth,
        "text" or "crisp" => GifGradientMode.Text,
        _ => throw new ScriptExecutionException($"{source} option gradientMode= must be one of: {GradientModeValues}.")
    };

    private static bool ParseBoolean(
        IReadOnlyDictionary<string, string> options,
        string name,
        bool fallback,
        string source) => !options.TryGetValue(name, out var value) ? fallback : value.Trim().ToLowerInvariant() switch
        {
            "true" or "yes" or "on" or "1" => true,
            "false" or "no" or "off" or "0" => false,
            _ => throw new ScriptExecutionException($"{source} option {name}= must be true or false.")
        };
}
