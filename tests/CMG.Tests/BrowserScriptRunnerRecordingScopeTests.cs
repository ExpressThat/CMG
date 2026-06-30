using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerRecordingScopeTests
{
    [Fact]
    public void RecordingScope_AppliesPointerDefaultsToCommandRecorder()
    {
        var client = new FakeAutomationClient();
        var gif = TempGif();

        var result = Runner().RunText("""
            recording pointerDuration=200 {
              hover "#save"
            }
            """, "debug", client, gif);

        Assert.True(result.Success);
        Assert.Equal(2, client.MouseMoveCount);
    }

    [Fact]
    public void RecordingScope_ActionOptionsOverrideScopedDefaults()
    {
        var client = new FakeAutomationClient();
        var gif = TempGif();

        var result = Runner().RunText("""
            recording pointerDuration=200 {
              hover "#save" pointerDuration=500
            }
            """, "debug", client, gif);

        Assert.True(result.Success);
        Assert.Equal(5, client.MouseMoveCount);
    }

    [Fact]
    public void RecordingScope_FrameDelayControlsPointerFrameCount()
    {
        var client = new FakeAutomationClient();
        var gif = TempGif();

        var result = Runner().RunText("""
            recording pointerDuration=400 frameDelay=200 {
              hover "#save"
            }
            """, "debug", client, gif);

        Assert.True(result.Success);
        Assert.Equal(2, client.MouseMoveCount);
    }

    [Fact]
    public void RecordingScope_FrameDelayOverridesFps()
    {
        var client = new FakeAutomationClient();
        var gif = TempGif();

        var result = Runner().RunText("""
            recording pointerDuration=400 fps=20 frameDelay=200 {
              hover "#save"
            }
            """, "debug", client, gif);

        Assert.True(result.Success);
        Assert.Equal(2, client.MouseMoveCount);
    }

    [Fact]
    public void RecordingScope_FpsControlsPointerFrameCount()
    {
        var client = new FakeAutomationClient();
        var gif = TempGif();

        var result = Runner().RunText("""
            recording pointerDuration=200 fps=20 {
              hover "#save"
            }
            """, "debug", client, gif);

        Assert.True(result.Success);
        Assert.Equal(4, client.MouseMoveCount);
    }

    [Fact]
    public void GifBlockFrameDelayOverridesRecordingScopeFps()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif").Replace("\\", "/");

        var result = Runner().RunText($$"""
            recording fps=20 {
              gif "timing" output="{{path}}" frameDelay=200 {
                hover "#save" pointerDuration=400
              }
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal(2, client.MouseMoveCount);
    }

    [Fact]
    public void RecordingScope_DoesNotInjectPointerWithoutRecorder()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("""
            recording pointerDuration=200 {
              moveMouse "${missing}"
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal(0, client.MouseMoveCount);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_MOVE_MOUSE 002 status=skipped reason=no-active-recording", StringComparison.Ordinal));
    }

    [Fact]
    public void WithRecording_AppliesClickPulseDefault()
    {
        var client = new FakeAutomationClient();
        var gif = TempGif();

        var result = Runner().RunText("""
            withRecording clickPulse=none {
              click "#save"
            }
            """, "debug", client, gif);

        Assert.True(result.Success);
        Assert.Contains(ClickPulseStyle.None, client.CursorPulseStyles);
    }

    [Fact]
    public void RecordingScope_AppliesClickAndNavigationHolds()
    {
        var client = new FakeAutomationClient();
        var gif = TempGif();

        var result = Runner().RunText("""
            recording pointerDuration=0 preClickHold=200 postClickHold=0 holdAfterNavigation=0 {
              click "#save"
              reload
            }
            """, "debug", client, gif);

        Assert.True(result.Success, result.Error);
        Assert.Equal(4, client.PageScreenshotCount);
    }

    [Fact]
    public void RecordingScope_ActionHoldOverridesScopedDefault()
    {
        var client = new FakeAutomationClient();
        var gif = TempGif();

        var result = Runner().RunText("""
            recording pointerDuration=0 preClickHold=500 postClickHold=500 {
              click "#save" preClickHold=0 postClickHold=0
            }
            """, "debug", client, gif);

        Assert.True(result.Success, result.Error);
        Assert.Equal(3, client.PageScreenshotCount);
    }

    [Fact]
    public void RecordingScope_RejectsUnknownDefaults()
    {
        var result = Runner().RunText("""
            recording pointerWiggle=yes {
              hover "#save"
            }
            """, "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("recording option pointerWiggle= is not a supported recording default", result.Error);
    }

    private static FileInfo TempGif() =>
        new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
