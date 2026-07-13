using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
{
    private static GifPresetCliOptions BuildGifPresetOptions() => new(
        new Option<bool>("--gif-reduced-motion") { Description = "Use reduced-motion pointer choreography for whole-run GIFs." },
        new Option<bool>("--gif-high-contrast-pointer") { Description = "Use the high-contrast virtual pointer preset for whole-run GIFs." });

    private static void ApplyGifPresets(
        ParseResult result,
        GifPresetCliOptions options,
        int? duration,
        string? speed,
        string? easing,
        string? theme,
        string? color,
        int? size,
        string? shadow,
        ref ScriptPointerMotionOptions motion,
        ref PointerVisualOptions visual)
    {
        motion = GifRecordingPresetCli.Motion(result.GetValue(options.ReducedMotion), duration, speed, easing, motion);
        visual = GifRecordingPresetCli.Visual(result.GetValue(options.HighContrastPointer), theme, color, size, shadow, visual);
    }

    private sealed record GifPresetCliOptions(
        Option<bool> ReducedMotion,
        Option<bool> HighContrastPointer);
}
