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
        var action = Node("intercept", ["/api/**"], []);
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

        Assert.Contains("localStorage", Assert.Single(lowerer.Lower(Node("localStorage", ["set", "token", "abc"], []))));
        Assert.Contains("document.cookie", Assert.Single(lowerer.Lower(Node("cookie", [], []))));
        Assert.Contains("Expected URL", Assert.Single(lowerer.Lower(Node("expectUrl", ["checkout"], []))));
    }

    [Fact]
    public void Lower_WaitForUrlPollsUntilTimeout()
    {
        var action = Node("waitForUrl", ["checkout"], []);
        var line = Assert.Single(new CmgActionLowerer().Lower(action));

        Assert.Contains("new Promise", line);
        Assert.Contains("within 5000ms", line);
        Assert.Contains("setTimeout(poll, 50)", line);
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
    public void Lower_ConsoleActionsPassThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("waitForConsole", ["saved"], [])));

        Assert.Equal("waitForConsole \"saved\"", line);
    }

    [Fact]
    public void Lower_NetworkActionsPassThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("route", ["/api"], [])));

        Assert.Equal("route \"/api\"", line);
    }

    [Fact]
    public void Lower_HarActionsPassThrough()
    {
        var node = new CmgNode(1, "exportHar", "exportHar", [], new Dictionary<string, string> { ["path"] = "out.har" }, []);
        var line = Assert.Single(new CmgActionLowerer().Lower(node));

        Assert.Equal("exportHar path=\"out.har\"", line);
    }

    [Fact]
    public void Lower_FrameActionsPassThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("frameClick", ["#frame", "#save"], [])));

        Assert.Equal("frameClick \"#frame\" \"#save\"", line);
    }

    [Fact]
    public void Lower_ClockActionsPassThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("tick", ["250"], [])));

        Assert.Equal("tick \"250\"", line);
    }

    [Fact]
    public void Lower_ContextActionsPassThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("clearContext", [], [])));

        Assert.Equal("clearContext", line);
    }

    [Fact]
    public void Lower_AccessibilityActionsPassThrough()
    {
        var node = new CmgNode(1, "expectAccessible", "expectAccessible", [], new Dictionary<string, string> { ["role"] = "button" }, []);
        var line = Assert.Single(new CmgActionLowerer().Lower(node));

        Assert.Equal("expectAccessible role=\"button\"", line);
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
