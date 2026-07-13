using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerTitleCardTests
{
    [Theory]
    [InlineData("intro", "GIF_INTRO")]
    [InlineData("outro", "GIF_OUTRO")]
    public void TitleCard_WithoutRecorderSkipsBeforeExpansion(string action, string label)
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText($"{action} \"${{missing}}\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.StdoutLines, line => line.Contains($"{label} 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Empty(client.CursorStates);
    }

    [Fact]
    public void ExplicitTitleCardsCaptureWithoutExtraActionFrames()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();

        var result = Runner().RunText("intro \"Start\" duration=300\noutro \"Done\" duration=400", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(2, client.PageScreenshotCount);
        Assert.True(client.RemoveDomCursorCalled);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_INTRO 001 status=captured", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_OUTRO 002 status=captured", StringComparison.Ordinal));
    }

    [Fact]
    public void RecordingDefaultsCaptureIntroAndFinalOutro()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();

        var result = Runner().RunText("recording intro=Welcome outro=Complete introDuration=300 outroDuration=400 { click \"#save\" }", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("textContent = \"Welcome\"", StringComparison.Ordinal));
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("textContent = \"Complete\"", StringComparison.Ordinal));
        Assert.True(client.PageScreenshotCount >= 4);
    }

    [Fact]
    public void TitleCardRejectsZeroDuration()
    {
        using var gif = new TempGifFile();

        var result = Runner().RunText("intro \"Start\" duration=0", "debug", new FakeAutomationClient(), gif.File);

        Assert.False(result.Success);
        Assert.Contains("duration= must be greater than zero", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGifFile : IDisposable
    {
        public FileInfo File { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));

        public void Dispose()
        {
            if (File.Exists)
            {
                File.Delete();
            }
        }
    }
}
