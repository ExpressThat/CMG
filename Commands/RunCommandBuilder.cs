using System.CommandLine;
using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;
namespace CMG.Commands;
public sealed partial class RunCommandBuilder
{
    public Command Build(BrowserSelectionOptions browserOptions)
    {
        var pathArgument = new Argument<string>("path") { Description = "A CMG script file or folder containing .cmgscript files." };
        var configOption = new Option<FileInfo?>("--config") { Description = "JSON run config file. CLI options override config values." };
        var projectOption = new Option<string?>("--project") { Description = "Named project from the run config." };
        var gifOption = new Option<DirectoryInfo?>("--gif") { Description = "Write per-test GIF recordings to this directory." }; var noGifOption = new Option<bool>("--no-gif") { Description = "Disable command-level and script-block GIF recording." }; gifOption.Aliases.Add("-gif");
        var gifQualityOption = new Option<string>("--gif-quality")
        {
            Description = "GIF quality for --gif recordings: archival, highest, high, medium, or low.",
            DefaultValueFactory = _ => "highest"
        }; var encodingOptions = GifEncodingCliOptions.Build();
        var pointerDurationOption = new Option<int?>("--pointer-duration") { Description = "Default virtual pointer movement duration in milliseconds for --gif recordings." }; var pointerSpeedOption = new Option<string?>("--pointer-speed") { Description = "Default virtual pointer speed for --gif recordings: slow, normal, fast, instant, or a multiplier like 1.5x." };
        var pointerEasingOption = new Option<string?>("--pointer-easing") { Description = "Default virtual pointer easing for --gif recordings: linear, ease-in, ease-out, ease-in-out, or spring." }; var pointerPathOption = new Option<string?>("--pointer-path") { Description = $"Default virtual pointer route: {ScriptPointerPathParser.Values}." }; var dragPathOption = new Option<string?>("--drag-path") { Description = $"Default virtual drag route: {ScriptPointerPathParser.Values}." };
        var pointerVisualOptions = BuildPointerVisualOptions();
        var presetOptions = BuildGifPresetOptions();
        var showPointerOption = new Option<string?>("--show-pointer") { Description = $"Default virtual pointer visibility for --gif recordings: {PointerVisibilityOptions.Values}." };
        var captionOptions = BuildCaptionOptions();
        var clickPulseOption = new Option<string?>("--click-pulse") { Description = "Click pulse style for --gif: ring, ripple, dot, crosshair, or none." };
        var holdAfterActionOption = new Option<int?>("--gif-hold-after-action") { Description = "Default post-action hold in milliseconds for --gif recordings." };
        var holdOnFailureOption = new Option<int?>("--gif-hold-on-failure") { Description = "Final failure-state hold in milliseconds for --gif recordings." };
        var preClickHoldOption = new Option<int?>("--pointer-pre-click-hold") { Description = "Default hold before click/tap events in --gif recordings." };
        var postClickHoldOption = new Option<int?>("--pointer-post-click-hold") { Description = "Default hold after click/tap pulses in --gif recordings." };
        var holdAfterNavigationOption = new Option<int?>("--gif-hold-after-navigation") { Description = "Default hold after navigation actions in --gif recordings." };
        var holdAfterAssertionOption = new Option<int?>("--gif-hold-after-assertion") { Description = "Default hold after assertion actions in --gif recordings." };
        var gifFpsOption = new Option<int?>("--gif-fps") { Description = "GIF frame rate for --gif recordings. Must be between 1 and 100." };
        var gifFrameDelayOption = new Option<int?>("--gif-frame-delay") { Description = "GIF frame delay in milliseconds. Overrides --gif-fps." };
        var gifTimelineOption = new Option<string?>("--gif-timeline") { Description = "Write GIF timeline JSON files to this file or directory." };
        var gifWarnSizeOption = new Option<string?>("--gif-warn-size") { Description = "Warn when a recorded GIF exceeds this size, for example 500KB or 2MB." };
        var gifMaxSizeOption = new Option<string?>("--gif-max-size") { Description = "Fail a test when a recorded GIF exceeds this size, for example 500KB or 2MB." };
        var gifMaxDurationOption = new Option<string?>("--gif-max-duration") { Description = "Fail a test when a recorded GIF exceeds this duration, for example 2500ms, 10s, or 1m." };
        var retentionOptions = BuildGifRetentionOptions();
        var jsonOption = new Option<FileInfo?>("--report-json") { Description = "Write a JSON test report to this file." };
        var htmlOption = new Option<FileInfo?>("--report-html") { Description = "Write an HTML test report to this file." }; var junitOption = new Option<FileInfo?>("--report-junit") { Description = "Write a JUnit XML test report to this file." };
        var traceOption = new Option<DirectoryInfo?>("--trace") { Description = "Write per-test trace JSON files to this directory." };
        var grepOption = new Option<string?>("--grep") { Description = "Run tests whose names contain this text." };
        var tagOption = new Option<string?>("--tag") { Description = "Run tests with a matching tag option." };
        var retriesOption = new Option<int>("--retries") { Description = "Retry failed tests this many times.", DefaultValueFactory = _ => 0 };
        var maxFailuresOption = new Option<int>("--max-failures") { Description = "Stop the run after this many failed tests.", DefaultValueFactory = _ => 0 };
        var repeatEachOption = new Option<int>("--repeat-each") { Description = "Run each selected test this many times.", DefaultValueFactory = _ => 1 };
        var listOption = new Option<bool>("--list") { Description = "List selected tests without connecting to a browser." };
        var shardOption = new Option<string?>("--shard") { Description = "Run one shard as index/count, for example 1/3." };
        var timeoutOption = new Option<int?>("--timeout") { Description = "Default timeout in milliseconds for timeout-capable actions." };
        var navigationTimeoutOption = new Option<int?>("--navigation-timeout") { Description = "Default timeout in milliseconds for navigation actions." }; var assertionTimeoutOption = new Option<int?>("--assertion-timeout") { Description = "Default timeout in milliseconds for assertion actions." };
        var baseUrlOption = new Option<string?>("--base-url") { Description = "Base URL used to resolve relative navigation targets." };
        var browserPortOption = new Option<int?>("--browser-port") { Description = "Remote debugging port for the browser instance used by this run." };
        var autoLaunchOption = new Option<bool>("--auto-launch") { Description = "Launch the selected browser automatically when no CMG-controlled browser is running." }; var headlessOption = new Option<bool>("--headless") { Description = "Launch the browser in headless mode when --auto-launch starts a browser." };
        var browserIdleTimeoutOption = new Option<string?>("--browser-idle-timeout") { Description = "Opt-in idle lease for the selected CMG headless browser, such as 30m or 2h." };
        var noBrowserIdleCleanupOption = new Option<bool>("--no-browser-idle-cleanup") { Description = "Disable an existing idle lease for the selected browser." };
        var variableOption = new Option<string[]>("--var") { Description = "Initial runner variable as name=value. Can be repeated." };
        var envOption = new Option<string[]>("--env") { Description = "Alias for --var, useful for CI or agent-provided values." };
        var command = new Command("run", "Run CMG DSL tests with visual artifacts.")
        {
            pathArgument, configOption, projectOption, gifOption, noGifOption, gifQualityOption,
            encodingOptions.Dither, encodingOptions.Palette, encodingOptions.Colors, encodingOptions.KeepFrames,
            encodingOptions.Crop, encodingOptions.CropPadding, encodingOptions.SmartCrop, encodingOptions.SplitTabs, encodingOptions.Scale, encodingOptions.MaxWidth, encodingOptions.MaxHeight, encodingOptions.Viewport, encodingOptions.PixelRatio, encodingOptions.SafeArea, encodingOptions.LayoutStability, encodingOptions.Debug, encodingOptions.Accessibility, encodingOptions.EventCaptions, encodingOptions.Intro, encodingOptions.Outro, encodingOptions.IntroDuration, encodingOptions.OutroDuration, encodingOptions.ResultOutro, encodingOptions.DisableCoalescing, encodingOptions.SampleEvery, encodingOptions.PointerContrast, encodingOptions.PointerCallout, encodingOptions.PointerCalloutThreshold, encodingOptions.TargetZoom, encodingOptions.TargetZoomThreshold, encodingOptions.PagePosition, encodingOptions.TabContext, encodingOptions.DisableFocusPulse, encodingOptions.PointerIdle, encodingOptions.PointerIdleThreshold, encodingOptions.DisableTeleportMarker, encodingOptions.MouseDownHold, encodingOptions.Background, encodingOptions.GradientMode, encodingOptions.HighContrastPalette, encodingOptions.Redact, encodingOptions.Mask, encodingOptions.Blur, encodingOptions.AutoRedact, encodingOptions.RedactionSafety, encodingOptions.SizeBudget, encodingOptions.DisableBudgetQualityFallback, encodingOptions.DisableBudgetDownscale,
            pointerDurationOption, pointerSpeedOption, pointerEasingOption, pointerPathOption, dragPathOption,
            pointerVisualOptions.Theme, pointerVisualOptions.Color, pointerVisualOptions.Size, pointerVisualOptions.Shadow,
            presetOptions.ReducedMotion, presetOptions.HighContrastPointer,
            showPointerOption,
            captionOptions.Style, captionOptions.Position, captionOptions.Severity, captionOptions.Size, captionOptions.AutoCaptions, captionOptions.Template,
            clickPulseOption,
            holdAfterActionOption,
            holdOnFailureOption,
            preClickHoldOption,
            postClickHoldOption,
            holdAfterNavigationOption,
            holdAfterAssertionOption,
            gifFpsOption,
            gifFrameDelayOption,
            gifTimelineOption,
            gifWarnSizeOption,
            gifMaxSizeOption,
            gifMaxDurationOption,
            retentionOptions.Mode, retentionOptions.OnFailure, retentionOptions.OnRetry, retentionOptions.SampleRate, retentionOptions.CleanPassed,
            jsonOption,
            htmlOption,
            junitOption,
            traceOption,
            grepOption,
            tagOption,
            retriesOption,
            maxFailuresOption,
            repeatEachOption,
            listOption,
            shardOption,
            timeoutOption,
            navigationTimeoutOption,
            assertionTimeoutOption,
            baseUrlOption,
            browserPortOption,
            autoLaunchOption,
            headlessOption,
            browserIdleTimeoutOption,
            noBrowserIdleCleanupOption,
            variableOption,
            envOption
        };
        command.SetAction(parseResult =>
        {
            using var gifSuppression = GifRecordingPolicy.Suppress(parseResult.GetValue(noGifOption)); if (GifRecordingPolicy.IsDisabled) Console.WriteLine($"GIF_DISABLED source={GifRecordingPolicy.DisabledSource}");
            if (!RunConfigReader.TryRead(parseResult.GetValue(configOption), out var config, out var configError))
            { Console.Error.WriteLine(configError); return 1; }
            if (!TrySelectProject(parseResult.GetValue(projectOption), config, out var project, out var projectError))
            { Console.Error.WriteLine(projectError); return 1; }
            var variableValues = (parseResult.GetValue(variableOption) ?? []).Concat(parseResult.GetValue(envOption) ?? []);
            if (!VariableOptionParser.TryParse(variableValues, out var variables, out var error))
            { Console.Error.WriteLine(error); return 1; }
            if (!GifQualityParser.TryParse(parseResult.GetValue(gifQualityOption), out var gifQuality))
            { Console.Error.WriteLine($"--gif-quality must be one of: {GifQualityParser.Values}."); return 1; }
            if (!TryParseEncoding(parseResult, encodingOptions, config.GifSettings.Overlay(project?.GifSettings), out var gifEncoding)) return 1;
            if (!GifMotionOptionParser.TryParse(
                parseResult.GetValue(pointerDurationOption),
                parseResult.GetValue(pointerSpeedOption),
                parseResult.GetValue(pointerEasingOption),
                parseResult.GetValue(pointerPathOption),
                parseResult.GetValue(dragPathOption),
                parseResult.GetValue(clickPulseOption),
                out var pointerMotion,
                out var clickPulse,
                out var motionError))
            {
                Console.Error.WriteLine(motionError);
                return 1;
            }
            if (!TryParsePointerVisual(parseResult, pointerVisualOptions, out var pointerVisual, out var visualError))
            {
                Console.Error.WriteLine(visualError);
                return 1;
            }
            ApplyGifPresets(parseResult, presetOptions, parseResult.GetValue(pointerDurationOption), parseResult.GetValue(pointerSpeedOption), parseResult.GetValue(pointerEasingOption), parseResult.GetValue(pointerVisualOptions.Theme), parseResult.GetValue(pointerVisualOptions.Color), parseResult.GetValue(pointerVisualOptions.Size), parseResult.GetValue(pointerVisualOptions.Shadow), ref pointerMotion, ref pointerVisual);
            if (!PointerVisibilityOptions.TryParse(parseResult.GetValue(showPointerOption), out var showPointer, out var showPointerError))
            { Console.Error.WriteLine(showPointerError); return 1; }
            if (!TryParseCaption(parseResult, captionOptions, out var caption, out var captionError))
            {
                Console.Error.WriteLine(captionError);
                return 1;
            }
            if (!GifTimingOptionParser.TryParseHoldAfterAction(parseResult.GetValue(holdAfterActionOption), out var holdAfterAction, out var holdError))
            {
                Console.Error.WriteLine(holdError);
                return 1;
            }
            if (!GifTimingOptionParser.TryParseHoldOnFailure(parseResult.GetValue(holdOnFailureOption), out var holdOnFailure, out holdError))
            {
                Console.Error.WriteLine(holdError);
                return 1;
            }
            if (!GifTimingOptionParser.TryParsePreClickHold(parseResult.GetValue(preClickHoldOption), out var preClickHold, out holdError) ||
                !GifTimingOptionParser.TryParsePostClickHold(parseResult.GetValue(postClickHoldOption), out var postClickHold, out holdError) ||
                !GifTimingOptionParser.TryParseHoldAfterNavigation(parseResult.GetValue(holdAfterNavigationOption), out var holdAfterNavigation, out holdError) ||
                !GifTimingOptionParser.TryParseHoldAfterAssertion(parseResult.GetValue(holdAfterAssertionOption), out var holdAfterAssertion, out holdError))
            {
                Console.Error.WriteLine(holdError);
                return 1;
            }
            if (!GifFrameTimingOptionParser.TryParse(parseResult.GetValue(gifFpsOption), parseResult.GetValue(gifFrameDelayOption), out var frameDelay, out var frameError))
            {
                Console.Error.WriteLine(frameError);
                return 1;
            }
            if (!GifSizeOptionParser.TryParse(parseResult.GetValue(gifWarnSizeOption), out var gifWarnSizeBytes, out var sizeError))
            {
                Console.Error.WriteLine(sizeError);
                return 1;
            }
            if (!GifSizeOptionParser.TryParse(parseResult.GetValue(gifMaxSizeOption), out var gifMaxSizeBytes, out sizeError, "--gif-max-size"))
            {
                Console.Error.WriteLine(sizeError);
                return 1;
            }
            if (!GifDurationOptionParser.TryParse(parseResult.GetValue(gifMaxDurationOption), out var gifMaxDurationMilliseconds, out var durationError))
            {
                Console.Error.WriteLine(durationError);
                return 1;
            }
            if (!ApplyRunGifSettings(parseResult, config.GifSettings.Overlay(project?.GifSettings), gifQualityOption, pointerDurationOption, pointerSpeedOption, pointerEasingOption, pointerPathOption, dragPathOption, clickPulseOption, gifFpsOption, gifFrameDelayOption, encodingOptions, captionOptions, presetOptions, ref gifQuality, ref pointerMotion, ref clickPulse, ref frameDelay, ref gifEncoding, ref caption) || !TryParseGifRetention(parseResult, retentionOptions, config, out var gifRetention)) return 1;
            if (!TryParseBrowserIdle(parseResult, browserIdleTimeoutOption, noBrowserIdleCleanupOption, config,
                out var browserIdleTimeout, out var browserIdleDisabled, out var browserIdleError))
            {
                Console.Error.WriteLine(browserIdleError);
                return 1;
            }
            variables = MergeVariables(MergeVariables(config.Variables, project?.Variables), variables);
            var projectBrowser = BrowserKindFor(project?.Browser);
            if (projectBrowser is BrowserKind.InvalidSelection)
            {
                Console.Error.WriteLine($"Run config project '{project?.Name}' has unknown browser '{project?.Browser}'.");
                return 1;
            }
            var gifDirectory = DirectoryValue(parseResult, gifOption, config.Gif);
            if (!TryCleanExpiredGifArtifacts(parseResult.GetValue(listOption), gifDirectory, gifRetention.CleanupDays)) return 1;
            return handler.RunWithGifRetention(
                projectBrowser ?? CommandTreeBuilder.GetBrowserKind(parseResult, browserOptions),
                parseResult.GetValue(pathArgument) ?? string.Empty,
                gifDirectory,
                FileValue(parseResult, jsonOption, config.ReportJson),
                FileValue(parseResult, htmlOption, config.ReportHtml),
                FileValue(parseResult, junitOption, config.ReportJunit),
                DirectoryValue(parseResult, traceOption, config.Trace),
                StringValue(parseResult, grepOption, project?.Grep ?? config.Grep),
                StringValue(parseResult, tagOption, project?.Tag ?? config.Tag),
                IntValue(parseResult, retriesOption, project?.Retries ?? config.Retries),
                IntValue(parseResult, maxFailuresOption, config.MaxFailures),
                IntValue(parseResult, repeatEachOption, config.RepeatEach),
                parseResult.GetValue(listOption),
                StringValue(parseResult, shardOption, config.Shard),
                IntValue(parseResult, timeoutOption, project?.Timeout ?? config.Timeout),
                IntValue(parseResult, navigationTimeoutOption, project?.NavigationTimeout ?? config.NavigationTimeout),
                IntValue(parseResult, assertionTimeoutOption, project?.AssertionTimeout ?? config.AssertionTimeout),
                StringValue(parseResult, baseUrlOption, project?.BaseUrl ?? config.BaseUrl),
                variables,
                project?.Name ?? string.Empty,
                parseResult.GetValue(browserPortOption),
                parseResult.GetValue(autoLaunchOption),
                parseResult.GetValue(headlessOption),
                gifQuality,
                pointerMotion,
                pointerVisual,
                showPointer,
                caption,
                clickPulse,
                holdAfterAction,
                holdOnFailure,
                preClickHold,
                postClickHold,
                holdAfterNavigation,
                holdAfterAssertion,
                parseResult.GetValue(gifTimelineOption),
                frameDelay,
                gifWarnSizeBytes,
                gifMaxSizeBytes,
                gifMaxDurationMilliseconds,
                gifEncoding,
                browserIdleTimeout,
                browserIdleDisabled,
                gifRetention.Mode,
                gifRetention.SampleRate,
                gifRetention.CleanPassed);
        });
        return command;
    }
}
