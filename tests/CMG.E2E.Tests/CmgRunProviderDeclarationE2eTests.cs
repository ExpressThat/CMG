using System.Text.Json;
using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class CmgRunProviderDeclarationE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public CmgRunProviderDeclarationE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_ProviderOnlyRunsFocusedTestAndWritesAnnotations()
    {
        var script = fixture.CreateScript("provider-only-annotations.cmgscript", $$"""
        describe "annotated area" owner=qa annotation.requirement="REQ-42" {
          test.only "focused provider" issue="BUG-7" link="https://example.test/BUG-7" note="release evidence" {
            navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
            expectText "#title" "CMG E2E Fixture"
          }

          test "unfocused failure" {
            fail "should not run"
          }
        }
        """);
        var json = fixture.OutputPath("provider-only.json");
        var html = fixture.OutputPath("provider-only.html");
        var junit = fixture.OutputPath("provider-only.xml");

        var result = fixture.Cli.Run("run", script, "--report-json", json, "--report-html", html, "--report-junit", junit);

        result.ShouldPass();
        result.StdoutContains("TEST PASS annotated area / focused provider");
        Assert.DoesNotContain("unfocused failure", result.Stdout, StringComparison.Ordinal);
        AssertReportStatus(json, "annotated area / focused provider", "passed");
        AssertReportAnnotation(json, "annotated area / focused provider", "owner", "qa");
        AssertReportAnnotation(json, "annotated area / focused provider", "requirement", "REQ-42");
        AssertReportAnnotation(json, "annotated area / focused provider", "issue", "BUG-7");
        Assert.Contains("release evidence", File.ReadAllText(html), StringComparison.Ordinal);
        Assert.Contains("cmg.annotation.requirement", File.ReadAllText(junit), StringComparison.Ordinal);
    }

    [Fact]
    public void RunCommand_ProviderSkipFixmeTodoAndSuiteSkipReportSkipped()
    {
        var script = fixture.CreateScript("provider-skips.cmgscript", """
        test.fixme "known bug" {
          fail "fixme should not run"
        }

        it.todo "queued work" {
          fail "todo should not run"
        }

        describe.skip "legacy suite" {
          it "inherits skip" skip=false {
            fail "suite skip should win"
          }
        }

        test "active provider declarations" {
          caption "active ran"
        }
        """);
        var json = fixture.OutputPath("provider-skips.json");

        var result = fixture.Cli.Run("run", script, "--report-json", json);

        result.ShouldPass();
        result.StdoutContains("TEST SKIP known bug");
        result.StdoutContains("TEST SKIP queued work");
        result.StdoutContains("TEST SKIP legacy suite / inherits skip");
        result.StdoutContains("TEST PASS active provider declarations");
        Assert.DoesNotContain("should not run", result.Stdout, StringComparison.Ordinal);
        AssertReportStatus(json, "known bug", "skipped");
        AssertReportStatus(json, "queued work", "skipped");
        AssertReportStatus(json, "legacy suite / inherits skip", "skipped");
        AssertReportStatus(json, "active provider declarations", "passed");
    }

    private static void AssertReportStatus(string path, string name, string status)
    {
        var test = ReportTest(path, name);
        Assert.Equal(status, test.GetProperty("status").GetString());
    }

    private static void AssertReportAnnotation(string path, string name, string type, string description)
    {
        var test = ReportTest(path, name);
        Assert.Contains(test.GetProperty("annotations").EnumerateArray(), annotation =>
            annotation.GetProperty("type").GetString() == type &&
            annotation.GetProperty("description").GetString() == description);
    }

    private static JsonElement ReportTest(string path, string name)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        return document.RootElement.EnumerateArray()
            .First(item => item.GetProperty("name").GetString() == name)
            .Clone();
    }
}
