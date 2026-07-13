using CMG.Browser;

namespace CMG.Tests;

public sealed class BrowserDomScriptsAutoCaptionTests
{
    [Fact]
    public void AutoPositionMessageBar_PlacesCaptionOppositeTargetHalf()
    {
        var script = BrowserDomScripts.AutoPositionMessageBar("#save");

        Assert.Contains("rect.top + (rect.height / 2) < window.innerHeight / 2", script);
        Assert.Contains("placeAtBottom ? 'bottom' : 'top'", script);
        Assert.Contains("#save", script);
    }
}
