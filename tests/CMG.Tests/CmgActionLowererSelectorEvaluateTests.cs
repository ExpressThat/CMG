using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererSelectorEvaluateTests
{
    [Theory]
    [InlineData("evaluateOnSelector")]
    [InlineData("evalOnSelector")]
    [InlineData("evaluateAll")]
    [InlineData("evalAll")]
    public void Lower_SelectorEvaluationPassesThrough(string name)
    {
        var line = Assert.Single(new CmgActionLowerer().Lower(
            new CmgNode(1, name, name, ["#save", "element.textContent"], new Dictionary<string, string>(), [])));

        Assert.Equal($"{name} \"#save\" \"element.textContent\"", line);
    }
}
