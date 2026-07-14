using System.Text.Json;
using CMG.Runner;

namespace CMG.Tests;

public sealed class GifArtifactFamilyTests
{
    [Fact]
    public void AgeCleanup_RemovesBoundedCustomNarrationButNotExternalSidecarPath()
    {
        var directory = Directory.CreateTempSubdirectory();
        var outside = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.txt");
        try
        {
            var boundedGif = CreateExpiredGif(directory.FullName, "bounded");
            var boundedNarration = Path.Combine(directory.FullName, "review", "bounded.txt");
            Directory.CreateDirectory(Path.GetDirectoryName(boundedNarration)!);
            File.WriteAllText(boundedNarration, "bounded");
            WriteTimeline(boundedGif, boundedNarration);
            var externalGif = CreateExpiredGif(directory.FullName, "external");
            File.WriteAllText(outside, "external");
            WriteTimeline(externalGif, outside);

            GifArtifactRetentionCleaner.Clean(directory, 7);

            Assert.False(File.Exists(boundedNarration));
            Assert.True(File.Exists(outside));
        }
        finally
        {
            directory.Delete(recursive: true);
            File.Delete(outside);
        }
    }

    private static string CreateExpiredGif(string directory, string name)
    {
        var path = Path.Combine(directory, $"{name}.gif");
        File.WriteAllText(path, "gif");
        File.SetLastWriteTimeUtc(path, DateTime.UtcNow.AddDays(-8));
        return path;
    }

    private static void WriteTimeline(string gif, string narration) =>
        File.WriteAllText(Path.ChangeExtension(gif, ".timeline.json"),
            JsonSerializer.Serialize(new { review = new { narrationPath = narration } }));
}
