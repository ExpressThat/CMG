using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerReturnTests
{
    [Fact]
    public void RunText_MacroCanReturnBlockPayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Agent");
        client.EvaluateResponses.Enqueue("Agent");
        var result = Runner().RunText("""
        macro readName {
          return {
            evaluate "document.querySelector('#name').value"
          }
        }
        set name {
          call readName
        }
        evaluate "'${name}'"
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains(result.StdoutLines, line => line.StartsWith("RETURN ", StringComparison.Ordinal) && line.Contains("Agent", StringComparison.Ordinal));
        Assert.Contains("SET 006 name Agent", result.StdoutLines);
        Assert.Contains("EVALUATE 009 Agent", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
