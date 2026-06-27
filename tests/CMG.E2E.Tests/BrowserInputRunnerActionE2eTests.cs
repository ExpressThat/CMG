using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserInputRunnerActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserInputRunnerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_AdvancedInputActionsRunInsideTests()
    {
        Directory.CreateDirectory(fixture.OutputDirectory);
        File.WriteAllText(fixture.OutputPath("runner-download.txt"), "download-ready");
        var secondUpload = fixture.CreateScript("runner-upload-two.txt", "two");
        var traceDir = fixture.OutputPath("runner-input-traces");
        var script = fixture.CreateScript("runner-input-actions.cmgscript", $$"""
            test "runner advanced input actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              scrollIntoView "#primary"
              setClipboard "runner clipboard"
              set clip { readClipboard }
              expect ("${clip}" == "runner clipboard")
              clearClipboard
              tap "#primary"
              touchTap "#primary"
              dblclick "#primary"
              expectText "#status" "double clicked"
              rightClick "#primary"
              expectText "#status" "context clicked"
              fill "#name" ""
              pressSequentially "#name" "CMG" delay=1
              expectValue "#name" "CMG"
              selectText "#name"
              blur "#name"
              expectNotFocused "#name"
              focus "#name"
              keyDown "Shift"
              keyUp "Shift"
              expectEval "window.__cmgLastKeyDown + ':' + window.__cmgLastKeyUp" equals="Shift:Shift"
              hotkey "Control+A"
              insertText "Agent"
              expectValue "#name" "Agent"
              selectOption "#plan" optionLabel="Team"
              expectValue "#plan" "team"
              scrollIntoView "#drag-source"
              dragTo "#drag-source" "#drop-zone"
              expectText "#drop-result" "dragged payload"
              scrollIntoView "#primary"
              mouseMove selector="#primary" edge=center
              mouseDown selector="#primary" edge=center
              mouseUp selector="#primary" edge=center
              scrollTo bottom selector="#scroll-pane"
              expectEval "document.querySelector('#scroll-pane').scrollTop > 0" equals="True"
              scrollBy 0 -40 selector="#scroll-pane"
              wheel "#scroll-pane" deltaY=25
              uploadFiles "#file-input" "{{ScriptPath(E2ePaths.FixtureFile("upload-one.txt"))}}" "{{ScriptPath(secondUpload)}}"
              expectText "#file-result" "upload-one.txt,runner-upload-two.txt"
              setInputFiles "#file-input" "{{ScriptPath(secondUpload)}}"
              expectText "#file-result" "runner-upload-two.txt"
              selectFile "#file-input" "{{ScriptPath(E2ePaths.FixtureFile("upload-one.txt"))}}"
              expectText "#file-result" "upload-one.txt"
              scrollIntoView "#download-link"
              download "#download-link" directory="{{ScriptPath(fixture.OutputDirectory)}}" pattern="runner-download.txt" timeout=1000
              waitForDownload directory="{{ScriptPath(fixture.OutputDirectory)}}" pattern="runner-download.txt" timeout=1000
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner advanced input actions");
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "runner advanced input actions");
        AssertTraceContains(trace, "\"steps\"");
        AssertTraceContains(trace, "CLIPBOARD_SET");
        AssertTraceContains(trace, "CLIPBOARD_CLEARED");
        AssertTraceContains(trace, "TAP");
        AssertTraceContains(trace, "KEY_DOWN");
        AssertTraceContains(trace, "KEY_UP");
        AssertTraceContains(trace, "KEYBOARD_SHORTCUT");
        AssertTraceContains(trace, "TEXT_INSERTED");
        AssertTraceContains(trace, "SCROLL_TO");
        AssertTraceContains(trace, "SCROLL_BY");
        AssertTraceContains(trace, "WHEEL");
        AssertTraceContains(trace, "UPLOAD");
        AssertTraceContains(trace, "DOWNLOAD");
    }

    [Fact]
    public void RunCommand_InputActionFailureReportsStepReason()
    {
        var missing = fixture.OutputPath("missing-runner-upload.txt");
        var script = fixture.CreateScript("runner-input-failure.cmgscript", $$"""
            test "runner input failure" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              uploadFiles "#file-input" "{{ScriptPath(missing)}}"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=uploadFiles");
        result.StderrContains("missing-runner-upload.txt");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
