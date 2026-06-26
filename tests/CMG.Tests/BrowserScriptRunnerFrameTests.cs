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

    [Fact]
    public void RunText_FrameBlockRewritesChildActions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        frame "#checkoutFrame" {
          fill "#email" "agent@example.com"
          click "#save"
          contains "Saved"
          evaluate "document.title"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("FRAME 002 frameFill", result.StdoutLines);
        Assert.Contains("FRAME 003 frameClick", result.StdoutLines);
        Assert.Contains("FRAME 004 frameAssertText", result.StdoutLines);
        Assert.Contains("FRAME_EVALUATE 005", string.Join('\n', result.StdoutLines));
        Assert.Contains("document.title", client.LastExpression);
    }

    [Fact]
    public void RunText_FrameBlockComposesWithinSelectorsBeforeRewrite()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        frame "#frame" {
          within ".dialog" {
            click ".save"
          }
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("\"#frame\"", client.LastExpression);
        Assert.Contains("\".dialog .save\"", client.LastExpression);
    }

    [Fact]
    public void RunText_FrameLocatorAliasSupportsWeirdSpacing()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""	  frameLocator     "#frame"   {   hover     "#help"  ;  waitForSelector    "#ready" timeout=250   }""", "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("FRAME 002 frameHover", result.StdoutLines);
        Assert.Contains("FRAME 003 frameWaitForElement", result.StdoutLines);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
