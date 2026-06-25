using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerDialogActionTests
{
    [Fact]
    public void RunText_CaptureDialogsInstallsCurrentAndFuturePatch()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("captureDialogs", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("__cmgDialogs", client.LastExpression);
        Assert.Contains("__cmgDialogs", client.LastInitScript);
        Assert.Contains("DIALOG_CAPTURE 001", result.StdoutLines);
    }

    [Fact]
    public void RunText_SetDialogBehaviorValidatesBehavior()
    {
        var result = Runner().RunText("setDialogBehavior maybe", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("expects accept or dismiss", result.Error);
    }

    [Fact]
    public void RunText_WaitForDialogOutputsDialogJson()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":true,"value":{"type":"alert","message":"Saved","accepted":true}}""");

        var result = Runner().RunText("waitForDialog \"Saved\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains(result.StdoutLines, line => line.Contains("DIALOG 001", StringComparison.Ordinal) && line.Contains("\"type\":\"alert\"", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_WaitForDialogReportsTimeout()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("""{"success":false,"error":"Timed out waiting for dialog Saved"}""");

        var result = Runner().RunText("waitForDialog \"Saved\" timeout=1", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Timed out waiting for dialog Saved", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
