using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifFramingTests
{
    [Fact]
    public void GifBlock_CropsEveryCaptureToPaddedSelectorBounds()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(100, 80, 300, 200));

        var result = Runner().RunText($"gif \"crop\" output=\"{Slash(path)}\" crop=\"#panel\" cropPadding=20 {{ pauseGif 100 }}", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#panel", client.LastElementBoxSelector);
        Assert.Equal(new ScreenshotClip(80, 60, 340, 240), client.LastPageScreenshotOptions?.Clip);
        File.Delete(path);
    }

    [Theory]
    [InlineData("scale=0", "scale=")]
    [InlineData("scale=1.1", "scale=")]
    [InlineData("maxWidth=0", "maxWidth=")]
    [InlineData("maxHeight=10001", "maxHeight=")]
    [InlineData("cropPadding=4", "requires crop=")]
    [InlineData("viewport=wide", "viewport=")]
    [InlineData("pixelRatio=5", "pixelRatio=")]
    public void GifBlock_RejectsInvalidFraming(string option, string expected)
    {
        var result = Runner().RunText($"gif \"bad\" {option} {{ pauseGif 10 }}", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains(expected, result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void RecordingFramingWithoutGifDoesNotResolveCropOrCapture()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("recording crop=\"#panel\" scale=0.5 { click \"#save\" }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.True(string.IsNullOrEmpty(client.LastElementBoxSelector));
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Empty(client.CursorStates);
        Assert.Empty(client.ViewportOptionsHistory);
    }

    [Fact]
    public void GifBlock_AppliesAndRestoresRecordingViewport()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var client = new FakeAutomationClient();

        var result = Runner().RunText($"gif \"viewport\" output=\"{Slash(path)}\" viewport=640x480 pixelRatio=2 {{ click \"#save\" }}", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(new ViewportOptions(640, 480, 2), client.ViewportOptionsHistory[0]);
        Assert.Equal(new ViewportOptions(800, 600), client.ViewportOptionsHistory[^1]);
        Assert.Equal("#save", client.LastScrolledSelector);
        File.Delete(path);
    }

    [Fact]
    public void GifBlock_IntroCardUsesBoundsCachedBeforeTargetIsHidden()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(40, 50, 320, 180));
        client.ElementBoxes.Enqueue(new ElementBox(40, 50, 320, 180));

        var result = Runner().RunText($"gif \"intro crop\" output=\"{Slash(path)}\" crop=\"#panel\" intro=Start {{ pauseGif 10 }}", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(new ScreenshotClip(40, 50, 320, 180), client.PageScreenshotOptions[0].Clip);
        File.Delete(path);
    }

    private static string Slash(string path) => path.Replace('\\', '/');
    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
