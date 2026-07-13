using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;
using System.Text.Json;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerGifDebugTests
{
    [Fact]
    public void DebugHud_CapturesActionContextTargetPointerAndScroll()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();
        var script = $$"""
            gif "debug" output="{{gif.File.FullName.Replace(Path.DirectorySeparatorChar, '/')}}" debug=true {
              step "Checkout" {
                click "#save"
              }
            }
            """;

        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success, result.Error);
        var hud = client.EvaluatedExpressions.Where(expression => expression.Contains("cmgGifDebug", StringComparison.Ordinal)).ToArray();
        Assert.Contains(hud, expression => expression.Contains("#save", StringComparison.Ordinal));
        Assert.Contains(hud, expression => expression.Contains("step Checkout", StringComparison.Ordinal));
        Assert.Contains(hud, expression => expression.Contains("Pointer", StringComparison.Ordinal));
        Assert.Contains(hud, expression => expression.Contains("Scroll", StringComparison.Ordinal));
        Assert.Contains(client.EvaluatedExpressions, expression =>
            expression.Contains("[data-cmg-gif-debug]", StringComparison.Ordinal) && expression.Contains("remove", StringComparison.Ordinal));
        Assert.Contains(result.StdoutLines, line => line.Equals($"GIF_DEBUG {gif.DebugPath}", StringComparison.Ordinal));
        using var document = JsonDocument.Parse(File.ReadAllText(gif.DebugPath));
        var frames = document.RootElement.GetProperty("debugFrames").EnumerateArray().ToArray();
        Assert.NotEmpty(frames);
        Assert.Contains(frames, frame => frame.GetProperty("action").GetString() == "click" &&
            frame.GetProperty("context").GetString()?.Contains("step Checkout", StringComparison.Ordinal) is true);
    }

    [Fact]
    public void ChildDebugFalse_DisablesInheritedHud()
    {
        using var gif = new TempGif();
        var client = new FakeAutomationClient();
        var path = gif.File.FullName.Replace(Path.DirectorySeparatorChar, '/');
        var script = $"gif \"debug\" output=\"{path}\" debug=true {{ click \"#save\" debug=false }}";

        var result = Runner().RunText(script, "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.DoesNotContain(client.EvaluatedExpressions, expression => expression.Contains("cmgGifDebug", StringComparison.Ordinal));
    }

    [Fact]
    public void DebugScope_DoesNotInjectHudOrPointerWithoutRecording()
    {
        var client = new FakeAutomationClient();

        var result = Runner().RunText("recording debug=true { click \"#save\" }", "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Empty(client.CursorStates);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.DoesNotContain(client.EvaluatedExpressions, expression => expression.Contains("cmgGifDebug", StringComparison.Ordinal));
    }

    [Theory]
    [InlineData("debug")]
    [InlineData("debugAction")]
    [InlineData("debugContext")]
    [InlineData("debugTarget")]
    [InlineData("debugCoordinates")]
    [InlineData("debugScroll")]
    public void InvalidDebugBoolean_ExplainsOption(string option)
    {
        var error = Assert.Throws<ScriptExecutionException>(() => GifDebugOptions.FromOptions(
            new Dictionary<string, string> { [option] = "sometimes" }, "gif"));

        Assert.Contains($"{option}= must be true or false", error.Message, StringComparison.Ordinal);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());

    private sealed class TempGif : IDisposable
    {
        public TempGif() => File = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
        public FileInfo File { get; }
        public string DebugPath => Path.ChangeExtension(File.FullName, ".debug.json");
        public void Dispose()
        {
            if (File.Exists) File.Delete();
            if (System.IO.File.Exists(DebugPath)) System.IO.File.Delete(DebugPath);
        }
    }
}
