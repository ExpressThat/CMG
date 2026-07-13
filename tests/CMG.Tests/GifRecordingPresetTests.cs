using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class GifRecordingPresetTests
{
    [Fact]
    public void ReducedMotion_UsesStaticMovementAndAllowsChildOptOut()
    {
        var baseline = new ScriptPointerMotionOptions(500, null, ScriptPointerEasing.Spring);
        var reduced = baseline.WithAction(Action(new() { ["reducedMotion"] = "true" }));
        var restored = reduced.WithAction(Action(new() { ["reducedMotion"] = "false" }));

        Assert.Equal(0, reduced.DurationMilliseconds("test"));
        Assert.Equal(ScriptPointerEasing.Linear, reduced.PointerEasing);
        Assert.Equal(ScriptPointerMotionOptions.DefaultDurationMilliseconds, restored.DurationMilliseconds("test"));
        Assert.Equal(ScriptPointerEasing.EaseInOut, restored.PointerEasing);
    }

    [Fact]
    public void HighContrastPointer_AllowsPropertyAndPresetOverrides()
    {
        var highContrast = PointerVisualOptions.Default.WithAction(Action(new()
        {
            ["highContrastPointer"] = "true",
            ["pointerSize"] = "52"
        }), touch: false);
        var restored = highContrast.WithAction(Action(new() { ["highContrastPointer"] = "false" }), touch: false);

        Assert.Equal(PointerTheme.Ring, highContrast.Theme);
        Assert.Equal("#ffea00", highContrast.Color);
        Assert.Equal(52, highContrast.SizePixels);
        Assert.Equal(PointerVisualOptions.Default, restored);
    }

    [Theory]
    [InlineData("reducedMotion")]
    [InlineData("highContrastPointer")]
    public void InvalidPresetBoolean_ExplainsFailure(string name)
    {
        var error = Assert.Throws<ScriptExecutionException>(() =>
            GifRecordingPresetOptions.Boolean(new Dictionary<string, string> { [name] = "maybe" }, name, false, "gif"));

        Assert.Contains($"{name}= must be true or false", error.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Presets_DoNotInjectPointerWithoutGifRecording()
    {
        var client = new FakeAutomationClient();
        var runner = new BrowserScriptRunner(new BrowserScriptParser());

        var result = runner.RunText(
            "recording reducedMotion=true highContrastPointer=true { click \"#save\" }",
            "debug",
            client);

        Assert.True(result.Success, result.Error);
        Assert.Empty(client.CursorStates);
        Assert.Equal(0, client.PageScreenshotCount);
    }

    private static BrowserScriptAction Action(Dictionary<string, string> options) =>
        new(1, "click \"#save\"", "click", ["#save"], options, []);
}
