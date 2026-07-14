using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed partial class RunCommandBuilder
{
    private static bool ApplyRunGifSettings(
        ParseResult result,
        RunGifSettings settings,
        Option<string> qualityOption,
        Option<int?> durationOption,
        Option<string?> speedOption,
        Option<string?> easingOption,
        Option<string?> pathOption,
        Option<string?> dragPathOption,
        Option<string?> pulseOption,
        Option<int?> fpsOption,
        Option<int?> frameDelayOption,
        GifEncodingCliOptions encodingOptions,
        CaptionCliOptions captionOptions,
        GifPresetCliOptions presetOptions,
        ref GifQuality quality,
        ref ScriptPointerMotionOptions motion,
        ref ClickPulseStyle pulse,
        ref int frameDelay,
        ref GifEncodingOptions encoding,
        ref BrowserCaptionOptions? caption)
    {
        var qualityValue = QualityValue(result, qualityOption, settings.Quality);
        if (!GifQualityParser.TryParse(qualityValue, out quality))
            return Error($"gifSettings.quality must be one of: {GifQualityParser.Values}.");

        var duration = Value(result, durationOption, settings.PointerDuration);
        var speed = Value(result, speedOption, settings.PointerSpeed);
        var easing = Value(result, easingOption, settings.PointerEasing);
        var path = Value(result, pathOption, settings.PointerPath);
        var dragPath = Value(result, dragPathOption, settings.DragPath);
        var pulseValue = Value(result, pulseOption, settings.ClickPulse);
        if (!GifMotionOptionParser.TryParse(duration, speed, easing, path, dragPath, pulseValue, out var configuredMotion, out pulse, out var motionError))
            return Error(motionError);
        if (!result.GetValue(presetOptions.ReducedMotion)) motion = configuredMotion;

        var timingUsesCli = WasProvided(result, fpsOption) || WasProvided(result, frameDelayOption);
        var fps = timingUsesCli ? result.GetValue(fpsOption) : settings.Fps;
        var delay = timingUsesCli ? result.GetValue(frameDelayOption) : settings.FrameDelay;
        if (!GifFrameTimingOptionParser.TryParse(fps, delay, out frameDelay, out var frameError))
            return Error(frameError);

        if (!GifFramingOptions.TryParse(
            Value(result, encodingOptions.Crop, settings.Crop),
            Value(result, encodingOptions.CropPadding, settings.CropPadding),
            Value(result, encodingOptions.Scale, settings.Scale),
            Value(result, encodingOptions.MaxWidth, settings.MaxWidth),
            Value(result, encodingOptions.MaxHeight, settings.MaxHeight),
            Value(result, encodingOptions.Viewport, settings.Viewport),
            Value(result, encodingOptions.PixelRatio, settings.PixelRatio),
            Value(result, encodingOptions.SafeArea, settings.SafeArea),
            Value(result, encodingOptions.LayoutStability, settings.LayoutStability),
            out var framing, out var framingError)) return Error(framingError);
        encoding = encoding with { Framing = framing };

        if (!GifCaptionOptionParser.TryParse(
            Value(result, captionOptions.Style, settings.CaptionStyle),
            Value(result, captionOptions.Position, settings.CaptionPosition),
            Value(result, captionOptions.Severity, settings.CaptionSeverity),
            out caption, out var captionError,
            Value(result, captionOptions.Size, settings.CaptionSize),
            WasProvided(result, captionOptions.AutoCaptions) ? result.GetValue(captionOptions.AutoCaptions) : settings.AutoCaptions,
            Value(result, captionOptions.Template, settings.CaptionTemplate))) return Error(captionError);
        return true;
    }

    private static T? Value<T>(ParseResult result, Option<T?> option, T? fallback) where T : struct =>
        WasProvided(result, option) ? result.GetValue(option) : fallback;

    private static string? Value(ParseResult result, Option<string?> option, string? fallback) =>
        WasProvided(result, option) ? result.GetValue(option) : fallback;

    private static string QualityValue(ParseResult result, Option<string> option, string? fallback) =>
        (WasProvided(result, option) ? result.GetValue(option) : fallback) ?? "highest";

    private static bool Error(string? message)
    {
        Console.Error.WriteLine(message);
        return false;
    }
}
