using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgLocatorTests
{
    [Fact]
    public void PrefixExpressions_TextLocatorPrefersSmallestMatchingElement()
    {
        var expression = Assert.Single(CmgLocator.PrefixExpressions("text=Save", 3));

        Assert.Contains("querySelectorAll('*').length", expression);
        Assert.Contains("text=Save", expression);
    }

    [Theory]
    [InlineData("first=.item", "document.querySelector")]
    [InlineData("last=.item", ".at(-1)")]
    [InlineData("nth=.item|2", "Number('2')")]
    [InlineData("hasText=.item|Save", "includes('Save')")]
    [InlineData("visible=.item", "IsVisible")]
    public void PrefixExpressions_FilterLocatorsMarkElement(string locator, string expected)
    {
        var expression = Assert.Single(CmgLocator.PrefixExpressions(locator, 7));

        Assert.Contains(expected, expression);
        Assert.Contains("__cmg_locator_7", expression);
    }

    [Fact]
    public void Resolve_FilterLocatorsUseTemporaryMarkerSelector()
    {
        var resolved = CmgLocator.Resolve("nth=.item|1", 4);

        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_4\"]", resolved.Selector);
        Assert.Contains("nth=.item|1", Assert.Single(resolved.PrefixLines));
    }
}
