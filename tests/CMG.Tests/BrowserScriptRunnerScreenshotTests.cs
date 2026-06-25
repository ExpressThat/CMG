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
    public void RunText_ScreenshotPageRejectsInvalidFullPageOption()
    {
        var result = Runner().RunText("screenshotPage fullPage=maybe", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("fullPage= must be true or false", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
