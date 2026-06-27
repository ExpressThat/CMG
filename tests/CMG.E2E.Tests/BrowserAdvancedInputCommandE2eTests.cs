using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserAdvancedInputCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserAdvancedInputCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void AdvancedInputClipboardUploadScrollAndMouseCommands_RunAgainstBrowser()
    {
        Navigate();
        var upload = E2ePaths.FixtureFile("upload-one.txt");

        Run("browser", "control", "input", "clipboard", "setClipboard", "copied text");
        Run("browser", "control", "input", "clipboard", "read").StdoutContains("copied text");
        Run("browser", "control", "input", "clipboard", "writeClipboard", "second text");
        Run("browser", "control", "input", "clipboard", "readClipboard").StdoutContains("second text");
        Run("browser", "control", "input", "clipboard", "clear");
        Run("browser", "control", "input", "clipboard", "clearClipboard");
        Run("browser", "control", "input", "tap", "#primary");
        Run("browser", "control", "assertions", "expectText", "#status", "clicked");
        Run("browser", "control", "input", "touchTap", "#primary");
        Run("browser", "control", "input", "mouse", "move", "--selector", "#primary", "--edge", "center");
        Run("browser", "control", "input", "mouse", "down", "--selector", "#primary", "--edge", "center");
        Run("browser", "control", "input", "mouse", "up", "--selector", "#primary", "--edge", "center");
        Run("browser", "control", "input", "scroll", "to", "bottom", "--selector", "#scroll-pane");
        Run("browser", "control", "input", "scroll", "by", "0", "-40", "--selector", "#scroll-pane");
        Run("browser", "control", "input", "scroll", "wheel", "--selector", "#scroll-pane", "--delta-y", "25");
        Run("browser", "control", "input", "dispatchEvent", "#primary", "cmg-custom", "--detail", "{\"ok\":true}");
        Run("browser", "control", "input", "uploadFiles", "#file-input", upload);
        Run("browser", "control", "assertions", "expectText", "#file-result", "upload-one.txt");
        Run("browser", "control", "input", "setInputFiles", "#file-input", upload);
        Run("browser", "control", "assertions", "expectText", "#file-result", "upload-one.txt");
        Run("browser", "control", "events", "waitForDownload", "--directory", fixture.OutputDirectory, "--pattern", "manual-download.txt");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate()
    {
        Directory.CreateDirectory(fixture.OutputDirectory);
        File.WriteAllText(fixture.OutputPath("manual-download.txt"), "download-ready");
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
    }
}
