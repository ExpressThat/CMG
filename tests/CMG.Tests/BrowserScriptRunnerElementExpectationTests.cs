using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerElementExpectationTests
{
    [Theory]
    [InlineData("expectVisible", "visible")]
    [InlineData("waitForVisible", "visible")]
    [InlineData("expectHidden", "hidden")]
    [InlineData("waitForHidden", "hidden")]
    [InlineData("expectEnabled", "enabled")]
    [InlineData("expectDisabled", "disabled")]
    [InlineData("expectAttached", "attached")]
    [InlineData("expectDetached", "detached")]
    [InlineData("expectEditable", "editable")]
    [InlineData("expectEmpty", "empty")]
    [InlineData("expectFocused", "focused")]
    [InlineData("expectInViewport", "inviewport")]
    [InlineData("expectValue", "value")]
    [InlineData("expectClass", "class")]
    [InlineData("expectId", "id")]
    [InlineData("expectCSS", "css")]
    [InlineData("expectProperty", "property")]
    [InlineData("expectChecked", "checked")]
    [InlineData("expectCount", "count")]
    public void RunText_ElementExpectationOutputsExpectationLine(string action, string mode)
    {
        var client = new FakeAutomationClient();
        var script = action switch
        {
            "expectValue" => $"{action} \"#target\" \"saved\" timeout=250",
            "expectClass" => $"{action} \"#target\" \"ready\" timeout=250",
            "expectId" => $"{action} \"#target\" \"target\" timeout=250",
            "expectCSS" => $"{action} \"#target\" \"display\" \"block\" timeout=250",
            "expectProperty" => $"{action} \"#target\" \"dataset.ready\" \"true\" timeout=250",
            "expectCount" => $"{action} \"#target\" 1 timeout=250",
            _ => $"{action} \"#target\" timeout=250"
        };
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.querySelector", client.LastExpression);
        Assert.Contains($"EXPECT 001 {mode} #target", result.StdoutLines);
    }

    [Fact]
    public void RunText_ExpectAttributeUsesDirectExpectation()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("expectAttribute \"#target\" aria-label Save", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("getAttribute", client.LastExpression);
        Assert.Contains("EXPECT 001 attribute #target", result.StdoutLines);
    }

    [Theory]
    [InlineData("toHaveText \"#status\" \"Saved\"")]
    [InlineData("toBeVisible \"#status\"")]
    [InlineData("waitForVisible \"#status\"")]
    [InlineData("toBeHidden \"#status\"")]
    [InlineData("waitForHidden \"#status\"")]
    [InlineData("toBeEnabled \"#status\"")]
    [InlineData("toBeDisabled \"#status\"")]
    [InlineData("toBeAttached \"#status\"")]
    [InlineData("toBeDetached \"#status\"")]
    [InlineData("toBeEditable \"#status\"")]
    [InlineData("toBeEmpty \"#status\"")]
    [InlineData("toBeFocused \"#status\"")]
    [InlineData("toBeInViewport \"#status\"")]
    [InlineData("toHaveValue \"#status\" \"Saved\"")]
    [InlineData("toHaveAttribute \"#status\" \"aria-label\" \"Saved\"")]
    [InlineData("toHaveClass \"#status\" \"ready\"")]
    [InlineData("toHaveId \"#status\" \"status\"")]
    [InlineData("toHaveCSS \"#status\" \"display\" \"block\"")]
    [InlineData("toHaveJSProperty \"#status\" \"dataset.ready\" \"true\"")]
    [InlineData("toBeChecked \"#status\"")]
    [InlineData("toHaveCount \"#status\" 1")]
    public void RunText_PlaywrightExpectationAliasesExecute(string script)
    {
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Saved");
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success);
        Assert.DoesNotContain("Unknown", result.Error ?? string.Empty);
    }

    [Fact]
    public void RunText_ElementExpectationRunsLocatorPrefix()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("expectVisible \"text=Save\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(2, client.EvaluatedExpressions.Count);
        Assert.Contains("data-cmg-locator-id", client.EvaluatedExpressions[0]);
    }

    [Fact]
    public void RunText_ElementExpectationRequiresSelector()
    {
        var result = Runner().RunText("expectVisible", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("received too few arguments", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
