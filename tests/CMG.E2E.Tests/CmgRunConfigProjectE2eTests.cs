using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class CmgRunConfigProjectE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public CmgRunConfigProjectE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_ExecutesConfigProjectRepeatAndShard()
    {
        var startUrl = fixture.FixtureHttpUri("index.html");
        var script = fixture.CreateScript("run-config-project.cmgscript", """
        test "config alpha" tag=matrix {
          navigate "${startUrl}" waitUntil=domcontentloaded
          expectText "#title" "CMG E2E Fixture"
          expect ("${mode}" == "cli")
          expect ("${tenant}" == "project")
        }

        test "config beta" tag=matrix {
          navigate "${startUrl}" waitUntil=domcontentloaded
          expectText "#status" "ready"
          expect ("${global}" == "yes")
        }
        """);
        var config = fixture.CreateScript("cmg.run.json", $$"""
        {
          "reportJson": "artifacts/run-config-report.json",
          "trace": "artifacts/traces",
          "repeatEach": 2,
          "shard": "1/2",
          "variables": { "mode": "config", "global": "yes" },
          "projects": [
            {
              "name": "chrome-matrix",
              "browser": "chrome",
              "tag": "matrix",
              "variables": { "tenant": "project", "mode": "project", "startUrl": "{{startUrl}}" }
            }
          ]
        }
        """);
        var configDirectory = Path.GetDirectoryName(config)!;
        var report = Path.Combine(configDirectory, "artifacts", "run-config-report.json");
        var traces = Path.Combine(configDirectory, "artifacts", "traces");

        var result = fixture.Cli.Run("run", script, "--config", config, "--project", "chrome-matrix", "--var", "mode=cli");

        result.ShouldPass();
        result.StdoutContains("TEST PASS [chrome-matrix] config alpha [repeat 1/2]");
        result.StdoutContains("TEST PASS [chrome-matrix] config beta [repeat 1/2]");
        Assert.DoesNotContain("[repeat 2/2]", result.Stdout, StringComparison.Ordinal);
        CmgE2eAssert.FileExists(report);
        CmgE2eAssert.DirectoryHasFiles(traces, "*.trace.json");
        var json = File.ReadAllText(report);
        Assert.Contains("\"project\": \"chrome-matrix\"", json, StringComparison.Ordinal);
        Assert.Contains("config alpha [repeat 1/2]", json, StringComparison.Ordinal);
        Assert.DoesNotContain("config alpha [repeat 2/2]", json, StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_InvalidConfigProjectFailsBeforeRunning()
    {
        var script = fixture.CreateScript("missing-project.cmgscript", """
        test "should not run" {
          fail "project should fail first"
        }
        """);
        var config = fixture.CreateScript("missing-project.run.json", """{ "projects": [] }""");

        var result = fixture.Cli.Run("run", script, "--config", config, "--project", "missing");

        result.ShouldFail();
        result.StderrContains("Run config project 'missing' was not found.");
        Assert.DoesNotContain("TEST FAIL should not run", result.Stdout, StringComparison.Ordinal);
    }
}
