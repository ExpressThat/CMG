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
