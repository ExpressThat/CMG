using CMG.Browser;
using CMG.Browser.Scripting.Recording;
using CMG.Runner;

namespace CMG.Tests;

public sealed class GifReproductionCommandTests
{
    [Fact]
    public void RunnerCommand_PreservesBrowserPortProjectAndWholeRunGif()
    {
        var gif = Path.Combine(Path.GetTempPath(), "evidence", "flow.gif");
        var command = GifReproductionCommand.Runner(
            BrowserKind.Firefox, 9555, "flow.cmgscript", "checkout", "firefox-ci", gif, commandLevel: true);

        Assert.StartsWith("cmg --firefox run", command, StringComparison.Ordinal);
        Assert.Contains("--grep \"checkout\"", command, StringComparison.Ordinal);
        Assert.Contains("--project \"firefox-ci\"", command, StringComparison.Ordinal);
        Assert.Contains("--browser-port 9555", command, StringComparison.Ordinal);
        Assert.Contains("--gif", command, StringComparison.Ordinal);
    }

    [Fact]
    public void FocusedRunnerGif_DoesNotAddWholeRunGifMode()
    {
        var test = new CmgTestResult("checkout", "flow.cmgscript", true, [], null, "focused.gif", [])
        {
            Browser = BrowserKind.Edge,
            BrowserPort = 9333
        };

        var item = Assert.Single(CmgGifReproductions.For(test));
        Assert.StartsWith("cmg --edge run", item.Command, StringComparison.Ordinal);
        Assert.DoesNotContain("--gif", item.Command, StringComparison.Ordinal);
        var diagnostic = GifReproductionCommand.Diagnostic(item.GifPath, item.Command);
        Assert.StartsWith("GIF_REPRODUCE path=", diagnostic, StringComparison.Ordinal);
        Assert.Contains("\\\"checkout\\\"", diagnostic, StringComparison.Ordinal);
        Assert.DoesNotContain("\\u0022", diagnostic, StringComparison.Ordinal);
    }

    [Fact]
    public void DirectFileCommand_UsesBrowserScopedPortPlacement()
    {
        var command = GifReproductionCommand.DirectFile(
            BrowserKind.Chrome, 9223, "flow.cmgscript", "flow.gif", commandLevel: true);

        Assert.StartsWith("cmg browser --port 9223 control script --file", command, StringComparison.Ordinal);
        Assert.Contains("--gif", command, StringComparison.Ordinal);
    }
}
