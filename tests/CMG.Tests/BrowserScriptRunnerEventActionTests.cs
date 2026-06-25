using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerEventActionTests
{
    [Fact]
    public void RunText_WaitForEventPopupUsesTabWait()
    {
        var client = new FakeAutomationClient();
        client.TabResponses.Enqueue([new ChromePageTab("1", "one", "about:blank")]);

        var result = Runner().RunText("waitForEvent popup count=1 timeout=1", "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("TAB_COUNT", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForEventDialogUsesDialogMatcher()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"type":"alert","message":"Saved","accepted":true}}""");

        var result = Runner().RunText("waitForEvent dialog Saved timeout=100", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("Saved", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("DIALOG", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForEventResponseUsesPatternOption()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"url":"/api","status":200}}""");

        var result = Runner().RunText("waitForEvent response pattern=\"/api\" timeout=100", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgResponses", client.LastExpression);
        Assert.Contains(result.StdoutLines, line => line.Contains("RESPONSE", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForEventDownloadUsesDownloadWait()
    {
        var directory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(directory);
        File.WriteAllText(Path.Combine(directory, "report.csv"), "ok");

        var result = Runner().RunText($"waitForEvent download directory=\"{Slash(directory)}\" pattern=\"*.csv\"", "debug", new FakeAutomationClient());

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("DOWNLOAD", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForEventReportsUnknownEvent()
    {
        var result = Runner().RunText("waitForEvent websocket \"/socket\"", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("waitForEvent supports", result.Error);
    }

    [Fact]
    public void RunText_WaitForEventRequiresMatcherForMatchedEvents()
    {
        var result = Runner().RunText("waitForEvent console", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("requires a matcher", result.Error);
    }

    private static string Slash(string path) => path.Replace('\\', '/');

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
