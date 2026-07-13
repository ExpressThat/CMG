using System.Text.Json;
using CMG.Runner;

namespace CMG.Tests;

public sealed class CmgReportStepFilterTests
{
    [Fact]
    public void JsonReport_DoesNotHideFailedAssertionWithAggregateInternalOutput()
    {
        var failed = new CmgStepResult(12, "assertText", false, [
            "PASS 001 line=10 action=evaluate (() => true)()",
            "EVALUATE 001 line=10 action=evaluate true",
            "GIF_FAILURE_CAPTION 004 action=\"assertText\" status=captured"
        ], "Expected Complete; actual Pending", null, 4, "", "assertText");
        var test = new CmgTestResult(
            "failure",
            "flow.cmgscript",
            false,
            failed.Output,
            failed.Error,
            null,
            [failed]);

        using var document = JsonDocument.Parse(CmgJsonReportWriter.Write([test]));

        var step = Assert.Single(document.RootElement[0].GetProperty("steps").EnumerateArray());
        Assert.Equal("assertText", step.GetProperty("action").GetString());
        Assert.False(step.GetProperty("success").GetBoolean());
    }
}
