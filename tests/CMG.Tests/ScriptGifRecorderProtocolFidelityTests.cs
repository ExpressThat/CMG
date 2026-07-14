using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderProtocolFidelityTests
{
    [Theory]
    [InlineData("navigate")]
    [InlineData("reload")]
    public void NavigationCapture_ReinjectsVirtualPointer(string actionName)
    {
        using var fixture = new RecorderFixture();
        var action = new BrowserScriptAction(3, actionName, actionName, [], new Dictionary<string, string>(), []);

        fixture.Recorder.AfterAction(action);

        Assert.Contains(fixture.Client.EvaluatedExpressions,
            expression => expression.Contains("__cmg_virtual_cursor", StringComparison.Ordinal));
        Assert.Equal(1, fixture.Client.PageScreenshotCount);
    }

    [Fact]
    public void SameOriginFrameAction_UsesTopPagePointerCoordinates()
    {
        using var fixture = new RecorderFixture();
        fixture.Client.EvaluateResponses.Enqueue("""{"x":120,"y":85}""");
        var action = new BrowserScriptAction(4, "frameClick", "frameClick",
            ["#frame", "#save"], new Dictionary<string, string> { ["pointerDuration"] = "0" }, []);

        fixture.Recorder.BeforeAction(action);

        Assert.Equal(new ElementPoint(120, 85), fixture.Client.CursorPoints.Last());
        Assert.Contains(fixture.Client.EvaluatedExpressions,
            expression => expression.Contains("frameRect.left + rect.left", StringComparison.Ordinal));
    }

    private sealed class RecorderFixture : IDisposable
    {
        private readonly string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");

        public RecorderFixture()
        {
            Client = new FakeAutomationClient();
            Recorder = new ScriptGifRecorder(Client, new ScriptRecordingOptions(path));
            Recorder.Start("debug");
        }

        public FakeAutomationClient Client { get; }
        public ScriptGifRecorder Recorder { get; }

        public void Dispose()
        {
            Recorder.Dispose();
            if (File.Exists(path)) File.Delete(path);
        }
    }
}
