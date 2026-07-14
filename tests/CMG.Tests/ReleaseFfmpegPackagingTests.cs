namespace CMG.Tests;

public sealed class ReleaseFfmpegPackagingTests
{
    [Fact]
    public void ReleaseWorkflow_BundlesVerifiedRuntimeAndLicensesOnlyRequiredTool()
    {
        var workflow = File.ReadAllText(Path.Combine(RepositoryRoot(), ".github", "workflows", "release.yml"));

        Assert.Contains("autobuild-2026-06-30-13-34", workflow, StringComparison.Ordinal);
        Assert.Contains("FFMPEG_SHA256", workflow, StringComparison.Ordinal);
        Assert.Contains("3b9eceb438016b647e0755a51ce3a388cd4ed5679e2427cb83a01e1ae2cd0eba", workflow, StringComparison.Ordinal);
        Assert.Contains("RELEASE_PACKAGE_DIR }}\\ffmpeg.exe", workflow, StringComparison.Ordinal);
        Assert.Contains("RELEASE_PACKAGE_DIR }}\\licenses", workflow, StringComparison.Ordinal);
        Assert.Contains("THIRD-PARTY-NOTICES.txt", workflow, StringComparison.Ordinal);
        Assert.Contains("FFmpeg-LGPLv3.txt", workflow, StringComparison.Ordinal);
        Assert.Contains("OpenH264-BSD.txt", workflow, StringComparison.Ordinal);
        Assert.Contains("FFmpeg-Builds-MIT.txt", workflow, StringComparison.Ordinal);
        Assert.Contains("libopenh264", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("ffplay.exe", workflow, StringComparison.Ordinal);
        Assert.DoesNotContain("ffprobe.exe", workflow, StringComparison.Ordinal);
    }

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CMG.csproj")))
            directory = directory.Parent;
        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }
}
