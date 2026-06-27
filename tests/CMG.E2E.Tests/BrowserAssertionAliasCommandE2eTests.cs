using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserAssertionAliasCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserAssertionAliasCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void TextStateAndEvalAssertionAliases_RunAgainstBrowser()
    {
        Navigate();
        Run("browser", "control", "input", "fill", "#name", "CMG");
        Run("browser", "control", "input", "focus", "#name");
        Run("browser", "control", "assertions", "containsText", "#title", "cmg", "--ignore-case");
        Run("browser", "control", "assertions", "waitForText", "#status", "^ready$", "--match", "regex");
        Run("browser", "control", "assertions", "expectNoText", "#status", "missing", "--timeout", "50");
        Run("browser", "control", "assertions", "expectNotText", "#status", "missing", "--timeout", "50");
        Run("browser", "control", "assertions", "notContainsText", "#status", "missing", "--timeout", "50");
        Run("browser", "control", "assertions", "toNotContainText", "#status", "missing", "--timeout", "50");
        Run("browser", "control", "assertions", "contains", "CMG E2E Fixture");
        Run("browser", "control", "assertions", "notContains", "definitely absent", "--timeout", "50");
        Run("browser", "control", "assertions", "assertVisible", "#visible-target");
        Run("browser", "control", "assertions", "expectVisible", "#visible-target");
        Run("browser", "control", "assertions", "waitForVisible", "#visible-target");
        Run("browser", "control", "assertions", "expectNotVisible", "#hidden-target", "--timeout", "50");
        Run("browser", "control", "assertions", "expectHidden", "#hidden-target");
        Run("browser", "control", "assertions", "waitForHidden", "#hidden-target");
        Run("browser", "control", "assertions", "expectNotHidden", "#visible-target");
        Run("browser", "control", "assertions", "expectEnabled", "#primary");
        Run("browser", "control", "assertions", "expectNotDisabled", "#primary");
        Run("browser", "control", "assertions", "expectDisabled", "#disabled-button");
        Run("browser", "control", "assertions", "expectNotEnabled", "#disabled-button");
        Run("browser", "control", "assertions", "expectAttached", "#primary");
        Run("browser", "control", "assertions", "expectNotDetached", "#primary");
        Run("browser", "control", "assertions", "expectDetached", "#not-present", "--timeout", "50");
        Run("browser", "control", "assertions", "toBeDetached", "#not-present", "--timeout", "50");
        Run("browser", "control", "assertions", "expectNotAttached", "#not-present", "--timeout", "50");
        Run("browser", "control", "assertions", "expectEditable", "#name");
        Run("browser", "control", "assertions", "expectNotEditable", "#primary");
        Run("browser", "control", "assertions", "expectEmpty", "#empty-target");
        Run("browser", "control", "assertions", "expectNotEmpty", "#title");
        Run("browser", "control", "assertions", "expectFocused", "#name");
        Run("browser", "control", "assertions", "expectNotFocused", "#primary");
        Run("browser", "control", "input", "scrollIntoView", "#visible-target");
        Run("browser", "control", "assertions", "expectInViewport", "#visible-target");
        Run("browser", "control", "assertions", "expectNotInViewport", "#deep-button");
        Run("browser", "control", "assertions", "assertEval", "document.title", "--equals", "CMG E2E Fixture");
        Run("browser", "control", "assertions", "expectExpression", "document.body.innerText", "--contains", "Primary action");
        Run("browser", "control", "assertions", "assertExpression", "window.location.href", "--contains", "index.html");
    }

    [Fact]
    public void ValueAttributeAndCheckedAssertionAliases_RunAgainstBrowser()
    {
        Navigate();
        SelectMultipleValues();
        Run("browser", "control", "input", "fill", "#name", "CMG");
        Run("browser", "control", "assertions", "value", "#name", "CMG");
        Run("browser", "control", "assertions", "expectValue", "#name", "CMG");
        Run("browser", "control", "assertions", "expectValues", "#multi", "alpha", "beta");
        Run("browser", "control", "assertions", "attribute", "#primary", "data-state", "idle");
        Run("browser", "control", "assertions", "expectAttribute", "#primary", "data-state", "idle");
        Run("browser", "control", "assertions", "expectClass", "#class-target", "beta");
        Run("browser", "control", "assertions", "expectId", "#primary", "primary");
        Run("browser", "control", "assertions", "expectCSS", "#css-target", "color", "rgb(10, 20, 30)");
        Run("browser", "control", "assertions", "expectProperty", "#primary", "dataset.state", "idle");
        Run("browser", "control", "assertions", "expectAccessibleName", "#visible-target", "Visible target");
        Run("browser", "control", "assertions", "expectRole", "#visible-target", "button");
        Run("browser", "control", "assertions", "checked", "#agree", "--expected", "false");
        Run("browser", "control", "assertions", "expectNotChecked", "#agree");
        Run("browser", "control", "assertions", "unchecked", "#agree");
        Run("browser", "control", "assertions", "expectUnchecked", "#agree");
        Run("browser", "control", "input", "check", "#agree");
        Run("browser", "control", "assertions", "checked", "#agree");
        Run("browser", "control", "assertions", "expectChecked", "#agree");
        Run("browser", "control", "assertions", "count", ".item", "3");
        Run("browser", "control", "assertions", "expectCount", ".item", "3");
    }

    private void SelectMultipleValues() =>
        Run("browser", "control", "page", "evaluate",
            "const s=document.querySelector('#multi'); for (const o of s.options) o.selected=['alpha','beta'].includes(o.value); s.dispatchEvent(new Event('change',{bubbles:true})); true");

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
