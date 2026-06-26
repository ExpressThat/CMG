using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererExpectationAliasTests
{
    [Theory]
    [InlineData("toHaveText", "assertText \"#target\" \"Saved\"", "Saved")]
    [InlineData("containsText", "assertText \"#target\" \"Saved\"", "Saved")]
    [InlineData("waitForText", "assertText \"#target\" \"Saved\"", "Saved")]
    [InlineData("toBeVisible", "expectVisible \"#target\"")]
    [InlineData("waitForVisible", "expectVisible \"#target\"")]
    [InlineData("toBeHidden", "expectHidden \"#target\"")]
    [InlineData("waitForHidden", "expectHidden \"#target\"")]
    [InlineData("toBeEnabled", "expectEnabled \"#target\"")]
    [InlineData("toBeDisabled", "expectDisabled \"#target\"")]
    public void Lower_PlaywrightExpectationAliasesUseCmgAssertions(string name, string expected, string? value = null)
    {
        var args = value is null ? ["#target"] : new[] { "#target", value };
        var line = Assert.Single(new CmgActionLowerer().Lower(Node(name, args)));

        Assert.Equal(expected, line);
    }

    [Fact]
    public void Lower_CypressContainsDefaultsToBody()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("contains", ["Saved"])));

        Assert.Equal("assertText \"body\" \"Saved\"", line);
    }

    [Theory]
    [InlineData("toHaveValue", "Expected value")]
    [InlineData("toHaveAttribute", "Expected attribute")]
    [InlineData("toBeChecked", "Expected checked")]
    [InlineData("toHaveCount", "Expected ' + expected + ' elements")]
    public void Lower_PlaywrightElementExpectationAliasesEmitBrowserAssertions(string name, string expected)
    {
        var args = name.Equals("toHaveAttribute", StringComparison.OrdinalIgnoreCase)
            ? new[] { "#target", "aria-label", "Save" }
            : new[] { "#target", "Save" };

        var line = new CmgActionLowerer().Lower(Node(name, args)).Last();

        Assert.StartsWith("evaluate", line);
        Assert.Contains(expected, line);
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
