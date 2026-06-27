using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserContextPolicyActionE2eTests : IClassFixture<CmgBrowserFixture>
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

    [Fact]
    public void RunCommand_ContextPolicyActionsRunInsideTests()
    {
        var traceDir = fixture.OutputPath("runner-context-policy-traces");
        var script = fixture.CreateScript("runner-context-policy-actions.cmgscript", $$"""
            test "runner context policy actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              setJavaScriptEnabled false
              try {
                addScriptTag "window.__runnerBlockedTag = true;"
                fail "dynamic script should have been blocked"
              } catch error {
                expect ("${error}" contains "JavaScript blocking")
              }
              javaScriptEnabled true
              addScriptTag "window.__runnerAllowedTag = true;"
              expectEval "window.__runnerAllowedTag" equals="True"
              setContent "<meta http-equiv='Content-Security-Policy' content=\"script-src 'none'\"><main id='policy'>policy</main>"
              bypassCSP true
              expectEval "document.querySelector('meta[http-equiv=\"Content-Security-Policy\" i]') === null" equals="True"
              serviceWorkers block
              set sw { evaluate "navigator.serviceWorker.register('/worker-fixture.js').then(() => 'registered').catch(error => error.message)" }
              expect ("${sw}" contains "service worker blocking")
              localStorage set "runner-policy-key" "value"
              sessionStorage set "runner-policy-session" "value"
              clearContext
              set localValue { evaluate "localStorage.getItem('runner-policy-key') ?? ''" }
              set sessionValue { evaluate "sessionStorage.getItem('runner-policy-session') ?? ''" }
              expect ("${localValue}" == "")
              expect ("${sessionValue}" == "")
              resetContext
              expectEval "location.href" equals="about:blank"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner context policy actions");
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "JAVASCRIPT_ENABLED");
        AssertTraceContains(trace, "CSP_BYPASS");
        AssertTraceContains(trace, "SERVICE_WORKERS");
        AssertTraceContains(trace, "CONTEXT_CLEARED");
        AssertTraceContains(trace, "CONTEXT_RESET");
    }

    [Fact]
    public void RunCommand_ContextPolicyFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-context-policy-failure.cmgscript", """
            test "runner context policy failure" {
              serviceWorkers maybe
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=serviceWorkers");
        result.StderrContains("serviceWorkers expects allow or block.");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
