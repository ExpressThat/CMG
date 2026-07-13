namespace CMG.Browser.Scripting.Recording;

public enum GifDitherMode { Default, None, FloydSteinberg, Bayer, Atkinson, Sierra }

public enum GifPaletteMode { Default, Global, Local, Adaptive }

public sealed record GifEncodingOptions(
    GifDitherMode Dither = GifDitherMode.Default,
    GifPaletteMode Palette = GifPaletteMode.Default,
    int? Colors = null,
    string? KeepFramesDirectory = null,
    GifFramingOptions? Framing = null)
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
        return new(dither, palette, colors, keepFrames);
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
            Framing);
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
