using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerElementGetterTests
{
    [Theory]
    [InlineData("textContent", "TEXT")]
    [InlineData("innerText", "TEXT")]
    public void RunText_TextGettersReturnElementText(string action, string label)
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Saved");

        var result = Runner().RunText($"{action} \"#status\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#status", client.LastElementTextSelector);
        Assert.Contains($"{label} 001 Saved", result.StdoutLines);
    }

    [Fact]
    public void RunText_InputValueReturnsValuePayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("agent@example.com");

        var result = Runner().RunText("inputValue \"#email\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("element.value", client.LastExpression);
        Assert.Contains("VALUE 001 agent@example.com", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetBlockStoresAttributePayload()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("/profile");
        client.EvaluateResponses.Enqueue("/profile");

        var result = Runner().RunText("""
        set href {
          getAttribute "#profile" "href"
        }
        evaluate "'${href}'"
        """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains("ATTRIBUTE 002 /profile", result.StdoutLines);
        Assert.Contains("SET 001 href /profile", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
