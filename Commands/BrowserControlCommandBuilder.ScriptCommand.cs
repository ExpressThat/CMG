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
        var gifOption = new Option<FileInfo?>("--gif") { Description = "Write an animated GIF recording of the script to this path." };
        var gifQualityOption = new Option<string>("--gif-quality")
        {
            Description = "GIF quality: highest, high, medium, or low.",
            DefaultValueFactory = _ => "highest"
        };
        var pointerDurationOption = new Option<int?>("--pointer-duration") { Description = "Default virtual pointer movement duration in milliseconds for --gif recordings." };
        var pointerSpeedOption = new Option<string?>("--pointer-speed") { Description = "Default virtual pointer speed for --gif recordings: slow, normal, fast, instant, or a multiplier like 1.5x." };
        var pointerEasingOption = new Option<string?>("--pointer-easing") { Description = "Default virtual pointer easing for --gif recordings: linear, ease-in, ease-out, ease-in-out, or spring." };
        var pointerThemeOption = new Option<string?>("--pointer-theme") { Description = $"Default virtual pointer theme for --gif recordings: {PointerVisualOptions.ThemeValues}." };
        var pointerColorOption = new Option<string?>("--pointer-color") { Description = "Default virtual pointer CSS color for --gif recordings." };
        var pointerSizeOption = new Option<int?>("--pointer-size") { Description = "Default virtual pointer size in CSS pixels for --gif recordings. Valid range is 8 to 96." };
        var pointerShadowOption = new Option<string?>("--pointer-shadow") { Description = $"Default virtual pointer shadow for --gif recordings: {PointerVisualOptions.ShadowValues}." };
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
            fileOption, inlineOption, gifOption, gifQualityOption, pointerDurationOption, pointerSpeedOption,
            pointerEasingOption, pointerThemeOption, pointerColorOption, pointerSizeOption, pointerShadowOption,
            clickPulseOption, holdAfterActionOption, holdOnFailureOption, preClickHoldOption, postClickHoldOption,
            holdAfterNavigationOption, holdAfterAssertionOption, gifFpsOption, gifFrameDelayOption, gifTimelineOption,
            traceOption, timeoutOption, navigationTimeoutOption, assertionTimeoutOption, baseUrlOption, variableOption, envOption
        };

        command.SetAction(parseResult =>
        {
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
                ? browserControlCommandHandler.RunScript(browserKind, port, file, options.Gif, options.Trace, timeouts, parseResult.GetValue(baseUrlOption), variables, options.Quality, options.Motion, options.Visual, options.Pulse, options.HoldAfterAction, options.HoldOnFailure, options.PreClickHold, options.PostClickHold, options.HoldAfterNavigation, options.HoldAfterAssertion, options.Timeline, options.FrameDelay)
                : browserControlCommandHandler.RunInlineScript(browserKind, port, inline, options.Gif, options.Trace, timeouts, parseResult.GetValue(baseUrlOption), variables, options.Quality, options.Motion, options.Visual, options.Pulse, options.HoldAfterAction, options.HoldOnFailure, options.PreClickHold, options.PostClickHold, options.HoldAfterNavigation, options.HoldAfterAssertion, options.Timeline, options.FrameDelay);
        });

        return command;

        bool TryParseGifScriptOptions(ParseResult parseResult, out GifScriptCommandOptions options)
        {
            options = new GifScriptCommandOptions(parseResult.GetValue(gifOption), parseResult.GetValue(traceOption), GifQuality.Highest, null, null, ClickPulseStyle.Ring, 0, 0, 0, 0, 0, 0, parseResult.GetValue(gifTimelineOption), 0);
            if (!GifQualityParser.TryParse(parseResult.GetValue(gifQualityOption), out var gifQuality))
            {
                Console.Error.WriteLine($"--gif-quality must be one of: {GifQualityParser.Values}.");
                return false;
            }
            if (!GifMotionOptionParser.TryParse(parseResult.GetValue(pointerDurationOption), parseResult.GetValue(pointerSpeedOption), parseResult.GetValue(pointerEasingOption), parseResult.GetValue(clickPulseOption), out var pointerMotion, out var clickPulse, out var motionError))
            {
                Console.Error.WriteLine(motionError);
                return false;
            }
            if (!GifVisualOptionParser.TryParse(parseResult.GetValue(pointerThemeOption), parseResult.GetValue(pointerColorOption), parseResult.GetValue(pointerSizeOption), parseResult.GetValue(pointerShadowOption), out var pointerVisual, out var visualError))
            {
                Console.Error.WriteLine(visualError);
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

            options = new GifScriptCommandOptions(parseResult.GetValue(gifOption), parseResult.GetValue(traceOption), gifQuality, pointerMotion, pointerVisual, clickPulse, holds.HoldAfterAction, holds.HoldOnFailure, holds.PreClickHold, holds.PostClickHold, holds.HoldAfterNavigation, holds.HoldAfterAssertion, parseResult.GetValue(gifTimelineOption), frameDelay);
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
        ClickPulseStyle Pulse,
        int HoldAfterAction,
        int HoldOnFailure,
        int PreClickHold,
        int PostClickHold,
        int HoldAfterNavigation,
        int HoldAfterAssertion,
        string? Timeline,
        int FrameDelay);

    private readonly record struct GifHoldOptions(
        int HoldAfterAction,
        int HoldOnFailure,
        int PreClickHold,
        int PostClickHold,
        int HoldAfterNavigation,
        int HoldAfterAssertion);
}
