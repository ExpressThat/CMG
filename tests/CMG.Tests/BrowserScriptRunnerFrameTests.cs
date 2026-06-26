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
    public void RunText_FrameActionsResolveRichLocatorsInsideFrame()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("frameClick \"#frame\" \"getByRole=button|Save\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("const resolveFrameElement = locator =>", client.LastExpression);
        Assert.Contains("getByRole", client.LastExpression);
        Assert.Contains("accessibleName(e).includes(parts[1])", client.LastExpression);
    }

    [Fact]
    public void RunText_FrameWaitAndTextAssertionsUseFrameLocatorResolver()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        frameWaitForSelector "#frame" "text=Ready"
        frameToContainText "#frame" "getByTestId=status" "Saved"
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("resolveFrameElement(\"testid=status\")", client.LastExpression);
        Assert.Contains("data-testid", client.LastExpression);
    }

    [Fact]
    public void RunText_FrameEvaluateReturnsResultLine()
    {
        var result = Runner().RunText("frameEvaluate \"#frame\" \"document.title\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("FRAME_EVALUATE", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("frameTextContent \"#frame\" \"#status\"", "FRAME_TEXT 001 Ready", "textContent")]
    [InlineData("frameInputValue \"#frame\" \"#email\"", "FRAME_VALUE 001 agent@example.com", "value")]
    [InlineData("frameGetAttribute \"#frame\" \"#profile\" \"href\"", "FRAME_ATTRIBUTE 001 /profile", "getAttribute")]
    [InlineData("frameComputedStyle \"#frame\" \"#status\" \"display\"", "FRAME_STYLE 001 block", "getComputedStyle")]
    [InlineData("frameProperty \"#frame\" \"#status\" \"dataset.state\"", "FRAME_PROPERTY 001 ready", "dataset.state")]
    public void RunText_FrameGettersReturnPayload(string script, string expectedOutput, string expectedExpression)
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue(expectedOutput.Split(' ').Last());
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(expectedOutput, result.StdoutLines);
        Assert.Contains(expectedExpression, client.LastExpression);
    }

    [Theory]
    [InlineData("frameCount \"#frame\" \".item\"", "FRAME_COUNT 001 2", "querySelectorAll")]
    [InlineData("frameBoundingBox \"#frame\" \"#card\"", "FRAME_BOUNDING_BOX 001 {}", "getBoundingClientRect")]
    [InlineData("frameAllTextContents \"#frame\" \".item\"", "FRAME_TEXTS 001 []", "JSON.stringify")]
    public void RunText_FrameCollectionGettersReturnPayload(string script, string expectedOutput, string expectedExpression)
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue(expectedOutput.Split(' ').Last());
        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(expectedOutput, result.StdoutLines);
        Assert.Contains(expectedExpression, client.LastExpression);
    }

    [Fact]
    public void RunText_FrameCountTreatsCssAttributeSelectorsAsCss()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("1");
        var result = Runner().RunText("frameCount \"#frame\" \"input[name=email]\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("document.querySelectorAll(raw)", client.LastExpression);
        Assert.Contains("input[name=email]", client.LastExpression);
    }

    [Fact]
    public void RunText_FrameAssertTextSupportsRegexAndIgnoreCase()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("frameExpectText \"#frame\" \"#status\" \"save[d]\" match=regex ignoreCase=true", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("const matchMode = \"regex\";", client.LastExpression);
        Assert.Contains("const ignoreCase = true;", client.LastExpression);
        Assert.Contains("FRAME 001 frameExpectText", result.StdoutLines);
    }

    [Theory]
    [InlineData("frameToHaveText")]
    [InlineData("frameToContainText")]
    [InlineData("frameContains")]
    public void RunText_FrameTextAliasesUseFrameAssertion(string action)
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText($"{action} \"#frame\" \"#status\" \"Saved\"", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("Expected frame text", client.LastExpression);
        Assert.Contains($"FRAME 001 {action}", result.StdoutLines);
    }

    [Fact]
    public void RunText_FrameWaitForSelectorAliasUsesFrameWait()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("frameWaitForSelector \"#frame\" \"#ready\" timeout=250", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains("Timed out waiting for frame selector", client.LastExpression);
        Assert.Contains("FRAME 001 frameWaitForSelector", result.StdoutLines);
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
          computedStyle "#save" "display"
          property "#save" "dataset.state"
          evaluate "document.title"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error ?? string.Join('\n', result.StdoutLines));
        Assert.Contains("FRAME 002 frameFill", result.StdoutLines);
        Assert.Contains("FRAME 003 frameClick", result.StdoutLines);
        Assert.Contains("FRAME 004 frameAssertText", result.StdoutLines);
        Assert.Contains("FRAME_STYLE 005", string.Join('\n', result.StdoutLines));
        Assert.Contains("FRAME_PROPERTY 006", string.Join('\n', result.StdoutLines));
        Assert.Contains("FRAME_EVALUATE 007", string.Join('\n', result.StdoutLines));
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
