using CMG.Browser;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class BrowserScriptRunnerCaptionOptionsTests
{
    [Fact]
    public void RunText_CaptionUsesActionOptions()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""caption "Review state" captionStyle=qa captionPosition=bottom captionSeverity=success""", "debug", client);

        Assert.True(result.Success);
        Assert.Equal("Review state", client.LastMessageBar);
        Assert.Equal(CaptionStyle.Qa, client.LastCaptionOptions?.Style);
        Assert.Equal(CaptionPosition.Bottom, client.LastCaptionOptions?.Position);
        Assert.Equal(CaptionSeverity.Success, client.LastCaptionOptions?.Severity);
    }

    [Fact]
    public void RunText_RecordingScopeAppliesCaptionDefaults()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText("""
            recording captionStyle=teaching captionPosition=left captionSeverity=warning {
              caption "Watch this"
            }
            """, "debug", client);

        Assert.True(result.Success);
        Assert.Equal(CaptionStyle.Teaching, client.LastCaptionOptions?.Style);
        Assert.Equal(CaptionPosition.Left, client.LastCaptionOptions?.Position);
        Assert.Equal(CaptionSeverity.Warning, client.LastCaptionOptions?.Severity);
    }

    [Fact]
    public void RunText_CommandCaptionDefaultsCanBeOverriddenByAction()
    {
        var client = new FakeAutomationClient();
        var result = Runner().RunText(
            """caption "Saved" captionPosition=right""",
            "debug",
            client,
            captionOptions: new BrowserCaptionOptions(CaptionStyle.BugReport, CaptionPosition.Top, CaptionSeverity.Error));

        Assert.True(result.Success);
        Assert.Equal(CaptionStyle.BugReport, client.LastCaptionOptions?.Style);
        Assert.Equal(CaptionPosition.Right, client.LastCaptionOptions?.Position);
        Assert.Equal(CaptionSeverity.Error, client.LastCaptionOptions?.Severity);
    }

    [Fact]
    public void RunText_RejectsInvalidCaptionStyle()
    {
        var result = Runner().RunText("""caption "Nope" captionStyle=loud""", "debug", new FakeAutomationClient());

        Assert.False(result.Success);
        Assert.Contains("caption option captionStyle= must be one of", result.Error);
    }

    private static BrowserScriptRunner Runner() => new(new BrowserScriptParser());
}
