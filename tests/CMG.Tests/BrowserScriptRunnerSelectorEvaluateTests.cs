using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerSelectorEvaluateTests
{
    [Fact]
    public void RunText_EvaluateOnSelectorReturnsParseableOutput()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Save");

        var result = Runner().RunText("evaluateOnSelector \"#save\" \"element.textContent\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.querySelector(\"#save\")", client.LastExpression);
        Assert.Contains("EVALUATE 001 Save", result.StdoutLines);
    }

    [Fact]
    public void RunText_EvaluateAllPassesElementsArray()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("evalAll \".item\" \"elements => elements.length\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.querySelectorAll(\".item\")", client.LastExpression);
        Assert.Contains("value(elements)", client.LastExpression);
    }

    [Fact]
    public void RunText_SetBlockStoresSelectorEvaluationPayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Settings");
        client.EvaluateResponses.Enqueue("Settings");

        var result = Runner().RunText("""
        set label {
          evalOnSelector "#save" "element.textContent"
        }
        evaluate "'${label}'"
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains("SET 001 label Settings", result.StdoutLines);
        Assert.Contains("EVALUATE 004 Settings", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
