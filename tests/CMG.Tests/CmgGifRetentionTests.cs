using CMG.Browser;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgGifRetentionTests
{
    [Fact]
    public void OnRetry_KeepsFailedAttemptAndDeletesPassingAttempt()
    {
        using var files = new GifFiles(2);
        var attempts = new[]
        {
            Result(false, files.Paths[0]) with { Output = [$"GIF_CAPTURE_STATS path=\"{files.Paths[0]}\" attempt=1"] },
            Result(true, files.Paths[1]) with { Output = [$"GIF_CAPTURE_STATS path=\"{files.Paths[1]}\" attempt=2"] }
        };

        var result = CmgRunService.ApplyGifRetention(Test(), attempts,
            new(CmgGifRetentionMode.OnRetry, 1, false));

        Assert.True(result.Success);
        Assert.True(File.Exists(files.Paths[0]));
        Assert.False(File.Exists(files.Paths[1]));
        Assert.Equal(files.Paths[0], result.GifPath);
        Assert.Contains(result.Output, line => line.Contains("attempt=1", StringComparison.Ordinal));
        Assert.DoesNotContain(result.Output, line => line.Contains("attempt=2", StringComparison.Ordinal));
        Assert.Contains(result.Output, line => line.Contains("mode=onRetry", StringComparison.Ordinal));
    }

    [Fact]
    public void OnFailure_DeletesAllArtifactsWhenRetryEventuallyPasses()
    {
        using var files = new GifFiles(2);

        var result = CmgRunService.ApplyGifRetention(Test(),
            [Result(false, files.Paths[0]), Result(true, files.Paths[1])],
            new(CmgGifRetentionMode.OnFailure, 1, false));

        Assert.True(result.Success);
        Assert.All(files.Paths, path => Assert.False(File.Exists(path)));
        Assert.Null(result.GifPath);
    }

    [Fact]
    public void CleanPassed_DeletesOnlyAfterReportCleanupRuns()
    {
        using var files = new GifFiles(2);
        var test = Result(true, $"{files.Paths[0]};{files.Paths[1]}") with
        {
            CommandGifPath = files.Paths[0],
            CleanGifPathsAfterReport = [files.Paths[0]]
        };
        Assert.True(File.Exists(files.Paths[0]));

        var output = CmgRunService.CleanPassedGifsAfterReports([test]);

        Assert.False(File.Exists(files.Paths[0]));
        Assert.True(File.Exists(files.Paths[1]));
        Assert.Contains(output, line => line.Contains("GIF_CLEAN_PASSED", StringComparison.Ordinal));
    }

    [Fact]
    public void SampleRate_SuppressesCommandCaptureWithoutRemovingFocusedOutputDirectory()
    {
        var test = Test(new Dictionary<string, string> { ["gifSampleRate"] = "2" });
        var source = Options() with { GifSampleOrdinal = 2 };

        var success = CmgVisualSegmentExecutor.TryApplyDeclaredGifDefaults(test, source, out var effective, out var error);

        Assert.True(success, error);
        Assert.NotNull(source.GifDirectory);
        Assert.Equal(source.GifDirectory, effective.GifDirectory);
        Assert.True(CmgGifRetentionPolicy.TryParse(test, out var policy, out error), error);
        Assert.False(policy.ShouldRecord(source.GifSampleOrdinal));
    }

    [Fact]
    public void Off_DeletesOnlyCommandGifAndPreservesFocusedBlockGif()
    {
        using var files = new GifFiles(2);
        var attempt = Result(true, $"{files.Paths[0]};{files.Paths[1]}") with
        {
            CommandGifPath = files.Paths[0]
        };

        var result = CmgRunService.ApplyGifRetention(Test(), [attempt],
            new(CmgGifRetentionMode.Off, 1, false));

        Assert.False(File.Exists(files.Paths[0]));
        Assert.True(File.Exists(files.Paths[1]));
        Assert.Equal(files.Paths[1], result.GifPath);
    }

    [Theory]
    [InlineData("gif", "sometimes", "gif=")]
    [InlineData("gifSampleRate", "0", "gifSampleRate=")]
    [InlineData("gifCleanPassed", "maybe", "gifCleanPassed=")]
    public void Policy_RejectsInvalidDeclarations(string name, string value, string expected)
    {
        var success = CmgGifRetentionPolicy.TryParse(Test(new Dictionary<string, string> { [name] = value }), out _, out var error);

        Assert.False(success);
        Assert.Contains(expected, error, StringComparison.Ordinal);
    }

    private static CmgTestResult Result(bool success, string path) =>
        new("retention", "test.cmgscript", success, [], success ? null : "failed", path, [])
        { CommandGifPath = path };

    private static CmgTestCase Test(IReadOnlyDictionary<string, string>? options = null) =>
        new("test.cmgscript", "retention", [], options ?? new Dictionary<string, string>());

    private static CmgRunOptions Options() => new(
        BrowserKind.Chrome, new DirectoryInfo(Path.GetTempPath()), null, null, null, null,
        null, null, 0, 0, 1, false, 1, 1, null, null, null, null, new Dictionary<string, string>());

    private sealed class GifFiles : IDisposable
    {
        public GifFiles(int count)
        {
            Paths = Enumerable.Range(0, count).Select(_ => Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif")).ToArray();
            foreach (var path in Paths) File.WriteAllBytes(path, [1]);
        }
        public string[] Paths { get; }
        public void Dispose() { foreach (var path in Paths) if (File.Exists(path)) File.Delete(path); }
    }
}
