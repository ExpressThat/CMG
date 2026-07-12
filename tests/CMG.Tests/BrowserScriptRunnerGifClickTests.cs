using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifClickTests
{
    [Fact]
    public void RecordedClick_DispatchesExactlyOneBrowserClick()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();

        var result = new BrowserScriptRunner(new BrowserScriptParser())
            .RunText("click \"#save\"", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(1, client.ClickCount);
        Assert.Contains(client.CursorPulseStyles, style => style is not null);
    }

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
