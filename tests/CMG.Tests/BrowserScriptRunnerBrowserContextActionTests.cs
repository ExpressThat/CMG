using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerBrowserContextActionTests
{
    [Fact]
    public void RunText_NewContextStoresVariableAndCanUseAndClose()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        newContext ctx url="about:blank"
        useContext "${ctx}"
        closeContext "${ctx}"
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal("context-1", client.ActiveBrowserContext);
        Assert.Empty(client.BrowserContexts);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("CONTEXT_CREATED", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.StartsWith("CONTEXT_CLOSED", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ListContextsWritesParseableRows()
    {
        var client = new FakeAutomationClient();
        _ = client.NewBrowserContext("debug", "about:blank");
        var result = Runner().RunText("listContexts", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("id=context-1", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_UseContextRequiresId()
    {
        var result = Runner().RunText("useContext", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Expected 1 positional argument", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
