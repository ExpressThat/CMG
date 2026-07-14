using CMG.Browser;

namespace CMG.Tests;

public sealed class FirefoxContextSelectionTests
{
    [Fact]
    public void ResolveFirefoxSelectionIndex_PreservesTreePositionAcrossSessionIdsAndNavigation()
    {
        Assert.Equal(2, FirefoxBiDiClient.ResolveFirefoxSelectionIndex(2, 3));
    }

    [Fact]
    public void ResolveFirefoxSelectionIndex_ReturnsMissingWhenTabClosed()
    {
        Assert.Equal(-1, FirefoxBiDiClient.ResolveFirefoxSelectionIndex(2, 1));
    }
}
