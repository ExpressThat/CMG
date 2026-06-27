using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerElementDomActionTests
{
    [Fact]
    public void RunText_FillClearsAndTypesThroughResolvedLocator()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("fill text=Name Ross", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastClearedSelector);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastTypedSelector);
        Assert.Equal("Ross", client.LastTypedText);
    }

    [Theory]
    [InlineData("check \"#enabled\"", "checked = true")]
    [InlineData("uncheck \"#enabled\"", "checked = false")]
    [InlineData("focus \"#name\"", "focus({ preventScroll: true })")]
    [InlineData("blur \"#name\"", "blur()")]
    [InlineData("selectText \"#name\"", "select?.()")]
    public void RunText_ElementDomActionsEvaluateExpectedScript(string script, string expected)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success);
        Assert.Contains(expected, client.LastExpression);
    }

    [Fact]
    public void RunText_SelectOptionAliasesSelect()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("selectOption label=Plan pro", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastSelectedSelector);
        Assert.Equal("pro", client.LastSelectedValue);
    }

    [Fact]
    public void RunText_HighlightDrawsTemporaryOverlay()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("highlight \"#save\" message=Save color=#2563eb duration=250", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("data-cmg-highlight", client.LastExpression);
        Assert.Contains("#2563eb", client.LastExpression);
        Assert.Contains("Save", client.LastExpression);
        Assert.Contains("setTimeout(() => overlay.remove(), 250)", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("HIGHLIGHT 001 #save", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
