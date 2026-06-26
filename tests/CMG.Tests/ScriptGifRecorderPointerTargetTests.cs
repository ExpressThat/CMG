using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderPointerTargetTests
{
    [Fact]
    public void BeforeAction_UsesElementOffsetForPointerTarget()
    {
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(10, 20, 100, 40));
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(1, "hover", ["#save"], new Dictionary<string, string>
        {
            ["x"] = "4",
            ["y"] = "8"
        }));

        Assert.Equal(new ElementPoint(14, 28), client.LastMouseMove);
    }
}
