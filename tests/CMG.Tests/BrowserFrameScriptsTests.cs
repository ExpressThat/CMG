using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserFrameScriptsTests
{
    [Fact]
    public void Click_UsesFrameContentDocumentAndMouseEvents()
    {
        var script = BrowserFrameScripts.Click("#frame", "#save");

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
}
