using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserControlCommandSurfaceE2eTests : IClassFixture<CmgBrowserFixture>
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
    public void ProviderStyleAssertionAliases_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "input", "fill", "#name", "CMG");
        Run("browser", "control", "input", "focus", "#name");
        Run("browser", "control", "input", "check", "#agree");
        Run("browser", "control", "page", "evaluate", "const select = document.querySelector('#multi'); for (const option of select.options) option.selected = ['alpha','beta'].includes(option.value); select.dispatchEvent(new Event('change', { bubbles: true })); true");

        Run("browser", "control", "assertions", "toHaveText", "#title", "CMG");
        Run("browser", "control", "assertions", "toHaveNoText", "#status", "missing", "--timeout", "50");
        Run("browser", "control", "assertions", "toBeVisible", "#visible-target");
        Run("browser", "control", "assertions", "toBeHidden", "#hidden-target");
        Run("browser", "control", "assertions", "toBeEnabled", "#primary");
        var disabled = fixture.Cli.Run("browser", "control", "assertions", "toBeDisabled", "#primary", "--timeout", "50");
        disabled.ShouldFail();
        Run("browser", "control", "assertions", "toBeAttached", "#primary");
        Run("browser", "control", "assertions", "toBeNotAttached", "#not-present", "--timeout", "50");
        Run("browser", "control", "assertions", "toBeEditable", "#name");
        Run("browser", "control", "assertions", "toBeNotEditable", "#primary");
        Run("browser", "control", "assertions", "toBeEmpty", "#empty-target");
        Run("browser", "control", "assertions", "toBeNotEmpty", "#title");
        Run("browser", "control", "assertions", "toBeFocused", "#name");
        Run("browser", "control", "assertions", "toBeNotFocused", "#primary");
        Run("browser", "control", "input", "scrollIntoView", "#visible-target");
        Run("browser", "control", "assertions", "toBeInViewport", "#visible-target");
        Run("browser", "control", "assertions", "toHaveValue", "#name", "CMG");
        Run("browser", "control", "assertions", "toHaveValues", "#multi", "alpha", "beta");
        Run("browser", "control", "assertions", "toHaveAttribute", "#primary", "data-state", "idle");
        Run("browser", "control", "assertions", "toHaveClass", "#class-target", "beta");
        Run("browser", "control", "assertions", "toHaveId", "#primary", "primary");
        Run("browser", "control", "assertions", "toHaveCSS", "#css-target", "color", "rgb(10, 20, 30)");
        Run("browser", "control", "assertions", "toHaveJSProperty", "#primary", "dataset.state", "idle");
        Run("browser", "control", "assertions", "toHaveAccessibleName", "#visible-target", "Visible target");
        Run("browser", "control", "assertions", "toHaveRole", "#visible-target", "button");
        Run("browser", "control", "assertions", "toBeChecked", "#agree");
        Run("browser", "control", "input", "uncheck", "#agree");
        Run("browser", "control", "assertions", "toBeUnchecked", "#agree");
        Run("browser", "control", "assertions", "toHaveCount", ".item", "3");
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
