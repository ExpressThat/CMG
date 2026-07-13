using CMG.Browser;

namespace CMG.Tests;

public sealed class BrowserDomScriptsTitleCardTests
{
    [Fact]
    public void ShowTitleCard_UsesTextContentAndFullViewportOverlay()
    {
        var script = BrowserDomScripts.ShowTitleCard("Release ready", "outro");

        Assert.Contains("textContent = \"Release ready\"", script);
        Assert.Contains("textContent = \"OUTRO\"", script);
        Assert.Contains("position:fixed;inset:0", script);
        Assert.Contains("showPopover", script);
    }

    [Fact]
    public void RemoveTitleCard_RemovesPopoverOverlay()
    {
        var script = BrowserDomScripts.RemoveTitleCard();

        Assert.Contains("hidePopover", script);
        Assert.Contains("card?.remove()", script);
    }
}
