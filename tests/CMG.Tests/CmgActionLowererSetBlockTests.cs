using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererSetBlockTests
{
    [Fact]
    public void Lower_SetBlockPreservesChildActions()
    {
        var action = Node("set", ["title"], [Node("evaluate", ["document.title"], [])]);
        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal("set \"title\" {", lines[0]);
        Assert.Equal("evaluate \"document.title\"", lines[1]);
        Assert.Equal("}", lines[2]);
    }

    [Fact]
    public void Lower_SetLiteralStillPassesThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(Node("set", ["name", "CMG"], [])));

        Assert.Equal("set \"name\" \"CMG\"", line);
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyList<CmgNode> children) =>
        new(1, kind, args.FirstOrDefault() ?? kind, args, new Dictionary<string, string>(), children);
}
