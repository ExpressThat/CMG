using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

[Collection(ConsoleCommandTestCollection.Name)]
public sealed class GifRecordingPolicyTests
{
    [Fact]
    public void SuppressionScope_DisablesAndRestoresRecording()
    {
        var previous = Environment.GetEnvironmentVariable("CMG_DISABLE_GIF");
        Environment.SetEnvironmentVariable("CMG_DISABLE_GIF", null);
        try
        {
            Assert.False(GifRecordingPolicy.IsDisabled);

            using (GifRecordingPolicy.Suppress(true))
                Assert.True(GifRecordingPolicy.IsDisabled);

            Assert.False(GifRecordingPolicy.IsDisabled);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CMG_DISABLE_GIF", previous);
        }
    }

    [Fact]
    public void DisabledRun_ExecutesBlockWithoutArtifactsScreenshotsOrPointer()
    {
        var commandPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var blockPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var client = new FakeAutomationClient();

        using var suppression = GifRecordingPolicy.Suppress(true);
        var result = new BrowserScriptRunner(new BrowserScriptParser()).RunText($$"""
        gif "private" output="{{Slash(blockPath)}}" {
          evaluate "true"
        }
        """, "debug", client, new FileInfo(commandPath));

        Assert.True(result.Success, result.Error);
        Assert.False(File.Exists(commandPath));
        Assert.False(File.Exists(blockPath));
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Contains("true", client.EvaluatedExpressions);
        Assert.DoesNotContain(client.EvaluatedExpressions, value => value.Contains("cmg-virtual-pointer", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(result.StdoutLines, line => line.Contains("reason=recording-disabled source=cli", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("1")]
    [InlineData("true")]
    [InlineData("yes")]
    [InlineData("on")]
    public void EnvironmentSwitch_DisablesRecording(string value)
    {
        var previous = Environment.GetEnvironmentVariable("CMG_DISABLE_GIF");
        try
        {
            Environment.SetEnvironmentVariable("CMG_DISABLE_GIF", value);
            Assert.True(GifRecordingPolicy.IsDisabled);
            Assert.Equal("environment", GifRecordingPolicy.DisabledSource);
        }
        finally
        {
            Environment.SetEnvironmentVariable("CMG_DISABLE_GIF", previous);
        }
    }

    private static string Slash(string path) => path.Replace('\\', '/');
}
