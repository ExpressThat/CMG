using System.Text.Json;
using System.Xml.Linq;
using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class CmgRunE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public CmgRunE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_WritesReportsTracesAndOptionalGifs()
    {
        var testScript = fixture.CreateScript("runner-flow.cmgscript", $$"""
            suite "fixture suite" tag=smoke owner=e2e {
              beforeEach {
                navigate "{{E2ePaths.FixtureUri("index.html")}}"
                setViewport 1280 1400
              }

              test "clicks fixture button" {
                scrollIntoView "#primary"
                click "#primary"
                expectText "#status" "clicked"
              }

              test "uses variables and macros" {
                macro writeName value {
                  fill "#name" "${value}"
                  return { inputValue "#name" }
                }
                set result { call writeName ${userName} }
                if (${result} == "Ada") {
                  expectValue "#name" "Ada"
                } else {
                  fail "unexpected name"
                }
              }
            }
            """);
        var json = fixture.OutputPath("run.json");
        var html = fixture.OutputPath("run.html");
        var junit = fixture.OutputPath("run.xml");
        var traceDir = fixture.OutputPath("traces");
        var gifDir = fixture.OutputPath("gifs");

        var result = fixture.Cli.Run(
            "run", testScript,
            "--tag", "smoke",
            "--var", "userName=Ada",
            "--report-json", json,
            "--report-html", html,
            "--report-junit", junit,
            "--trace", traceDir,
            "--gif", gifDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS fixture suite / clicks fixture button");
        result.StdoutContains("TEST PASS fixture suite / uses variables and macros");
        CmgE2eAssert.FileExists(json);
        CmgE2eAssert.FileExists(html);
        CmgE2eAssert.FileExists(junit);
        CmgE2eAssert.DirectoryHasFiles(traceDir, "*.json");
        CmgE2eAssert.DirectoryHasFiles(gifDir, "*.gif");
        AssertJsonReport(json);
        AssertHtmlReport(html);
        AssertJunitReport(junit);
        AssertTraceFiles(traceDir);
    }

    [Fact]
    public void RunCommand_ListDoesNotStartBrowserActions()
    {
        var testScript = fixture.CreateScript("runner-list.cmgscript", """
            test "listed only" tag=list {
              fail "should not run"
            }
            """);

        var result = fixture.Cli.Run("run", testScript, "--tag", "list", "--list");

        result.ShouldPass();
        result.StdoutContains("TEST LIST run listed only");
    }

    [Fact]
    public void RunCommand_BaseUrlAndEnvResolveRelativeNavigationAndVariables()
    {
        var testScript = fixture.CreateScript("runner-base-url-env.cmgscript", """
            test "uses cli base url and env" {
              navigate "index.html" waitUntil=domcontentloaded
              expectText "#title" "${expectedTitle}"
            }
            """);

        var result = fixture.Cli.Run(
            "run",
            testScript,
            "--base-url",
            fixture.FixtureHttpPath("/"),
            "--env",
            "expectedTitle=CMG E2E Fixture");

        result.ShouldPass();
        result.StdoutContains("TEST PASS uses cli base url and env");
    }

    private static void AssertJsonReport(string path)
    {
        using var document = JsonDocument.Parse(File.ReadAllText(path));
        var tests = document.RootElement.EnumerateArray().ToArray();
        Assert.Contains(tests, test => HasStatus(test, "fixture suite / clicks fixture button", "passed"));
        Assert.Contains(tests, test => HasStatus(test, "fixture suite / uses variables and macros", "passed"));
        Assert.All(tests, test =>
        {
            Assert.True(test.GetProperty("success").GetBoolean());
            Assert.False(string.IsNullOrWhiteSpace(test.GetProperty("gifPath").GetString()));
            Assert.NotEmpty(test.GetProperty("steps").EnumerateArray());
        });
        Assert.Contains(tests, test => OutputContains(test, "PASS 008 assertText #status clicked"));
        Assert.Contains(tests, test => StepOutputContains(test, "VALUE 008 Ada"));
    }

    private static void AssertHtmlReport(string path)
    {
        var html = File.ReadAllText(path);
        Assert.Contains("fixture suite / clicks fixture button - pass", html, StringComparison.Ordinal);
        Assert.Contains("fixture suite / uses variables and macros - pass", html, StringComparison.Ordinal);
        Assert.Contains("GIF:", html, StringComparison.Ordinal);
    }

    private static void AssertJunitReport(string path)
    {
        var cases = XDocument.Load(path).Descendants("testcase").ToArray();
        Assert.Contains(cases, test => test.Attribute("name")?.Value == "fixture suite / clicks fixture button");
        Assert.Contains(cases, test => test.Attribute("name")?.Value == "fixture suite / uses variables and macros");
        Assert.DoesNotContain(cases, test => test.Element("failure") is not null || test.Element("skipped") is not null);
    }

    private static void AssertTraceFiles(string directory)
    {
        var traces = Directory.EnumerateFiles(directory, "*.trace.json").Select(File.ReadAllText).ToArray();
        Assert.Equal(2, traces.Length);
        Assert.Contains(traces, trace => trace.Contains("\"name\": \"fixture suite / clicks fixture button\"", StringComparison.Ordinal));
        Assert.Contains(traces, trace => trace.Contains("\"name\": \"fixture suite / uses variables and macros\"", StringComparison.Ordinal));
        Assert.Contains(traces, trace => trace.Contains("VALUE 008 Ada", StringComparison.Ordinal));
    }

    private static bool HasStatus(JsonElement test, string name, string status) =>
        test.GetProperty("name").GetString() == name &&
        test.GetProperty("status").GetString() == status;

    private static bool OutputContains(JsonElement test, string expected) =>
        test.GetProperty("output").EnumerateArray().Any(line => line.GetString()?.Contains(expected, StringComparison.Ordinal) is true);

    private static bool StepOutputContains(JsonElement test, string expected) =>
        test.GetProperty("steps").EnumerateArray().Any(step => OutputContains(step, expected));
}
