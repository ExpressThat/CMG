using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class GifCaptionScopeTests
{
    [Fact]
    public void NestedSteps_StackAndRestoreCaptionsDuringRecording()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            recording sourceLineCaptions=true persistentStepTitle=true {
              step "Checkout" {
                step "Payment" { pauseGif 10 }
                pauseGif 10
              }
            }
            """, "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.MessageBars, text => text.Contains("Checkout (line 2)  >  Payment (line 3)", StringComparison.Ordinal));
        Assert.True(client.MessageBars.Count(text => text.StartsWith("Checkout (line 2)", StringComparison.Ordinal)) >= 2);
    }

    [Fact]
    public void DebugNarration_CapturesControlFlowTransitions()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            macro "work" { pauseGif 10 }
            recording debugNarration=true sourceLineCaptions=true {
              repeat 2 { call "work" }
              try { fail "expected" } catch { pauseGif 10 } finally { pauseGif 10 }
            }
            """, "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.MessageBars, text => text.StartsWith("repeat[1/2] (line 3)", StringComparison.Ordinal));
        Assert.Contains(client.MessageBars, text => text.StartsWith("Macro work (line 3)", StringComparison.Ordinal));
        Assert.Contains(client.MessageBars, text => text.StartsWith("Catch", StringComparison.Ordinal));
        Assert.Contains(client.MessageBars, text => text.StartsWith("Finally", StringComparison.Ordinal));
    }

    [Fact]
    public void RecordingCaptionDefaults_AreInertWithoutGif()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("recording debugNarration=true sourceLineCaptions=true { repeat 2 { evaluate \"true\" } }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Empty(client.MessageBars);
        Assert.Empty(client.CursorStates);
        Assert.Equal(0, client.PageScreenshotCount);
    }

    [Fact]
    public void MarkdownCaption_UsesSafeNodeRendering()
    {
        var script = BrowserDomScripts.ShowMessageBar("Use **Save** then `Enter`", new BrowserCaptionOptions(Markdown: true));

        Assert.Contains("createElement(match[0][0] === '`' ? 'code' : 'strong')", script, StringComparison.Ordinal);
        Assert.DoesNotContain("innerHTML = message", script, StringComparison.Ordinal);
    }

    [Fact]
    public void AssertionCaption_UsesScopedLocalizedLabels()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            recording captionPassLabel=OK captionExpectedLabel=Wanted captionActualLabel=Observed {
              assertEval "true"
            }
            """, "debug", client, gif.File);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.MessageBars, text => text.StartsWith("OK: assertEval\nWanted:", StringComparison.Ordinal) && text.Contains("Observed:", StringComparison.Ordinal));
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGif : IDisposable
    {
        public FileInfo File { get; } = new(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
        public void Dispose() { if (File.Exists) File.Delete(); }
    }
}
