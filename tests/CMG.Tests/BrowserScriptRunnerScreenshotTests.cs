using CMG.Browser;
using CMG.Browser.Scripting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerScreenshotTests
{
    [Fact]
    public void RunText_ScreenshotPagePassesFullPageOption()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("screenshotPage fullPage=true", "debug", client);

        Assert.True(result.Success);
        Assert.True(client.LastFullPageScreenshot);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("SCREENSHOT 001 data:image/png;base64,", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ScreenshotPagePassesImageOptions()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("screenshotPage type=jpeg quality=72 omitBackground=true fullPage=true", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(new ScreenshotOptions("jpeg", 72, FullPage: true, OmitBackground: true), client.LastPageScreenshotOptions);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("SCREENSHOT 001 data:image/jpeg;base64,", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ScreenshotPagePassesClipOptions()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText(
            "screenshotPage clipX=10 clipY=20 clipWidth=300 clipHeight=180",
            "debug",
            client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(new ScreenshotClip(10, 20, 300, 180), client.LastPageScreenshotOptions?.Clip);
    }

    [Fact]
    public void RunText_ScreenshotPageAppliesTemporaryStyle()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("screenshotPage style=\".clock{visibility:hidden}\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("data-cmg-screenshot-style", StringComparison.Ordinal) &&
            expression.Contains(".clock{visibility:hidden}", StringComparison.Ordinal));
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("?.remove(); true", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ScreenshotReadsTemporaryStyleFromFile()
    {
        var path = Path.Combine(Path.GetTempPath(), $"cmg-style-{Guid.NewGuid():N}.css");
        File.WriteAllText(path, ".ad{display:none}");
        var client = new FakeAutomationClient();

        try
        {
            var scriptPath = path.Replace("\\", "\\\\", StringComparison.Ordinal);
            var result = Runner().RunText($"screenshotPage stylePath=\"{scriptPath}\"", "debug", client);

            Assert.True(result.Success, result.Error);
            Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains(".ad{display:none}", StringComparison.Ordinal));
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void RunText_ElementScreenshotPassesImageOptions()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("screenshot #card type=jpg quality=80", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(new ScreenshotOptions("jpeg", 80), client.LastElementScreenshotOptions);
    }

    [Fact]
    public void RunText_ScreenshotPageRejectsInvalidFullPageOption()
    {
        var result = Runner().RunText("screenshotPage fullPage=maybe", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("fullPage= must be true or false", result.Error);
    }

    [Fact]
    public void RunText_ScreenshotRejectsInvalidType()
    {
        var result = Runner().RunText("screenshotPage type=webp", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("type= must be png, jpeg, or jpg", result.Error);
    }

    [Fact]
    public void RunText_ScreenshotRejectsPngQuality()
    {
        var result = Runner().RunText("screenshotPage type=png quality=80", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("quality= is only valid when type=jpeg", result.Error);
    }

    [Fact]
    public void RunText_ScreenshotPageRejectsInvalidClipSize()
    {
        var result = Runner().RunText("screenshotPage clipX=0 clipY=0 clipWidth=0 clipHeight=100", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("clipWidth= and clipHeight= must be greater than 0", result.Error);
    }

    [Fact]
    public void RunText_ScreenshotPageRejectsIncompleteClip()
    {
        var result = Runner().RunText("screenshotPage clipWidth=100 clipHeight=100", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("clip options require clipX=, clipY=, clipWidth=, and clipHeight=", result.Error);
    }

    [Fact]
    public void RunText_ElementScreenshotRejectsClipOptions()
    {
        var result = Runner().RunText("screenshot #card clipX=0 clipY=0 clipWidth=10 clipHeight=10", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("clip options are only valid with screenshotPage", result.Error);
    }

    [Fact]
    public void RunText_ScreenshotRejectsStyleAndStylePathTogether()
    {
        var result = Runner().RunText("screenshotPage style=\"body{}\" stylePath=\"style.css\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("style= and stylePath= cannot be used together", result.Error);
    }

    [Fact]
    public void ScreenshotImage_ClipsBeforeEncoding()
    {
        using var source = new Image<Rgba32>(4, 3, Color.White);
        using var stream = new MemoryStream();
        source.SaveAsPng(stream);

        var bytes = ScreenshotImage.ConvertIfNeeded(
            stream.ToArray(),
            new ScreenshotOptions(Clip: new ScreenshotClip(1, 1, 2, 1)));

        using var clipped = Image.Load<Rgba32>(bytes);
        Assert.Equal(2, clipped.Width);
        Assert.Equal(1, clipped.Height);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
