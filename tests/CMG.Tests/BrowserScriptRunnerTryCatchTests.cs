using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerTryCatchTests
{
    [Fact]
    public void RunText_TryCatchHandlesFailureAndBindsError()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        try {
          click "${missing}"
        } catch error {
          set handled {
            return "${error}"
          }
          type "#status" "handled"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("Variable 'missing' is not defined", result.StdoutLines.Single(line => line.StartsWith("SET 004 handled", StringComparison.Ordinal)));
        Assert.Equal("handled", client.LastTypedText);
    }

    [Fact]
    public void RunText_FinallyRunsAfterSuccess()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        try {
          type "#status" "try"
        } finally {
          type "#status" "finally"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Equal("finally", client.LastTypedText);
    }

    [Fact]
    public void RunText_FinallyRunsBeforeUnhandledFailure()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        try {
          click "${missing}"
        } finally {
          type "#status" "finally"
        }
        """, "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Variable 'missing' is not defined", result.Error);
        Assert.Equal("finally", client.LastTypedText);
    }

    [Fact]
    public void RunText_CatchWithoutTryFailsClearly()
    {
        var result = Runner().RunText("""
        catch {
          type "#status" "bad"
        }
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("catch must follow a try block", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
