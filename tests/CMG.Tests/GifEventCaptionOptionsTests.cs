using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Tests;

public sealed class GifEventCaptionOptionsTests
{
    [Fact]
    public void FromOptions_UmbrellaSupportsCategoryOverride()
    {
        var options = GifEventCaptionOptions.FromOptions(
            new Dictionary<string, string> { ["eventCaptions"] = "true", ["consoleCaptions"] = "false" },
            "gif option");

        Assert.True(options.Network);
        Assert.True(options.Dialogs);
        Assert.False(options.Console);
        Assert.True(options.Downloads);
        Assert.True(options.Uploads);
        Assert.True(options.ServiceWorkers);
        Assert.True(options.WebSockets);
        Assert.True(options.Workers);
    }

    [Fact]
    public void WithOptions_PreservesUnchangedParentCategories()
    {
        var parent = new GifEventCaptionOptions(true, true, true, true, true, true, true, true);

        var child = parent.WithOptions(
            new Dictionary<string, string> { ["networkCaptions"] = "false" },
            "wait option");

        Assert.False(child.Network);
        Assert.True(child.Dialogs);
        Assert.True(child.Console);
        Assert.True(child.Downloads);
        Assert.True(child.Uploads);
        Assert.True(child.ServiceWorkers);
        Assert.True(child.WebSockets);
        Assert.True(child.Workers);
    }

    [Theory]
    [InlineData("eventCaptions")]
    [InlineData("networkCaptions")]
    [InlineData("dialogCaptions")]
    [InlineData("consoleCaptions")]
    [InlineData("downloadCaptions")]
    [InlineData("uploadCaptions")]
    [InlineData("serviceWorkerCaptions")]
    [InlineData("webSocketCaptions")]
    [InlineData("workerCaptions")]
    public void FromOptions_RejectsInvalidBoolean(string option)
    {
        var error = Assert.Throws<ScriptExecutionException>(() => GifEventCaptionOptions.FromOptions(
            new Dictionary<string, string> { [option] = "perhaps" },
            "gif option"));

        Assert.Contains($"{option}= must be true or false", error.Message, StringComparison.Ordinal);
    }
}
