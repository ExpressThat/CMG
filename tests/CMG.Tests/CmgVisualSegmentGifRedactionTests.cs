using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgVisualSegmentGifRedactionTests
{
    [Fact]
    public void RunnerGifBlock_UsesSameRedactionOptionsAsDirectScript()
    {
        var path = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif");
        var options = new Dictionary<string, string>
        {
            ["output"] = path,
            ["redact"] = "#token",
            ["redactStyle"] = "replacement",
            ["redactReplacement"] = "Protected"
        };
        var caption = new CmgNode(3, "caption", "caption", ["evidence"], new Dictionary<string, string>(), []);
        var gif = new CmgNode(2, "gif", "privacy", ["privacy"], options, [caption]);
        var test = new CmgTestCase("flow.cmgscript", "privacy", [gif], new Dictionary<string, string>());
        var client = new FakeAutomationClient();

        try
        {
            var result = Executor(client).Run(test, "debug", RunOptions(), attempt: 1);

            Assert.True(result.Success, result.Error);
            Assert.Contains(client.EvaluatedExpressions, expression => expression.Contains("#token", StringComparison.Ordinal) && expression.Contains("Protected", StringComparison.Ordinal));
        }
        finally
        {
            if (File.Exists(path)) File.Delete(path);
        }
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

    private static CmgRunOptions RunOptions() =>
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
            null,
            new Dictionary<string, string>());
}
