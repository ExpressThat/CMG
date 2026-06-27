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
    public void RunText_SetBlockStoresStorageGetterValueOnly()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("token-value");
        client.EvaluateResponses.Enqueue("token-value");

        var result = Runner().RunText("""
        set token {
          localStorage get "token"
        }
        evaluate "'${token}'"
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("SET ", StringComparison.Ordinal) && line.Contains("token token-value", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.StartsWith("EVALUATE ", StringComparison.Ordinal) && line.Contains("token-value", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_SetBlockStoresMissingStorageGetterAsEmptyValue()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue(string.Empty);

        var result = Runner().RunText("""
        set token {
          localStorage get "token"
        }
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("SET ", StringComparison.Ordinal) && line.Contains(" token", StringComparison.Ordinal));
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
