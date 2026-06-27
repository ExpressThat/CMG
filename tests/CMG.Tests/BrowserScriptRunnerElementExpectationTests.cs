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
    [InlineData("expectAccessibleName", "accessiblename")]
    [InlineData("expectRole", "role")]
    [InlineData("expectChecked", "checked")]
    [InlineData("expectCount", "count")]
    [InlineData("expectValues", "values")]
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
            "expectAccessibleName" => $"{action} \"#target\" \"Save\" timeout=250",
            "expectRole" => $"{action} \"#target\" \"button\" timeout=250",
            "expectCount" => $"{action} \"#target\" 1 timeout=250",
            "expectValues" => $"{action} \"#target\" one two timeout=250",
            _ => $"{action} \"#target\" timeout=250"
        };
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.querySelector", client.LastExpression);
        Assert.Contains($"EXPECT 001 {mode} #target", result.StdoutLines);
    }

    [Theory]
    [InlineData("expectValues")]
    [InlineData("toHaveValues")]
    public void RunText_ElementValuesExpectationUsesSelectedOptions(string action)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"{action} \"#plans\" basic pro", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("selectedOptions", client.LastExpression);
        Assert.Contains("EXPECT 001 values #plans", result.StdoutLines);
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
    [InlineData("expectNotVisible", "hidden")]
    [InlineData("toBeNotVisible", "hidden")]
    [InlineData("expectNotHidden", "visible")]
    [InlineData("toBeNotHidden", "visible")]
    [InlineData("expectNotEnabled", "disabled")]
    [InlineData("toBeNotEnabled", "disabled")]
    [InlineData("expectNotDisabled", "enabled")]
    [InlineData("toBeNotDisabled", "enabled")]
    [InlineData("expectNotAttached", "detached")]
    [InlineData("toBeNotAttached", "detached")]
    [InlineData("expectNotDetached", "attached")]
    [InlineData("toBeNotDetached", "attached")]
    public void RunText_NegativeElementStateAliasesUseInverseExpectation(string action, string mode)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"{action} \"#target\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains($"EXPECT 001 {mode} #target", result.StdoutLines);
    }

    [Theory]
    [InlineData("expectNotEditable", "noteditable")]
    [InlineData("toBeNotEditable", "noteditable")]
    [InlineData("expectNotEmpty", "notempty")]
    [InlineData("toBeNotEmpty", "notempty")]
    [InlineData("expectNotFocused", "notfocused")]
    [InlineData("toBeNotFocused", "notfocused")]
    [InlineData("expectNotInViewport", "notinviewport")]
    [InlineData("toBeNotInViewport", "notinviewport")]
    public void RunText_NegativeElementStateAliasesUseNegativeModes(string action, string mode)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"{action} \"#target\" timeout=250", "debug", client);

        Assert.True(result.Success);
        Assert.Contains($"EXPECT 001 {mode} #target", result.StdoutLines);
    }

    [Theory]
    [InlineData("unchecked")]
    [InlineData("expectUnchecked")]
    [InlineData("toBeUnchecked")]
    [InlineData("expectNotChecked")]
    [InlineData("toBeNotChecked")]
    public void RunText_UncheckedAliasesExpectFalseCheckedState(string action)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"{action} \"#agree\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("const expected = false", client.LastExpression);
        Assert.Contains("EXPECT 001 unchecked #agree", result.StdoutLines);
    }

    [Theory]
    [InlineData("toHaveText \"#status\" \"Saved\"")]
    [InlineData("toBeVisible \"#status\"")]
    [InlineData("toBeNotVisible \"#status\"")]
    [InlineData("waitForVisible \"#status\"")]
    [InlineData("toBeHidden \"#status\"")]
    [InlineData("toBeNotHidden \"#status\"")]
    [InlineData("waitForHidden \"#status\"")]
    [InlineData("toBeEnabled \"#status\"")]
    [InlineData("toBeNotEnabled \"#status\"")]
    [InlineData("toBeDisabled \"#status\"")]
    [InlineData("toBeNotDisabled \"#status\"")]
    [InlineData("toBeAttached \"#status\"")]
    [InlineData("toBeDetached \"#status\"")]
    [InlineData("toBeNotAttached \"#status\"")]
    [InlineData("toBeNotDetached \"#status\"")]
    [InlineData("toBeEditable \"#status\"")]
    [InlineData("toBeNotEditable \"#status\"")]
    [InlineData("toBeEmpty \"#status\"")]
    [InlineData("toBeNotEmpty \"#status\"")]
    [InlineData("toBeFocused \"#status\"")]
    [InlineData("toBeNotFocused \"#status\"")]
    [InlineData("toBeInViewport \"#status\"")]
    [InlineData("toBeNotInViewport \"#status\"")]
    [InlineData("toHaveValue \"#status\" \"Saved\"")]
    [InlineData("toHaveAttribute \"#status\" \"aria-label\" \"Saved\"")]
    [InlineData("toHaveClass \"#status\" \"ready\"")]
    [InlineData("toHaveId \"#status\" \"status\"")]
    [InlineData("toHaveCSS \"#status\" \"display\" \"block\"")]
    [InlineData("toHaveJSProperty \"#status\" \"dataset.ready\" \"true\"")]
    [InlineData("toHaveAccessibleName \"#status\" \"Save\"")]
    [InlineData("toHaveRole \"#status\" \"button\"")]
    [InlineData("toBeChecked \"#status\"")]
    [InlineData("toBeUnchecked \"#status\"")]
    [InlineData("toBeNotChecked \"#status\"")]
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
    public void RunText_ExpectHiddenPassesWhenElementIsMissing()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("expectHidden \"#toast\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("if (!element || !element.isConnected) return true", client.LastExpression);
    }

    [Fact]
    public void RunText_ExpectVisibleChecksOpacity()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("expectVisible \"#toast\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("Number(style.opacity || '1') !== 0", client.LastExpression);
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
