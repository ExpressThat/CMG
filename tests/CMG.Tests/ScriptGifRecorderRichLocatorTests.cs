using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderRichLocatorTests
{
    [Fact]
    public void BeforeAction_ResolvesRichLocatorBeforeMovingPointer()
    {
        var client = new FakeAutomationClient();
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        using var recorder = new ScriptGifRecorder(client, new ScriptRecordingOptions(path));
        recorder.Start("debug");

        recorder.BeforeAction(new BrowserScriptAction(7, "fill", "fill", ["getByLabel=Email", "a@b.test"], new Dictionary<string, string>(), []));

        Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("window.__cmgLocatorResolvers", StringComparison.Ordinal));
        Assert.Equal("[data-cmg-locator-id=\"__cmg_locator_7\"]", client.LastElementCenterSelector);
    }
}
