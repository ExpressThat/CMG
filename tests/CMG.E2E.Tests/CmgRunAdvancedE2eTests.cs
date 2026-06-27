using System.Text.Json;
using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class CmgRunAdvancedE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public CmgRunAdvancedE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_ExecutesHooksParameterizedTestsAndSkips()
    {
        var script = fixture.CreateScript("runner-advanced-pass.cmgscript", $$"""
        beforeAll {
          navigate "{{fixture.FixtureHttpUri("index.html")}}"
          localStorage set "root-once" "ready"
        }

        suite "runner matrix" tag=advanced {
          beforeAll {
            localStorage set "suite-once" "ready"
          }
          beforeEach {
            navigate "{{fixture.FixtureHttpUri("index.html")}}"
            set rootValue { localStorage get "root-once" }
            expect ("${rootValue}" == "ready")
          }
          afterEach {
            caption "after each"
          }

          test.each "opens ${page}" as=page values="profile,checkout" {
            set suiteValue { localStorage get "suite-once" }
            expect ("${suiteValue}" == "ready")
            expectText "#title" "CMG E2E Fixture"
            caption "page ${page}"
          }

          test.skip "declared skip" reason="not relevant" {
            fail "should not run"
          }

          test "runtime skip" {
            skip "feature disabled"
            fail "should not run"
          }
        }
        """);
        var json = fixture.OutputPath("runner-advanced-pass.json");

        var result = fixture.Cli.Run("run", script, "--tag", "advanced", "--report-json", json);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner matrix / opens profile");
        result.StdoutContains("TEST PASS runner matrix / opens checkout");
        result.StdoutContains("TEST SKIP runner matrix / declared skip");
        result.StdoutContains("TEST SKIP runner matrix / runtime skip");
        AssertReportStatus(json, "runner matrix / declared skip", "skipped");
        AssertReportStatus(json, "runner matrix / runtime skip", "skipped");
    }

    [Fact]
    public void RunCommand_ReportsSoftFailureRetryOutputAndMaxFailureStop()
    {
        var script = fixture.CreateScript("runner-advanced-fail.cmgscript", $$"""
        test "retry recovers" tag=advanced {
          navigate "{{fixture.FixtureHttpUri("index.html")}}"
          retry max=2 delay=1 {
            set attempt { evaluate "window.__retryCount = (window.__retryCount || 0) + 1" }
            expect (${attempt} >= 2) message="retry not ready"
          }
          expectText "#title" "CMG E2E Fixture"
        }

        test "soft failure reports after diagnostics" tag=advanced {
          navigate "{{fixture.FixtureHttpUri("index.html")}}"
          softExpect (1 > 2) message="soft broke"
          expectText "#title" "CMG E2E Fixture"
        }

        test "not scheduled after max failure" tag=advanced {
          fail "max failure did not stop scheduling"
        }
        """);
        var json = fixture.OutputPath("runner-advanced-fail.json");

        var result = fixture.Cli.Run("run", script, "--tag", "advanced", "--max-failures", "1", "--report-json", json);

        result.ShouldFail();
        result.StdoutContains("TEST PASS retry recovers");
        result.StdoutContains("TEST FAIL soft failure reports after diagnostics");
        result.StdoutContains("RUN STOP maxFailures=1");
        Assert.DoesNotContain("not scheduled after max failure", result.Stdout, StringComparison.Ordinal);
        AssertReportStatus(json, "retry recovers", "passed");
        AssertReportStatus(json, "soft failure reports after diagnostics", "failed");
        AssertReportOutputContains(json, "retry recovers", "RETRY ");
        AssertReportOutputContains(json, "soft failure reports after diagnostics", "SOFT_EXPECT");
    }

    private static void AssertReportStatus(string path, string name, string status)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var test = document.RootElement.EnumerateArray().First(item => item.GetProperty("name").GetString() == name);
        Assert.Equal(status, test.GetProperty("status").GetString());
    }

    private static void AssertReportOutputContains(string path, string name, string expected)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var test = document.RootElement.EnumerateArray().First(item => item.GetProperty("name").GetString() == name);
        Assert.Contains(test.GetProperty("output").EnumerateArray(), item => item.GetString()?.Contains(expected, StringComparison.Ordinal) is true);
    }
}
