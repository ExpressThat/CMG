using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerAssertTextTests
{
    [Fact]
    public void RunText_AssertTextRetriesUntilMatch()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("loading");
        client.TextResponses.Enqueue("ready");

        var result = Runner().RunText("assertText \"#status\" \"ready\" timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("PASS 001 assertText", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_AssertTextReportsTimeoutReason()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("loading");

        var result = Runner().RunText("assertText \"#status\" \"ready\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("within 1ms", result.Error);
        Assert.Contains("loading", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
