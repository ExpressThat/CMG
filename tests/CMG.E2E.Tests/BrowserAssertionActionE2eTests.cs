using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserAssertionActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserAssertionActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_AssertionAliasesRunAgainstBrowser()
    {
        var script = fixture.CreateScript("assertion-actions.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
        fill "#name" "CMG"
        focus "#name"
        expectVisible "#visible-target"
        expectHidden "#hidden-target"
        expectEnabled "#primary"
        expectDisabled "#disabled-button"
        expectAttached "#primary"
        expectDetached "#not-present" timeout=50
        expectEditable "#name"
        expectNotEditable "#primary"
        expectEmpty "#empty-target"
        expectNotEmpty "#title"
        expectFocused "#name"
        expectNotFocused "#primary"
        scrollIntoView "#visible-target"
        expectInViewport "#visible-target"
        expectNotInViewport "#deep-button"
        evaluate "const s=document.querySelector('#multi'); for (const o of s.options) o.selected=['alpha','beta'].includes(o.value); s.dispatchEvent(new Event('change',{bubbles:true})); true"
        expectValue "#name" "CMG"
        expectValues "#multi" "alpha" "beta"
        expectAttribute "#primary" "data-state" "idle"
        expectClass "#class-target" "beta"
        expectId "#primary" "primary"
        expectCss "#css-target" "color" "rgb(10, 20, 30)"
        expectProperty "#primary" "dataset.state" "idle"
        expectAccessibleName "#visible-target" "Visible target"
        expectRole "#visible-target" "button"
        expectUnchecked "#agree"
        check "#agree"
        expectChecked "#agree"
        expectCount ".item" "3"
        containsText "#title" "cmg" ignoreCase=true
        expectNoText "#status" "missing" timeout=50
        expectEval "document.title" equals="CMG E2E Fixture"
        assertExpression "document.body.innerText" contains="Primary action"
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("EXPECT ");
        result.StdoutContains("EXPECT_EVAL ");
    }

    [Fact]
    public void DirectScript_AssertionFailureReportsReason()
    {
        var script = fixture.CreateScript("assertion-action-failure.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
        expectRole "#visible-target" "link"
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldFail();
        result.StderrContains("expectRole failed");
        result.StderrContains("Expected role to be link");
    }
}
