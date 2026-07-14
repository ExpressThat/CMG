using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class GifSplitViewTests
{
    [Fact]
    public void Compose_UsesStableTwoColumnGrid()
    {
        using var composed = Image.Load<Rgba32>(GifSplitViewComposer.Compose([Png(Color.Red), Png(Color.Blue), Png(Color.Green)]));

        Assert.Equal(22, composed.Width);
        Assert.Equal(18, composed.Height);
        Assert.Equal(Color.Red.ToPixel<Rgba32>(), composed[2, 2]);
        Assert.Equal(Color.Blue.ToPixel<Rgba32>(), composed[16, 2]);
        Assert.Equal(Color.Green.ToPixel<Rgba32>(), composed[2, 14]);
    }

    [Fact]
    public void Compose_AlwaysModeReservesSecondTileForFuturePopup()
    {
        using var composed = Image.Load<Rgba32>(GifSplitViewComposer.Compose([Png(Color.Red)], reserveSecondTile: true));

        Assert.Equal((22, 6), (composed.Width, composed.Height));
        Assert.Equal(Color.ParseHex("111827").ToPixel<Rgba32>(), composed[16, 2]);
    }

    [Fact]
    public void Recorder_SplitTabsComposesAndPropagatesRedactionScripts()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var client = new FakeAutomationClient();
        client.TabResponses.Enqueue([new("a", "A", "about:blank"), new("b", "B", "about:blank")]);
        client.TabScreenshotResponses.Enqueue([Png(Color.Red), Png(Color.Blue)]);
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path,
            Framing: new GifFramingOptions(SplitTabs: PointerTargetCalloutMode.Always)));
        recorder.Start("debug");

        recorder.Pause(new BrowserScriptAction(1, "pauseGif", "pauseGif", ["100"], new Dictionary<string, string>(), []));
        recorder.Finish();

        var inspection = GifInspector.Inspect(new FileInfo(path));
        Assert.Equal((22, 6), (inspection.Width, inspection.Height));
        Assert.Contains(client.LastTabPreparationScripts, script => script.Contains("input[type=\"password\"]", StringComparison.Ordinal));
        Assert.Contains(client.LastTabCleanupScripts, script => script.Contains("RemoveGifRedactions", StringComparison.OrdinalIgnoreCase) || script.Contains("data-cmg-gif-redaction", StringComparison.Ordinal));
        File.Delete(path);
    }

    [Fact]
    public void Recorder_AutoSplitWithOneTabUsesNormalCapture()
    {
        var client = new FakeAutomationClient();
        client.TabResponses.Enqueue([new("a", "A", "about:blank")]);
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path,
            Framing: new GifFramingOptions(SplitTabs: PointerTargetCalloutMode.Auto)));
        recorder.Start("debug");

        recorder.Pause(new BrowserScriptAction(1, "pauseGif", "pauseGif", ["100"], new Dictionary<string, string>(), []));

        Assert.Equal(0, client.TabScreenshotCount);
        Assert.Equal(1, client.PageScreenshotCount);
    }

    private static byte[] Png(Color color)
    {
        using var image = new Image<Rgba32>(8, 6, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }
}
