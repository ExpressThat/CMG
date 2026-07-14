namespace CMG.Browser.Scripting.Recording;

public enum GifDitherMode { Default, None, FloydSteinberg, Bayer, Atkinson, Sierra }

public enum GifPaletteMode { Default, Global, Local, Adaptive }

public sealed record GifEncodingOptions(
    GifDitherMode Dither = GifDitherMode.Default,
    GifPaletteMode Palette = GifPaletteMode.Default,
    int? Colors = null,
    string? KeepFramesDirectory = null,
    GifFramingOptions? Framing = null,
    GifDebugOptions? Diagnostics = null,
    GifAccessibilityOptions? Accessibility = null,
    GifEventCaptionOptions? EventCaptions = null,
    GifTitleCardOptions? TitleCards = null,
    GifCaptureOptimizationOptions? CaptureOptimization = null,
    GifPointerEvidenceOptions? PointerEvidence = null,
    GifColorOptions? Color = null,
    GifRedactionOptions? Redaction = null,
    GifSizeBudgetOptions? SizeBudget = null)
{
    public static GifEncodingOptions FromOptions(
        IReadOnlyDictionary<string, string> options,
        string context,
        string outputPath)
    {
        var dither = ParseDither(options.GetValueOrDefault("dither"), context);
        var palette = ParsePalette(options.GetValueOrDefault("palette"), context);
        var colors = ParseColors(options.GetValueOrDefault("colors"), context);
        var keepFrames = ParseKeepFrames(options.GetValueOrDefault("keepFrames"), context, outputPath);
        return new(dither, palette, colors, keepFrames,
            Diagnostics: GifDebugOptions.FromOptions(options, context),
            Accessibility: GifAccessibilityOptions.FromOptions(options, context),
            EventCaptions: GifEventCaptionOptions.FromOptions(options, context),
            TitleCards: new GifTitleCardOptions().WithOptions(options, context),
            CaptureOptimization: new GifCaptureOptimizationOptions().WithOptions(options, context),
            PointerEvidence: GifPointerEvidenceOptions.FromOptions(options, context),
            Color: GifColorOptions.FromOptions(options, context),
            Redaction: GifRedactionOptions.FromOptions(options, context),
            SizeBudget: GifSizeBudgetOptions.FromOptions(options, context));
    }

    public GifEncodingOptions WithOptions(
        IReadOnlyDictionary<string, string> options,
        string context,
        string outputPath)
    {
        var parsed = FromOptions(options, context, outputPath);
        return new(
            options.ContainsKey("dither") ? parsed.Dither : Dither,
            options.ContainsKey("palette") ? parsed.Palette : Palette,
            options.ContainsKey("colors") ? parsed.Colors : Colors,
            options.ContainsKey("keepFrames") ? parsed.KeepFramesDirectory : KeepFramesDirectory,
            Framing,
            GifDebugOptions.FromOptions(options, context, Diagnostics),
            MergeAccessibility(options, context),
            (EventCaptions ?? new()).WithOptions(options, context),
            (TitleCards ?? new()).WithOptions(options, context),
            (CaptureOptimization ?? new()).WithOptions(options, context),
            (PointerEvidence ?? new()).WithOptions(options, context),
            (Color ?? new()).WithOptions(options, context),
            Redaction,
            GifSizeBudgetOptions.FromOptions(options, context, SizeBudget));
    }

    private GifAccessibilityOptions MergeAccessibility(IReadOnlyDictionary<string, string> options, string context)
    {
        var parsed = GifAccessibilityOptions.FromOptions(options, context);
        var defaults = Accessibility ?? new();
        if (options.ContainsKey("accessibilityEvidence")) return parsed;
        return parsed with
        {
            ShowKeystrokes = options.ContainsKey("showKeystrokes") ? parsed.ShowKeystrokes : defaults.ShowKeystrokes,
            FocusEvidence = options.ContainsKey("focusEvidence") ? parsed.FocusEvidence : defaults.FocusEvidence,
            AccessibleNames = options.ContainsKey("accessibleNames") ? parsed.AccessibleNames : defaults.AccessibleNames,
            HighContrast = options.ContainsKey("highContrast") ? parsed.HighContrast : defaults.HighContrast,
            ContrastWarnings = options.ContainsKey("contrastWarnings") ? parsed.ContrastWarnings : defaults.ContrastWarnings,
            ShowMouseButtons = options.ContainsKey("showMouseButtons") ? parsed.ShowMouseButtons : defaults.ShowMouseButtons
        };
    }

    public GifEncodingOptions ForOutput(string outputPath, bool isolate)
    {
        if (!isolate || KeepFramesDirectory is null) return this;
        var name = Path.GetFileNameWithoutExtension(outputPath);
        return this with { KeepFramesDirectory = Path.Combine(KeepFramesDirectory, name) };
    }

    public static string DitherValues => "none, floyd-steinberg, bayer, atkinson, sierra";
    public static string PaletteValues => "global, local, adaptive";

    public static bool TryParse(
        string? ditherValue,
        string? paletteValue,
        int? colors,
        string? keepFramesDirectory,
        out GifEncodingOptions encoding,
        out string? error)
    {
        encoding = new();
        error = null;
        try
        {
            var dither = ParseDither(ditherValue, "GIF");
            var palette = ParsePalette(paletteValue, "GIF");
            if (colors is < 2 or > 256)
                throw new ScriptExecutionException("GIF option colors= must be an integer from 2 to 256.");
            encoding = new(dither, palette, colors, keepFramesDirectory);
            return true;
        }
        catch (ScriptExecutionException exception)
        {
            error = exception.Message;
            return false;
        }
    }

    private static GifDitherMode ParseDither(string? value, string context) => value?.ToLowerInvariant() switch
    {
        null => GifDitherMode.Default,
        "none" => GifDitherMode.None,
        "floyd-steinberg" or "floydsteinberg" => GifDitherMode.FloydSteinberg,
        "bayer" => GifDitherMode.Bayer,
        "atkinson" => GifDitherMode.Atkinson,
        "sierra" => GifDitherMode.Sierra,
        _ => throw new ScriptExecutionException($"{context} option dither= must be one of: {DitherValues}.")
    };

    private static GifPaletteMode ParsePalette(string? value, string context) => value?.ToLowerInvariant() switch
    {
        null => GifPaletteMode.Default,
        "global" => GifPaletteMode.Global,
        "local" => GifPaletteMode.Local,
        "adaptive" => GifPaletteMode.Adaptive,
        _ => throw new ScriptExecutionException($"{context} option palette= must be one of: {PaletteValues}.")
    };

    private static int? ParseColors(string? value, string context)
    {
        if (value is null) return null;
        return int.TryParse(value, out var colors) && colors is >= 2 and <= 256
            ? colors
            : throw new ScriptExecutionException($"{context} option colors= must be an integer from 2 to 256.");
    }

    private static string? ParseKeepFrames(string? value, string context, string outputPath)
    {
        if (value is null || value.Equals("false", StringComparison.OrdinalIgnoreCase)) return null;
        if (value.Equals("true", StringComparison.OrdinalIgnoreCase))
        {
            var fullPath = Path.GetFullPath(outputPath);
            return Path.Combine(Path.GetDirectoryName(fullPath)!, $"{Path.GetFileNameWithoutExtension(fullPath)}.frames");
        }
        if (string.IsNullOrWhiteSpace(value))
            throw new ScriptExecutionException($"{context} option keepFrames= must be true, false, or a directory.");
        return Path.GetFullPath(value);
    }
}
