using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserAccessibilityScriptsTests
{
    [Fact]
    public void Snapshot_BuildsRoleNameTree()
    {
        var script = BrowserAccessibilityScripts.Snapshot("#root");

        Assert.Contains("implicitRole", script);
        Assert.Contains("aria-label", script);
        Assert.Contains("#root", script);
    }

    [Fact]
    public void Expect_SearchesRoleAndName()
    {
        var script = BrowserAccessibilityScripts.Expect("button", "Save");

        Assert.Contains("role=button", script);
        Assert.Contains("name=Save", script);
        Assert.Contains("querySelectorAll", script);
    }
}
