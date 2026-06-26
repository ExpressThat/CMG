using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererProviderLocatorTests
{
    [Fact]
    public void Lower_ProviderLocatorOptionMarksElementBeforeActionability()
    {
        var node = new CmgNode(1, "click", "click", [], new Dictionary<string, string> { ["getByText"] = "Save" }, []);
        var lines = new CmgActionLowerer().Lower(node);

        Assert.Equal(3, lines.Count);
        Assert.Contains("No element matched locator text=Save", lines[0]);
        Assert.Contains("not actionable", lines[1]);
        Assert.Equal("click \"[data-cmg-locator-id=\\\"__cmg_locator_1\\\"]\"", lines[2]);
    }
}
