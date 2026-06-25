using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgApiRequestRunnerTests
{
    [Fact]
    public void Run_FailsWhenArgumentsAreMissing()
    {
        var result = new CmgApiRequestRunner().Run(Node("apiRequest", ["GET"]));

        Assert.False(result.Success);
        Assert.Contains("requires method and URL", result.Error);
    }

    [Fact]
    public void Run_FailsForInvalidUrlWithReason()
    {
        var result = new CmgApiRequestRunner().Run(Node("apiRequest", ["GET", "not a url"]));

        Assert.False(result.Success);
        Assert.False(string.IsNullOrWhiteSpace(result.Error));
    }

    private static CmgNode Node(string kind, IReadOnlyList<string> args) =>
        new(3, kind, kind, args, new Dictionary<string, string>(), []);
}
