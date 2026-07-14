using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Commands;

public sealed partial class BrowserControlCommandBuilder
{
    private Command BuildScriptCommand(BrowserSelectionOptions browserOptions)
    {
        var fileOption = new Option<string>("--file") { Description = "Path to a .cmgscript file, or '-' to read from stdin." };
        var inlineOption = new Option<string>("--inline") { Description = "Inline .cmgscript text to run." };
        var previewOption = new Option<bool>("--preview-gif-settings") { Description = "Parse and print effective GIF settings without launching or connecting to a browser." };
        var gifOption = new Option<FileInfo?>("--gif") { Description = "Write visual evidence for the script to this path." };
        var noGifOption = new Option<bool>("--no-gif") { Description = "Disable all GIF recording, including script recording blocks." };
        var gifQualityOption = new Option<string>("--gif-quality")
        {
            Description = "Recording quality: archival, highest, high, medium, or low.",
            DefaultValueFactory = _ => "highest"
        };
        var encodingOptions = GifEncodingCliOptions.Build();
        var pointerDurationOption = new Option<int?>("--pointer-duration") { Description = "Default virtual pointer movement duration in milliseconds for --gif recordings." };
        var pointerSpeedOption = new Option<string?>("--pointer-speed") { Description = "Default virtual pointer speed for --gif recordings: slow, normal, fast, instant, or a multiplier like 1.5x." };
        var pointerEasingOption = new Option<string?>("--pointer-easing") { Description = "Default virtual pointer easing for --gif recordings: linear, ease-in, ease-out, ease-in-out, or spring." };
        var pointerPathOption = new Option<string?>("--pointer-path") { Description = $"Default virtual pointer route: {ScriptPointerPathParser.Values}." };
        var dragPathOption = new Option<string?>("--drag-path") { Description = $"Default virtual drag route: {ScriptPointerPathParser.Values}." };
        var pointerThemeOption = new Option<string?>("--pointer-theme") { Description = $"Default virtual pointer theme for --gif recordings: {PointerVisualOptions.ThemeValues}." };
        var pointerColorOption = new Option<string?>("--pointer-color") { Description = "Default virtual pointer CSS color for --gif recordings." };
        var pointerSizeOption = new Option<int?>("--pointer-size") { Description = "Default virtual pointer size in CSS pixels for --gif recordings. Valid range is 8 to 96." };
        var pointerShadowOption = new Option<string?>("--pointer-shadow") { Description = $"Default virtual pointer shadow for --gif recordings: {PointerVisualOptions.ShadowValues}." };
        var showPointerOption = new Option<string?>("--show-pointer") { Description = $"Default virtual pointer visibility for --gif recordings: {PointerVisibilityOptions.Values}." };
        var reducedMotionOption = new Option<bool>("--gif-reduced-motion") { Description = "Use reduced-motion pointer choreography for the whole GIF." };
        var highContrastPointerOption = new Option<bool>("--gif-high-contrast-pointer") { Description = "Use the high-contrast virtual pointer preset for the whole GIF." };
        var captionStyleOption = new Option<string?>("--caption-style") { Description = $"Default caption style for --gif recordings: {BrowserCaptionOptions.StyleValues}." };
        var captionPositionOption = new Option<string?>("--caption-position") { Description = $"Default caption position for --gif recordings: {BrowserCaptionOptions.PositionValues}." };
        var captionSeverityOption = new Option<string?>("--caption-severity") { Description = $"Default caption severity color for --gif recordings: {BrowserCaptionOptions.SeverityValues}." };
        var captionSizeOption = new Option<string?>("--caption-size") { Description = $"Default caption text size for --gif recordings: {BrowserCaptionOptions.SizeValues}." };
        var autoCaptionsOption = new Option<bool>("--auto-captions") { Description = "Automatically caption supported visual actions in --gif recordings." };
        var captionTemplateOption = new Option<string?>("--caption-template") { Description = "Automatic-caption template for --gif recordings." };
        var clickPulseOption = new Option<string?>("--click-pulse") { Description = "Default click pulse style for --gif recordings: ring, ripple, dot, crosshair, or none." };
        var holdAfterActionOption = new Option<int?>("--gif-hold-after-action") { Description = "Default post-action hold in milliseconds for --gif recordings." };
        var holdOnFailureOption = new Option<int?>("--gif-hold-on-failure") { Description = "Final failure-state hold in milliseconds for --gif recordings." };
        static Option<int?> HoldOption(string name, string description) => new(name) { Description = description };
        var preClickHoldOption = HoldOption("--pointer-pre-click-hold", "Default hold before click/tap events in --gif recordings.");
        var postClickHoldOption = HoldOption("--pointer-post-click-hold", "Default hold after click/tap pulses in --gif recordings.");
        var holdAfterNavigationOption = HoldOption("--gif-hold-after-navigation", "Default hold after navigation actions in --gif recordings.");
        var holdAfterAssertionOption = HoldOption("--gif-hold-after-assertion", "Default hold after assertion actions in --gif recordings.");
        var gifFpsOption = new Option<int?>("--gif-fps") { Description = "GIF frame rate for --gif recordings. Must be between 1 and 100." };
        var gifFrameDelayOption = new Option<int?>("--gif-frame-delay") { Description = "GIF frame delay in milliseconds. Overrides --gif-fps." };
        var gifTimelineOption = new Option<string?>("--gif-timeline") { Description = "Write GIF timeline JSON to a file or directory." };
        var traceOption = new Option<FileInfo?>("--trace") { Description = "Write a CMG script trace JSON file for the run." };
        var timeoutOption = new Option<int?>("--timeout") { Description = "Default timeout in milliseconds for timeout-capable actions." };
        var navigationTimeoutOption = new Option<int?>("--navigation-timeout") { Description = "Default timeout in milliseconds for navigation actions." };
        var assertionTimeoutOption = new Option<int?>("--assertion-timeout") { Description = "Default timeout in milliseconds for assertion actions." };
        var baseUrlOption = new Option<string?>("--base-url") { Description = "Base URL used to resolve relative navigation targets." };
        var variableOption = new Option<string[]>("--var") { Description = "Initial script variable as name=value. Can be repeated." };
        var envOption = new Option<string[]>("--env") { Description = "Alias for --var, useful for agent-provided environment values." };

        var command = new Command("script", "Run a .cmgscript browser automation script.")
        {
            fileOption, inlineOption, previewOption, gifOption, noGifOption, gifQualityOption,
            encodingOptions.Dither, encodingOptions.Palette, encodingOptions.Colors, encodingOptions.KeepFrames,
            encodingOptions.Crop, encodingOptions.CropPadding, encodingOptions.SmartCrop, encodingOptions.SplitTabs, encodingOptions.Scale, encodingOptions.MaxWidth, encodingOptions.MaxHeight, encodingOptions.Viewport, encodingOptions.PixelRatio, encodingOptions.SafeArea, encodingOptions.LayoutStability, encodingOptions.Debug, encodingOptions.Accessibility, encodingOptions.EventCaptions,
            encodingOptions.Intro, encodingOptions.Outro, encodingOptions.IntroDuration, encodingOptions.OutroDuration, encodingOptions.ResultOutro,
            encodingOptions.DisableCoalescing, encodingOptions.SampleEvery,
            encodingOptions.PointerContrast, encodingOptions.PointerCallout, encodingOptions.PointerCalloutThreshold, encodingOptions.TargetZoom, encodingOptions.TargetZoomThreshold, encodingOptions.PagePosition, encodingOptions.TabContext, encodingOptions.DisableFocusPulse, encodingOptions.PointerIdle, encodingOptions.PointerIdleThreshold, encodingOptions.DisableTeleportMarker, encodingOptions.MouseDownHold,
            encodingOptions.Background, encodingOptions.GradientMode, encodingOptions.HighContrastPalette, encodingOptions.Redact, encodingOptions.Mask, encodingOptions.Blur, encodingOptions.AutoRedact, encodingOptions.RedactionSafety,
            encodingOptions.SizeBudget, encodingOptions.DisableBudgetQualityFallback, encodingOptions.DisableBudgetDownscale,
            encodingOptions.NarrationSidecar, encodingOptions.StillPdf, encodingOptions.AltText, encodingOptions.Description,
            encodingOptions.Format, encodingOptions.Ffmpeg,
            pointerDurationOption, pointerSpeedOption,
            pointerEasingOption, pointerPathOption, dragPathOption, pointerThemeOption, pointerColorOption, pointerSizeOption, pointerShadowOption,
            showPointerOption, reducedMotionOption, highContrastPointerOption, captionStyleOption, captionPositionOption, captionSeverityOption, captionSizeOption, autoCaptionsOption, captionTemplateOption,
            clickPulseOption, holdAfterActionOption, holdOnFailureOption, preClickHoldOption, postClickHoldOption,
            holdAfterNavigationOption, holdAfterAssertionOption, gifFpsOption, gifFrameDelayOption, gifTimelineOption,
            traceOption, timeoutOption, navigationTimeoutOption, assertionTimeoutOption, baseUrlOption, variableOption, envOption
        };

        command.SetAction(parseResult =>
        {
            using var gifSuppression = GifRecordingPolicy.Suppress(parseResult.GetValue(noGifOption));
            if (GifRecordingPolicy.IsDisabled)
                Console.WriteLine($"GIF_DISABLED source={GifRecordingPolicy.DisabledSource}");
            var file = parseResult.GetValue(fileOption) ?? string.Empty;
            var inline = parseResult.GetValue(inlineOption);
            if (!ValidateScriptInput(file, inline))
            {
                return 1;
            }

            if (!TryParseGifScriptOptions(parseResult, out var options))
            {
                return 1;
            }

            if (parseResult.GetValue(previewOption))
                return inline is null ? browserControlCommandHandler.PreviewGifSettings(file) : browserControlCommandHandler.PreviewInlineGifSettings(inline);

            var timeouts = new ScriptTimeoutOptions(
                parseResult.GetValue(timeoutOption),
                parseResult.GetValue(navigationTimeoutOption),
                parseResult.GetValue(assertionTimeoutOption));
            var variableValues = (parseResult.GetValue(variableOption) ?? []).Concat(parseResult.GetValue(envOption) ?? []);
            if (!VariableOptionParser.TryParse(variableValues, out var variables, out var error))
            {
                Console.Error.WriteLine(error);
                return 1;
            }

            var browserKind = CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions);
            var port = CommandTreeBuilder.GetBrowserPort(parseResult, browserOptions);
            return inline is null
                ? browserControlCommandHandler.RunScript(browserKind, port, file, options.Gif, options.Trace, timeouts, parseResult.GetValue(baseUrlOption), variables, options.Quality, options.Motion, options.Visual, options.ShowPointer, options.Caption, options.Pulse, options.HoldAfterAction, options.HoldOnFailure, options.PreClickHold, options.PostClickHold, options.HoldAfterNavigation, options.HoldAfterAssertion, options.Timeline, options.FrameDelay, options.Encoding)
                : browserControlCommandHandler.RunInlineScript(browserKind, port, inline, options.Gif, options.Trace, timeouts, parseResult.GetValue(baseUrlOption), variables, options.Quality, options.Motion, options.Visual, options.ShowPointer, options.Caption, options.Pulse, options.HoldAfterAction, options.HoldOnFailure, options.PreClickHold, options.PostClickHold, options.HoldAfterNavigation, options.HoldAfterAssertion, options.Timeline, options.FrameDelay, options.Encoding);
        });

