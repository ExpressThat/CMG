using CMG.Browser.Scripting;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.PixelFormats;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerConditionalGifTests
{
    [Fact]
    public void GifIfChanged_DiscardsUnchangedArtifact()
    {
        using var artifact = new GifArtifact();
        var result = Run($$"""
            gifIfChanged "unchanged" output="{{artifact.Path}}" {
              set value "same"
            }
            """, new FakeAutomationClient());

        Assert.True(result.Success, result.Error);
        Assert.False(File.Exists(artifact.Path));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_SKIPPED", StringComparison.Ordinal) && line.Contains("reason=unchanged", StringComparison.Ordinal));
    }

    [Fact]
    public void GifDotIfChanged_WritesChangedArtifactAndSnapshot()
    {
        using var artifact = new GifArtifact();
        var client = new FakeAutomationClient();
        client.PageScreenshotResponses.Enqueue(Png(Color.White));
        client.PageScreenshotResponses.Enqueue(Png(Color.Red));
        client.PageScreenshotResponses.Enqueue(Png(Color.Blue));
        var result = Run($$"""
            gif.ifChanged "changed" output="{{artifact.Path}}" {
              gif.snapshot "changed state" duration=200
            }
            """, client);

        Assert.True(result.Success, result.Error);
        Assert.True(File.Exists(artifact.Path));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_SNAPSHOT", StringComparison.Ordinal) && line.Contains("status=captured", StringComparison.Ordinal));
    }

    [Fact]
    public void GifOnFailure_DiscardsPassingArtifact()
    {
        using var artifact = new GifArtifact();
        var result = Run($$"""
            gifOnFailure "passing" output="{{artifact.Path}}" {
              set value "pass"
            }
            """, new FakeAutomationClient());

        Assert.True(result.Success, result.Error);
        Assert.False(File.Exists(artifact.Path));
        Assert.Contains(result.StdoutLines, line => line.Contains("reason=passed", StringComparison.Ordinal));
    }

    [Fact]
    public void GifDotOnFailure_RetainsFailedArtifact()
    {
        using var artifact = new GifArtifact();
        var client = new FakeAutomationClient();
        var result = Run($$"""
            gif.onFailure "failure" output="{{artifact.Path}}" {
              fail "expected failure"
            }
            """, client);

        Assert.False(result.Success);
        Assert.True(File.Exists(artifact.Path));
        Assert.Contains(client.MessageBars, value => value.Contains("FAILED: fail", StringComparison.Ordinal));
    }

    [Fact]
    public void GifSnapshot_SkipsWithoutRecorder()
    {
        var client = new FakeAutomationClient();
        var result = Run("gifSnapshot \"state\" duration=200", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_SNAPSHOT", StringComparison.Ordinal) && line.Contains("reason=no-active-recording", StringComparison.Ordinal));
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Empty(client.CursorStates);
    }

    private static ScriptRunResult Run(string script, FakeAutomationClient client) =>
        new BrowserScriptRunner(new BrowserScriptParser()).RunText(script, "debug", client);

    private static byte[] Png(Color color)
    {
        using var image = new Image<Rgba32>(1, 1, color);
        using var stream = new MemoryStream();
        image.SaveAsPng(stream);
        return stream.ToArray();
    }

    private sealed class GifArtifact : IDisposable
    {
        public string Path { get; } = System.IO.Path.Combine(System.IO.Path.GetTempPath(), $"{Guid.NewGuid():N}.gif").Replace("\\", "/");
        public void Dispose() { if (File.Exists(Path)) File.Delete(Path); }
    }
}
