using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerScrollActionTests
{
    [Fact]
    public void RunText_ScrollToWindowCoordinates()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("scrollTo 0 250", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("window.scrollTo(0, 250)", client.LastExpression);
        Assert.Contains("SCROLL_TO 001 0,250", result.StdoutLines);
    }

    [Fact]
    public void RunText_ScrollByElementSelector()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("scrollBy x=0 y=-80 selector=\"text=Panel\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("No element matched locator text=Panel", client.EvaluatedExpressions[0]);
        Assert.Contains("scrollBy(0, -80)", client.LastExpression);
        Assert.Contains("[data-cmg-locator-id", client.LastExpression);
    }

    [Fact]
    public void RunText_WheelDispatchesWheelEvent()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("wheel \"#pane\" deltaY=-120", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("WheelEvent('wheel'", client.LastExpression);
        Assert.Contains("deltaY: -120", client.LastExpression);
        Assert.Contains("querySelector(\"#pane\")", client.LastExpression);
        Assert.Contains("WHEEL 001 0,-120", result.StdoutLines);
    }

    [Fact]
    public void RunText_ScrollRejectsBadNumber()
    {
        var result = Runner().RunText("scrollBy x=nope", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("x must be a whole number", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
