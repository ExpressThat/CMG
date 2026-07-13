using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifAccessibilityTests
{
    [Fact]
    public void AccessibilityPreset_AddsAndRemovesFrameOnlyEvidence()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();

        var result = Runner().RunText(
            "recording accessibilityEvidence=true { click \"#save\" }",
            "debug",
            client,
            gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression =>
            expression.Contains("cmgGifA11y", StringComparison.Ordinal) &&
            expression.Contains("#save", StringComparison.Ordinal));
        Assert.Contains(client.EvaluatedExpressions, expression =>
            expression.Contains("[data-cmg-gif-a11y]", StringComparison.Ordinal) &&
            expression.Contains("remove", StringComparison.Ordinal));
    }

    [Fact]
    public void KeystrokeEvidence_DoesNotExposeTypedValue()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();
        const string secret = "not-for-the-gif";

        var result = Runner().RunText(
            $"recording showKeystrokes=true {{ fill \"#password\" \"{secret}\" }}",
            "debug",
            client,
            gif.File);

        Assert.True(result.Success, result.Error);
        var overlays = client.EvaluatedExpressions.Where(expression => expression.Contains("cmgGifA11y", StringComparison.Ordinal)).ToArray();
        Assert.Contains(overlays, expression => expression.Contains("Text input", StringComparison.Ordinal));
        Assert.DoesNotContain(overlays, expression => expression.Contains(secret, StringComparison.Ordinal));
    }

    [Fact]
    public void AccessibilityOptions_AreInertWithoutRecording()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText(
            "recording accessibilityEvidence=true { press \"Tab\" }",
            "debug",
            client);

        Assert.True(result.Success, result.Error);
        Assert.Empty(client.EvaluatedExpressions);
        Assert.Empty(client.CursorStates);
        Assert.Equal(0, client.PageScreenshotCount);
    }

    [Fact]
    public void ShowKeystrokesBlock_EnablesSafeKeyboardOverlay()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();

        var result = Runner().RunText("showKeystrokes { press \"Control+S\" }", "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions, expression =>
            expression.Contains("Control", StringComparison.Ordinal) &&
            expression.Contains("cmgGifA11y", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("accessibilityEvidence")]
    [InlineData("showKeystrokes")]
    [InlineData("focusEvidence")]
    [InlineData("accessibleNames")]
    [InlineData("highContrast")]
    public void InvalidAccessibilityBoolean_ExplainsAcceptedValues(string option)
    {
        var error = Assert.Throws<ScriptExecutionException>(() => GifAccessibilityOptions.FromOptions(
            new Dictionary<string, string> { [option] = "sometimes" },
            "gif option"));

        Assert.Contains($"{option}= must be true or false", error.Message, StringComparison.Ordinal);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGif : IDisposable
    {
        public TempGif() => File = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));

        public FileInfo File { get; }

        public void Dispose()
        {
            if (File.Exists) File.Delete();
        }
    }
}
