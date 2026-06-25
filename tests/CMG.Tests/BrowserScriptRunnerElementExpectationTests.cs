using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerElementExpectationTests
{
    [Theory]
    [InlineData("expectVisible", "visible")]
    [InlineData("expectHidden", "hidden")]
    [InlineData("expectEnabled", "enabled")]
    [InlineData("expectDisabled", "disabled")]
    public void RunText_ElementExpectationOutputsExpectationLine(string action, string mode)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"{action} \"#target\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.querySelector", client.LastExpression);
        Assert.Contains($"EXPECT 001 {mode} #target", result.StdoutLines);
    }

    [Fact]
    public void RunText_ElementExpectationRunsLocatorPrefix()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("expectVisible \"text=Save\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(2, client.EvaluatedExpressions.Count);
        Assert.Contains("data-cmg-locator-id", client.EvaluatedExpressions[0]);
    }

    [Fact]
    public void RunText_ElementExpectationRequiresSelector()
    {
        var result = Runner().RunText("expectVisible", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("received too few arguments", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
