using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerFailActionTests
{
    [Fact]
    public void RunText_FailStopsWithCustomReason()
    {
        var result = Runner().RunText("fail \"Missing required setup\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Equal("Line 1: fail failed. Missing required setup", result.Error);
    }

    [Fact]
    public void RunText_FailCanBeRecoveredByCatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        try {
          fail "Expected branch"
        } catch error {
          type "#status" "${error}"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal("Line 2: fail failed. Expected branch", client.LastTypedText);
    }

    [Fact]
    public void RunText_FailRequiresMessage()
    {
        var result = Runner().RunText("fail", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Expected 1-2147483647 positional argument(s)", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
