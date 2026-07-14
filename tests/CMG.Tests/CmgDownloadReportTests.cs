using System.Text.Json;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgDownloadReportTests
{
    [Fact]
    public void Reports_LinkCompletedDownloadsWithoutChangingVisualCaptions()
    {
        var directory = Directory.CreateTempSubdirectory();
        var path = Path.Combine(directory.FullName, "export data.csv");
        try
        {
            File.WriteAllText(path, "id,name\n1,CMG");
            var test = new CmgTestResult(
                "download", "flow.cmg", true, [$"DOWNLOAD 004 {path}"], null, null, []);

            using var json = JsonDocument.Parse(CmgJsonReportWriter.Write([test]));
            var artifact = json.RootElement[0].GetProperty("downloadArtifacts")[0];
            Assert.Equal(Path.GetFullPath(path), artifact.GetProperty("path").GetString());
            Assert.Equal("export data.csv", artifact.GetProperty("name").GetString());
            var html = CmgHtmlReportWriter.Write([test]);
            Assert.Contains("Downloads", html, StringComparison.Ordinal);
            Assert.Contains("export data.csv", html, StringComparison.Ordinal);
            Assert.Contains("cmg.download.path", CmgJUnitReportWriter.Write([test]), StringComparison.Ordinal);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }

    [Fact]
    public void Reports_IgnoreMissingAndUnrelatedPaths()
    {
        var test = new CmgTestResult(
            "download", "flow.cmg", true,
            ["DOWNLOAD 001 C:/missing/private.csv", "Download completed"], null, null, []);

        using var json = JsonDocument.Parse(CmgJsonReportWriter.Write([test]));
        Assert.Empty(json.RootElement[0].GetProperty("downloadArtifacts").EnumerateArray());
        Assert.DoesNotContain("<h3>Downloads</h3>", CmgHtmlReportWriter.Write([test]), StringComparison.Ordinal);
    }
}
