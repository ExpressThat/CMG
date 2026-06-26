using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgActionLowererGenericExpectTests
{
    [Theory]
    [InlineData("expect")]
    [InlineData("assert")]
    [InlineData("softExpect")]
    [InlineData("softAssert")]
    [InlineData("expect.soft")]
    public void Lower_GenericExpectPassesThroughForRunnerScripts(string name)
    {
        var node = new CmgNode(
            1,
            name,
            name,
            ["${count}", ">", "5"],
            new Dictionary<string, string> { ["message"] = "too small" },
            []);

        var line = Assert.Single(new CmgActionLowerer().Lower(node));

        Assert.Equal($"{name} \"${{count}}\" \">\" \"5\" message=\"too small\"", line);
    }
}
