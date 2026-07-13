using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

internal static class GifRecordingPresetCli
{
    public static ScriptPointerMotionOptions Motion(
        bool enabled,
        int? explicitDuration,
        string? explicitSpeed,
        string? explicitEasing,
        ScriptPointerMotionOptions parsed)
    {
        if (!enabled) return parsed;
        return parsed with
        {
            PointerDurationMilliseconds = explicitDuration ?? 0,
            PointerSpeed = explicitSpeed,
            PointerEasing = explicitEasing is null ? ScriptPointerEasing.Linear : parsed.PointerEasing
        };
    }

    public static PointerVisualOptions Visual(
        bool enabled,
        string? explicitTheme,
        string? explicitColor,
        int? explicitSize,
        string? explicitShadow,
        PointerVisualOptions parsed)
    {
        if (!enabled) return parsed;
        var preset = PointerVisualOptions.HighContrast;
        return parsed with
        {
            Theme = explicitTheme is null ? preset.Theme : parsed.Theme,
            Color = explicitColor is null ? preset.Color : parsed.Color,
            SizePixels = explicitSize is null ? preset.SizePixels : parsed.SizePixels,
            Shadow = explicitShadow is null ? preset.Shadow : parsed.Shadow
        };
    }
}
