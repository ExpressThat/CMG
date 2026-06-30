using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifPauseTests
{
    [Fact]
    public void MoveMouse_WithoutRecorder_SkipsWithoutPointerFrame()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("moveMouse \"center\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Equal(0, client.MouseMoveCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_MOVE_MOUSE 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void MoveMouse_InsideUnrecordedDragBlock_SkipsAndStillRunsNativeDrag()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            dragAndDrop "#source" {
              moveMouse "bottom"
              drop "#target"
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_MOVE_MOUSE 002 status=skipped reason=no-active-recording", StringComparison.Ordinal));
        Assert.Equal("#source", client.LastDragSource);
        Assert.Equal("#target", client.LastDragTarget);
    }

    [Fact]
    public void MoveMouse_WithoutRecorder_SkipsBlockBodyValidation()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            moveMouse "center" {
              click "#ignored"
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_MOVE_MOUSE 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void MoveMouse_InsideGifBlock_RejectsBlockBody()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif").Replace("\\", "/");
        var result = Runner().RunText($$"""
            gif "move body" output="{{path}}" {
              moveMouse "center" {
                click "#ignored"
              }
            }
            """, "debug", client);

        Assert.False(result.Success);
        Assert.Contains("moveMouse does not accept a block body.", result.Error);
    }

    [Fact]
    public void PauseGif_WithoutRecorder_DoesNotCapturePointerFrame()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("pauseGif 500", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_PAUSE 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void PauseGif_WithoutRecorder_SkipsBeforeArgumentValidation()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("pauseGif nope", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_PAUSE 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void PauseGif_WithoutRecorder_SkipsBlockBodyValidation()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            pauseGif 500 {
              click "#ignored"
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_PAUSE 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void PauseGif_InsideGifBlock_RejectsBlockBody()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif").Replace("\\", "/");
        var result = Runner().RunText($$"""
            gif "pause body" output="{{path}}" {
              pauseGif 500 {
                click "#ignored"
              }
            }
            """, "debug", client);

        Assert.False(result.Success);
        Assert.Contains("pauseGif does not accept a block body.", result.Error);
    }

    [Fact]
    public void PauseGif_InsideGifBlock_CapturesOnlyPauseFrame()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif").Replace("\\", "/");
        var result = Runner().RunText($$"""
            gif "pause" output="{{path}}" {
              pauseGif 500
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal(1, client.PageScreenshotCount);
    }

    [Fact]
    public void RecordCheckpoint_WithoutRecorder_SkipsWithoutPointerFrame()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("recordCheckpoint \"after setup\"", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_CHECKPOINT 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void RecordCheckpoint_WithoutRecorder_SkipsBeforeArgumentValidation()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("recordCheckpoint", "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_CHECKPOINT 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void RecordCheckpoint_WithoutRecorder_SkipsBlockBodyValidation()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            recordCheckpoint "ignored" {
              click "#ignored"
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_CHECKPOINT 001 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void RecordCheckpoint_InsideGifBlock_RejectsBlockBody()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif").Replace("\\", "/");
        var result = Runner().RunText($$"""
            gif "checkpoint body" output="{{path}}" {
              recordCheckpoint "ready" {
                click "#ignored"
              }
            }
            """, "debug", client);

        Assert.False(result.Success);
        Assert.Contains("recordCheckpoint does not accept a block body.", result.Error);
    }

    [Fact]
    public void RecordCheckpoint_InsideGifBlock_DoesNotCaptureFrame()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif").Replace("\\", "/");
        var result = Runner().RunText($$"""
            gif "checkpoint" output="{{path}}" {
              recordCheckpoint "ready"
              pauseGif 200
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal(1, client.PageScreenshotCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_CHECKPOINT 002 name=\"ready\" status=recorded", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
