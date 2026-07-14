using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserFrameScriptsTests
{
    [Fact]
    public void Click_UsesFrameContentDocumentAndMouseEvents()
    {
        var script = BrowserFrameScripts.Click("#frame", "#save");

        Assert.Contains("globalThis.document.querySelector", script);
        Assert.Contains("const frameDocument = frame.contentDocument", script);
        Assert.Contains("contentDocument", script);
        Assert.Contains("MouseEvent('click'", script);
        Assert.Contains("#save", script);
    }

    [Fact]
    public void WaitForElement_ReportsFrameSelectorTimeout()
    {
        var script = BrowserFrameScripts.WaitForElement("#frame", "#ready", 1000);

        Assert.Contains("Timed out waiting for frame selector", script);
        Assert.Contains("1000", script);
    }

    [Fact]
    public void TargetCenter_ReturnsTopLevelCoordinates()
    {
        var script = BrowserFrameScripts.TargetCenter("#frame", "#save");

        Assert.Contains("frameRect.left + rect.left", script);
        Assert.Contains("JSON.stringify", script);
    }

    [Fact]
    public void FrameActions_ExplainCrossOriginRestriction()
    {
        var script = BrowserFrameScripts.Click("#payment-frame", "#pay");

        Assert.Contains("is not same-origin or is not ready", script, StringComparison.Ordinal);
    }

    [Fact]
    public void AssertText_UsesMatchModeAndIgnoreCase()
    {
        var script = BrowserFrameScripts.AssertText("#frame", "#status", "Saved", "regex", ignoreCase: true);

        Assert.Contains("const matchMode = \"regex\";", script);
        Assert.Contains("const ignoreCase = true;", script);
        Assert.Contains("matchesText(actual, expected, matchMode, ignoreCase)", script);
    }

    [Fact]
    public void Fill_UsesFrameNativeValueSetterAndInputEvent()
    {
        var script = BrowserFrameScripts.Fill("#frame", "#name", "CMG");

        Assert.Contains("Object.getOwnPropertyDescriptor(prototype, 'value')?.set", script);
        Assert.Contains("new view.InputEvent('input'", script);
        Assert.Contains("setInput(element, \"CMG\", \"CMG\")", script);
    }
}
