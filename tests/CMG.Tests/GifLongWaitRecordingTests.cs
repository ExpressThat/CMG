using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class GifLongWaitRecordingTests
{
    [Fact]
    public void PauseGif_CompressesLongWaitWithProgressAndDiagnostics()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();

        var result = Runner().RunText("recording coalesceDuplicates=false { pauseGif 5000 }", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(3, GifInspector.Inspect(gif.File).FrameCount);
        Assert.Equal(1200, GifInspector.Inspect(gif.File).DurationMilliseconds);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_WAIT_COMPRESSION", StringComparison.Ordinal) && line.Contains("savedMs=3800", StringComparison.Ordinal));
        Assert.Contains(client.EvaluatedExpressions, script => script.Contains("__cmg_wait_progress", StringComparison.Ordinal));
    }

    [Fact]
    public void PauseGif_CompressionCanBeDisabled()
    {
        using var gif = new TempGif();

        var result = Runner().RunText("recording coalesceDuplicates=false compressLongWaits=false { pauseGif 2100 }", "debug", new FakeAutomationClient(), gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(2100, GifInspector.Inspect(gif.File).DurationMilliseconds);
    }

    [Fact]
    public void LongWaitControls_AreInertWithoutRecorder()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("recording compressLongWaits=true waitProgress=true { pauseGif 5000 }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.DoesNotContain(client.EvaluatedExpressions, script => script.Contains("__cmg_wait_progress", StringComparison.Ordinal));
        Assert.Empty(client.CursorStates);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
    private sealed class TempGif : IDisposable
    {
        public FileInfo File { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
        public void Dispose() { if (File.Exists) File.Delete(); }
    }
}
