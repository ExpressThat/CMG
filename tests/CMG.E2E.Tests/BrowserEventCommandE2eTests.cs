using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserEventCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserEventCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void EventCommands_ReportConsolePageErrorAndDialogs()
    {
        Navigate();
        Run("browser", "control", "events", "console", "capture");
        Evaluate("console.warn('cli console event'); true");
        Run("browser", "control", "events", "console", "wait", "cli console", "--level", "warn");
        Run("browser", "control", "events", "wait", "console", "cli console", "--level", "warn");
        Run("browser", "control", "events", "console", "expectNoConsole", "not emitted", "--level", "error", "--timeout", "10");

        Run("browser", "control", "events", "pageErrors", "capture");
        Evaluate("setTimeout(() => { throw new Error('cli page boom') }, 0); true");
        Run("browser", "control", "events", "pageErrors", "wait", "cli page boom", "--timeout", "5000");
        Run("browser", "control", "events", "wait", "pageError", "cli page boom", "--timeout", "5000");
        Run("browser", "control", "events", "pageErrors", "expectNoPageError", "not emitted", "--timeout", "10");

        Navigate();
        Run("browser", "control", "events", "dialogs", "capture", "--prompt-text", "cli prompt value");
        Run("browser", "control", "input", "scrollIntoView", "#dialog-prompt");
        Run("browser", "control", "input", "click", "#dialog-prompt");
        Run("browser", "control", "events", "dialogs", "wait", "fixture prompt");
        Run("browser", "control", "assertions", "expectText", "#status", "cli prompt value");

        Run("browser", "control", "events", "dialogs", "behavior", "dismiss");
        Run("browser", "control", "input", "click", "#dialog-confirm");
        Run("browser", "control", "events", "wait", "dialog", "fixture confirm");
        Run("browser", "control", "assertions", "expectText", "#status", "confirm dismissed");
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
