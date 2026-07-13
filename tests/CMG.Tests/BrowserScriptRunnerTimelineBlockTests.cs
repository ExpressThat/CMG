using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerTimelineBlockTests
{
    [Theory]
    [InlineData("speedUpGif", 2, 500)]
    [InlineData("slowDownGif", 2, 2000)]
    public void PlaybackBlock_ScalesEncodedDuration(string block, double factor, int expectedMilliseconds)
    {
        using var gif = new TempGifFile();

        var result = Runner().RunText($"{block} factor={factor} {{ pauseGif 1000 }}", "debug", new FakeAutomationClient(), gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(expectedMilliseconds, GifInspector.Inspect(gif.File).DurationMilliseconds);
    }

    [Fact]
    public void NestedPlaybackBlocksRestoreAndComposeRates()
    {
        using var gif = new TempGifFile();
        var script = "speedUpGif factor=2 { slowDownGif factor=2 { pauseGif 1000 } }\npauseGif 1000";

        var result = Runner().RunText(script, "debug", new FakeAutomationClient(), gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(2000, GifInspector.Inspect(gif.File).DurationMilliseconds);
    }

    [Fact]
    public void HiddenBlockExecutesActionsWithoutAddingFrames()
    {
        using var baselineGif = new TempGifFile();
        using var hiddenGif = new TempGifFile();
        var baselineClient = new FakeAutomationClient();
        var hiddenClient = new FakeAutomationClient();

        var baseline = Runner().RunText("click \"#before\"\nclick \"#after\"", "debug", baselineClient, baselineGif.File);
        var hidden = Runner().RunText("click \"#before\"\nhideFromGif { click \"#hidden\"; pauseGif 1000 }\nclick \"#after\"", "debug", hiddenClient, hiddenGif.File);

        Assert.True(baseline.Success, baseline.Error);
        Assert.True(hidden.Success, hidden.Error);
        Assert.Equal(3, hiddenClient.ClickCount);
        Assert.Equal(GifInspector.Inspect(baselineGif.File).FrameCount, GifInspector.Inspect(hiddenGif.File).FrameCount);
        Assert.True(hiddenClient.RemoveDomCursorCalled);
        Assert.Contains(hidden.StdoutLines, line => line.Contains("GIF_PAUSE", StringComparison.Ordinal) && line.Contains("status=suppressed reason=timeline-cut", StringComparison.Ordinal));
    }

    [Fact]
    public void HiddenBlockSuppressesNestedGifArtifact()
    {
        using var outerGif = new TempGifFile();
        var nestedPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var script = $"hideFromGif {{ gif \"nested\" output=\"{Slash(nestedPath)}\" {{ click \"#hidden\" }} }}\npauseGif 100";

        var result = Runner().RunText(script, "debug", new FakeAutomationClient(), outerGif.File);

        Assert.True(result.Success, result.Error);
        Assert.False(File.Exists(nestedPath));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_BLOCK_SUPPRESSED", StringComparison.Ordinal));
    }

    [Fact]
    public void WithoutGif_TimelineBlocksExecuteWithoutPointer()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("cutGif { click \"#save\" }\nspeedUpGif factor=2 { click \"#next\" }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(2, client.ClickCount);
        Assert.Empty(client.CursorStates);
        Assert.Contains(result.StdoutLines, line => line.Contains("status=inactive", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("speedUpGif factor=0 { pauseGif 100 }")]
    [InlineData("slowDownGif factor=101 { pauseGif 100 }")]
    [InlineData("speedUpGif { pauseGif 100 }")]
    public void PlaybackBlock_RejectsInvalidFactor(string script)
    {
        using var gif = new TempGifFile();

        var result = Runner().RunText(script, "debug", new FakeAutomationClient(), gif.File);

        Assert.False(result.Success);
        Assert.Contains("factor= must be greater than zero and at most 100", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
    private static string Slash(string path) => path.Replace("\\", "/", StringComparison.Ordinal);

    private sealed class TempGifFile : IDisposable
    {
        public FileInfo File { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
        public void Dispose() { if (File.Exists) File.Delete(); }
    }
}
