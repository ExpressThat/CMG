using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgVisualSegmentExecutorTests
{
    [Fact]
    public void Run_AppliesSlowTimeoutDefaultsToLoweredActions()
    {
        var client = new FakeAutomationClient();
        var test = new CmgTestCase(
            "flow.cmgscript",
            "slow flow",
            [Node("waitForSelector", ["#ready"])],
            new Dictionary<string, string> { ["slow"] = "true" });

        var result = Executor(client).Run(test, "debug", Options(), attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.Equal(15_000, client.LastWaitTimeout);
    }

    [Fact]
    public void Run_ExplicitTimeoutOverridesSlowDefault()
    {
        var client = new FakeAutomationClient();
        var test = new CmgTestCase(
            "flow.cmgscript",
            "slow flow",
            [Node("waitForSelector", ["#ready"], new Dictionary<string, string> { ["timeout"] = "250" })],
            new Dictionary<string, string> { ["slow"] = "true" });

        var result = Executor(client).Run(test, "debug", Options(), attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.Equal(250, client.LastWaitTimeout);
    }

    [Fact]
    public void Run_RuntimeSkipMarksTestSkipped()
    {
        var test = new CmgTestCase(
            "flow.cmgscript",
            "conditional flow",
            [Node("skip", ["Feature unavailable"])],
            new Dictionary<string, string>());

        var result = Executor(new FakeAutomationClient()).Run(test, "debug", Options(), attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.Equal("skipped", result.Status);
        Assert.Equal("Feature unavailable", result.Error);
        Assert.Contains("SKIP 001 Feature unavailable", result.Output);
    }

    [Fact]
    public void Run_AppliesCommandVariablesToScriptActions()
    {
        var client = new FakeAutomationClient();
        var test = new CmgTestCase(
            "flow.cmgscript",
            "parameterized flow",
            [Node("type", ["#name", "${user}"])],
            new Dictionary<string, string>());
        var options = Options(new Dictionary<string, string> { ["user"] = "Ada" });

        var result = Executor(client).Run(test, "debug", options, attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.Equal("Ada", client.LastTypedText);
    }

    [Fact]
    public void Run_AppliesBaseUrlToRelativeNavigation()
    {
        var test = new CmgTestCase(
            "flow.cmgscript",
            "relative navigation",
            [Node("navigate", ["checkout"])],
            new Dictionary<string, string>());
        var options = Options(baseUrl: "https://example.test/app/");

        var result = Executor(new FakeAutomationClient()).Run(test, "debug", options, attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.Contains("NAVIGATED 001 https://example.test/app/checkout", result.Output);
    }

    [Fact]
    public void Run_DeclarationBaseUrlOverridesCommandBaseUrl()
    {
        var test = new CmgTestCase(
            "flow.cmgscript",
            "relative navigation",
            [Node("navigate", ["checkout"])],
            new Dictionary<string, string> { ["baseUrl"] = "https://override.test/" });
        var options = Options(baseUrl: "https://example.test/app/");

        var result = Executor(new FakeAutomationClient()).Run(test, "debug", options, attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.Contains("NAVIGATED 001 https://override.test/checkout", result.Output);
    }

    [Fact]
    public void Run_DoesNotEmitPlannedPlaceholderSteps()
    {
        var test = new CmgTestCase(
            "flow.cmgscript",
            "runtime steps",
            [
                Node("navigate", ["https://example.test"]),
                Node("click", ["#save"])
            ],
            new Dictionary<string, string>());

        var result = Executor(new FakeAutomationClient()).Run(test, "debug", Options(), attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.All(result.Steps, step => Assert.NotEqual(0, step.Sequence));
        Assert.Contains(result.Steps, step => step.Action == "navigate");
        Assert.Contains(result.Steps, step => step.Action == "click");
    }

    [Fact]
    public void Run_NestedBlockActionsKeepTheirOwnSourceLines()
    {
        var child = new CmgNode(11, "click", "click", ["#save"], new Dictionary<string, string>(), []);
        var parent = new CmgNode(10, "narrate", "narrate", ["Save"], new Dictionary<string, string>(), [child]);
        var test = new CmgTestCase("flow.cmgscript", "nested lines", [parent], new Dictionary<string, string>());

        var result = Executor(new FakeAutomationClient()).Run(test, "debug", Options(), attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.Contains(result.Steps, step => step.Action == "narrate" && step.LineNumber == 10);
        Assert.Contains(result.Steps, step => step.Action == "click" && step.LineNumber == 11);
    }

    [Fact]
    public void Run_FailureReasonUsesNestedActionSourceLine()
    {
        var child = new CmgNode(24, "fail", "fail", ["broken"], new Dictionary<string, string>(), []);
        var parent = new CmgNode(20, "narrate", "narrate", ["check"], new Dictionary<string, string>(), [child]);
        var test = new CmgTestCase("flow.cmgscript", "nested failure", [parent], new Dictionary<string, string>());

        var result = Executor(new FakeAutomationClient()).Run(test, "debug", Options(), attempt: 1);

        Assert.False(result.Success);
        Assert.StartsWith("Line 24:", result.Error, StringComparison.Ordinal);
        Assert.Contains(result.Steps, step => !step.Success && step.LineNumber == 24 && step.Error!.StartsWith("Line 24:", StringComparison.Ordinal));
    }

    [Fact]
    public void Run_CommandGifAppliesEncodingAndIsolatesRetainedFrames()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-run-encoding-");
        var gifs = Directory.CreateDirectory(Path.Combine(directory.FullName, "gifs"));
        var frames = Directory.CreateDirectory(Path.Combine(directory.FullName, "frames"));
        var test = new CmgTestCase("flow.cmgscript", "color evidence", [Node("caption", ["Ready"])], new Dictionary<string, string>());
        var options = Options() with
        {
            GifDirectory = gifs,
            GifQuality = GifQuality.Archival,
            GifEncoding = new GifEncodingOptions(GifDitherMode.None, GifPaletteMode.Local, 32, frames.FullName)
        };

        var result = Executor(new FakeAutomationClient()).Run(test, "debug", options, attempt: 1);

        Assert.True(result.Success, result.Error);
        var gif = Assert.Single(result.GifPath!.Split(';', StringSplitOptions.RemoveEmptyEntries));
        Assert.True(File.Exists(gif));
        var retained = Path.Combine(frames.FullName, Path.GetFileNameWithoutExtension(gif));
        Assert.NotEmpty(Directory.GetFiles(retained, "*.png"));
        Assert.Contains(result.Output, line => line.StartsWith("GIF_FRAMES path=", StringComparison.Ordinal) && line.Contains(Path.GetFileName(retained), StringComparison.Ordinal));
        directory.Delete(recursive: true);
    }

    [Fact]
    public void Run_CommandGifKeepsUploadInsideRecordedActionPipeline()
    {
        var directory = Directory.CreateTempSubdirectory("cmg-run-upload-gif-");
        var upload = Path.Combine(directory.FullName, "evidence.txt");
        File.WriteAllText(upload, "evidence");
        var test = new CmgTestCase("flow.cmgscript", "upload evidence",
            [Node("caption", ["Before upload"]), Node("uploadFiles", ["#file", upload]), Node("caption", ["After upload"])],
            new Dictionary<string, string>());
        var client = new FakeAutomationClient();
        var options = Options() with
        {
            GifDirectory = Directory.CreateDirectory(Path.Combine(directory.FullName, "gifs")),
            GifEncoding = new GifEncodingOptions(EventCaptions: new GifEventCaptionOptions(Uploads: true))
        };

        var result = Executor(client).Run(test, "debug", options, attempt: 1);

        Assert.True(result.Success, result.Error);
        Assert.Contains("UPLOAD 002 1", result.Output);
        Assert.Contains(result.Steps, step => step.Action.Equals("uploadFiles", StringComparison.OrdinalIgnoreCase));
        Assert.NotNull(result.GifPath);
        Assert.True(File.Exists(result.GifPath));
        Assert.Equal("After upload", client.LastMessageBar);
        directory.Delete(recursive: true);
    }

    private static CmgVisualSegmentExecutor Executor(IBrowserAutomationClient client) =>
        new(
            new BrowserScriptRunner(new BrowserScriptParser()),
            client,
            new CmgActionLowerer(),
            new CmgApiRequestRunner(),
            new CmgStorageStateRunner(),
            new CmgVisualAssertionRunner(),
            new CmgUploadRunner());

    private static CmgNode Node(string kind, IReadOnlyList<string> args, IReadOnlyDictionary<string, string>? options = null) =>
        new(1, kind, kind, args, options ?? new Dictionary<string, string>(), []);

    private static CmgRunOptions Options(
        IReadOnlyDictionary<string, string>? variables = null,
        string? baseUrl = null) =>
        new(
            BrowserKind.Chrome,
            null,
            null,
            null,
            null,
            null,
            null,
            null,
            0,
            0,
            1,
            false,
            1,
            1,
            null,
            null,
            null,
            baseUrl,
            variables ?? new Dictionary<string, string>());
}
