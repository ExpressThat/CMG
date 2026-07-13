using CMG.Browser;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgGifAuthoringWarningTests
{
    [Fact]
    public void LargeUnboundedGifRun_SuggestsFailureRetention()
    {
        var warnings = CmgRunService.GifAuthoringWarnings(Tests(21), Options());

        Assert.Equal(
            "GIF_RETENTION_WARN tests=21 threshold=20 reason=large-suite suggestion=--gif-on-failure",
            Assert.Single(warnings));
    }

    [Theory]
    [InlineData(CmgGifRetentionMode.OnFailure, 1, false)]
    [InlineData(CmgGifRetentionMode.Always, 2, false)]
    [InlineData(CmgGifRetentionMode.Always, 1, true)]
    public void BoundedGifRun_DoesNotWarn(CmgGifRetentionMode mode, int sampleRate, bool cleanPassed)
    {
        var options = Options() with
        {
            GifRetentionMode = mode,
            GifRetentionSampleRate = sampleRate,
            GifCleanPassed = cleanPassed
        };

        Assert.Empty(CmgRunService.GifAuthoringWarnings(Tests(21), options));
    }

    [Fact]
    public void RunWithoutCommandGif_DoesNotWarn()
    {
        Assert.Empty(CmgRunService.GifAuthoringWarnings(Tests(21), Options() with { GifDirectory = null }));
    }

    [Fact]
    public void DisabledRecording_DoesNotWarn()
    {
        using var suppression = CMG.Browser.Scripting.Recording.GifRecordingPolicy.Suppress(true);

        Assert.Empty(CmgRunService.GifAuthoringWarnings(Tests(21), Options()));
    }

    private static CmgTestCase[] Tests(int count) => Enumerable.Range(1, count)
        .Select(index => new CmgTestCase("suite.cmgscript", $"test {index}", [], new Dictionary<string, string>()))
        .ToArray();

    private static CmgRunOptions Options() => new(
        BrowserKind.Chrome, new DirectoryInfo(Path.GetTempPath()), null, null, null, null,
        null, null, 0, 0, 1, false, 1, 1, null, null, null, null, new Dictionary<string, string>());
}
