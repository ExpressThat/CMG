using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerDragOptionTests
{
    [Fact]
    public void RunText_DragOptionsUseConfiguredElementOffsets()
    {
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(10, 20, 100, 40));
        client.ElementBoxes.Enqueue(new ElementBox(200, 300, 80, 60));
        var result = Runner().RunText(
            "dragAndDrop #source #target sourceX=4 sourceY=8 targetX=12 targetY=16",
            "debug",
            client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(new ElementPoint(14, 28), client.LastBeginDragPoint);
        Assert.Equal(new ElementPoint(212, 316), client.LastMoveDragPoint);
        Assert.Equal(new ElementPoint(212, 316), client.LastEndDragPoint);
    }

    [Fact]
    public void RunText_DragOptionsValidateOffsets()
    {
        var result = Runner().RunText("dragAndDrop #source #target sourceX=-1", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("sourceX= must be zero or greater", result.Error);
    }

    [Fact]
    public void RunText_BlockDragAcceptsParentAndChildRecordingOptions()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();
        var result = Runner().RunText("""
        dragAndDrop "#source" pointerDuration=400 {
          hover "#mid" pointerDuration=200
          drop "#target" dropPointerDuration=300
        }
        """, "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(13, client.MouseMoveCount);
    }

    [Fact]
    public void RunText_BlockDragAcceptsInheritedRecordingDefaults()
    {
        var client = new FakeAutomationClient();
        using var gif = new TempGifFile();
        var result = Runner().RunText($$"""
        gif "drag" output="{{gif.File.FullName.Replace("\\", "/")}}" clickPulse=ripple preClickHold=100 holdAfterAssertion=200 {
          dragAndDrop "#source" pointerDuration=400 {
            drop "#target"
          }
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(ClickPulseStyle.Ripple, client.CursorPulseStyles);
    }


    [Fact]
    public void RunText_UnrecordedBlockDragSkipsGifChoreography()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        dragAndDrop "#source" {
          delay 200
          hover "#mid"
          moveMouse "bottom"
          drop "#target"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#source", client.LastDragSource);
        Assert.Equal("#target", client.LastDragTarget);
        Assert.Equal(string.Empty, client.LastHoveredSelector);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_DRAG_DELAY 002 status=skipped reason=no-active-recording", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_DRAG_HOVER 003 status=skipped reason=no-active-recording", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_MOVE_MOUSE 004 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_UnrecordedBlockDragStillRunsPrepActions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
        dragAndDrop "#source" {
          waitForElement "#ready"
          scrollIntoView "#target"
          drop "#target"
        }
        """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#ready", client.LastWaitSelector);
        Assert.Equal("#source", client.LastDragSource);
        Assert.Equal("#target", client.LastDragTarget);
    }

    [Fact]
    public void RunText_BlockDragRejectsUnknownOptions()
    {
        using var gif = new TempGifFile();
        var result = Runner().RunText("""
        dragAndDrop "#source" nope=true {
          drop "#target"
        }
        """, "debug", new FakeAutomationClient(), gif.File);

        Assert.False(result.Success);
        Assert.Contains("accepts only recording choreography options", result.Error);
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
