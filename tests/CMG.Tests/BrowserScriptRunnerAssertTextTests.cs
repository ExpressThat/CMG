using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerAssertTextTests
{
    [Fact]
    public void RunText_AssertTextRetriesUntilMatch()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("loading");
        client.TextResponses.Enqueue("ready");

        var result = Runner().RunText("assertText \"#status\" \"ready\" timeout=500", "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("action=assertText #status ready", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_AssertTextReportsTimeoutReason()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("loading");

        var result = Runner().RunText("assertText \"#status\" \"ready\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("within 1ms", result.Error);
        Assert.Contains("loading", result.Error);
    }

    [Fact]
    public void RunText_AssertTextSupportsExactMatchMode()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("assertText \"#status\" \"Ready\" match=exact", "debug", client);

        Assert.True(result.Success);
    }

    [Fact]
    public void RunText_AssertTextSupportsRegexMatchMode()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Saved 42 items");

        var result = Runner().RunText("assertText \"#status\" \"Saved \\\\d+ items\" match=regex", "debug", client);

        Assert.True(result.Success);
    }

    [Fact]
    public void RunText_AssertTextSupportsIgnoreCase()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("READY");

        var result = Runner().RunText("assertText \"#status\" \"ready\" match=exact ignoreCase=true", "debug", client);

        Assert.True(result.Success);
    }

    [Fact]
    public void RunText_AssertTextRejectsInvalidMatchMode()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("assertText \"#status\" \"ready\" match=fuzzy", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("match= must be contains, exact, or regex", result.Error);
    }

    [Fact]
    public void RunText_AssertTextReportsInvalidRegex()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("assertText \"#status\" \"[\" match=regex", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Invalid text regex", result.Error);
    }

    [Fact]
    public void RunText_ContainsWithOneArgumentChecksBodyText()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Welcome back");

        var result = Runner().RunText("contains \"Welcome\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("body", client.LastElementTextSelector);
    }

    [Fact]
    public void RunText_NotContainsWithOneArgumentChecksBodyText()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Welcome back");

        var result = Runner().RunText("notContains \"Error\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("body", client.LastElementTextSelector);
    }

    [Fact]
    public void RunText_NegativeTextAssertionFailsWhenTextRemains()
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Saved with warning");

        var result = Runner().RunText("expectNoText \"#status\" \"warning\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("was still found within 1ms", result.Error);
        Assert.Contains("Saved with warning", result.Error);
    }

    [Theory]
    [InlineData("containsText")]
    [InlineData("waitForText")]
    [InlineData("notContainsText")]
    [InlineData("toNotContainText")]
    [InlineData("toHaveNoText")]
    public void RunText_TextProviderAliasesCheckSelectorText(string action)
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue(action.StartsWith("not", StringComparison.OrdinalIgnoreCase) ||
            action.StartsWith("toNot", StringComparison.OrdinalIgnoreCase) ||
            action.StartsWith("toHaveNo", StringComparison.OrdinalIgnoreCase)
                ? "Ready"
                : "Saved");

        var result = Runner().RunText($"{action} \"#status\" \"Saved\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#status", client.LastElementTextSelector);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