        return command;

        bool TryParseGifScriptOptions(ParseResult parseResult, out GifScriptCommandOptions options)
        {
            options = new GifScriptCommandOptions(parseResult.GetValue(gifOption), parseResult.GetValue(traceOption), GifQuality.Highest, null, null, PointerVisibility.Auto, null, ClickPulseStyle.Ring, 0, 0, 0, 0, 0, 0, parseResult.GetValue(gifTimelineOption), 0, new());
            if (!GifQualityParser.TryParse(parseResult.GetValue(gifQualityOption), out var gifQuality))
            {
                Console.Error.WriteLine($"--gif-quality must be one of: {GifQualityParser.Values}.");
                return false;
            }
            if (!GifMotionOptionParser.TryParse(parseResult.GetValue(pointerDurationOption), parseResult.GetValue(pointerSpeedOption), parseResult.GetValue(pointerEasingOption), parseResult.GetValue(pointerPathOption), parseResult.GetValue(dragPathOption), parseResult.GetValue(clickPulseOption), out var pointerMotion, out var clickPulse, out var motionError))
            {
                Console.Error.WriteLine(motionError);
                return false;
            }
            if (!GifVisualOptionParser.TryParse(parseResult.GetValue(pointerThemeOption), parseResult.GetValue(pointerColorOption), parseResult.GetValue(pointerSizeOption), parseResult.GetValue(pointerShadowOption), out var pointerVisual, out var visualError))
            {
                Console.Error.WriteLine(visualError);
                return false;
            }
            pointerMotion = GifRecordingPresetCli.Motion(parseResult.GetValue(reducedMotionOption), parseResult.GetValue(pointerDurationOption), parseResult.GetValue(pointerSpeedOption), parseResult.GetValue(pointerEasingOption), pointerMotion);
            pointerVisual = GifRecordingPresetCli.Visual(parseResult.GetValue(highContrastPointerOption), parseResult.GetValue(pointerThemeOption), parseResult.GetValue(pointerColorOption), parseResult.GetValue(pointerSizeOption), parseResult.GetValue(pointerShadowOption), pointerVisual);
            if (!PointerVisibilityOptions.TryParse(parseResult.GetValue(showPointerOption), out var showPointer, out var showPointerError))
            {
                Console.Error.WriteLine(showPointerError);
                return false;
            }
            if (!GifCaptionOptionParser.TryParse(parseResult.GetValue(captionStyleOption), parseResult.GetValue(captionPositionOption), parseResult.GetValue(captionSeverityOption), out var captionOptions, out var captionError, parseResult.GetValue(captionSizeOption), parseResult.GetValue(autoCaptionsOption) ? true : null, parseResult.GetValue(captionTemplateOption)))
            {
                Console.Error.WriteLine(captionError);
                return false;
            }
            if (!TryParseGifHolds(parseResult, out var holds))
            {
                return false;
            }
            if (!GifFrameTimingOptionParser.TryParse(parseResult.GetValue(gifFpsOption), parseResult.GetValue(gifFrameDelayOption), out var frameDelay, out var frameError))
            {
                Console.Error.WriteLine(frameError);
                return false;
            }

            if (!encodingOptions.TryParse(parseResult, out var encoding, out var encodingError))
            {
                Console.Error.WriteLine(encodingError);
                return false;
            }

            options = new GifScriptCommandOptions(parseResult.GetValue(gifOption), parseResult.GetValue(traceOption), gifQuality, pointerMotion, pointerVisual, showPointer, captionOptions, clickPulse, holds.HoldAfterAction, holds.HoldOnFailure, holds.PreClickHold, holds.PostClickHold, holds.HoldAfterNavigation, holds.HoldAfterAssertion, parseResult.GetValue(gifTimelineOption), frameDelay, encoding);
            return true;
        }

