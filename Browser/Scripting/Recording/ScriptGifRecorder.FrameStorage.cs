namespace CMG.Browser.Scripting.Recording;

public sealed partial class ScriptGifRecorder
{
    private int sampleCandidateCount;
    public int SourceFrameCount => frameSink.SourceFrameCount;
    public int DuplicateFramesCoalesced => frameSink.DuplicateFramesCoalesced;
    public int SampledFramesSkipped => frameSink.SampledFramesSkipped;
    public int BlankFrameCount => frameSink.BlankFrameCount;
    public long PeakRetainedPixelBytes => frameSink.PeakRetainedPixelBytes;
    public double FrameProcessingMilliseconds => frameSink.ProcessingMilliseconds;
    public int ColorProfileChangeCount => frameSink.ColorProfileChangeCount;

    public IReadOnlyList<string> CaptureDiagnosticLines()
    {
        var path = Quote(OutputPath);
        var lines = new List<string>
        {
            $"GIF_CAPTURE_STATS path={path} sourceFrames={SourceFrameCount} retainedFrames={FrameCount} " +
            $"duplicateFrames={DuplicateFramesCoalesced} sampledFrames={SampledFramesSkipped} blankFrames={BlankFrameCount} " +
            $"iccFrames={frameSink.IccProfileFrameCount} cicpFrames={frameSink.CicpProfileFrameCount} gammaFrames={frameSink.GammaMetadataFrameCount} " +
            $"profileChanges={ColorProfileChangeCount} peakRetainedPixelBytes={PeakRetainedPixelBytes} processingMs={FrameProcessingMilliseconds.ToString("0.00", System.Globalization.CultureInfo.InvariantCulture)} " +
            $"budgetBytes={frameSink.BudgetBytes?.ToString() ?? "none"} budgetApplied={frameSink.BudgetApplied.ToString().ToLowerInvariant()} budgetMet={frameSink.BudgetMet.ToString().ToLowerInvariant()} " +
                $"budgetAttempts={frameSink.BudgetAttempts} finalSizeBytes={frameSink.FinalSizeBytes} finalQuality={frameSink.FinalBudgetQuality.ToString().ToLowerInvariant()} finalScale={frameSink.FinalBudgetScale.ToString("0.##", System.Globalization.CultureInfo.InvariantCulture)}"
        };
        if (frameSink.Geometry is { } geometry)
            lines.Add($"GIF_CAPTURE_GEOMETRY path={path} coordinateSpace={geometry.CoordinateSpace} " +
                $"pageZoom={Number(geometry.PageZoom)} visualScale={Number(geometry.VisualScale)} " +
                $"devicePixelRatio={Number(geometry.DevicePixelRatio)} visualOffsetX={Number(geometry.VisualOffsetX)} " +
                $"visualOffsetY={Number(geometry.VisualOffsetY)} correction=css-pixel-preserving");
        if (LongWaitCount > 0) lines.Add($"GIF_WAIT_COMPRESSION path={path} waits={LongWaitCount} savedMs={LongWaitMillisecondsSaved}");
        if (SourceFrameCount >= 5 && DuplicateFramesCoalesced / (double)SourceFrameCount >= .6)
            lines.Add($"GIF_WARN_UNCHANGED path={path} sourceFrames={SourceFrameCount} duplicateFrames={DuplicateFramesCoalesced}");
        if (SourceFrameCount >= 2 && FrameCount >= 1 && BlankFrameCount / (double)FrameCount >= .8)
            lines.Add($"GIF_WARN_BLANK path={path} retainedFrames={FrameCount} blankFrames={BlankFrameCount}");
        if (ColorProfileChangeCount > 0)
            lines.Add($"GIF_WARN_COLOR_PROFILE path={path} profileChanges={ColorProfileChangeCount}");
        lines.AddRange(evidenceWarnings);
        return lines;
    }

    private void AddCapturedFrame(
        byte[] screenshot,
        int delay,
        string kind,
        BrowserScriptAction? action = null)
    {
        var debugCount = debugFrames.Count;
        RecordDebugFrame(delay * 10, kind, action);
        var source = action ?? activeAction;
        var optimization = options.EffectiveCaptureOptimization.WithOptions(
            source?.Options ?? EmptyCaptureOptions,
            $"{source?.Name ?? "gif"} option");
        var result = frameSink.AddFrame(
            screenshot,
            delay,
            optimization.CoalesceDuplicates);
        if (result.Retained) return;
        if (debugFrames.Count > debugCount) debugFrames.RemoveAt(debugFrames.Count - 1);
        if (result.DelayMergedToPrevious && debugFrames.Count > 0 && debugFrames[^1].FrameIndex == result.FrameIndex)
        {
            debugFrames[^1] = debugFrames[^1] with
            {
                DelayMilliseconds = debugFrames[^1].DelayMilliseconds + delay * 10
            };
        }
    }

    private bool TrySkipCapture(int delay, BrowserScriptAction? action, bool sampleEligible)
    {
        if (!sampleEligible) return false;
        var source = action ?? activeAction;
        var optimization = options.EffectiveCaptureOptimization.WithOptions(
            source?.Options ?? EmptyCaptureOptions,
            $"{source?.Name ?? "gif"} option");
        sampleCandidateCount++;
        if (optimization.SampleEvery <= 1 || sampleCandidateCount % optimization.SampleEvery == 1) return false;
        if (!frameSink.TrySkipSampledFrame(delay)) return false;
        ExtendLastDebugDelay(delay * 10, frameSink.FrameCount - 1);
        return true;
    }

    private void ExtendLastDebugDelay(int delayMilliseconds, int frameIndex)
    {
        if (debugFrames.Count > 0 && debugFrames[^1].FrameIndex == frameIndex)
            debugFrames[^1] = debugFrames[^1] with { DelayMilliseconds = debugFrames[^1].DelayMilliseconds + delayMilliseconds };
    }

    private static readonly IReadOnlyDictionary<string, string> EmptyCaptureOptions =
        new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

    private static string Quote(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal)}\"";

    private static string Number(double value) =>
        value.ToString("0.###", System.Globalization.CultureInfo.InvariantCulture);
}
