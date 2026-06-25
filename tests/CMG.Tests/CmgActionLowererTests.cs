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

        Assert.Equal(["showMessageBar \"Open\"", "click \"#open\""], lines);
    }

    [Fact]
    public void Lower_GifBlockCanBeFlattenedWhenCommandGifIsActive()
    {
        var action = Node("gif", ["Only this"], [Node("click", ["#open"], [])]);
        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal(["showMessageBar \"Only this\"", "click \"#open\""], lines);
    }

    [Fact]
    public void Lower_PlannedUnsupportedActionFailsExplicitly()
    {
        var action = Node("intercept", ["/api/**"], []);
        var line = Assert.Single(new CmgActionLowerer().Lower(action));

        Assert.Contains("planned but not implemented", line);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyList<CmgNode> children) =>
        new(1, kind, args.FirstOrDefault() ?? kind, args, new Dictionary<string, string>(), children);
}
