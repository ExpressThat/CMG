using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererCaptionTests
{
    [Fact]
    public void Lower_StepPreservesCaptionOptions()
    {
        var options = new Dictionary<string, string> { ["captionStyle"] = "qa", ["captionPosition"] = "bottom" };
        var action = Node("step", ["Open"], options, [Node("click", ["#open"], [])]);
        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal("showMessageBar \"Open\" captionStyle=\"qa\" captionPosition=\"bottom\"", lines[0]);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyList<CmgNode> children) =>
        new(1, kind, kind, args, new Dictionary<string, string>(), children);

    private static CmgNode Node(
        string kind,
        IReadOnlyList<string> args,
        IReadOnlyDictionary<string, string> options,
        IReadOnlyList<CmgNode> children) =>
        new(1, kind, kind, args, options, children);
}
