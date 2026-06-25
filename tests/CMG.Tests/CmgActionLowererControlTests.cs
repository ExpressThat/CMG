using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererControlTests
{
    [Theory]
    [InlineData("if")]
    [InlineData("elseif")]
    [InlineData("else")]
    [InlineData("for")]
    [InlineData("foreach")]
    [InlineData("foreachSelector")]
    [InlineData("macro")]
    public void Lower_ControlBlocksPreserveChildren(string name)
    {
        var node = new CmgNode(1, name, name, ["x"], new Dictionary<string, string>(), [
            new CmgNode(2, "evaluate", "evaluate", ["true"], new Dictionary<string, string>(), [])
        ]);

        var lines = new CmgActionLowerer().Lower(node);

        Assert.Equal($"{name} \"x\" {{", lines[0]);
        Assert.Equal("evaluate \"true\"", lines[1]);
        Assert.Equal("}", lines[2]);
    }

    [Fact]
    public void Lower_CallPassesThrough()
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(
            new CmgNode(1, "call", "call", ["login", "agent"], new Dictionary<string, string>(), [])));

        Assert.Equal("call \"login\" \"agent\"", line);
    }
}
