using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgStorageStateRunnerTests
{
    [Fact]
    public void Run_FailsWhenOperationIsMissing()
    {
        var result = new CmgStorageStateRunner().Run(Node([]), string.Empty, new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("requires save or load", result.Error);
    }

    [Fact]
    public void Run_FailsWhenLoadFileIsMissing()
    {
        var result = new CmgStorageStateRunner().Run(Node(["load", "missing-state.json"]), string.Empty, new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("not found", result.Error);
    }

    private static CmgNode Node(IReadOnlyList<string> args) => new(4, "storageState", "storageState", args, new Dictionary<string, string>(), []);
}
