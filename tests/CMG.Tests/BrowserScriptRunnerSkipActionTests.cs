using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerSkipActionTests
{
    [Fact]
    public void RunText_SkipStopsScriptAsSkipped()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        skip "Feature flag disabled"
        type "#after" "should-not-run"
        """, "debug", client);

        Assert.False(result.Success);
        Assert.True(result.Skipped);
        Assert.Equal("Feature flag disabled", result.Error);
        Assert.Contains("SKIP 001 Feature flag disabled", result.StdoutLines);
        Assert.Equal(string.Empty, client.LastTypedText);
    }

    [Fact]
    public void RunText_SkipUsesDefaultReason()
    {
        var result = Runner().RunText("skip", "debug", new FakeAutomationClient());

        Assert.True(result.Skipped);
        Assert.Equal("Skipped by script.", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
