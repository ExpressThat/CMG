using System.Text.RegularExpressions;
using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed partial class BrowserContextCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserContextCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void BrowserContextCommands_CreateUseListAndCloseRealContext()
    {
        Navigate();
        Run("browser", "control", "storage", "local", "set", "context-key", "default");
        var created = Run(
            "browser",
            "control",
            "context",
            "browserContexts",
            "new",
            "--url",
            fixture.FixtureHttpUri("index.html"));
        var contextId = ContextCreatedRegex().Match(created.Stdout).Groups["id"].Value;
        Assert.False(string.IsNullOrWhiteSpace(contextId), created.Stdout);

        var listed = Run("browser", "control", "context", "browserContexts", "list");
        listed.StdoutContains(contextId);
        listed.StdoutContains("active=true");
        var emptyContextValue = Run("browser", "control", "storage", "local", "get", "context-key");
        emptyContextValue.StdoutContains("LOCAL_STORAGE");
        Assert.DoesNotContain("default", emptyContextValue.Stdout, StringComparison.Ordinal);
        Run("browser", "control", "storage", "local", "set", "context-key", "isolated");
        Run("browser", "control", "context", "browserContexts", "use", contextId);
        Run("browser", "control", "storage", "local", "get", "context-key").StdoutContains("isolated");
        Run("browser", "control", "context", "browserContexts", "close", contextId);

        var missing = fixture.Cli.Run("browser", "control", "context", "browserContexts", "use", contextId);
        missing.ShouldFail();
        Assert.Contains(contextId, missing.Stderr + missing.Stdout, StringComparison.Ordinal);
    }

    [Fact]
    public void ScriptAction_NewContextStoresVariableAndKeepsStorageIsolated()
    {
        var script = fixture.CreateScript("context-flow.cmgscript", $$"""
        navigate "{{fixture.FixtureHttpUri("index.html")}}"
        localStorage set "context-key" "default"
        newContext ctx url="{{fixture.FixtureHttpUri("index.html")}}"
        listContexts
        localStorage get "context-key"
        set isolatedValue { localStorage get "context-key" }
        if ("${isolatedValue}" == "") {
          localStorage set "context-key" "isolated"
        } else {
          fail "new context leaked local storage"
        }
        useContext "${ctx}"
        expectEval "localStorage.getItem('context-key')" equals="isolated"
        closeContext "${ctx}"
        """);

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        result.StdoutContains("CONTEXT_CREATED");
        result.StdoutContains("CONTEXT 0");
        result.StdoutContains("CONTEXT_ACTIVE");
        result.StdoutContains("CONTEXT_CLOSED");
    }

    [Fact]
    public void RunCommand_BrowserContextActionsRunInsideTests()
    {
        var traceDir = fixture.OutputPath("runner-context-traces");
        var script = fixture.CreateScript("runner-context-flow.cmgscript", $$"""
        test "runner browser context flow" {
          navigate "{{fixture.FixtureHttpUri("index.html")}}"
          localStorage set "runner-context-key" "default"
          newContext ctx url="{{fixture.FixtureHttpUri("index.html")}}"
          listContexts
          set isolatedValue { localStorage get "runner-context-key" }
          if ("${isolatedValue}" == "") {
            localStorage set "runner-context-key" "isolated"
          } else {
            fail "runner new context leaked local storage"
          }
          useContext "${ctx}"
          expectEval "localStorage.getItem('runner-context-key')" equals="isolated"
          closeContext "${ctx}"
        }
        """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner browser context flow");
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "CONTEXT_CREATED");
        AssertTraceContains(trace, "listContexts");
        AssertTraceContains(trace, "CONTEXT_ACTIVE");
        AssertTraceContains(trace, "CONTEXT_CLOSED");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);

    [GeneratedRegex("CONTEXT_CREATED\\s+\\d+\\s+id=(?<id>\\S+)")]
    private static partial Regex ContextCreatedRegex();
}
