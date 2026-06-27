using CMG.Runner;

namespace CMG.Tests;

public sealed partial class CmgActionLowererTests
{
    [Fact]
    public void Lower_DragAndDropBlockPreservesChildren()
    {
        var action = Node("dragAndDrop", ["#source"], [
            Node("hover", ["#drop"], []),
            Node("drop", ["#drop"], [])
        ]);
        var lines = new CmgActionLowerer().Lower(action);

        Assert.Equal("dragAndDrop \"#source\" {", lines[0]);
        Assert.Contains("hover \"#drop\"", lines);
        Assert.Equal("drop \"#drop\"", lines[^2]);
        Assert.Equal("}", lines[^1]);
    }
}