        bool TryParseGifHolds(ParseResult parseResult, out GifHoldOptions holds)
        {
            holds = default;
            if (!GifTimingOptionParser.TryParseHoldAfterAction(parseResult.GetValue(holdAfterActionOption), out var holdAfterAction, out var holdError) ||
                !GifTimingOptionParser.TryParseHoldOnFailure(parseResult.GetValue(holdOnFailureOption), out var holdOnFailure, out holdError) ||
                !GifTimingOptionParser.TryParsePreClickHold(parseResult.GetValue(preClickHoldOption), out var preClickHold, out holdError) ||
                !GifTimingOptionParser.TryParsePostClickHold(parseResult.GetValue(postClickHoldOption), out var postClickHold, out holdError) ||
                !GifTimingOptionParser.TryParseHoldAfterNavigation(parseResult.GetValue(holdAfterNavigationOption), out var holdAfterNavigation, out holdError) ||
                !GifTimingOptionParser.TryParseHoldAfterAssertion(parseResult.GetValue(holdAfterAssertionOption), out var holdAfterAssertion, out holdError))
            {
                Console.Error.WriteLine(holdError);
                return false;
            }

            holds = new(holdAfterAction, holdOnFailure, preClickHold, postClickHold, holdAfterNavigation, holdAfterAssertion);
            return true;
        }
    }

    private sealed record GifScriptCommandOptions(
        FileInfo? Gif,
        FileInfo? Trace,
        GifQuality Quality,
        ScriptPointerMotionOptions? Motion,
        PointerVisualOptions? Visual,
        PointerVisibility ShowPointer,
        BrowserCaptionOptions? Caption,
        ClickPulseStyle Pulse,
        int HoldAfterAction,
        int HoldOnFailure,
        int PreClickHold,
        int PostClickHold,
        int HoldAfterNavigation,
        int HoldAfterAssertion,
        string? Timeline,
        int FrameDelay,
        GifEncodingOptions Encoding);

    private readonly record struct GifHoldOptions(
        int HoldAfterAction,
        int HoldOnFailure,
        int PreClickHold,
        int PostClickHold,
        int HoldAfterNavigation,
        int HoldAfterAssertion);
}
