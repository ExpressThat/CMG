using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerSetActionTests
{
    [Fact]
    public void RunText_SetLiteralStillStoresVariable()
    {
        var result = Runner().RunText("set greeting hello\nevaluate \"'${greeting}'\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("EVALUATE 002", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_SetBlockStoresEvaluatePayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("CMG Title");
        client.EvaluateResponses.Enqueue("CMG Title");

        var result = Runner().RunText("""
        set title {
          evaluate "document.title"
        }
        evaluate "'${title}'"
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains("SET 001 title CMG Title", result.StdoutLines);
        Assert.Contains("EVALUATE 004 CMG Title", result.StdoutLines);
    }

    [Fact]
    public void RunText_InitialVariablesAreAvailableToActions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(
            "type \"#name\" \"${user}\"",
            "debug",
            client,
            variables: new Dictionary<string, string> { ["user"] = "Ada" });

        Assert.True(result.Success);
        Assert.Equal("Ada", client.LastTypedText);
    }

    [Fact]
    public void RunText_SetBlockStoresLastOutputPayloadFromAnyCommand()
    {
        var result = Runner().RunText("""
        set count {
          waitForTimeout 1
        }
        evaluate "'${count}'"
        """, "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains("SET 001 count 1", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetBlockFailsWhenWrappedActionsProduceNoOutput()
    {
        var result = Runner().RunText("""
        set missing {
          click "#save"
        }
        """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("did not produce output", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
