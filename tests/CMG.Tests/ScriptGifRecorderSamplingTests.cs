using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderSamplingTests
{
    [Fact]
    public void BeforeAction_SamplesOnlyIntermediateMovementFrames()
    {
        var client = new FakeAutomationClient();
        using var fixture = Recorder(client, new GifCaptureOptimizationOptions(false, 3));

        fixture.Recorder.BeforeAction(Action());

        Assert.Equal(8, client.MouseMoveCount);
        Assert.Equal(4, client.PageScreenshotCount);
        Assert.Equal(4, fixture.Recorder.SampledFramesSkipped);
    }

    [Fact]
    public void BeforeAction_ActionSamplingOverridesRecordingDefault()
    {
        var client = new FakeAutomationClient();
        using var fixture = Recorder(client, new GifCaptureOptimizationOptions(false, 3));

        fixture.Recorder.BeforeAction(Action(new Dictionary<string, string> { ["sampleEvery"] = "1" }));

        Assert.Equal(8, client.PageScreenshotCount);
        Assert.Equal(0, fixture.Recorder.SampledFramesSkipped);
    }

    private static BrowserScriptAction Action(IReadOnlyDictionary<string, string>? options = null) =>
        new(1, "hover", "hover", ["#save"], options ?? new Dictionary<string, string>(), []);

    private static Fixture Recorder(FakeAutomationClient client, GifCaptureOptimizationOptions optimization)
    {
        var encoding = new GifEncodingOptions(CaptureOptimization: optimization);
        return new Fixture(client, new ScriptRecordingOptions(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"), Encoding: encoding));
    }

    private sealed class Fixture : IDisposable
    {
        public Fixture(FakeAutomationClient client, ScriptRecordingOptions options)
        {
            Recorder = new ScriptGifRecorder(client, options);
            Recorder.Start("debug");
        }

        public ScriptGifRecorder Recorder { get; }
        public void Dispose() => Recorder.Dispose();
    }
}
