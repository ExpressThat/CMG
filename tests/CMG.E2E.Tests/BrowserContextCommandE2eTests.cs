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

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");

    [GeneratedRegex("CONTEXT_CREATED\\s+\\d+\\s+id=(?<id>\\S+)")]
    private static partial Regex ContextCreatedRegex();
}
