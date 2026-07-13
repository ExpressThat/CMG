using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerPointerEvidenceTests
{
    [Fact]
    public void RunText_MouseDownCapturesPressedStateAfterActualDown()
    {
        using var fixture = new Fixture(new GifPointerEvidenceOptions(MouseDownHoldMilliseconds: 250));

        var result = fixture.Run("mouseDown center pointerDuration=0\nmouseUp center pointerDuration=0");

        Assert.True(result.Success, result.Error);
        Assert.Contains(fixture.Client.CursorStates, state => state.Pressed);
        Assert.False(fixture.Client.LastCursorPressed);
        Assert.Equal(new ElementPoint(400, 300), fixture.Client.LastMouseDown);
        Assert.Equal(new ElementPoint(400, 300), fixture.Client.LastMouseUp);
        Assert.Null(fixture.Client.LastMoveDragPoint);
    }

    [Fact]
    public void RunText_InstantMoveRendersTeleportEvidence()
    {
        using var fixture = new Fixture();

        var result = fixture.Run("moveMouse center pointerSpeed=instant");

        Assert.True(result.Success, result.Error);
        Assert.Contains(fixture.Client.EvaluatedExpressions, expression =>
            expression.Contains("stroke-dasharray", StringComparison.Ordinal) &&
            expression.Contains("const origin = { x: 32, y: 32 }", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_TargetAndFocusEvidenceUseActionOverrides()
    {
        using var fixture = new Fixture();

        var result = fixture.Run("fill #tiny value targetCallout=always focusPulse=true pointerDuration=0 holdAfterAction=100");

        Assert.True(result.Success, result.Error);
        Assert.Contains(fixture.Client.EvaluatedExpressions, expression =>
            expression.Contains("const calloutMode = 'always'", StringComparison.Ordinal));
        Assert.Contains(fixture.Client.EvaluatedExpressions, expression =>
            expression.Contains("const focused = true ? document.activeElement", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_LongPauseUsesThreeFramesAndPreservesDuration()
    {
        using var fixture = new Fixture(new GifPointerEvidenceOptions(IdleThresholdMilliseconds: 500));

        var result = fixture.Run("pauseGif 1500");
        var inspection = GifInspector.Inspect(fixture.Gif);

        Assert.True(result.Success, result.Error);
        Assert.Equal(3, inspection.FrameCount);
        Assert.Equal(1500, inspection.DurationMilliseconds);
        Assert.Contains(fixture.Client.EvaluatedExpressions, expression => expression.Contains("const idlePhase = 3", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WithoutGifDoesNotCreatePointerEvidence()
    {
        var client = new FakeAutomationClient();

        var result = new BrowserScriptRunner(new BrowserScriptParser()).RunText(
            "mouseDown center\npauseGif 1500\nmouseUp center", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Empty(client.CursorStates);
        Assert.Empty(client.EvaluatedExpressions);
    }

    private sealed class Fixture : IDisposable
    {
        public Fixture(GifPointerEvidenceOptions? evidence = null)
        {
            Gif = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
            Encoding = new GifEncodingOptions(
                CaptureOptimization: new GifCaptureOptimizationOptions(CoalesceDuplicates: false),
                PointerEvidence: evidence ?? new());
        }

        public FakeAutomationClient Client { get; } = new();
        public FileInfo Gif { get; }
        private GifEncodingOptions Encoding { get; }

        public ScriptRunResult Run(string script) => new BrowserScriptRunner(new BrowserScriptParser()).RunText(
            script, "debug", Client, Gif, pointerMotion: new ScriptPointerMotionOptions(0),
            holdAfterActionMilliseconds: 0, postClickHoldMilliseconds: 0, gifEncoding: Encoding);

        public void Dispose()
        {
            if (Gif.Exists) Gif.Delete();
        }
    }
}
