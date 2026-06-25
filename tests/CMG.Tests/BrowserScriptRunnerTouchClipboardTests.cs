using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerTouchClipboardTests
{
    [Fact]
    public void RunText_TapResolvesRichLocatorAndDispatchesTouchEvents()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("tap text=Save", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("No element matched locator text=Save", client.EvaluatedExpressions[0]);
        Assert.Contains("TouchEvent('touchstart'", client.LastExpression);
        Assert.Contains("TAP 001 [data-cmg-locator-id=\"__cmg_locator_1\"]", result.StdoutLines);
    }

    [Fact]
    public void RunText_TapCanUseCoordinates()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("tap x=12 y=24", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("document.elementFromPoint", client.LastExpression);
        Assert.Contains("TAP 001 12,24", result.StdoutLines);
    }

    [Fact]
    public void RunText_ClipboardActionsInstallShimAndReadValue()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("true");
        client.EvaluateResponses.Enqueue("hello");

        var result = Runner().RunText("setClipboard hello\nreadClipboard\nclearClipboard", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgClipboard", client.EvaluatedExpressions[0]);
        Assert.Contains("CLIPBOARD_SET 001 5", result.StdoutLines);
        Assert.Contains("CLIPBOARD 002 hello", result.StdoutLines);
        Assert.Contains("CLIPBOARD_CLEARED 003", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetClipboardRequiresText()
    {
        var result = Runner().RunText("setClipboard", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("Expected 1 positional", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
