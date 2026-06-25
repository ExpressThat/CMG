using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerClockTests
{
    [Fact]
    public void RunText_ClockInstallsFakeTime()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("clock now=1000", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("FakeDate", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("CLOCK", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_TickAdvancesFakeTime()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("tick 250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("clock.current", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("TICK", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_TickValidatesMilliseconds()
    {
        var result = Runner().RunText("tick -1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("positive whole number", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
