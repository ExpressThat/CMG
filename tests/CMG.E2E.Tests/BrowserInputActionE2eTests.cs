using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserInputActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserInputActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_ClipboardUploadAndDownloadActionsRunAgainstBrowser()
    {
        Directory.CreateDirectory(fixture.OutputDirectory);
        File.WriteAllText(fixture.OutputPath("script-download.txt"), "download-ready");
        var secondUpload = fixture.CreateScript("upload-two.txt", "two");
        var script = fixture.CreateScript("input-actions.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            setClipboard "alpha clipboard"
            set clip { readClipboard }
            expect ("${clip}" == "alpha clipboard")
            writeClipboard "beta clipboard"
            set clipTwo { readClipboard }
            expect ("${clipTwo}" == "beta clipboard")
            clearClipboard
            set clipEmpty { readClipboard }
            expect ("${clipEmpty}" == "")
            uploadFiles "#file-input" "{{ScriptPath(E2ePaths.FixtureFile("upload-one.txt"))}}" "{{ScriptPath(secondUpload)}}"
            expectText "#file-result" "upload-one.txt,upload-two.txt"
            setInputFiles "#file-input" "{{ScriptPath(secondUpload)}}"
            expectText "#file-result" "upload-two.txt"
            selectFile "#file-input" "{{ScriptPath(E2ePaths.FixtureFile("upload-one.txt"))}}"
            expectText "#file-result" "upload-one.txt"
            scrollIntoView "#download-link"
            download "#download-link" directory="{{ScriptPath(fixture.OutputDirectory)}}" pattern="script-download.txt" timeout=1000
            waitForDownload directory="{{ScriptPath(fixture.OutputDirectory)}}" pattern="script-download.txt" timeout=1000
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("CLIPBOARD_SET");
        result.StdoutContains("CLIPBOARD_CLEARED");
        result.StdoutContains("UPLOAD");
        result.StdoutContains("DOWNLOAD");
    }

    [Fact]
    public void DirectScript_UploadFailureReportsMissingFile()
    {
        var missing = fixture.OutputPath("missing-upload.txt");
        var script = fixture.CreateScript("upload-missing.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            uploadFiles "#file-input" "{{ScriptPath(missing)}}"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("Upload file");
        result.StderrContains("missing-upload.txt");
    }

    [Fact]
    public void DirectScript_DragAndDropBlockRunsAgainstBrowser()
    {
        var script = fixture.CreateScript("drag-block.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            scrollIntoView "#drag-source"
            dragAndDrop "#drag-source" {
              hover "#drop-zone"
              waitForElement "#drop-zone"
              drop "#drop-zone"
            }
            expectText "#drop-result" "dragged payload"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("PASS 003 dragAndDrop");
    }

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
