using System.Text.Json;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifStepTimelineTests
{
    [Fact]
    public void Timeline_MapsNestedActionsToFrameSpans()
    {
        using var gif = new TempGifFile();
        var timeline = Path.ChangeExtension(gif.File.FullName, ".timeline.json");
        var script = """
            narrate "outer" {
              caption "inside" duration=100 fadeIn=0 fadeOut=0
            }
            """;

        var result = Runner().RunText(script, "debug", new FakeAutomationClient(), gif.File, gifTimelinePath: timeline);

        Assert.True(result.Success, result.Error);
        using var document = JsonDocument.Parse(File.ReadAllText(timeline));
        var steps = document.RootElement.GetProperty("steps").EnumerateArray().ToArray();
        Assert.Equal(2, steps.Length);
        Assert.Equal([1, 2], steps.Select(step => step.GetProperty("sequence").GetInt32()).ToArray());
        Assert.Equal(["narrate", "caption"], steps.Select(step => step.GetProperty("action").GetString() ?? string.Empty).ToArray());
        Assert.All(steps, step => Assert.True(step.GetProperty("success").GetBoolean()));
        Assert.True(steps[0].GetProperty("endFrameIndex").GetInt32() >= steps[1].GetProperty("endFrameIndex").GetInt32());
        File.Delete(timeline);
    }

    [Fact]
    public void Timeline_FailureBookmarkPointsAtDiagnosticCaption()
    {
        using var gif = new TempGifFile();
        var timeline = Path.ChangeExtension(gif.File.FullName, ".timeline.json");

        var result = Runner().RunText("fail \"Deliberate\"", "debug", new FakeAutomationClient(), gif.File, gifTimelinePath: timeline);

        Assert.False(result.Success);
        using var document = JsonDocument.Parse(File.ReadAllText(timeline));
        var root = document.RootElement;
        var step = Assert.Single(root.GetProperty("steps").EnumerateArray());
        Assert.False(step.GetProperty("success").GetBoolean());
        Assert.Equal(root.GetProperty("frameCount").GetInt32() - 1, step.GetProperty("failureFrameIndex").GetInt32());
        Assert.Contains("Deliberate", step.GetProperty("error").GetString(), StringComparison.Ordinal);
        File.Delete(timeline);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGifFile : IDisposable
    {
        public TempGifFile() => File = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));

        public FileInfo File { get; }

        public void Dispose()
        {
            if (File.Exists)
            {
                File.Delete();
            }

            var timeline = Path.ChangeExtension(File.FullName, ".timeline.json");
            if (System.IO.File.Exists(timeline))
            {
                System.IO.File.Delete(timeline);
            }
        }
    }
}
