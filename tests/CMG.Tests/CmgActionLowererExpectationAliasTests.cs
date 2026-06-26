using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererExpectationAliasTests
{
    [Theory]
    [InlineData("toHaveText", "assertText \"#target\" \"Saved\"", "Saved")]
    [InlineData("containsText", "assertText \"#target\" \"Saved\"", "Saved")]
    [InlineData("waitForText", "assertText \"#target\" \"Saved\"", "Saved")]
    [InlineData("expectNoText", "expectNoText \"#target\" \"Saved\"", "Saved")]
    [InlineData("notContainsText", "notContainsText \"#target\" \"Saved\"", "Saved")]
    [InlineData("toNotContainText", "toNotContainText \"#target\" \"Saved\"", "Saved")]
    [InlineData("toHaveNoText", "toHaveNoText \"#target\" \"Saved\"", "Saved")]
    [InlineData("toBeVisible", "expectVisible \"#target\"")]
    [InlineData("waitForVisible", "expectVisible \"#target\"")]
    [InlineData("toBeHidden", "expectHidden \"#target\"")]
    [InlineData("expectNotVisible", "expectHidden \"#target\"")]
    [InlineData("toBeNotVisible", "expectHidden \"#target\"")]
    [InlineData("expectNotHidden", "expectVisible \"#target\"")]
    [InlineData("toBeNotHidden", "expectVisible \"#target\"")]
    [InlineData("waitForHidden", "expectHidden \"#target\"")]
    [InlineData("toBeEnabled", "expectEnabled \"#target\"")]
    [InlineData("expectNotDisabled", "expectEnabled \"#target\"")]
    [InlineData("toBeNotDisabled", "expectEnabled \"#target\"")]
    [InlineData("toBeDisabled", "expectDisabled \"#target\"")]
    [InlineData("expectNotEnabled", "expectDisabled \"#target\"")]
    [InlineData("toBeNotEnabled", "expectDisabled \"#target\"")]
    public void Lower_PlaywrightExpectationAliasesUseCmgAssertions(string name, string expected, string? value = null)
    {
        var args = value is null ? ["#target"] : new[] { "#target", value };
        var line = Assert.Single(new CmgActionLowerer().Lower(Node(name, args)));

        Assert.Equal(expected, line);
    }

    [Theory]
    [InlineData("expectNotAttached", "expectDetached \"#target\"")]
    [InlineData("toBeNotAttached", "expectDetached \"#target\"")]
    [InlineData("expectNotDetached", "expectAttached \"#target\"")]
    [InlineData("toBeNotDetached", "expectAttached \"#target\"")]
    [InlineData("expectNotEditable", "expectNotEditable \"#target\"")]
    [InlineData("toBeNotEditable", "expectNotEditable \"#target\"")]
    [InlineData("expectNotEmpty", "expectNotEmpty \"#target\"")]
    [InlineData("toBeNotEmpty", "expectNotEmpty \"#target\"")]
    [InlineData("expectNotFocused", "expectNotFocused \"#target\"")]
    [InlineData("toBeNotFocused", "expectNotFocused \"#target\"")]
    [InlineData("expectNotInViewport", "expectNotInViewport \"#target\"")]
    [InlineData("toBeNotInViewport", "expectNotInViewport \"#target\"")]
    public void Lower_NegativeStateAliasesUseCmgAssertions(string name, string expected)
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node(name, ["#target"])));

        Assert.Equal(expected, line);
    }

    [Fact]
    public void Lower_CypressContainsDefaultsToBody()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("contains", ["Saved"])));

        Assert.Equal("assertText \"body\" \"Saved\"", line);
    }

    [Fact]
    public void Lower_NegativeContainsDefaultsToBody()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("notContains", ["Error"])));

        Assert.Equal("notContains \"body\" \"Error\"", line);
    }

    [Theory]
    [InlineData("toHaveValue", "Expected value")]
    [InlineData("toHaveValues", "Expected values")]
    [InlineData("toHaveAttribute", "Expected attribute")]
    [InlineData("toHaveAccessibleName", "Expected accessible name")]
    [InlineData("toHaveRole", "Expected role")]
    [InlineData("toBeChecked", "Expected checked")]
    [InlineData("toHaveCount", "Expected ' + expected + ' elements")]
    public void Lower_PlaywrightElementExpectationAliasesEmitBrowserAssertions(string name, string expected)
    {
        var args = name.Equals("toHaveAttribute", StringComparison.OrdinalIgnoreCase)
            ? new[] { "#target", "aria-label", "Save" }
            : name.Equals("toHaveValues", StringComparison.OrdinalIgnoreCase)
                ? new[] { "#target", "one", "two" }
                : name.Equals("toHaveRole", StringComparison.OrdinalIgnoreCase)
                    ? new[] { "#target", "button" }
                : new[] { "#target", "Save" };

        var line = new CmgActionLowerer().Lower(Node(name, args)).Last();

        Assert.StartsWith("evaluate", line);
        Assert.Contains(expected, line);
    }

    [Theory]
    [InlineData("unchecked")]
    [InlineData("expectUnchecked")]
    [InlineData("toBeUnchecked")]
    [InlineData("expectNotChecked")]
    [InlineData("toBeNotChecked")]
    public void Lower_UncheckedAliasesExpectFalseCheckedState(string name)
    {
        var line = new CmgActionLowerer().Lower(Node(name, ["#target"])).Last();

        Assert.StartsWith("evaluate", line);
        Assert.Contains("const expected = false", line);
    }

    [Fact]
    public void Lower_HiddenExpectationPassesWhenElementIsMissing()
    {
        var line = CmgExpectationScripts.Element(Node("expectHidden", ["#toast"]), "hidden").Last();

        Assert.Contains("if (!element || !element.isConnected) return true", line);
    }

    [Fact]
    public void Lower_VisibleExpectationTreatsTransparentElementAsHidden()
    {
        var line = CmgExpectationScripts.Element(Node("expectVisible", ["#toast"]), "visible").Last();

        Assert.Contains("Number(style.opacity || '1') !== 0", line);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args) =>
        new(1, kind, args.FirstOrDefault() ?? kind, args, new Dictionary<string, string>(), []);
}
