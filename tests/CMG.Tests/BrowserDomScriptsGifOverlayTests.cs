using CMG.Browser;

namespace CMG.Tests;

public sealed class BrowserDomScriptsGifOverlayTests
{
    [Fact]
    public void CursorOverlay_UsesStableTopLayerPositionAndBoundedFilter()
    {
        var script = BrowserDomScripts.MoveDomCursor(new ElementPoint(20, 30));

        Assert.Contains("!element.matches(':popover-open')", script, StringComparison.Ordinal);
        Assert.Contains("cursor.style.inset = 'auto'", script, StringComparison.Ordinal);
        Assert.Contains("cursor.style.filter = 'none'", script, StringComparison.Ordinal);
        Assert.Contains("cursor.firstElementChild.style.filter", script, StringComparison.Ordinal);
    }

    [Fact]
    public void EvidencePromotion_DoesNotHideAnOpenPopover()
    {
        var script = BrowserDomScripts.PromoteGifEvidence();

        Assert.Contains("if(!e.matches(':popover-open')) e.showPopover()", script, StringComparison.Ordinal);
        Assert.DoesNotContain("hidePopover", script, StringComparison.Ordinal);
    }

    [Fact]
    public void DragGhost_UsesStableTopLayerPositionAndPromotion()
    {
        var script = BrowserDomScripts.DragAndDrop("#source", "#target");

        Assert.Contains("ghost.style.inset = 'auto'", script, StringComparison.Ordinal);
        Assert.Contains("!state.defaultGhost.matches(':popover-open')", script, StringComparison.Ordinal);
        Assert.DoesNotContain("state.defaultGhost.hidePopover()", script, StringComparison.Ordinal);
    }
}
