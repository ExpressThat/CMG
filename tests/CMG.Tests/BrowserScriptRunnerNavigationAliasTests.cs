using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerNavigationAliasTests
{
    [Theory]
    [InlineData("goto")]
    [InlineData("visit")]
    public void RunText_NavigationAliasesUseNormalNavigation(string alias)
    {
        var result = Runner().RunText($"{alias} \"https://example.test\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("NAVIGATED 001 https://example.test", result.StdoutLines);
    }

    [Fact]
    public void RunText_NavigateResolvesRelativeTargetAgainstBaseUrl()
    {
        var result = Runner().RunText(
            "navigate \"checkout\"",
            "debug",
            new FakeAutomationClient(),
            baseUrl: "https://example.test/app/");

        Assert.True(result.Success, result.Error);
        Assert.Contains("NAVIGATED 001 https://example.test/app/checkout", result.StdoutLines);
    }

    [Fact]
    public void RunText_BaseUrlWinsWhenRelativeTargetExistsLocally()
    {
        var file = Path.Combine(Directory.GetCurrentDirectory(), "cmg-local-target.html");
        File.WriteAllText(file, "<!doctype html>");
        try
        {
            var result = Runner().RunText(
                "navigate \"cmg-local-target.html\"",
                "debug",
                new FakeAutomationClient(),
                baseUrl: "https://example.test/app/");

            Assert.True(result.Success, result.Error);
            Assert.Contains("NAVIGATED 001 https://example.test/app/cmg-local-target.html", result.StdoutLines);
        }
        finally
        {
            File.Delete(file);
        }
    }

    [Fact]
    public void RunText_NavigateReportsInvalidBaseUrl()
    {
        var result = Runner().RunText(
            "navigate \"checkout\"",
            "debug",
            new FakeAutomationClient(),
            baseUrl: "example.test");

        Assert.False(result.Success);
        Assert.Contains("baseUrl must be an absolute URL", result.Error);
    }

    [Fact]
    public void RunText_NavigateCanWaitForProviderLoadState()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("navigate \"https://example.test\" waitUntil=domcontentloaded timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("interactive", client.LastExpression);
        Assert.Contains("NAVIGATED 001 https://example.test waitUntil=domcontentloaded state={}", result.StdoutLines);
    }

    [Fact]
    public void RunText_NavigateValidatesWaitUntil()
    {
        var result = Runner().RunText("goto \"https://example.test\" waitUntil=paint", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("goto waitUntil= expects load, domcontentloaded, networkidle, or commit", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
