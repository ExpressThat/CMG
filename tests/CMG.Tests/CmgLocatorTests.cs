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
}
