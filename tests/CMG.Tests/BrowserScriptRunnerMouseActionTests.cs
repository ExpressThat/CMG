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
    [InlineData("doubleClick text=Save", "dblclick", 0, 1)]
    [InlineData("rightClick text=Save", "contextmenu", 2, 2)]
    [InlineData("contextClick text=Save", "contextmenu", 2, 2)]
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

    [Fact]
    public void RunText_ClickWithoutOptionsUsesNativeClick()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("click #save", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#save", client.LastClickedSelector);
        Assert.Empty(client.LastHoveredSelector);
    }

    [Fact]
    public void RunText_ClickWithOptionsDispatchesConfiguredMouseSequence()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("click #save button=middle clickCount=2 delay=10 modifiers=Control+Shift x=4 y=8", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#save", client.LastHoveredSelector);
        Assert.Contains("new MouseEvent('auxclick'", client.LastExpression);
        Assert.Contains("button: 1", client.LastExpression);
        Assert.Contains("buttons: 4", client.LastExpression);
        Assert.Contains("ctrlKey: true", client.LastExpression);
        Assert.Contains("shiftKey: true", client.LastExpression);
        Assert.Contains("rect.left + 4", client.LastExpression);
        Assert.Contains("rect.top + 8", client.LastExpression);
        Assert.Contains("await wait(10)", client.LastExpression);
        Assert.Contains("MOUSE_EVENT 001 auxclick #save", result.StdoutLines);
    }

    [Theory]
    [InlineData("button=side", "button= must be left")]
    [InlineData("clickCount=0", "clickCount= must be one")]
    [InlineData("delay=-1", "delay= must be zero")]
    [InlineData("modifiers=Hyper", "modifiers= supports")]
    [InlineData("x=-1", "x= must be zero")]
    public void RunText_ClickOptionsValidateInput(string option, string expected)
    {
        var result = Runner().RunText($"click #save {option}", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains(expected, result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
