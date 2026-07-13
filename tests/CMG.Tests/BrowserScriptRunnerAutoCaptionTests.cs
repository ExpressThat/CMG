using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerAutoCaptionTests
{
    [Fact]
    public void RecordingScope_AutoCaptionsRecordedAction()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();

        var result = Runner().RunText("recording autoCaptions=true captionPosition=auto { click \"#save\" }", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal("Click #save", client.LastMessageBar);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("placeAtBottom", StringComparison.Ordinal));
    }

    [Fact]
    public void RecordingScope_ActionCanDisableAutoCaption()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();

        var result = Runner().RunText("recording autoCaptions=true { click \"#save\" autoCaptions=false }", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(string.Empty, client.LastMessageBar);
    }

    [Fact]
    public void WithoutGif_AutoCaptionsDoNotTouchPage()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("recording autoCaptions=true { click \"#save\" }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(string.Empty, client.LastMessageBar);
        Assert.Empty(client.CursorStates);
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
