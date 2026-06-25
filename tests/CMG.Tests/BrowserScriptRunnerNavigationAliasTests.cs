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

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
