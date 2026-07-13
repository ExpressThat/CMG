using CMG.Browser;

namespace CMG.Tests;

public sealed class BrowserDomScriptsCaptionSizeTests
{
    [Theory]
    [InlineData(CaptionSize.Normal, "font:600 16px")]
    [InlineData(CaptionSize.Large, "font-size:19px")]
    [InlineData(CaptionSize.ExtraLarge, "font-size:24px")]
    public void ShowMessageBar_UsesTeachingCaptionSize(CaptionSize size, string expectedCss)
    {
        var script = BrowserDomScripts.ShowMessageBar(
            "Review",
            new BrowserCaptionOptions(CaptionStyle.Teaching, Size: size));

        Assert.Contains(expectedCss, script, StringComparison.Ordinal);
        Assert.Contains("width:max-content", script, StringComparison.Ordinal);
    }
}
