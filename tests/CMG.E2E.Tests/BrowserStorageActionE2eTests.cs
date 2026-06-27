using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserStorageActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserStorageActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DirectScript_StorageActionsMutateAndRestoreBrowserState()
    {
        var state = fixture.OutputPath("script-storage-state.json");
        var script = fixture.CreateScript("storage-actions.cmgscript", Script("direct", state));

        var result = fixture.Cli.Run("browser", "control", "script", "--file", script);

        result.ShouldPass();
        AssertOutput(result.Stdout);
        CmgE2eAssert.FileExists(state);
    }

    [Fact]
    public void RunCommand_StorageActionsRunInsideTests()
    {
        var state = fixture.OutputPath("runner-storage-state.json");
        var traceDir = fixture.OutputPath("runner-storage-traces");
        var script = fixture.CreateScript("runner-storage-actions.cmgscript", $$"""
            test "runner storage actions" {
            {{Indent(Script("runner", state))}}
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner storage actions");
        CmgE2eAssert.FileExists(state);
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.trace.json");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertOutput(trace);
    }

    [Fact]
    public void RunCommand_StorageActionFailureReportsStepReason()
    {
        var script = fixture.CreateScript("runner-storage-failure.cmgscript", """
            test "runner storage failure" {
              cookie set "mode" "demo" sameSite=Maybe
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=cookie");
        result.StderrContains("cookie sameSite expects Strict, Lax, or None.");
    }

    private string Script(string prefix, string state) => $$"""
          navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
          localStorage set "{{prefix}}-local" "alpha"
          sessionStorage set "{{prefix}}-session" "bravo"
          cookie set "{{prefix}}-cookie" "charlie" path="/" sameSite=Lax maxAge=60
          storageState save path="{{ScriptPath(state)}}"
          localStorage remove "{{prefix}}-local"
          sessionStorage clear
          cookie remove "{{prefix}}-cookie" path="/"
          set removedLocal { localStorage get "{{prefix}}-local" }
          set removedSession { sessionStorage get "{{prefix}}-session" }
          set removedCookie { cookie get "{{prefix}}-cookie" }
          expect ("${removedLocal}" == "")
          expect ("${removedSession}" == "")
          expect ("${removedCookie}" == "")
          storageState load path="{{ScriptPath(state)}}"
          set restoredLocal { localStorage get "{{prefix}}-local" }
          set restoredSession { sessionStorage get "{{prefix}}-session" }
          set restoredCookie { cookie get "{{prefix}}-cookie" }
          expect ("${restoredLocal}" == "alpha")
          expect ("${restoredSession}" == "bravo")
          expect ("${restoredCookie}" == "charlie")
          localStorage clear
          cookie clear path="/"
        """;

    private static void AssertOutput(string output)
    {
        Assert.Contains("LOCAL_STORAGE", output, StringComparison.Ordinal);
        Assert.Contains("SESSION_STORAGE", output, StringComparison.Ordinal);
        Assert.Contains("COOKIE", output, StringComparison.Ordinal);
        Assert.Contains("STORAGE_STATE", output, StringComparison.Ordinal);
    }

    private static string Indent(string text) =>
        string.Join(Environment.NewLine, text.Split(Environment.NewLine).Select(line => "  " + line));

    private static string ScriptPath(string path) =>
        path.Replace("\\", "/", StringComparison.Ordinal);
}
