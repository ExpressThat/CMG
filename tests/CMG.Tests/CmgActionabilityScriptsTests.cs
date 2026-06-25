using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionabilityScriptsTests
{
    [Fact]
    public void WaitForActionable_ChecksVisibilityEnabledAndStability()
    {
        var action = new CmgNode(1, "click", "click", ["#save"], new Dictionary<string, string>(), []);
        var line = CmgActionabilityScripts.WaitForActionable("#save", action);

        Assert.Contains("getBoundingClientRect", line);
        Assert.Contains(":disabled", line);
        Assert.Contains("requestAnimationFrame", line);
    }
}
