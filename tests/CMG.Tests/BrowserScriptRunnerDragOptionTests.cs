using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerDragOptionTests
{
    [Fact]
    public void RunText_DragOptionsUseConfiguredElementOffsets()
    {
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(10, 20, 100, 40));
        client.ElementBoxes.Enqueue(new ElementBox(200, 300, 80, 60));
        var result = Runner().RunText(
            "dragAndDrop #source #target sourceX=4 sourceY=8 targetX=12 targetY=16",
            "debug",
            client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(new ElementPoint(14, 28), client.LastBeginDragPoint);
        Assert.Equal(new ElementPoint(212, 316), client.LastMoveDragPoint);
        Assert.Equal(new ElementPoint(212, 316), client.LastEndDragPoint);
    }

    [Fact]
    public void RunText_DragOptionsValidateOffsets()
    {
        var result = Runner().RunText("dragAndDrop #source #target sourceX=-1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("sourceX= must be zero or greater", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
