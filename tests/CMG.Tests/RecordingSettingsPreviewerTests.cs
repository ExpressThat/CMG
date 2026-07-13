using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class RecordingSettingsPreviewerTests
{
    [Fact]
    public void Preview_ResolvesMutableAndNestedDefaults()
    {
        var result = RecordingSettingsPreviewer.PreviewText("""
            setRecording quality=high pointerSpeed=fast
            recordingDefaults captionStyle=qa {
              gif "proof" pointerSpeed=slow { pauseGif 100 }
            }
            previewRecordingSettings
            """);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.Lines, line => line.Contains("scope=gif", StringComparison.Ordinal) &&
            line.Contains("captionStyle=qa", StringComparison.Ordinal) && line.Contains("pointerSpeed=slow", StringComparison.Ordinal));
        Assert.Contains(result.Lines, line => line.Contains("action=previewRecordingSettings", StringComparison.Ordinal) &&
            line.Contains("pointerSpeed=fast", StringComparison.Ordinal) && !line.Contains("captionStyle", StringComparison.Ordinal));
    }

    [Fact]
    public void Preview_WarnsForExplicitVisualOptionOnNonVisualAction()
    {
        var result = RecordingSettingsPreviewer.PreviewText("evaluate \"true\" pointerDuration=200");

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.Lines, line => line == "GIF_SETTINGS_WARN line=1 action=evaluate option=pointerDuration reason=non-visual-action");
    }

    [Fact]
    public void Preview_RejectsUnknownRecordingDefault()
    {
        var result = RecordingSettingsPreviewer.PreviewText("recording pointerWiggle=yes { pauseGif 100 }");

        Assert.False(result.Success);
        Assert.Contains("pointerWiggle= is not a supported recording default", result.Error, StringComparison.Ordinal);
    }

    [Fact]
    public void RuntimeSetRecordingChangesOnlyCurrentScopeAndIsInertWithoutGif()
    {
        var client = new FakeAutomationClient();
        var result = new BrowserScriptRunner(new BrowserScriptParser()).RunText("""
            setRecording pointerSpeed=fast
            recordingDefaults captionStyle=qa {
              setRecording pointerSpeed=slow
              previewRecordingSettings
            }
            previewRecordingSettings
            """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.StdoutLines, line => line.Contains("pointerSpeed=slow", StringComparison.Ordinal) && line.Contains("captionStyle=qa", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("pointerSpeed=fast", StringComparison.Ordinal) && !line.Contains("captionStyle=qa", StringComparison.Ordinal));
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Empty(client.CursorStates);
    }

    [Fact]
    public void RuntimeSetRecordingChangesSubsequentPointerChoreography()
    {
        var gifPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var client = new FakeAutomationClient();
        try
        {
            var result = new BrowserScriptRunner(new BrowserScriptParser()).RunText($$"""
                gif "settings" output="{{gifPath.Replace("\\", "/")}}" frameDelay=100 pointerDuration=0 {
                  hover "#first"
                  setRecording pointerDuration=400
                  hover "#second"
                }
                """, "debug", client);

            Assert.True(result.Success, result.Error);
            Assert.Equal(5, client.MouseMoveCount);
        }
        finally
        {
            if (File.Exists(gifPath)) File.Delete(gifPath);
        }
    }
}
