using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerTypingOptionTests
{
    [Fact]
    public void RunText_TypeWithoutDelayUsesFastType()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("type #name CMG", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#name", client.LastTypedSelector);
        Assert.Equal("CMG", client.LastTypedText);
        Assert.Equal(0, client.LastTypeDelay);
    }

    [Theory]
    [InlineData("type #name CMG delay=25")]
    [InlineData("pressSequentially #name CMG delay=25")]
    [InlineData("type #name CMG typingDelay=25")]
    public void RunText_TypeDelayUsesProgressiveTyping(string script)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#name", client.LastTypedSelector);
        Assert.Equal("CMG", client.LastTypedText);
        Assert.Equal(25, client.LastTypeDelay);
    }

    [Fact]
    public void RunText_FillDelayClearsThenTypesProgressively()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("fill #name CMG delay=10", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#name", client.LastClearedSelector);
        Assert.Equal("#name", client.LastTypedSelector);
        Assert.Equal("CMG", client.LastTypedText);
        Assert.Equal(10, client.LastTypeDelay);
    }

    [Fact]
    public void RunText_RecordingScopeSuppliesTypingDelayAndChildOverridesIt()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(
            "recording typingDelay=40 { fill #name CMG typingDelay=12 }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(12, client.LastTypeDelay);
    }

    [Fact]
    public void RunText_TypeDelayValidatesInput()
    {
        var result = Runner().RunText("type #name CMG delay=-1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("delay= must be zero or greater", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
