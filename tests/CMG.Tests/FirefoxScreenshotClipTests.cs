using CMG.Browser;

namespace CMG.Tests;

public sealed class FirefoxScreenshotClipTests
{
    [Fact]
    public void NormalizePageClip_ConvertsToViewportCoordinates()
    {
        var options = new ScreenshotOptions(Clip: new ScreenshotClip(120, 680, 300, 200, 2));

        var normalized = FirefoxBiDiClient.NormalizePageClip(options, new ElementPoint(20, 500));

        Assert.Equal(new ScreenshotClip(100, 180, 300, 200, 2), normalized.Clip);
    }
}
