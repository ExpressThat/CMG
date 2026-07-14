using System.Text.Json;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerHoverRecordingTests
{
    [Fact]
    public void RunText_PostHoverHoldOverridesParentDefault()
    {
        var directory = Directory.CreateTempSubdirectory();
        var gif = Path.Combine(directory.FullName, "hover.gif");
        try
        {
            var result = new BrowserScriptRunner(new BrowserScriptParser()).RunText($$"""
            gif output="{{gif.Replace("\\", "/")}}" timeline=true pointerDuration=0 holdAfterAction=0 postHoverHold=200 {
              hover "#menu" postHoverHold=700
            }
            """, "debug", new FakeAutomationClient());

            Assert.True(result.Success, result.Error);
            using var timeline = JsonDocument.Parse(File.ReadAllText(Path.ChangeExtension(gif, ".timeline.json")));
            var step = Assert.Single(timeline.RootElement.GetProperty("steps").EnumerateArray());
            Assert.True(step.GetProperty("capturedDurationMilliseconds").GetInt32() >= 700);
        }
        finally
        {
            directory.Delete(recursive: true);
        }
    }
}
