using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererControlTests
{
    [Theory]
    [InlineData("if")]
    [InlineData("elseif")]
    [InlineData("else")]
    [InlineData("narrate")]
    [InlineData("for")]
    [InlineData("foreach")]
    [InlineData("foreachJson")]
    [InlineData("foreachList")]
    [InlineData("foreachSelector")]
    [InlineData("macro")]
    [InlineData("return")]
    [InlineData("within")]
    [InlineData("frame")]
    [InlineData("frameLocator")]
    [InlineData("while")]
    [InlineData("until")]
    [InlineData("doWhile")]
    [InlineData("doUntil")]
    [InlineData("repeat")]
    [InlineData("retry")]
    [InlineData("toPass")]
    [InlineData("withTimeout")]
    [InlineData("withDefaultTimeout")]
    [InlineData("withNavigationTimeout")]
    [InlineData("withAssertionTimeout")]
    [InlineData("withExpectTimeout")]
    [InlineData("try")]
    [InlineData("catch")]
    [InlineData("finally")]
    [InlineData("switch")]
    [InlineData("case")]
    [InlineData("default")]
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

    [Theory]
    [InlineData("call", "call \"login\" \"agent\"")]
    [InlineData("return", "return \"login\" \"agent\"")]
    [InlineData("break", "break")]
    [InlineData("continue", "continue")]
    [InlineData("skip", "skip \"login\" \"agent\"")]
    public void Lower_ControlActionsPassThrough(string name, string expected)
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(
            new CmgNode(1, name, name, name is "break" or "continue" ? [] : ["login", "agent"], new Dictionary<string, string>(), [])));

        Assert.Equal(expected, line);
    }

    [Theory]
    [InlineData("recording")]
    [InlineData("withRecording")]
    [InlineData("recordingDefaults")]
    [InlineData("showKeystrokes")]
    [InlineData("hideFromGif")]
    [InlineData("cutGif")]
    [InlineData("speedUpGif")]
    [InlineData("slowDownGif")]
    public void Lower_RecordingBlocksPreserveChildrenAndOptions(string name)
    {
        var node = new CmgNode(1, name, name, [], new Dictionary<string, string>
        {
            ["pointerDuration"] = "200",
            ["autoCaptions"] = "true"
        }, [
            new CmgNode(2, "hover", "hover", ["#save"], new Dictionary<string, string>(), [])
        ]);

        var lines = new CmgActionLowerer().Lower(node);

        Assert.Contains($"{name} ", lines[0]);
        Assert.Contains("pointerDuration=\"200\"", lines[0]);
        Assert.Contains("autoCaptions=\"true\"", lines[0]);
        Assert.Equal("hover \"#save\"", lines[^2]);
        Assert.Equal("}", lines[^1]);
    }
}
