using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerDispatchEventTests
{
    [Fact]
    public void RunText_DispatchEventUsesResolvedLocator()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("dispatchEvent text=Save ready", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("[data-cmg-locator-id=\"__cmg_locator_1\"]", client.LastExpression);
        Assert.Contains("new Event", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.StartsWith("EVENT 001 ready", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_DispatchEventSupportsCustomDetail()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("dispatchEvent \"#target\" \"cmg:event\" detail=\"{\\\"ok\\\":true}\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("new CustomEvent", client.LastExpression);
        Assert.Contains("JSON.parse", client.LastExpression);
    }

    [Fact]
    public void RunText_DispatchEventRejectsInvalidBoolean()
    {
        var result = Runner().RunText("dispatchEvent \"#target\" ready bubbles=maybe", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("bubbles= must be true or false", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
