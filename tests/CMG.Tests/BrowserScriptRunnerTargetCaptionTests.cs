using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerTargetCaptionTests
{
    [Theory]
    [InlineData("caption")]
    [InlineData("showMessageBar")]
    public void RunText_AutoPositionsManualCaptionAgainstTarget(string action)
    {
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(20, 400, 120, 40));

        var result = Runner().RunText(
            $"{action} \"Review this action\" target=\"#approve\" captionPosition=auto",
            "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Equal("#approve", client.LastElementBoxSelector);
        Assert.Equal(CaptionPosition.Auto, client.LastCaptionOptions?.Position);
        Assert.Contains(client.EvaluatedExpressions,
            expression => expression.Contains("placeAtBottom", StringComparison.Ordinal) &&
                expression.Contains("#approve", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ManualCaptionSupportsRichLocatorTarget()
    {
        var client = new FakeAutomationClient();
        client.ElementBoxes.Enqueue(new ElementBox(0, 0, 0, 0));

        var result = Runner().RunText(
            "caption \"Review\" target=\"getByRole=button|Approve\" captionPosition=auto",
            "debug", client);

        Assert.True(result.Success, result.Error);
        Assert.Contains(client.EvaluatedExpressions,
            expression => expression.Contains("__cmg_locator_", StringComparison.Ordinal));
    }

    [Fact]
    public void RunText_ManualCaptionExplainsInvalidPosition()
    {
        var result = Runner().RunText(
            "caption Review target=#approve captionPosition=nearby", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("captionPosition= must be one of: top, bottom, left, right, auto", result.Error, StringComparison.Ordinal);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
