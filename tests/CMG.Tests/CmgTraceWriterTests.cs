using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgTraceWriterTests
{
    [Fact]
    public void Write_CreatesPerTestTraceFile()
    {
        var directory = new DirectoryInfo(Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N")));
        var test = new CmgTestResult(
            "suite / case",
            "flow.cmgscript",
            false,
            [],
            "failed",
            "case.gif",
            [new CmgStepResult(2, "click", false, [], "missing", "case.gif")]);

        CmgTraceWriter.Write(directory, [test]);

        var file = Assert.Single(directory.GetFiles("*.trace.json"));
        var json = File.ReadAllText(file.FullName);
        Assert.Contains("suite / case", json);
        Assert.Contains("missing", json);
    }
}
