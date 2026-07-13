namespace CMG.Browser.Scripting.Recording;

public sealed record GifFramingOptions(
    string? CropSelector = null,
    int CropPadding = 0,
    double Scale = 1d,
    int? MaxWidth = null,
    int? MaxHeight = null,
    int? ViewportWidth = null,
    int? ViewportHeight = null,
    double PixelRatio = 1d)
{
    public static GifFramingOptions FromOptions(IReadOnlyDictionary<string, string> options, string context)
    {
        var crop = options.GetValueOrDefault("crop");
        var padding = ParseInt(options.GetValueOrDefault("cropPadding"), 0, 0, 2000, context, "cropPadding");
        var scale = ParseScale(options.GetValueOrDefault("scale"), context);
        var maxWidth = ParseNullableInt(options.GetValueOrDefault("maxWidth"), 1, 10000, context, "maxWidth");
        var maxHeight = ParseNullableInt(options.GetValueOrDefault("maxHeight"), 1, 10000, context, "maxHeight");
        var (viewportWidth, viewportHeight) = ParseViewport(options.GetValueOrDefault("viewport"), context);
        var pixelRatio = ParsePixelRatio(options.GetValueOrDefault("pixelRatio"), context);
        if (crop is null && options.ContainsKey("cropPadding"))
            throw new ScriptExecutionException($"{context} option cropPadding= requires crop=.");
        return new(string.IsNullOrWhiteSpace(crop) ? null : crop, padding, scale, maxWidth, maxHeight,
            viewportWidth, viewportHeight, pixelRatio);
    }

    public GifFramingOptions WithOptions(IReadOnlyDictionary<string, string> options, string context)
    {
        var parsed = FromOptions(options, context);
        return new(
            options.ContainsKey("crop") ? parsed.CropSelector : CropSelector,
            options.ContainsKey("cropPadding") ? parsed.CropPadding : CropPadding,
            options.ContainsKey("scale") ? parsed.Scale : Scale,
            options.ContainsKey("maxWidth") ? parsed.MaxWidth : MaxWidth,
            options.ContainsKey("maxHeight") ? parsed.MaxHeight : MaxHeight,
            options.ContainsKey("viewport") ? parsed.ViewportWidth : ViewportWidth,
            options.ContainsKey("viewport") ? parsed.ViewportHeight : ViewportHeight,
            options.ContainsKey("pixelRatio") ? parsed.PixelRatio : PixelRatio);
    }

    public static bool TryParse(
        string? crop,
        int? cropPadding,
        double? scale,
        int? maxWidth,
        int? maxHeight,
        string? viewport,
        double? pixelRatio,
        out GifFramingOptions framing,
        out string? error)
    {
        var options = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        Add(options, "crop", crop);
        Add(options, "cropPadding", cropPadding);
        Add(options, "scale", scale);
        Add(options, "maxWidth", maxWidth);
        Add(options, "maxHeight", maxHeight);
        Add(options, "viewport", viewport);
        Add(options, "pixelRatio", pixelRatio);
        try { framing = FromOptions(options, "GIF"); error = null; return true; }
        catch (ScriptExecutionException exception) { framing = new(); error = exception.Message; return false; }
    }

    private static double ParseScale(string? value, string context)
    {
        if (value is null) return 1d;
        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var scale) && scale is >= 0.05 and <= 1
            ? scale
            : throw new ScriptExecutionException($"{context} option scale= must be a number from 0.05 to 1.");
    }

    private static double ParsePixelRatio(string? value, string context)
    {
        if (value is null) return 1d;
        return double.TryParse(value, System.Globalization.NumberStyles.Float, System.Globalization.CultureInfo.InvariantCulture, out var ratio) && ratio is >= 1 and <= 4
            ? ratio : throw new ScriptExecutionException($"{context} option pixelRatio= must be a number from 1 to 4.");
    }

    private static (int? Width, int? Height) ParseViewport(string? value, string context)
    {
        if (value is null) return (null, null);
        var parts = value.ToLowerInvariant().Split('x', StringSplitOptions.TrimEntries);
        if (parts.Length == 2 && int.TryParse(parts[0], out var width) && int.TryParse(parts[1], out var height) &&
            width is >= 100 and <= 10000 && height is >= 100 and <= 10000) return (width, height);
        throw new ScriptExecutionException($"{context} option viewport= must use <width>x<height> with dimensions from 100 to 10000.");
    }

    private static int ParseInt(string? value, int fallback, int min, int max, string context, string name) =>
        value is null ? fallback : int.TryParse(value, out var parsed) && parsed >= min && parsed <= max
            ? parsed : throw new ScriptExecutionException($"{context} option {name}= must be an integer from {min} to {max}.");

    private static int? ParseNullableInt(string? value, int min, int max, string context, string name) =>
        value is null ? null : ParseInt(value, min, min, max, context, name);

    private static void Add(IDictionary<string, string> options, string name, object? value)
    {
        if (value is not null) options[name] = Convert.ToString(value, System.Globalization.CultureInfo.InvariantCulture)!;
    }
}
