using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserContextPolicyActionE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserContextPolicyActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_ContextPolicyActionsAffectPageBehavior()
    {
        var script = fixture.CreateScript("context-policy-actions.cmgscript", $$"""
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            setJavaScriptEnabled false
            try {
              addScriptTag "window.__blockedTag = true;"
              fail "dynamic script should have been blocked"
            } catch error {
              expect ("${error}" contains "JavaScript blocking")
            }
            javaScriptEnabled true
            addScriptTag "window.__allowedTag = true;"
            expectEval "window.__allowedTag" equals="True"
            setContent "<meta http-equiv='Content-Security-Policy' content=\"script-src 'none'\"><main id='policy'>policy</main>"
            bypassCSP true
            expectEval "document.querySelector('meta[http-equiv=\"Content-Security-Policy\" i]') === null" equals="True"
            serviceWorkers block
            set sw { evaluate "navigator.serviceWorker.register('/worker-fixture.js').then(() => 'registered').catch(error => error.message)" }
            expect ("${sw}" contains "service worker blocking")
            localStorage set "policy-key" "value"
            sessionStorage set "policy-session" "value"
            clearContext
            set localValue { evaluate "localStorage.getItem('policy-key') ?? ''" }
            set sessionValue { evaluate "sessionStorage.getItem('policy-session') ?? ''" }
            expect ("${localValue}" == "")
            expect ("${sessionValue}" == "")
            resetContext
            expectEval "location.href" equals="about:blank"
            """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("JAVASCRIPT_ENABLED 002 false");
        result.StdoutContains("JAVASCRIPT_ENABLED");
        result.StdoutContains("true");
        result.StdoutContains("CSP_BYPASS");
        result.StdoutContains("SERVICE_WORKERS");
        result.StdoutContains("CONTEXT_CLEARED");
        result.StdoutContains("CONTEXT_RESET");
    }
}
