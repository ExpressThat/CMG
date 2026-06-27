using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerEvaluateAssertionTests
{
    [Fact]
    public void RunText_ExpectEvalPassesOnTruthyValue()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("true");

        var result = Runner().RunText("expectEval \"window.ready\"", "debug", client);

        Assert.True(result.Success);
        Assert.Contains("EXPECT_EVAL 001 true", result.StdoutLines);
    }

    [Fact]
    public void RunText_AssertEvalSupportsEquals()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Saved");

        var result = Runner().RunText("assertEval \"window.status\" equals=Saved", "debug", client);

        Assert.True(result.Success);
    }

    [Fact]
    public void RunText_ExpectExpressionSupportsContains()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("Profile saved");

        var result = Runner().RunText("expectExpression \"window.message\" contains=saved", "debug", client);

        Assert.True(result.Success);
    }

    [Fact]
    public void RunText_AssertExpressionReportsActualValueOnFailure()
    {
        var client = new FakeAutomationClient();
        client.EvaluateResponses.Enqueue("false");

        var result = Runner().RunText("assertExpression \"window.ready\"", "debug", client);

        Assert.False(result.Success);
        Assert.Contains("Expected evaluated value to be truthy", result.Error);
        Assert.Contains("Actual: 'false'", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
