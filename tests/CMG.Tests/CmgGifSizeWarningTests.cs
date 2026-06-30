using CMG.Browser;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgGifSizeWarningTests
{
    [Fact]
    public void GifSizeWarnings_EmitsWarningsForOversizedArtifacts()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        File.WriteAllBytes(path, new byte[4]);
        try
        {
            var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, path, []);
            var warnings = CmgRunService.GifSizeWarnings(test, Options(threshold: 3));

            var warning = Assert.Single(warnings);
            Assert.Contains("GIF_WARN_SIZE", warning, StringComparison.Ordinal);
            Assert.Contains("test=\"checkout\"", warning, StringComparison.Ordinal);
            Assert.Contains("sizeBytes=4", warning, StringComparison.Ordinal);
            Assert.Contains("thresholdBytes=3", warning, StringComparison.Ordinal);
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public void GifSizeWarnings_SkipsWhenNoThreshold()
    {
        var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, "missing.gif", []);

        Assert.Empty(CmgRunService.GifSizeWarnings(test, Options(threshold: null)));
    }

    private static CmgRunOptions Options(long? threshold) =>
        new(
            BrowserKind.Chrome,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            0,
            1,
            false,
            1,
            1,
            null,
            null,
            null,
            null,
            new Dictionary<string, string>(),
            GifWarnSizeBytes: threshold);
}
