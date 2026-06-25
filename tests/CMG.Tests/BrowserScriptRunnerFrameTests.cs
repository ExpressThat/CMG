using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerFrameTests
{
    [Fact]
    public void RunText_FrameClickEvaluatesFrameScript()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("frameClick \"#frame\" \"#save\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("contentDocument", client.LastExpression);
        Assert.Contains("FRAME", string.Join('\n', result.StdoutLines));
    }

    [Fact]
    public void RunText_FrameEvaluateReturnsResultLine()
    {
        var result = Runner().RunText("frameEvaluate \"#frame\" \"document.title\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("FRAME_EVALUATE", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_FrameActionValidatesArguments()
    {
        var result = Runner().RunText("frameClick \"#frame\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Expected 2 positional", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
