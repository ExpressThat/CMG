using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private readonly BrowserScriptRunner scriptRunner;
    private readonly IBrowserAutomationClient automationClient;
    private readonly CmgActionLowerer lowerer;
    private readonly CmgApiRequestRunner apiRequestRunner;
    private readonly CmgStorageStateRunner storageStateRunner;
    private readonly CmgVisualAssertionRunner visualAssertionRunner;
    private readonly CmgUploadRunner uploadRunner;

    public CmgVisualSegmentExecutor(
        BrowserScriptRunner scriptRunner,
        IBrowserAutomationClient automationClient,
        CmgActionLowerer lowerer,
        CmgApiRequestRunner apiRequestRunner,
        CmgStorageStateRunner storageStateRunner,
        CmgVisualAssertionRunner visualAssertionRunner,
        CmgUploadRunner uploadRunner)
    {
        this.scriptRunner = scriptRunner;
        this.automationClient = automationClient;
        this.lowerer = lowerer;
        this.apiRequestRunner = apiRequestRunner;
        this.storageStateRunner = storageStateRunner;
        this.visualAssertionRunner = visualAssertionRunner;
        this.uploadRunner = uploadRunner;
    }

    public CmgTestResult Run(CmgTestCase test, string remoteDebuggingUrl, CmgRunOptions options, int attempt)
    {
        var output = new List<string>();
        var gifs = new List<string>();
        var gifQualities = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var pending = new List<string>();
        var pendingLineMap = new Dictionary<int, int>();
        var steps = new List<CmgStepResult>();
        if (!TryApplyDeclaredGifDefaults(test, options, out options, out var declarationError))
            return Fail(test, output, declarationError, gifs, steps, gifQualities);
        var commandGif = BuildGifPath(test, options, attempt);
        var suppressGifBlocks = commandGif is not null;
        var timeouts = BuildTimeoutOptions(test, options);
        var baseUrl = CmgNavigationOptions.BaseUrl(test, options);

        foreach (var action in CmgVariables.FromRunOptions(options).Concat(test.Actions))
        {
            if (IsRecordingBlock(action.Kind) && !suppressGifBlocks)
            {
                var flush = FlushPending(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl, options);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps, gifQualities);
                }

                var gif = ResolveGifPath(test, action, options);
                if (gif is not null)
                {
                    gifs.Add(gif.FullName);
                }

                if (!TryGifQualityFor(action, out var blockQuality, out error) ||
                    !TryGifEncodingFor(action, options.GifEncoding, gif, out var blockEncoding, out error) ||
                    !TryGifMotionFor(action, options.PointerMotion, out var blockMotion, out error) ||
                    !TryGifVisualFor(action, options.PointerVisual, out var blockVisual, out error) ||
                    !TryGifPointerVisibilityFor(action, options.ShowPointer, out var blockShowPointer, out error) ||
                    !TryGifCaptionFor(action, options.CaptionOptions, out var blockCaption, out error) ||
                    !TryGifPulseFor(action, options.ClickPulse, out var blockPulse, out error) ||
                    !TryGifHoldFor(action, options.HoldAfterActionMilliseconds, out var blockHold, out error) ||
                    !TryGifFailureHoldFor(action, options.HoldOnFailureMilliseconds, out var blockFailureHold, out error) ||
                    !TryGifPreClickHoldFor(action, options.PreClickHoldMilliseconds, out var blockPreClickHold, out error) ||
                    !TryGifPostClickHoldFor(action, options.PostClickHoldMilliseconds, out var blockPostClickHold, out error) ||
                    !TryGifNavigationHoldFor(action, options.HoldAfterNavigationMilliseconds, out var blockNavigationHold, out error) ||
                    !TryGifAssertionHoldFor(action, options.HoldAfterAssertionMilliseconds, out var blockAssertionHold, out error) ||
                    !TryGifTimelineFor(action, gif, options, out var blockTimeline, out error) ||
                    !TryGifRedactionFor(action, options.GifRedaction, out var blockRedaction, out error) ||
                    !TryGifAccessibilityFor(action, options.GifAccessibility, out var blockAccessibility, out error) ||
                    !TryGifFrameDelayFor(action, options.FrameDelayMilliseconds, out var blockFrameDelay, out error))
                {
                    return Fail(test, output, error, gifs, steps, gifQualities);
                }

                if (gif is not null)
                {
                    gifQualities[gif.FullName] = FormatQuality(blockQuality);
                }

                if (!RunActions(action.Children, remoteDebuggingUrl, gif, timeouts, baseUrl, blockQuality, blockMotion, blockVisual, blockShowPointer, blockCaption, blockPulse, blockHold, blockFailureHold, blockPreClickHold, blockPostClickHold, blockNavigationHold, blockAssertionHold, blockTimeline, output, steps, out error, blockFrameDelay, blockEncoding, blockRedaction, blockAccessibility))
                {
                    return Fail(test, output, error, gifs, steps, gifQualities);
                }

                continue;
            }
            if (action.Kind.Equals("apiRequest", StringComparison.OrdinalIgnoreCase))
            {
                var flush = FlushPending(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl, options);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps, gifQualities);
                }

                var apiStep = apiRequestRunner.Run(ApplyRunTimeoutDefault(action, options));
                output.AddRange(apiStep.Output);
                steps.Add(apiStep);
                if (!apiStep.Success)
                {
                    return Fail(test, output, apiStep.Error, gifs, steps, gifQualities);
                }

                continue;
            }

            if (action.Kind.Equals("storageState", StringComparison.OrdinalIgnoreCase))
            {
                var flush = FlushPending(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl, options);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps, gifQualities);
                }

                var step = storageStateRunner.Run(action, remoteDebuggingUrl, automationClient);
                output.AddRange(step.Output);
                steps.Add(step);
                if (!step.Success)
                {
                    return Fail(test, output, step.Error, gifs, steps, gifQualities);
                }

                continue;
            }

            if (action.Kind.Equals("expectScreenshot", StringComparison.OrdinalIgnoreCase))
            {
                var flush = FlushPending(pending, pendingLineMap, remoteDebuggingUrl, gif: null, timeouts, baseUrl, options);
                if (!AppendResult(flush.Result, flush.LineMap, output, steps, action, gif: null, out var error))
                {
                    return Fail(test, output, error, gifs, steps, gifQualities);
                }

                var step = visualAssertionRunner.Run(action, remoteDebuggingUrl, automationClient);
                output.AddRange(step.Output);
                steps.Add(step);
                if (!step.Success)
                {
                    return Fail(test, output, step.Error, gifs, steps, gifQualities);
                }

                continue;
            }

            var lines = suppressGifBlocks && IsRecordingBlock(action.Kind)
                ? lowerer.LowerRecordingBlock(action)
                : lowerer.Lower(action);
            AddPending(pending, pendingLineMap, action, lines);
        }

        if (commandGif is not null)
        {
            gifs.Add(commandGif.FullName);
            gifQualities[commandGif.FullName] = FormatQuality(options.GifQuality);
        }

        var final = FlushPending(pending, pendingLineMap, remoteDebuggingUrl, commandGif, timeouts, baseUrl, options, GifTimelineFor(commandGif, options.GifTimelinePath));
        if (!AppendResult(final.Result, final.LineMap, output, steps, test.Actions.LastOrDefault(), commandGif, out var finalError))
        {
            return Fail(test, output, finalError, gifs, steps, gifQualities);
        }

        return new CmgTestResult(test.Name, test.SourcePath, true, output, null, string.Join(';', gifs), steps) { Annotations = test.Annotations, GifQualities = gifQualities };
    }

    private static bool AppendResult(
        ScriptRunResult result,
        IReadOnlyDictionary<int, int> lineMap,
        List<string> output,
        List<CmgStepResult> steps,
        CmgNode? action,
        FileInfo? gif,
        out string? error)
    {
        output.AddRange(result.StdoutLines);
        AttachStepOutput(steps, result.StdoutLines, result.StepRecords, lineMap, action, gif);
        error = result.Error;
        if (!result.Success && !result.Skipped && action is not null)
        {
            AttachStepFailure(steps, result.Error, result.StdoutLines, lineMap, action, gif);
        }

        return result.Success;
    }

    private CmgScriptBatchRun FlushPending(
        List<string> pending,
        Dictionary<int, int> pendingLineMap,
        string remoteDebuggingUrl,
        FileInfo? gif,
        ScriptTimeoutOptions? timeouts,
        string? baseUrl,
        CmgRunOptions options,
        string? gifTimelinePath = null) =>
        RunLines(pending, pendingLineMap, remoteDebuggingUrl, gif, timeouts, baseUrl, options.GifQuality, options.PointerMotion,
            options.PointerVisual, options.ShowPointer, options.CaptionOptions, options.ClickPulse, options.HoldAfterActionMilliseconds, options.HoldOnFailureMilliseconds,
            options.PreClickHoldMilliseconds, options.PostClickHoldMilliseconds, options.HoldAfterNavigationMilliseconds,
            options.HoldAfterAssertionMilliseconds, gifTimelinePath, options.FrameDelayMilliseconds,
            options.GifEncoding?.ForOutput(gif?.FullName ?? "recording.gif", isolate: true), options.GifRedaction, options.GifAccessibility);

    private static CmgTestResult Fail(
        CmgTestCase test,
        IReadOnlyList<string> output,
        string? error,
        IReadOnlyList<string> gifs,
        IReadOnlyList<CmgStepResult> steps,
        IReadOnlyDictionary<string, string> gifQualities)
    {
        if (output.Any(line => line.StartsWith("SKIP ", StringComparison.Ordinal)))
        {
            return new CmgTestResult(test.Name, test.SourcePath, true, output, error, string.Join(';', gifs), steps) { Status = "skipped", Annotations = test.Annotations, GifQualities = gifQualities };
        }

        return new CmgTestResult(test.Name, test.SourcePath, false, output, error, string.Join(';', gifs), steps) { Annotations = test.Annotations, GifQualities = gifQualities };
    }

}
