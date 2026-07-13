using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifAnnotationTests
{
    [Fact]
    public void AnnotationActions_SkipWithoutRecorderOrPointerInjection()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            pointerStyle pointerTheme=hand
            annotateTarget "#save" "Primary action"
            recordVariable "missing"
            """, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_POINTER_STYLE", StringComparison.Ordinal) && line.Contains("reason=no-active-recording", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_TARGET_ANNOTATION", StringComparison.Ordinal) && line.Contains("reason=no-active-recording", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Contains("GIF_VARIABLE", StringComparison.Ordinal) && line.Contains("reason=no-active-recording", StringComparison.Ordinal));
        Assert.Empty(client.CursorStates);
        Assert.Equal(0, client.PageScreenshotCount);
    }

    [Fact]
    public void AnnotationActions_StyleHighlightAndMaskRecordedEvidence()
    {
        var gifPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var client = new FakeAutomationClient();
        try
        {
            var result = Runner().RunText($$"""
                set apiToken "private-value"
                gif "annotations" output="{{gifPath.Replace("\\", "/")}}" pointerDuration=0 {
                  pointerStyle pointerTheme=hand pointerColor=#dc2626 pointerSize=40
                  click "#save"
                  annotateTarget "#save" "Primary action" duration=200
                  recordVariable "apiToken" label="API token" duration=200
                }
                """, "debug", client);

            Assert.True(result.Success, result.Error);
            Assert.Contains(client.CursorStates, state => state.Visual?.Theme is PointerTheme.Hand && state.Visual.Color == "#dc2626");
            Assert.Contains(client.EvaluatedExpressions, value => value.Contains("data-cmg-highlight", StringComparison.Ordinal));
            Assert.Contains(client.MessageBars, value => value == "API token: [masked]");
            Assert.DoesNotContain(client.MessageBars, value => value.Contains("private-value", StringComparison.Ordinal));
            Assert.Contains(result.StdoutLines, line => line.Contains("GIF_POINTER_STYLE", StringComparison.Ordinal) && line.Contains("status=updated", StringComparison.Ordinal));
            Assert.Contains(result.StdoutLines, line => line.Contains("GIF_TARGET_ANNOTATION", StringComparison.Ordinal) && line.Contains("status=captured", StringComparison.Ordinal));
            Assert.Contains(result.StdoutLines, line => line.Contains("GIF_VARIABLE", StringComparison.Ordinal) && line.Contains("value=\"[masked]\"", StringComparison.Ordinal));
        }
        finally
        {
            if (File.Exists(gifPath)) File.Delete(gifPath);
        }
    }

    [Fact]
    public void RecordVariable_RevealMustBeBoolean()
    {
        var gifPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        try
        {
            var result = Runner().RunText($$"""
                set value "x"
                gif "annotations" output="{{gifPath.Replace("\\", "/")}}" {
                  recordVariable "value" reveal=maybe
                }
                """, "debug", new FakeAutomationClient());

            Assert.False(result.Success);
            Assert.Contains("recordVariable option reveal= must be true or false", result.Error, StringComparison.Ordinal);
        }
        finally
        {
            if (File.Exists(gifPath)) File.Delete(gifPath);
        }
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
