using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererTests
{
    [Fact]
    public void Lower_FillUsesVisualClearAndType()
    {
        var action = Node("fill", ["#name", "Ross"], []);
        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal(["clear \"#name\"", "type \"#name\" \"Ross\""], lines);
    }

    [Fact]
    public void Lower_StepAddsCaptionBeforeChildren()
    {
        var action = Node("step", ["Open"], [Node("click", ["#open"], [])]);
        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal(3, lines.Count);
        Assert.Equal("showMessageBar \"Open\"", lines[0]);
        Assert.Contains("not actionable", lines[1]);
        Assert.Equal("click \"#open\"", lines[2]);
    }

    [Fact]
    public void Lower_GifBlockCanBeFlattenedWhenCommandGifIsActive()
    {
        var action = Node("gif", ["Only this"], [Node("click", ["#open"], [])]);
        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal(3, lines.Count);
        Assert.Equal("showMessageBar \"Only this\"", lines[0]);
        Assert.Contains("not actionable", lines[1]);
        Assert.Equal("click \"#open\"", lines[2]);
    }

    [Fact]
    public void Lower_PlannedUnsupportedActionFailsExplicitly()
    {
        var action = Node("notARealParityAction", ["/api/**"], []);
        var line = Assert.Single(new CmgActionLowerer().Lower(action));

        Assert.Contains("planned but not implemented", line);
    }

    [Theory]
    [InlineData("check")]
    [InlineData("uncheck")]
    [InlineData("focus")]
    [InlineData("blur")]
    [InlineData("selectText")]
    [InlineData("dblclick")]
    [InlineData("rightClick")]
    public void Lower_VisualElementActionsUsePageFacingCommands(string name)
    {
        var lines = new CmgActionLowerer().Lower(Node(name, ["#target"], []));

        Assert.NotEmpty(lines);
        Assert.All(lines, line => Assert.DoesNotContain("planned but not implemented", line));
    }

    [Fact]
    public void Lower_StorageAndExpectationCommands()
    {
        var lowerer = new CmgActionLowerer();

        Assert.Equal("localStorage \"set\" \"token\" \"abc\"", Assert.Single(lowerer.Lower(Node("localStorage", ["set", "token", "abc"], []))));
        Assert.Equal("cookie", Assert.Single(lowerer.Lower(Node("cookie", [], []))));
        Assert.Equal("expectUrl \"checkout\"", Assert.Single(lowerer.Lower(Node("expectUrl", ["checkout"], []))));
    }

    [Fact]
    public void Lower_WaitForUrlPollsUntilTimeout()
    {
        var action = Node("waitForUrl", ["checkout"], []);
        var line = Assert.Single(new CmgActionLowerer().Lower(action));

        Assert.Equal("waitForUrl \"checkout\"", line);
    }

    [Theory]
    [InlineData("expectValue", "Expected value")]
    [InlineData("expectAttribute", "Expected attribute")]
    [InlineData("expectChecked", "Expected checked")]
    [InlineData("expectCount", "Expected ${expected} elements")]
    public void Lower_ElementExpectationsEmitBrowserAssertions(string name, string expected)
    {
        var args = name.Equals("expectAttribute", StringComparison.OrdinalIgnoreCase)
            ? new[] { "#target", "aria-label", "Save" }
            : new[] { "#target", "Save" };

        var line = new CmgActionLowerer().Lower(Node(name, args, [])).Last();

        Assert.StartsWith("evaluate", line);
        Assert.Contains(expected, line);
    }

    [Fact]
    public void Lower_ElementExpectationAcceptsLocatorOption()
    {
        var node = new CmgNode(1, "expectVisible", "expectVisible", [], new Dictionary<string, string> { ["text"] = "Save" }, []);
        var line = Assert.Single(new CmgActionLowerer().Lower(node));

        Assert.Equal("expectVisible text=\"Save\"", line);
    }

    [Theory]
    [InlineData("expectVisible", "expectVisible \"#target\"")]
    [InlineData("expectHidden", "expectHidden \"#target\"")]
    [InlineData("expectEnabled", "expectEnabled \"#target\"")]
    [InlineData("expectDisabled", "expectDisabled \"#target\"")]
    public void Lower_ElementStateExpectationsPassThrough(string name, string expected)
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node(name, ["#target"], [])));

        Assert.Equal(expected, line);
    }

    [Fact]
    public void Lower_SelectOptionPassesThrough()
    {
        var lines = new CmgActionLowerer().Lower(Node("selectOption", ["#plan", "pro"], []));

        Assert.Equal(2, lines.Count);
        Assert.Contains("not actionable", lines[0]);
        Assert.Equal("selectOption \"#plan\" \"pro\"", lines[1]);
    }

    [Fact]
    public void Lower_ExpectTextPreservesTimeoutOption()
    {
        var node = new CmgNode(1, "expectText", "expectText", ["#status", "Ready"], new Dictionary<string, string> { ["timeout"] = "500" }, []);
        var line = Assert.Single(new CmgActionLowerer().Lower(node));

        Assert.Contains("timeout=\"500\"", line);
    }

    [Fact]
    public void Lower_EmulatePassesOptionsToScriptRunner()
    {
        var node = new CmgNode(1, "emulate", "emulate", [], new Dictionary<string, string> { ["width"] = "390", ["height"] = "844" }, []);
        var line = Assert.Single(new CmgActionLowerer().Lower(node));

        Assert.Equal("emulate width=\"390\" height=\"844\"", line);
    }

    [Fact]
    public void Lower_DownloadUsesActionabilityBeforePointerClick()
    {
        var lines = new CmgActionLowerer().Lower(Node("download", ["#export"], []));

        Assert.Equal(2, lines.Count);
        Assert.Contains("not actionable", lines[0]);
        Assert.Equal("download \"#export\"", lines[1]);
    }

    [Fact]
    public void Lower_WaitForEventPassesThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("waitForEvent", ["dialog", "Saved"], [])));

        Assert.Equal("waitForEvent \"dialog\" \"Saved\"", line);
    }

    [Fact]
    public void Lower_HttpCredentialActionsPassThrough()
    {
        var lowerer = new CmgActionLowerer();

        Assert.Equal("setHttpCredentials \"user\" \"secret\"", Assert.Single(lowerer.Lower(Node("setHttpCredentials", ["user", "secret"], []))));
        Assert.Equal("clearHttpCredentials", Assert.Single(lowerer.Lower(Node("clearHttpCredentials", [], []))));
    }

    [Fact]
    public void Lower_ExposeFunctionActionsPassThrough()
    {
        var lowerer = new CmgActionLowerer();

        Assert.Equal("exposeFunction \"cmg\" \"() => true\"", Assert.Single(lowerer.Lower(Node("exposeFunction", ["cmg", "() => true"], []))));
        Assert.Equal("exposeBinding \"cmg\" \"(source) => source.name\"", Assert.Single(lowerer.Lower(Node("exposeBinding", ["cmg", "(source) => source.name"], []))));
    }

    [Fact]
    public void Lower_RichLocatorMarksElementThenUsesVisualSelector()
    {
        var lines = new CmgActionLowerer().Lower(Node("click", ["role=button"], []));

        Assert.Equal(3, lines.Count);
        Assert.StartsWith("evaluate", lines[0]);
        Assert.Contains("not actionable", lines[1]);
        Assert.Contains("data-cmg-locator-id", lines[2]);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyList<CmgNode> children) =>
        new(1, kind, args.FirstOrDefault() ?? kind, args, new Dictionary<string, string>(), children);
}
