using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserInputAliasCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserInputAliasCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void PointerKeyboardAndFormAliases_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "input", "scrollIntoView", "#primary");
        Run("browser", "control", "input", "dblclick", "#primary");
        Run("browser", "control", "assertions", "expectText", "#status", "double clicked");
        Run("browser", "control", "input", "doubleClick", "#primary");
        Run("browser", "control", "assertions", "expectText", "#status", "double clicked");
        Run("browser", "control", "input", "rightClick", "#primary");
        Run("browser", "control", "assertions", "expectText", "#status", "context clicked");
        Run("browser", "control", "input", "contextClick", "#primary");
        Run("browser", "control", "assertions", "expectText", "#status", "context clicked");

        Run("browser", "control", "input", "fill", "#name", "");
        Run("browser", "control", "input", "pressSequentially", "#name", "CMG", "--delay", "1");
        Run("browser", "control", "page", "runtime", "inputValue", "#name").StdoutContains("CMG");
        Run("browser", "control", "input", "selectText", "#name");
        Run("browser", "control", "page", "evaluate", "document.querySelector('#name').selectionStart + ':' + document.querySelector('#name').selectionEnd").StdoutContains("0:3");
        Run("browser", "control", "input", "blur", "#name");
        Run("browser", "control", "assertions", "expectNotFocused", "#name");
        Run("browser", "control", "input", "focus", "#name");
        Run("browser", "control", "input", "keyDown", "Shift");
        Run("browser", "control", "input", "keyUp", "Shift");
        Run("browser", "control", "page", "evaluate", "window.__cmgLastKeyDown + ':' + window.__cmgLastKeyUp").StdoutContains("Shift:Shift");
        Run("browser", "control", "input", "hotkey", "Control+A");
        Run("browser", "control", "input", "insertText", "Agent");
        Run("browser", "control", "page", "runtime", "inputValue", "#name").StdoutContains("Agent");
        Run("browser", "control", "input", "selectOption", "#plan", "--label", "Team");
        Run("browser", "control", "assertions", "expectValue", "#plan", "team");
    }

    [Fact]
    public void DragMouseScrollAndUploadAliases_RunAgainstBrowser()
    {
        Navigate();
        var upload = E2ePaths.FixtureFile("upload-one.txt");

        Run("browser", "control", "input", "scrollIntoView", "#drag-source");
        Run("browser", "control", "input", "dragAndDrop", "#drag-source", "#drop-zone");
        Run("browser", "control", "assertions", "expectText", "#drop-result", "dragged payload");
        Run("browser", "control", "page", "evaluate", "document.querySelector('#drop-result').textContent = 'not dropped'; true");
        Run("browser", "control", "input", "dragTo", "#drag-source", "#drop-zone");
        Run("browser", "control", "assertions", "expectText", "#drop-result", "dragged payload");

        Run("browser", "control", "input", "mouse", "mouseMove", "--selector", "#primary", "--edge", "center");
        Run("browser", "control", "input", "mouse", "mouseDown", "--selector", "#primary", "--edge", "center");
        Run("browser", "control", "input", "mouse", "mouseUp", "--selector", "#primary", "--edge", "center");
        Run("browser", "control", "input", "scroll", "scrollTo", "bottom", "--selector", "#scroll-pane");
        Run("browser", "control", "page", "evaluate", "document.querySelector('#scroll-pane').scrollTop > 0").StdoutContains("True");
        Run("browser", "control", "input", "scroll", "scrollBy", "--x", "0", "--y", "-40", "--selector", "#scroll-pane");
        Run("browser", "control", "input", "selectFile", "#file-input", upload);
        Run("browser", "control", "assertions", "expectText", "#file-result", "upload-one.txt");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
