using CMG.Browser.Scripting.Recording;
using CMG.Browser.Scripting;

namespace CMG.Tests;

public sealed class GifActionDefaultsTests
{
    [Fact]
    public void WithOptions_MergesActionDefaultsPerProperty()
    {
        var encoding = new GifEncodingOptions(ActionDefaults: new GifActionDefaults(40, 500));

        var child = encoding.WithOptions(
            new Dictionary<string, string> { ["typingDelay"] = "15" }, "gif", "flow.gif");

        Assert.Equal(15, child.ActionDefaults?.TypingDelayMilliseconds);
        Assert.Equal(500, child.ActionDefaults?.PostHoverHoldMilliseconds);
        Assert.Equal("15", child.ActionDefaults?.ToOptions()["typingDelay"]);
    }

    [Fact]
    public void FromValues_RejectsNegativeDurations()
    {
        var exception = Assert.Throws<CMG.Browser.Scripting.ScriptExecutionException>(() =>
            GifActionDefaults.FromValues(-1, null, "GIF"));

        Assert.Contains("typingDelay= must be zero or greater", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void WholeRunEncodingDefaultsApplyAndActionOverridesWin()
    {
        var client = new FakeAutomationClient();
        var encoding = new GifEncodingOptions(ActionDefaults: new GifActionDefaults(40, 500));
        var gif = new FileInfo(Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid():N}.gif"));
        try
        {
            var runner = new BrowserScriptRunner(new BrowserScriptParser());
            var inherited = runner.RunText("type #name CMG", "debug", client, gif, gifEncoding: encoding);

            Assert.True(inherited.Success, inherited.Error);
            Assert.Equal(40, client.LastTypeDelay);
            var result = runner.RunText("type #name CMG typingDelay=12", "debug", client, gif, gifEncoding: encoding);
            Assert.True(result.Success, result.Error);
            Assert.Equal(12, client.LastTypeDelay);
        }
        finally { if (gif.Exists) gif.Delete(); }
    }

    [Fact]
    public void WholeRunEncodingDefaultsAreInertWithoutRecording()
    {
        var client = new FakeAutomationClient();
        var encoding = new GifEncodingOptions(ActionDefaults: new GifActionDefaults(40, 500));

        var result = new BrowserScriptRunner(new BrowserScriptParser()).RunText(
            "type #name CMG", "debug", client, gifEncoding: encoding);

        Assert.True(result.Success, result.Error);
        Assert.Equal(0, client.LastTypeDelay);
    }
}
