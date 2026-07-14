using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerDownloadTests
{
    [Fact]
    public void RunText_WaitForDownloadFindsMatchingFile()
    {
        var directory = Directory.CreateTempSubdirectory();
        var file = Path.Combine(directory.FullName, "report.csv");
        var writer = WriteLater(file);

        var path = directory.FullName.Replace('\\', '/');
        var result = Runner().RunText($"waitForDownload directory=\"{path}\" pattern=\"*.csv\" timeout=1000", "debug", new FakeAutomationClient());
        writer.Join();

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("DOWNLOAD", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_DownloadClicksThenWaits()
    {
        var directory = Directory.CreateTempSubdirectory();
        var writer = WriteLater(Path.Combine(directory.FullName, "report.csv"));
        var client = new FakeAutomationClient();

        var path = directory.FullName.Replace('\\', '/');
        var result = Runner().RunText($"download \"#export\" directory=\"{path}\" pattern=\"*.csv\" timeout=1000", "debug", client);
        writer.Join();

        Assert.True(result.Success);
        Assert.Equal("#export", client.LastClickedSelector);
    }

    [Fact]
    public void RunText_WaitForDownloadIgnoresStaleAndPartialFiles()
    {
        var directory = Directory.CreateTempSubdirectory();
        File.WriteAllText(Path.Combine(directory.FullName, "stale.csv"), "old");
        File.WriteAllText(Path.Combine(directory.FullName, "active.part"), "partial");
        var result = Runner().RunText(
            $"waitForDownload directory=\"{directory.FullName.Replace('\\', '/')}\" pattern=\"*\" timeout=80",
            "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("No new completed download", result.Error, StringComparison.Ordinal);
    }

    private static Thread WriteLater(string path)
    {
        var thread = new Thread(() =>
        {
            Thread.Sleep(60);
            File.WriteAllText(path, "ok");
        });
        thread.Start();
        return thread;
    }

    [Fact]
    public void RunText_WaitForDownloadReportsTimeout()
    {
        var directory = Directory.CreateTempSubdirectory();
        var path = directory.FullName.Replace('\\', '/');
        var result = Runner().RunText($"waitForDownload directory=\"{path}\" pattern=\"*.zip\" timeout=1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("No new completed download matching", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
