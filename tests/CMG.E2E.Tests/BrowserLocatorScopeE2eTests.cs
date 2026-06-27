using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserLocatorScopeE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserLocatorScopeE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_UsesRichLocatorsShadowDomAndWithinScope()
    {
        var script = fixture.CreateScript("locator-scope.cmgscript", LocatorScenario());

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("EXPECT_EVAL");
        result.StdoutContains("Second");
    }

    [Fact]
    public void RunCommand_UsesRichLocatorsShadowDomAndWithinScope()
    {
        var script = fixture.CreateScript("locator-scope-runner.cmgscript", $$"""
        test "locator scope parity" {
        {{Indent(LocatorScenario())}}
        }
        """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldPass();
        result.StdoutContains("TEST PASS locator scope parity");
    }

    private static string LocatorScenario() =>
        """
        setContent "<main><section class='scope'><button class='cmd primary selected' data-kind='first'>First</button><button class='cmd secondary' data-kind='second'>Second <span class='badge'>New</span></button></section><button class='cmd outside' data-kind='outside'>Outside</button><button class='cmd hidden' data-kind='hidden' style='display:none'>Hidden</button><div id='shadowHost'></div><output id='result'>none</output></main>"
        evaluate "(() => { window.__scopedItems = []; document.querySelectorAll('.cmd').forEach(button => button.addEventListener('click', () => document.querySelector('#result').textContent = button.dataset.kind)); const root = document.querySelector('#shadowHost').attachShadow({mode:'open'}); root.innerHTML = '<button class=\"shadow-save\">Shadow Save</button><button class=\"shadow-text\">Shadow Text Action</button>'; root.querySelector('.shadow-save').addEventListener('click', () => document.querySelector('#result').textContent = 'shadow'); root.querySelector('.shadow-text').addEventListener('click', () => document.querySelector('#result').textContent = 'shadowText'); return true; })()"
        click "inside=.scope|.primary"
        expectText "#result" "first"
        click "nth=.cmd|1"
        expectText "#result" "second"
        click "has=.cmd|.badge"
        expectText "#result" "second"
        click "hasNot=.cmd|.badge"
        expectText "#result" "first"
        click "hasText=.cmd|Second"
        expectText "#result" "second"
        click "hasNotText=.cmd|Second"
        expectText "#result" "first"
        click "or=.missing|.outside"
        expectText "#result" "outside"
        click "and=.cmd|.selected"
        expectText "#result" "first"
        click "closest=.badge|.cmd"
        expectText "#result" "second"
        click "parent=.badge|.cmd"
        expectText "#result" "second"
        click "next=.primary|.secondary"
        expectText "#result" "second"
        click "previous=.hidden|.outside"
        expectText "#result" "outside"
        click "shadow=#shadowHost|button.shadow-save"
        expectText "#result" "shadow"
        click "shadowText=#shadowHost|Shadow Text Action"
        expectText "#result" "shadowText"
        within ".scope" {
          click ".primary"
          foreachSelector row ".cmd" {
            set itemText { textContent "${row}" }
            evaluate "window.__scopedItems = window.__scopedItems || []; window.__scopedItems.push('${index}:${itemText}')"
          }
          contains "Second"
        }
        expectText "#result" "first"
        expectEval "window.__scopedItems.join('|')" equals="0:First|1:Second New"
        """;

    private static string Indent(string script) =>
        string.Join(Environment.NewLine, script.Split(Environment.NewLine).Select(line => "  " + line));
}
