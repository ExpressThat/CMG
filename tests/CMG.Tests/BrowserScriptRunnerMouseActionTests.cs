using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerMouseActionTests
{
    [Fact]
    public void RunText_MouseMoveMovesToCoordinate()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("mouseMove x=12 y=24", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(new ElementPoint(12, 24), client.LastMouseMove);
        Assert.Contains("MOUSE_MOVED 001 12,24", result.StdoutLines);
    }

    [Fact]
    public void RunText_MouseDownAndUpUseAliases()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("mouseDown center\nmouseUp center", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(new ElementPoint(400, 300), client.LastMouseDown);
        Assert.Equal(new ElementPoint(400, 300), client.LastMouseUp);
        Assert.Contains("MOUSE_DOWN 001 400,300", result.StdoutLines);
        Assert.Contains("MOUSE_UP 002 400,300", result.StdoutLines);
    }

    [Fact]
    public void RunText_MouseMoveValidatesTarget()
    {
        var result = Runner().RunText("mouseMove", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("requires either one alias", result.Error);
    }

    [Theory]
    [InlineData("dblclick text=Save", "dblclick", 0, 1)]
    [InlineData("rightClick text=Save", "contextmenu", 2, 2)]
    public void RunText_MouseClickVariantsResolveAndDispatch(string script, string eventName, int button, int buttons)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastHoveredSelector);
        Assert.Contains($"MouseEvent('{eventName}'", client.LastExpression);
        Assert.Contains($"button: {button}", client.LastExpression);
        Assert.Contains($"buttons: {buttons}", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.StartsWith($"MOUSE_EVENT 001 {eventName}", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
