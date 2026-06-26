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

    [Theory]
    [InlineData("softExpect")]
    [InlineData("softAssert")]
    [InlineData("expect.soft")]
    [InlineData("assert.soft")]
    public void RunText_SoftExpectContinuesAndFailsAtEnd(string action)
    {
        var result = Runner().RunText($$"""
        {{action}} (1 > 2) message="first soft failure"
        type "#after" "continued"
        {{action}} ("ready" == "ready")
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("first soft failure", result.Error);
        Assert.Contains(result.StdoutLines, line => line.Contains("SOFT_EXPECT 001 false first soft failure", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("type #after continued", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("SOFT_EXPECT 003 true", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
