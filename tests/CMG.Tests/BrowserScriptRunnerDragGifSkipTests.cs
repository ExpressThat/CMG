using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerDragGifSkipTests
{
    [Fact]
    public void UnrecordedDragBlockSkipsRecordingOnlyChildren()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            dragAndDrop "#source" {
              pauseGif ${missing}
              recordCheckpoint
              showPointer "${missing}"
              hidePointer {
                click "#ignored"
              }
              drop "#target"
            }
            """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#source", client.LastDragSource);
        Assert.Equal("#target", client.LastDragTarget);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_PAUSE 002 status=skipped reason=no-active-recording", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_CHECKPOINT 003 status=skipped reason=no-active-recording", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_SHOW_POINTER 004 status=skipped reason=no-active-recording", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_HIDE_POINTER 005 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void RecordedDragBlockRunsRecordingOnlyChildren()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();
        var result = Runner().RunText($$"""
            gif "drag" output="{{gif.File.FullName.Replace("\\", "/")}}" {
              dragAndDrop "#source" pointerDuration=0 {
                pauseGif 100
                recordCheckpoint "before drop"
                showPointer
                hidePointer
                drop "#target"
              }
            }
            """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.True(client.PageScreenshotCount > 0);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_PAUSE 003 milliseconds=100 status=captured", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_CHECKPOINT 004 name=\"before drop\" status=recorded", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_SHOW_POINTER 005 status=captured", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_HIDE_POINTER 006 status=captured", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGifFile : IDisposable
    {
        public FileInfo File { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));

        public void Dispose()
        {
            if (File.Exists)
            {
                File.Delete();
            }
        }
    }
}
