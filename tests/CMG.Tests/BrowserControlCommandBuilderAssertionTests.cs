using System.CommandLine;
using CMG.Browser;
using CMG.Commands;

namespace CMG.Tests;

public sealed class BrowserControlCommandBuilderAssertionTests
{
    [Theory]
    [InlineData("assertText #status Ready --timeout 500", "assertText \"#status\" \"Ready\" timeout=\"500\"")]
    [InlineData("expectText #status Ready", "expectText \"#status\" \"Ready\"")]
    [InlineData("toHaveText #status Ready", "toHaveText \"#status\" \"Ready\"")]
    [InlineData("toContainText #status Ready", "toContainText \"#status\" \"Ready\"")]
    [InlineData("containsText #status Ready", "containsText \"#status\" \"Ready\"")]
    [InlineData("waitForText #status Ready --timeout 1000", "waitForText \"#status\" \"Ready\" timeout=\"1000\"")]
    [InlineData("contains Ready --timeout 250", "contains \"Ready\" timeout=\"250\"")]
    [InlineData("assertVisible #save --timeout 5000", "assertVisible \"#save\" timeout=\"5000\"")]
    [InlineData("visible #save --timeout 5000", "expectVisible \"#save\" timeout=\"5000\"")]
    [InlineData("hidden text=Done", "expectHidden \"text=Done\"")]
    [InlineData("enabled #save", "expectEnabled \"#save\"")]
    [InlineData("disabled #save", "expectDisabled \"#save\"")]
    [InlineData("expectVisible #save", "expectVisible \"#save\"")]
    [InlineData("toBeVisible #save", "toBeVisible \"#save\"")]
    [InlineData("waitForVisible #save --timeout 100", "waitForVisible \"#save\" timeout=\"100\"")]
    [InlineData("expectHidden #toast", "expectHidden \"#toast\"")]
    [InlineData("toBeHidden #toast", "toBeHidden \"#toast\"")]
    [InlineData("waitForHidden #toast --timeout 100", "waitForHidden \"#toast\" timeout=\"100\"")]
    [InlineData("expectEnabled #save", "expectEnabled \"#save\"")]
    [InlineData("toBeEnabled #save", "toBeEnabled \"#save\"")]
    [InlineData("expectDisabled #save", "expectDisabled \"#save\"")]
    [InlineData("toBeDisabled #save", "toBeDisabled \"#save\"")]
    [InlineData("expectAttached #save", "expectAttached \"#save\"")]
    [InlineData("toBeDetached #toast", "toBeDetached \"#toast\"")]
    [InlineData("expectEditable #name", "expectEditable \"#name\"")]
    [InlineData("toBeEmpty #name", "toBeEmpty \"#name\"")]
    [InlineData("expectFocused #name", "expectFocused \"#name\"")]
    [InlineData("toBeInViewport #save", "toBeInViewport \"#save\"")]
    [InlineData("value #name CMG --timeout 100", "expectValue \"#name\" \"CMG\" timeout=\"100\"")]
    [InlineData("expectValue #name CMG", "expectValue \"#name\" \"CMG\"")]
    [InlineData("toHaveValue #name CMG", "toHaveValue \"#name\" \"CMG\"")]
    [InlineData("expectValues #plans basic pro", "expectValues \"#plans\" \"basic\" \"pro\"")]
    [InlineData("toHaveValues #plans basic pro --timeout 100", "toHaveValues \"#plans\" \"basic\" \"pro\" timeout=\"100\"")]
    [InlineData("attribute #save data-state ready", "expectAttribute \"#save\" \"data-state\" \"ready\"")]
    [InlineData("expectAttribute #save data-state ready", "expectAttribute \"#save\" \"data-state\" \"ready\"")]
    [InlineData("toHaveAttribute #save data-state ready", "toHaveAttribute \"#save\" \"data-state\" \"ready\"")]
    [InlineData("expectClass #save ready", "expectClass \"#save\" \"ready\"")]
    [InlineData("toHaveClass #save ready", "toHaveClass \"#save\" \"ready\"")]
    [InlineData("expectId #save save", "expectId \"#save\" \"save\"")]
    [InlineData("toHaveId #save save", "toHaveId \"#save\" \"save\"")]
    [InlineData("expectCSS #save display block", "expectCSS \"#save\" \"display\" \"block\"")]
    [InlineData("toHaveCSS #save display block", "toHaveCSS \"#save\" \"display\" \"block\"")]
    [InlineData("expectProperty #save dataset.ready true", "expectProperty \"#save\" \"dataset.ready\" \"true\"")]
    [InlineData("toHaveJSProperty #save dataset.ready true", "toHaveJSProperty \"#save\" \"dataset.ready\" \"true\"")]
    [InlineData("expectAccessibleName #save Save", "expectAccessibleName \"#save\" \"Save\"")]
    [InlineData("toHaveAccessibleName #save Save", "toHaveAccessibleName \"#save\" \"Save\"")]
    [InlineData("expectRole #save button", "expectRole \"#save\" \"button\"")]
    [InlineData("toHaveRole #save button", "toHaveRole \"#save\" \"button\"")]
    [InlineData("checked #agree", "expectChecked \"#agree\"")]
    [InlineData("checked #agree --expected false", "expectChecked \"#agree\" \"false\"")]
    [InlineData("expectChecked #agree", "expectChecked \"#agree\"")]
    [InlineData("toBeChecked #agree --expected false", "toBeChecked \"#agree\" \"false\"")]
    [InlineData("count .row 2", "expectCount \".row\" \"2\"")]
    [InlineData("expectCount .row 2", "expectCount \".row\" \"2\"")]
    [InlineData("toHaveCount .row 2", "toHaveCount \".row\" \"2\"")]
    public void AssertionCommands_MapToScriptActions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control assertions {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(BrowserKind.Chrome, handler.BrowserKind);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Theory]
    [InlineData("eval document.title --equals Checkout", "expectEval \"document.title\" equals=\"Checkout\"")]
    [InlineData("expectEval document.title --equals Checkout", "expectEval \"document.title\" equals=\"Checkout\"")]
    [InlineData("assertEval document.title --equals Checkout", "assertEval \"document.title\" equals=\"Checkout\"")]
    [InlineData("expectExpression window.appReady --timeout 5000", "expectExpression \"window.appReady\" timeout=\"5000\"")]
    [InlineData("assertExpression document.body.innerText --contains Ready", "assertExpression \"document.body.innerText\" contains=\"Ready\"")]
    [InlineData("eval window.appReady --timeout 5000", "expectEval \"window.appReady\" timeout=\"5000\"")]
    [InlineData("eval document.body.innerText --contains Ready", "expectEval \"document.body.innerText\" contains=\"Ready\"")]
    public void EvalAssertionCommand_MapsExpectationOptions(string commandTail, string expectedScript)
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse($"control assertions {commandTail}").Invoke();

