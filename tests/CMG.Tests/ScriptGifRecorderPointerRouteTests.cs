using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderPointerRouteTests
{
    [Fact]
    public void BeforeAction_CapturesEachGeneratedCoordinateInsteadOfEndpointOnly()
    {
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(200, 100, 160, 60));
        client.ElementBoxes.Enqueue(new ElementBox(200, 100, 160, 60));
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "hover", "hover", ["#save"], new Dictionary<string, string>
        {
            ["pointerDuration"] = "400",
            ["pointerPath"] = "auto",
            ["x"] = "80",
            ["y"] = "30"
        }, []));

        Assert.True(client.CursorPoints.Distinct().Count() >= 4);
        Assert.Equal(new ElementPoint(280, 130), client.CursorPoints[^1]);
    }
}
