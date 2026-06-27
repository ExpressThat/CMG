using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserEventAliasCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserEventAliasCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ConsoleAndPageErrorAliasCommands_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "events", "console", "captureConsole")
            .StdoutContains("CONSOLE_CAPTURE 001");
        Evaluate("console.warn('alias console ready'); true");
        Run("browser", "control", "events", "console", "waitForConsole", "^alias console", "--match", "regex", "--level", "warn")
            .StdoutContains("CONSOLE 001");
        Run("browser", "control", "events", "console", "toHaveNoConsole", "not emitted", "--level", "error", "--timeout", "10")
            .StdoutContains("CONSOLE_OK 001");
        Evaluate("console.info('generic event console'); true");
        Run("browser", "control", "events", "waitForEvent", "console", "--message", "generic event console", "--level", "info")
            .StdoutContains("CONSOLE 001");

        Run("browser", "control", "events", "pageErrors", "capturePageErrors")
            .StdoutContains("PAGE_ERROR_CAPTURE 001");
        Evaluate("setTimeout(() => { throw new Error('alias page error') }, 0); true");
        Run("browser", "control", "events", "pageErrors", "waitForPageError", "alias page error", "--timeout", "5000")
            .StdoutContains("PAGE_ERROR 001");
        Run("browser", "control", "events", "pageErrors", "toHaveNoPageError", "not emitted", "--timeout", "10")
            .StdoutContains("PAGE_ERROR_OK 001");
        Run("browser", "control", "events", "waitForEvent", "pageError", "--text", "alias page error", "--timeout", "5000")
            .StdoutContains("PAGE_ERROR 001");
    }

    [Fact]
    public void DialogBehaviorAliasCommands_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "events", "dialogs", "onDialog", "accept", "--prompt-text", "alias prompt")
            .StdoutContains("DIALOG_BEHAVIOR 001 accept");
        Run("browser", "control", "input", "scrollIntoView", "#dialog-prompt");
        Run("browser", "control", "input", "click", "#dialog-prompt");
        Run("browser", "control", "events", "dialogs", "waitForDialog", "fixture prompt")
            .StdoutContains("DIALOG 001");
        Run("browser", "control", "assertions", "expectText", "#status", "alias prompt");

        Run("browser", "control", "events", "dialogs", "handleDialog", "dismiss")
            .StdoutContains("DIALOG_BEHAVIOR 001 dismiss");
        Run("browser", "control", "input", "click", "#dialog-confirm");
        Run("browser", "control", "events", "waitForEvent", "dialog", "--message", "fixture confirm")
            .StdoutContains("DIALOG 001");
        Run("browser", "control", "assertions", "expectText", "#status", "confirm dismissed");

        var invalid = fixture.Cli.Run("browser", "control", "events", "dialogs", "handleDialog", "ignore");
        invalid.ShouldFail();
        invalid.StderrContains("accept or dismiss");
    }

    private CmgResult Evaluate(string expression) =>
        Run("browser", "control", "page", "evaluate", expression);

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
