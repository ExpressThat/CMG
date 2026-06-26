using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGenericExpectTests
{
    [Fact]
    public void RunText_ExpectSupportsStaticAndVariableConditions()
    {
        var result = Runner().RunText("""
        set count 7
        expect (${count} > 5 && "checkout" in "checkout" "billing")
        assert (${count} != 2)
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success, result.Error);
        Assert.Contains("EXPECT 002 true", result.StdoutLines);
        Assert.Contains("EXPECT 003 true", result.StdoutLines);
    }

    [Fact]
    public void RunText_ExpectSupportsValueProducingActions()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("CMG");

        var result = Runner().RunText("expect evaluate \"document.title\" == \"CMG\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("EXPECT 001 true", result.StdoutLines);
    }

    [Fact]
    public void RunText_ExpectFailureUsesCustomMessage()
    {
        var result = Runner().RunText(
            "expect (1 > 2) message=\"math guard failed\"",
            "debug",
            new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("math guard failed", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
