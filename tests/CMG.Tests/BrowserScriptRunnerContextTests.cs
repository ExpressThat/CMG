using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerContextTests
{
    [Fact]
    public void RunText_ClearContextEvaluatesContextClear()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("clearContext", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("localStorage.clear", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("CONTEXT_CLEARED", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ResetContextNavigatesBlank()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("resetContext", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("about:blank", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("CONTEXT_RESET", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
