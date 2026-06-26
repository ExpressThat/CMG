using CMG.Browser;
using CMG.Browser.Scripting;

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

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
