using System.Text.Json;

namespace CMG.Commands;

internal static partial class RunConfigReader
{
    private static RunGifSettings GifSettings(JsonElement root)
    {
        if (!root.TryGetProperty("gifSettings", out var value)) return RunGifSettings.Empty;
        if (value.ValueKind is not JsonValueKind.Object)
            throw new RunConfigException("Run config option 'gifSettings' must be an object.");
        foreach (var property in value.EnumerateObject())
            if (!GifSettingNames.Contains(property.Name))
                throw new RunConfigException($"Run config option 'gifSettings.{property.Name}' is not supported.");
        return new RunGifSettings(
            StringOption(value, "quality"),
            IntOption(value, "pointerDuration"),
            StringOption(value, "pointerSpeed"),
            StringOption(value, "pointerEasing"),
            StringOption(value, "pointerPath"),
            StringOption(value, "dragPath"),
            StringOption(value, "clickPulse"),
            IntOption(value, "fps"),
            IntOption(value, "frameDelay"),
            IntOption(value, "typingDelay"),
            IntOption(value, "postHoverHold"),
            StringOption(value, "crop"),
            IntOption(value, "cropPadding"),
            StringOption(value, "smartCrop"),
            StringOption(value, "splitTabs"),
            DoubleOption(value, "scale"),
            IntOption(value, "maxWidth"),
            IntOption(value, "maxHeight"),
            StringOption(value, "viewport"),
            DoubleOption(value, "pixelRatio"),
            IntOption(value, "safeArea"),
            IntOption(value, "layoutStability"),
            StringOption(value, "targetZoom"),
            IntOption(value, "targetZoomThreshold"),
            StringOption(value, "pagePosition"),
            StringOption(value, "tabContext"),
            StringOption(value, "captionStyle"),
            StringOption(value, "captionPosition"),
            StringOption(value, "captionSeverity"),
            StringOption(value, "captionSize"),
            BoolOption(value, "autoCaptions"),
            StringOption(value, "captionTemplate"),
            StringArrayOption(value, "redact"),
            StringArrayOption(value, "mask"),
            StringArrayOption(value, "blur"),
            StringOption(value, "autoRedact"),
            StringOption(value, "redactionSafety"),
            StringOption(value, "sizeBudget"),
            BoolOption(value, "budgetQualityFallback"),
            BoolOption(value, "budgetDownscaleFallback"),
            StringOption(value, "narrationSidecar"),
            StringOption(value, "stillPdf"),
            StringOption(value, "altText"),
            StringOption(value, "description"),
            StringOption(value, "format"),
            StringOption(value, "ffmpegPath"));
    }

    private static double? DoubleOption(JsonElement root, string name) =>
        !root.TryGetProperty(name, out var value)
            ? null
            : value.ValueKind is JsonValueKind.Number && value.TryGetDouble(out var number)
                ? number
                : throw new RunConfigException($"Run config option 'gifSettings.{name}' must be a number.");

    private static IReadOnlyList<string>? StringArrayOption(JsonElement root, string name)
    {
        if (!root.TryGetProperty(name, out var value)) return null;
        if (value.ValueKind is not JsonValueKind.Array || value.EnumerateArray().Any(item => item.ValueKind is not JsonValueKind.String))
            throw new RunConfigException($"Run config option 'gifSettings.{name}' must be an array of strings.");
        return value.EnumerateArray().Select(item => item.GetString()!).ToArray();
    }

    private static readonly HashSet<string> GifSettingNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "quality", "pointerDuration", "pointerSpeed", "pointerEasing", "pointerPath", "dragPath", "clickPulse", "fps", "frameDelay", "typingDelay", "postHoverHold",
        "crop", "cropPadding", "smartCrop", "splitTabs", "scale", "maxWidth", "maxHeight", "viewport", "pixelRatio", "safeArea", "layoutStability",
        "targetZoom", "targetZoomThreshold", "pagePosition", "tabContext",
        "captionStyle", "captionPosition", "captionSeverity", "captionSize", "autoCaptions", "captionTemplate",
        "redact", "mask", "blur", "autoRedact", "redactionSafety",
        "sizeBudget", "budgetQualityFallback", "budgetDownscaleFallback",
        "narrationSidecar", "stillPdf", "altText", "description", "format", "ffmpegPath"
    };
}
