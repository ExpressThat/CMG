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
    public void RunText_FrameAssertTextSupportsRegexAndIgnoreCase()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("frameAssertText \"#frame\" \"#status\" \"save[d]\" match=regex ignoreCase=true", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("const matchMode = \"regex\";", client.LastExpression);
        Assert.Contains("const ignoreCase = true;", client.LastExpression);
    }

    [Fact]
    public void RunText_FrameAssertTextRejectsInvalidRegex()
    {
        var result = Runner().RunText("frameAssertText \"#frame\" \"#status\" \"[\" match=regex", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Invalid text regex '['", result.Error);
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
