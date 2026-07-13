using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerCaptionTimelineTests
{
    [Fact]
    public void Caption_DurationAndFadesProduceExplicitTimelineAndRemoveOverlay()
    {
        using var gif = new TempGifFile();
        var client = new FakeAutomationClient();

        var result = Runner().RunText("caption \"Explain\" duration=400 fadeIn=200 fadeOut=200", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(5, GifInspector.Inspect(gif.File).FrameCount);
        Assert.Equal(800, GifInspector.Inspect(gif.File).DurationMilliseconds);
        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("__cmg_message_bar", StringComparison.Ordinal) && expression.Contains("remove", StringComparison.Ordinal));
    }

    [Fact]
    public void SuccessfulAssertion_ShowsExpectedAndActualEvidence()
    {
        using var gif = new TempGifFile();
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("expectText \"#status\" \"Ready\"", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Contains("PASS: expectText", client.LastMessageBar, StringComparison.Ordinal);
        Assert.Contains("Expected: Ready", client.LastMessageBar, StringComparison.Ordinal);
        Assert.Contains("Actual: Ready", client.LastMessageBar, StringComparison.Ordinal);
        Assert.Equal(CaptionSeverity.Success, client.LastCaptionOptions?.Severity);
    }

    [Fact]
    public void SensitiveAssertion_MasksExpectedAndActualValues()
    {
        using var gif = new TempGifFile();
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("secret-value");

        var result = Runner().RunText("expectText \"#password\" \"secret-value\"", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.DoesNotContain("secret-value", client.LastMessageBar, StringComparison.Ordinal);
        Assert.Contains("[masked]", client.LastMessageBar, StringComparison.Ordinal);
    }

    [Fact]
    public void Failure_ShowsBugReportCaptionAndDiagnosticOutput()
    {
        using var gif = new TempGifFile();
        var client = new FakeAutomationClient();

        var result = Runner().RunText("fail \"Deliberate failure\"", "debug", client, gif.File);

        Assert.False(result.Success);
        Assert.Contains("FAILED: fail", client.LastMessageBar, StringComparison.Ordinal);
        Assert.Equal(CaptionSeverity.Error, client.LastCaptionOptions?.Severity);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("GIF_FAILURE_CAPTION", StringComparison.Ordinal));
        Assert.True(gif.File.Exists);
    }

    [Fact]
    public void EvidenceCaptions_CanBeDisabledPerAction()
    {
        using var gif = new TempGifFile();
        var client = new FakeAutomationClient();
        client.TextResponses.Enqueue("Ready");

        var result = Runner().RunText("expectText \"#status\" \"Ready\" assertionCaptions=false", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Equal(string.Empty, client.LastMessageBar);
    }

    [Fact]
    public void Narrate_UsesTeachingCaptionAndExecutesNestedActionsWithoutGifPointerLeak()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("narrate \"Open settings\" { click \"#settings\" }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#settings", client.LastClickedSelector);
        Assert.Equal(CaptionStyle.Teaching, client.LastCaptionOptions?.Style);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("NARRATE", StringComparison.Ordinal));
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Empty(client.CursorStates);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGifFile : IDisposable
    {
        public FileInfo File { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
        public void Dispose() { if (File.Exists) File.Delete(); }
    }
}
