using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerHoverOptionTests
{
    [Fact]
    public void RunText_HoverWithoutOptionsUsesNativeHover()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("hover #save", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#save", client.LastHoveredSelector);
        Assert.Empty(client.LastExpression);
    }

    [Fact]
    public void RunText_HoverWithOptionsDispatchesConfiguredPointerSequence()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("hover #save modifiers=Control+Shift x=4 y=8", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("new PointerCtor('pointerover'", client.LastExpression);
        Assert.Contains("new MouseEvent('mousemove'", client.LastExpression);
        Assert.Contains("ctrlKey: true", client.LastExpression);
        Assert.Contains("shiftKey: true", client.LastExpression);
        Assert.Contains("rect.left + 4", client.LastExpression);
        Assert.Contains("rect.top + 8", client.LastExpression);
    }

    [Theory]
    [InlineData("modifiers=Hyper", "modifiers= supports")]
    [InlineData("x=-1", "x= must be zero")]
    [InlineData("y=-1", "y= must be zero")]
    public void RunText_HoverOptionsValidateInput(string option, string expected)
    {
        var result = Runner().RunText($"hover #save {option}", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains(expected, result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
