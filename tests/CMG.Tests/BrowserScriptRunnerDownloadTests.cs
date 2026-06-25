using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerDownloadTests
{
    [Fact]
    public void RunText_WaitForDownloadFindsMatchingFile()
    {
        var directory = Directory.CreateTempSubdirectory();
        var file = Path.Combine(directory.FullName, "report.csv");
        File.WriteAllText(file, "ok");

        var path = directory.FullName.Replace('\\', '/');
        var result = Runner().RunText($"waitForDownload directory=\"{path}\" pattern=\"*.csv\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("DOWNLOAD", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_DownloadClicksThenWaits()
    {
        var directory = Directory.CreateTempSubdirectory();
        File.WriteAllText(Path.Combine(directory.FullName, "report.csv"), "ok");
        var client = new FakeAutomationClient();

        var path = directory.FullName.Replace('\\', '/');
        var result = Runner().RunText($"download \"#export\" directory=\"{path}\" pattern=\"*.csv\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("#export", client.LastClickedSelector);
    }

    [Fact]
    public void RunText_WaitForDownloadReportsTimeout()
    {
        var directory = Directory.CreateTempSubdirectory();
        var path = directory.FullName.Replace('\\', '/');
        var result = Runner().RunText($"waitForDownload directory=\"{path}\" pattern=\"*.zip\" timeout=1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("No download matching", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
