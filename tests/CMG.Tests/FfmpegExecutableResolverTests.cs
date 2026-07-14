using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class FfmpegExecutableResolverTests
{
    [Fact]
    public void Resolve_PrefersConfiguredPathOverBundledExecutable()
    {
        using var directory = new TempDirectory();
        File.WriteAllText(Path.Combine(directory.Path, ExecutableName), string.Empty);

        var result = FfmpegExecutableResolver.Resolve("configured-ffmpeg", directory.Path, "environment-ffmpeg");

        Assert.Equal("configured-ffmpeg", result.Path);
        Assert.False(result.IsBundled);
    }

    [Fact]
    public void Resolve_UsesExecutableBesideCmgBeforeEnvironment()
    {
        using var directory = new TempDirectory();
        var sibling = Path.Combine(directory.Path, ExecutableName);
        File.WriteAllText(sibling, string.Empty);

        var result = FfmpegExecutableResolver.Resolve(null, directory.Path, "environment-ffmpeg");

        Assert.Equal(sibling, result.Path);
        Assert.True(result.IsBundled);
    }

    [Fact]
    public void Resolve_FallsBackToEnvironmentThenPath()
    {
        using var directory = new TempDirectory();

        var environment = FfmpegExecutableResolver.Resolve(null, directory.Path, "environment-ffmpeg");
        var path = FfmpegExecutableResolver.Resolve(null, directory.Path, string.Empty);

        Assert.Equal("environment-ffmpeg", environment.Path);
        Assert.False(environment.IsBundled);
        Assert.Equal("ffmpeg", path.Path);
        Assert.False(path.IsBundled);
    }

    private static string ExecutableName => OperatingSystem.IsWindows() ? "ffmpeg.exe" : "ffmpeg";

    private sealed class TempDirectory : IDisposable
    {
        public string Path { get; } = Directory.CreateTempSubdirectory().FullName;
        public void Dispose() => Directory.Delete(Path, recursive: true);
    }
}