        Assert.Equal(0, exitCode);
        Assert.Equal(expectedScript, handler.ScriptLine);
    }

    [Fact]
    public void BrowserSelectionRejectsMultipleBrowserOptions()
    {
        var handler = new CapturingBrowserControlCommandHandler();
        var exitCode = BuildRoot(handler).Parse("--chrome --edge control assertions visible #save").Invoke();

        Assert.Equal(1, exitCode);
        Assert.Equal(BrowserKind.InvalidSelection, handler.BrowserKind);
    }

    private static RootCommand BuildRoot(CapturingBrowserControlCommandHandler handler)
    {
        var chrome = new Option<bool>("--chrome");
        var edge = new Option<bool>("--edge");
        var firefox = new Option<bool>("--firefox");
        var root = new RootCommand();
        root.Options.Add(chrome);
        root.Options.Add(edge);
        root.Options.Add(firefox);
        root.Subcommands.Add(new BrowserControlCommandBuilder(handler).Build(new BrowserSelectionOptions(chrome, edge, firefox)));
        return root;
    }

    private sealed class CapturingBrowserControlCommandHandler : IBrowserControlCommandHandler
    {
        public BrowserKind BrowserKind { get; private set; }

        public string? ScriptLine { get; private set; }

        public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output) => 0;

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif) => 0;

        public int ValidateScript(string file) => 0;

        public int RunScriptAction(BrowserKind browserKind, string scriptLine)
        {
            BrowserKind = browserKind;
            ScriptLine = scriptLine;
            return browserKind is BrowserKind.InvalidSelection ? 1 : 0;
        }
    }
}
