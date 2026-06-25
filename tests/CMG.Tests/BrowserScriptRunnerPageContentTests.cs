using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerPageContentTests
{
    [Theory]
    [InlineData("url", "https://example.com", "URL")]
    [InlineData("title", "Checkout", "TITLE")]
    [InlineData("content", "<html></html>", "CONTENT")]
    public void RunText_PageReadActionsOutputParseableLines(string action, string value, string label)
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue(value);

        var result = Runner().RunText(action, "debug", client);

        Assert.True(result.Success);
        Assert.Contains($"{label} 001 {value}", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetContentWritesDocument()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("setContent \"<main>CMG</main>\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.write", client.LastExpression);
        Assert.Contains("<main>CMG</main>", client.LastExpression);
        Assert.Contains("CONTENT_SET 001 length=16", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetContentRequiresHtmlArgument()
    {
        var result = Runner().RunText("setContent", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Expected 1 positional argument", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
