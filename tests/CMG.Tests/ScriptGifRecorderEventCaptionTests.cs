using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class ScriptGifRecorderEventCaptionTests
{
    [Theory]
    [InlineData("waitForResponse", "Network response matched", CaptionSeverity.Info)]
    [InlineData("waitForDialog", "Dialog observed", CaptionSeverity.Info)]
    [InlineData("waitForConsole", "Console event observed", CaptionSeverity.Warning)]
    [InlineData("waitForPageError", "Page error observed", CaptionSeverity.Error)]
    [InlineData("waitForDownload", "Download completed", CaptionSeverity.Info)]
    [InlineData("uploadFiles", "Selected 2 files", CaptionSeverity.Info)]
    [InlineData("waitForServiceWorker", "Service worker available", CaptionSeverity.Info)]
    [InlineData("waitForWebSocket", "WebSocket connected", CaptionSeverity.Info)]
    [InlineData("waitForWebSocketMessage", "WebSocket message observed", CaptionSeverity.Info)]
    [InlineData("workerEvaluate", "Worker expression evaluated", CaptionSeverity.Info)]
    public void AfterAction_ShowsSafeEventOutcome(string name, string expected, CaptionSeverity severity)
    {
        using var fixture = new RecorderFixture();
        var action = Action(name);

        fixture.Recorder.BeforeAction(action);
        fixture.Recorder.AfterAction(action, Output(name));

        Assert.Equal(expected, fixture.Client.LastMessageBar);
        Assert.Equal(CaptionStyle.Qa, fixture.Client.LastCaptionOptions?.Style);
        Assert.Equal(CaptionPosition.Bottom, fixture.Client.LastCaptionOptions?.Position);
        Assert.Equal(severity, fixture.Client.LastCaptionOptions?.Severity);
        Assert.Contains(fixture.Client.EvaluatedExpressions, expression => expression.Contains("__cmg_message_bar", StringComparison.Ordinal));
    }

    [Fact]
    public void ChildCategoryOverride_DisablesInheritedCaption()
    {
        using var fixture = new RecorderFixture();
        var action = Action("waitForConsole", new Dictionary<string, string> { ["consoleCaptions"] = "false" });

        fixture.Recorder.BeforeAction(action);
        fixture.Recorder.AfterAction(action, Output(action.Name));

        Assert.Equal(string.Empty, fixture.Client.LastMessageBar);
    }

    [Theory]
    [InlineData("{\"type\":\"alert\",\"accepted\":true}", "Dialog accepted")]
    [InlineData("{\"type\":\"confirm\",\"accepted\":false}", "Dialog dismissed")]
    [InlineData("{\"type\":\"prompt\",\"accepted\":true}", "Dialog prompt submitted")]
    public void DialogCaption_ReportsHandledState(string result, string expected)
    {
        using var fixture = new RecorderFixture();
        var action = Action("waitForDialog");

        fixture.Recorder.AfterAction(action, [$"DIALOG 007 {result}"]);

        Assert.Equal(expected, fixture.Client.LastMessageBar);
    }

    [Fact]
    public void WaitForEvent_UsesSpecificNetworkEventName()
    {
        using var fixture = new RecorderFixture();
        var action = new BrowserScriptAction(7, "waitForEvent", "waitForEvent", ["response", "/api"], new Dictionary<string, string>(), []);

        fixture.Recorder.AfterAction(action, ["RESPONSE 007 {}"]);

        Assert.Equal("Network response matched", fixture.Client.LastMessageBar);
    }

    [Theory]
    [InlineData("listConsole", "CONSOLE_LIST 007 count=3", "Console entries: 3")]
    [InlineData("listPageErrors", "PAGE_ERROR_LIST 007 count=2", "Page errors: 2")]
    public void ListCaption_ReportsCountWithoutPayload(string name, string output, string expected)
    {
        using var fixture = new RecorderFixture();

        fixture.Recorder.AfterAction(Action(name), [output, "PRIVATE_ENTRY should not be captioned"]);

        Assert.Equal(expected, fixture.Client.LastMessageBar);
        Assert.DoesNotContain("PRIVATE_ENTRY", fixture.Client.LastMessageBar, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("expectNoConsole", "No matching console events")]
    [InlineData("toHaveNoPageError", "No matching page errors")]
    public void AbsenceAssertion_UsesEventSuccessCaption(string name, string expected)
    {
        using var fixture = new RecorderFixture();
        var action = Action(name);

        fixture.Recorder.AfterAction(action, ["OK"]);

        Assert.Equal(expected, fixture.Client.LastMessageBar);
        Assert.Equal(CaptionSeverity.Success, fixture.Client.LastCaptionOptions?.Severity);
    }

    [Fact]
    public void EventCaptionOptions_AreInertWithoutRecorder()
    {
        var client = new FakeAutomationClient();
        var result = new BrowserScriptRunner(new BrowserScriptParser()).RunText(
            "recording eventCaptions=true { captureConsole }",
            "debug",
            client);

        Assert.True(result.Success, result.Error);
        Assert.Equal(string.Empty, client.LastMessageBar);
        Assert.Equal(0, client.PageScreenshotCount);
        Assert.Empty(client.CursorStates);
    }

    private static BrowserScriptAction Action(string name, IReadOnlyDictionary<string, string>? options = null) =>
        new(7, name, name, name == "uploadFiles" ? ["#file", "secret-one.txt", "secret-two.txt"] : ["match"],
            options ?? new Dictionary<string, string>(), []);

    private static IReadOnlyList<string> Output(string name) => name switch
    {
        "uploadFiles" => ["UPLOAD 007 2"],
        "waitForConsole" => ["CONSOLE 007 sensitive console text"],
        "waitForPageError" => ["PAGE_ERROR 007 sensitive stack"],
        _ => [$"EVENT 007 {name}"]
    };

    private sealed class RecorderFixture : IDisposable
    {
        private readonly string path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");

        public RecorderFixture()
        {
            Client = new FakeAutomationClient();
            var encoding = new GifEncodingOptions(EventCaptions: new GifEventCaptionOptions(true, true, true, true, true, true, true, true));
            Recorder = new ScriptGifRecorder(Client, new ScriptRecordingOptions(path, Encoding: encoding));
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
