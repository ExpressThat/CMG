using CMG.Browser;
using CMG.Browser.Scripting;
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

    private static CmgRunOptions Options(IReadOnlyDictionary<string, string>? variables = null) =>
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
            variables ?? new Dictionary<string, string>());
}
