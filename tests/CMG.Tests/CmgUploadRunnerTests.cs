using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgUploadRunnerTests
{
    [Fact]
    public void Run_FailsWhenFilePathIsMissing()
    {
        var result = new CmgUploadRunner().Run(Node(["#file"]), string.Empty, new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("selector and at least one file path", result.Error);
    }

    [Fact]
    public void Run_FailsWhenFileDoesNotExist()
    {
        var result = new CmgUploadRunner().Run(Node(["#file", "missing.txt"]), string.Empty, new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("was not found", result.Error);
    }

    [Fact]
    public void Run_AssignsFilesAndDispatchesEvents()
    {
        var file = Path.GetTempFileName();
        File.WriteAllText(file, "hello");
        var client = new FakeAutomationClient();

        try
        {
            var result = new CmgUploadRunner().Run(Node(["#file", file]), "debug", client);

            Assert.True(result.Success);
            Assert.Contains("DataTransfer", client.LastExpression);
            Assert.Contains("input.files = transfer.files", client.LastExpression);
            Assert.Contains("change", client.LastExpression);
        }
        finally
        {
            File.Delete(file);
        }
    }

    private static CmgNode Node(IReadOnlyList<string> args) =>
        new(7, "uploadFiles", "uploadFiles", args, new Dictionary<string, string>(), []);
}
