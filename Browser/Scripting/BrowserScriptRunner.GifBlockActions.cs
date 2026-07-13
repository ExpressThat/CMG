using CMG.Browser.Scripting.Recording;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private IReadOnlyList<string> ExecuteGifBlock(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        ScriptGifRecorder? commandRecorder)
    {
        if (action.Children.Count is 0)
        {
            throw new ScriptExecutionException("gif requires a block body.");
        }

        var gifPath = GifBlockPath(action);
        var recorder = commandRecorder ?? new ScriptGifRecorder(
            automationClient,
            new ScriptRecordingOptions(gifPath, GifBlockQuality(action), GifBlockMotion(action), GifBlockVisual(action), GifBlockPointerVisibility(action), GifBlockPulse(action),
                GifBlockHold(action), GifBlockFailureHold(action), GifBlockPreClickHold(action), GifBlockPostClickHold(action),
                GifBlockNavigationHold(action), GifBlockAssertionHold(action), GifBlockTimeline(action, gifPath), GifBlockFrameDelay(action),
                GifEncodingOptions.FromOptions(action.Options, "gif", gifPath),
                GifFramingOptions.FromOptions(action.Options, "gif"),
                GifRedactionOptions.FromOptions(action.Options, "gif option"),
                GifAccessibilityOptions.FromOptions(action.Options, "gif option")));
        var output = new List<string>();
        var failed = false;
        if (commandRecorder is null)
        {
            recorder.Start(remoteDebuggingUrl);
        }
        else
        {
            output.Add($"GIF_BLOCK_SUPPRESSED {action.LineNumber:000}");
        }

        try
        {
            context.PushRecordingDefaults(RecordingDefaultsFrom(action.Options), () =>
            {
                foreach (var child in action.Children)
                {
                    var prepared = PrepareActionForDispatch(child, context);
                    recorder.BeforeAction(prepared, context: context.CurrentContext);
                    var lines = ExecuteAction(remoteDebuggingUrl, automationClient, prepared, context, recorder);
                    if (ShouldCaptureAfterAction(prepared))
                    {
                        recorder.AfterAction(prepared, lines);
                    }
                    output.AddRange(lines);
                }
            });
        }
        catch
        {
            failed = true;
            throw;
        }
        finally
        {
            if (commandRecorder is null)
            {
                FinishRecording(recorder, output, failed);
                recorder.Dispose();
            }
        }

        return output;
    }

    private static string GifBlockPath(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            return output;
        }

        var name = action.Arguments.Count > 0 ? action.Arguments[0] : $"gif-{action.LineNumber:000}";
        return Path.Combine(Directory.GetCurrentDirectory(), $"{SafeFileName(name)}.gif");
    }

    private static GifQuality GifBlockQuality(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("quality", out var value))
        {
            return GifQuality.Highest;
        }

        if (GifQualityParser.TryParse(value, out var quality))
        {
            return quality;
        }

        throw new ScriptExecutionException($"gif quality must be one of: {GifQualityParser.Values}.");
    }

    private static ScriptPointerMotionOptions GifBlockMotion(BrowserScriptAction action) =>
        ScriptPointerMotionOptions.Default.WithAction(action).Validate(action.Name);

    private static PointerVisualOptions GifBlockVisual(BrowserScriptAction action) =>
        PointerVisualOptions.FromOptions(action.Options, action.Name);

    private static PointerVisibility GifBlockPointerVisibility(BrowserScriptAction action) =>
        action.Options.TryGetValue("showPointer", out var value)
            ? PointerVisibilityOptions.Parse(value, "gif option")
            : PointerVisibility.Auto;

    private static ClickPulseStyle GifBlockPulse(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("clickPulse", out var value))
        {
            return ClickPulseStyle.Ring;
        }

        return ClickPulseStyleParser.TryParse(value, out var style)
            ? style
            : throw new ScriptExecutionException($"gif option clickPulse= must be one of: {ClickPulseStyleParser.Values}.");
    }

    private static int GifBlockHold(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("holdAfterAction", out var value))
        {
            return ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds;
        }

        return ScriptPointerMotionOptions.ParseDuration(value, "gif option holdAfterAction=");
    }

    private static int GifBlockFailureHold(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("holdOnFailure", out var value))
        {
            return ScriptRecordingOptions.DefaultHoldOnFailureMilliseconds;
        }

        return ScriptPointerMotionOptions.ParseDuration(value, "gif option holdOnFailure=");
    }

    private static int GifBlockPreClickHold(BrowserScriptAction action) =>
        GifBlockDuration(action, "preClickHold", 0);

    private static int GifBlockPostClickHold(BrowserScriptAction action) =>
        GifBlockDuration(action, "postClickHold", ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds);

    private static int GifBlockNavigationHold(BrowserScriptAction action) =>
        GifBlockDuration(action, "holdAfterNavigation", ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds);

    private static int GifBlockAssertionHold(BrowserScriptAction action) =>
        GifBlockDuration(action, "holdAfterAssertion", ScriptRecordingOptions.DefaultHoldAfterActionMilliseconds);

    private static int GifBlockDuration(BrowserScriptAction action, string option, int fallback)
    {
        if (!action.Options.TryGetValue(option, out var value))
        {
            return fallback;
        }

        return ScriptPointerMotionOptions.ParseDuration(value, $"gif option {option}=");
    }

    private static string? GifBlockTimeline(BrowserScriptAction action, string gifPath)
    {
        if (!action.Options.TryGetValue("timeline", out var value))
        {
            return null;
        }

        return GifTimelinePath.Resolve(gifPath, value);
    }

    private static int GifBlockFrameDelay(BrowserScriptAction action) =>
        ScriptFrameTimingOptions.FromOptions(action.Options, "gif");

    private static string SafeFileName(string value)
    {
        var safe = string.Concat(value.Select(character => char.IsLetterOrDigit(character) ? character : '-')).Trim('-');
        return string.IsNullOrWhiteSpace(safe) ? "gif-block" : safe;
    }

    private static bool IsRecordingBlock(string name) =>
        name.Equals("gif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("recordVideo", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("screencast", StringComparison.OrdinalIgnoreCase);
}
