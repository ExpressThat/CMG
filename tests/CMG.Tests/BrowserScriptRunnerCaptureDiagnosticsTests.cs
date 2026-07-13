using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerCaptureDiagnosticsTests
{
    [Fact]
    public void RepeatedBlankFrames_AreCoalescedAndExplained()
    {
        using var gif = new TempGif();

        var result = Runner().RunText(
            "caption Explain duration=400 fadeIn=200 fadeOut=200",
            "debug",
            new FakeAutomationClient(),
            gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(1, GifInspector.Inspect(gif.File).FrameCount);
        Assert.Equal(800, GifInspector.Inspect(gif.File).DurationMilliseconds);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_CAPTURE_STATS", StringComparison.Ordinal) &&
            line.Contains("sourceFrames=5", StringComparison.Ordinal) && line.Contains("retainedFrames=1", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.StartsWith("GIF_WARN_UNCHANGED", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.StartsWith("GIF_WARN_BLANK", StringComparison.Ordinal));
    }

    [Fact]
    public void CoalescingCanBeDisabledInDslScope()
    {
        using var gif = new TempGif();

        var result = Runner().RunText(
            "recording coalesceDuplicates=false { caption Explain duration=400 fadeIn=200 fadeOut=200 }",
            "debug",
            new FakeAutomationClient(),
            gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(5, GifInspector.Inspect(gif.File).FrameCount);
    }

    [Fact]
    public void DiagnosticsAreInertWithoutRecorder()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("recording sampleEvery=4 { hover #save }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.DoesNotContain(result.StdoutLines, line => line.StartsWith("GIF_", StringComparison.Ordinal));
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Empty(client.CursorStates);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGif : IDisposable
    {
        public FileInfo File { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
        public void Dispose() { if (File.Exists) File.Delete(); }
    }
}
