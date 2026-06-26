using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserControlCommandSurfaceE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserControlCommandSurfaceE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RuntimeWaitAssertionInputAndStorageCommands_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "wait", "element", "#title");
        Run("browser", "control", "wait", "selector", "#visible-target", "--state", "visible");
        Run("browser", "control", "wait", "function", "document.title.includes('CMG')");
        Run("browser", "control", "wait", "timeout", "1");
        Run("browser", "control", "input", "fill", "#name", "Grace Hopper");
        Run("browser", "control", "input", "check", "#agree");
        Run("browser", "control", "input", "select", "#plan", "team");
        Run("browser", "control", "input", "press", "Tab");
        Run("browser", "control", "input", "focus", "#name");
        Run("browser", "control", "input", "keyboardShortcut", "Control+A");
        Run("browser", "control", "input", "insertText", "typed");
        Run("browser", "control", "page", "runtime", "inputValue", "#name")
            .StdoutContains("typed");
        Run("browser", "control", "page", "runtime", "getAttribute", "#primary", "data-state")
            .StdoutContains("idle");
        Run("browser", "control", "page", "runtime", "computedStyle", "#css-target", "color")
            .StdoutContains("rgb(10, 20, 30)");
        Run("browser", "control", "page", "runtime", "count", ".item").StdoutContains("3");
        Run("browser", "control", "page", "runtime", "allTextContents", ".item").StdoutContains("Alpha");
        Run("browser", "control", "page", "runtime", "evaluateOnSelector", "#title", "element.textContent")
            .StdoutContains("CMG E2E Fixture");
        Run("browser", "control", "assertions", "expectChecked", "#agree");
        Run("browser", "control", "assertions", "expectValue", "#plan", "team");
        Run("browser", "control", "assertions", "expectCount", ".item", "3");
        Run("browser", "control", "assertions", "expectAttribute", "#primary", "data-state", "idle");
        Run("browser", "control", "storage", "local", "set", "cli-key", "cli-value");
        Run("browser", "control", "storage", "local", "get", "cli-key").StdoutContains("cli-value");
        Run("browser", "control", "storage", "session", "set", "session-key", "session-value");
        Run("browser", "control", "storage", "session", "get", "session-key").StdoutContains("session-value");
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"));
        Run("browser", "control", "storage", "cookie", "set", "cookie-key", "cookie-value", "--path", "/");
        Run("browser", "control", "storage", "cookie", "get", "cookie-key").StdoutContains("cookie-value");
    }

    [Fact]
    public void CaptureContextFrameClockAccessibilityAndTabCommands_RunAgainstBrowser()
    {
        Navigate();
        var pdf = fixture.OutputPath("command-page.pdf");
        var snapshot = fixture.OutputPath("accessibility.json");
        var state = fixture.OutputPath("command-storage.json");
        Run("browser", "control", "capture", "pdf", "--path", pdf, "--format", "A4");
        Run("browser", "control", "storage", "storageState", "save", "--path", state);
        Run("browser", "control", "context", "emulate", "--width", "640", "--height", "480", "--touch");
        Run("browser", "control", "context", "emulateMedia", "--color-scheme", "dark");
        Run("browser", "control", "context", "setGeolocation", "51.5", "-0.12", "--accuracy", "5");
        Run("browser", "control", "context", "grantPermissions", "geolocation");
        Run("browser", "control", "context", "clearPermissions");
        Run("browser", "control", "context", "bypassCSP", "true");
        Run("browser", "control", "context", "serviceWorkers", "block");
        Run("browser", "control", "clock", "install", "--now", "1000");
        Run("browser", "control", "clock", "tick", "250");
        Run("browser", "control", "clock", "restore");
        Run("browser", "control", "accessibility", "snapshot", "#visible-target", "--output", snapshot);
        Run("browser", "control", "accessibility", "expect", "--role", "button", "--name", "Visible target");
        Run("browser", "control", "frames", "click", "#fixture-frame", "#frame-button");
        Run("browser", "control", "frames", "expectText", "#fixture-frame", "#frame-status", "frame clicked");
        Run("browser", "control", "tabs", "list").StdoutContains("TAB");
        Run("browser", "control", "tabs", "open", E2ePaths.FixtureFile("index.html"));
        Run("browser", "control", "tabs", "wait", "--count", "2", "--timeout", "5000");
        Run("browser", "control", "tabs", "activate", "--index", "1");
        Run("browser", "control", "tabs", "close", "--index", "1");
        CmgE2eAssert.FileExists(pdf);
        CmgE2eAssert.FileExists(snapshot);
        CmgE2eAssert.FileExists(state);
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", E2ePaths.FixtureFile("index.html"), "--wait-until", "domcontentloaded");
}
