using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerPointerVisibilityTests
{
    [Theory]
    [InlineData("showPointer", "GIF_SHOW_POINTER")]
    [InlineData("hidePointer", "GIF_HIDE_POINTER")]
    public void PointerVisibility_WithoutRecorder_SkipsBeforeValidation(string action, string label)
    {
        var result = Runner().RunText($$"""
            {{action}} "${missing}" {
              click "#ignored"
            }
            """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.StdoutLines, line => line.Contains($"{label} 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void ShowPointer_WithRecorder_CapturesCurrentPointer()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();

        var result = Runner().RunText("showPointer", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(1, client.PageScreenshotCount);
        Assert.NotEmpty(client.CursorStates);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_SHOW_POINTER 001 status=captured", StringComparison.Ordinal));
    }

    [Fact]
    public void HidePointer_WithRecorder_RemovesPointerAndCapturesPage()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();

        var result = Runner().RunText("hidePointer", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(1, client.PageScreenshotCount);
        Assert.True(client.RemoveDomCursorCalled);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_HIDE_POINTER 001 status=captured", StringComparison.Ordinal));
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
