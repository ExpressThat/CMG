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
    [InlineData("textExact=Save", ".trim() === 'Save'")]
    [InlineData("textRegex=^Save", "new RegExp('^Save')")]
    [InlineData("role=button|Save", "accessibleName(e).includes('Save')")]
    [InlineData("roleRegex=button|^Save", "new RegExp('^Save').test(accessibleName(e))")]
    [InlineData("labelRegex=^Email", "new RegExp('^Email')")]
    [InlineData("has=.item|.badge", "e.querySelector('.badge')")]
    [InlineData("hasNot=.item|.badge", "!e.querySelector('.badge')")]
    [InlineData("hasText=.item|Save", "includes('Save')")]
    [InlineData("hasNotText=.item|Draft", "!")]
    [InlineData("visible=.item", "IsVisible")]
    [InlineData("or=.primary|.secondary", "document.querySelector('.primary') ?? document.querySelector('.secondary')")]
    [InlineData("and=.item|.selected", "e.matches('.selected')")]
    [InlineData("strict=.only", "expected exactly one match for .only")]
    [InlineData("shadow=#host|button.save", "shadowRoot?.querySelector('button.save')")]
    [InlineData("shadowText=#host|Shadow Save", "shadowRoot?.querySelectorAll('*')")]
    public void PrefixExpressions_FilterLocatorsMarkElement(string locator, string expected)
    {
        var expression = Assert.Single(CmgLocator.PrefixExpressions(locator, 7));

        Assert.Contains(expected, expression);
        Assert.Contains("__cmgLocatorElements", expression);
        Assert.Contains("__cmg_locator_7", expression);
    }

    [Fact]
    public void Resolve_FilterLocatorsUseTemporaryMarkerSelector()
    {
        var resolved = CmgLocator.Resolve("nth=.item|1", 4);

        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_4\"]", resolved.Selector);
        Assert.Contains("nth=.item|1", Assert.Single(resolved.PrefixLines));
    }

    [Fact]
    public void PrefixExpressions_RoleRegexRequiresRoleAndPattern()
    {
        var expression = Assert.Single(CmgLocator.PrefixExpressions("roleRegex=button", 7));

        Assert.Contains("Locator roleRegex= requires <role>|<name-regex>", expression);
    }

    [Theory]
    [InlineData("or=.primary", "Locator or= requires <selector>|<selector>")]
    [InlineData("and=.item", "Locator and= requires <selector>|<selector>")]
    [InlineData("strict=", "Locator strict= requires <selector>")]
    [InlineData("shadow=#host", "Locator shadow= requires <host-selector>|<inner-selector>")]
    [InlineData("shadowText=#host", "Locator shadowText= requires <host-selector>|<text>")]
    public void PrefixExpressions_CompositeLocatorsRequireTwoParts(string locator, string expected)
    {
        var expression = Assert.Single(CmgLocator.PrefixExpressions(locator, 7));

        Assert.Contains(expected, expression);
    }

    [Theory]
    [InlineData("testId=save")]
    [InlineData("data-testid=save")]
    public void Resolve_TestIdAliasesUseDataTestIdSelector(string locator)
    {
        var resolved = CmgLocator.Resolve(locator, 4);

        Assert.Equal("[data-testid=\"save\"]", resolved.Selector);
        Assert.Empty(resolved.PrefixLines);
    }
}
