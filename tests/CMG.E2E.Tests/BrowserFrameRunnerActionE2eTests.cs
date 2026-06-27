using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserFrameRunnerActionE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserFrameRunnerActionE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RunCommand_FrameActionsRunInsideTests()
    {
        var traceDir = fixture.OutputPath("runner-frame-traces");
        var script = fixture.CreateScript("runner-frame-actions.cmgscript", $$"""
            test "runner frame actions" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              frameWaitForElement "#fixture-frame" "#frame-button" timeout=1000
              frameClick "#fixture-frame" "#frame-button"
              frameAssertText "#fixture-frame" "#frame-status" "frame clicked"
              frameFill "#fixture-frame" "#frame-input" "filled"
              frameInputValue "#fixture-frame" "#frame-input"
              frameType "#fixture-frame" "#frame-input" " plus"
              frameTextContent "#fixture-frame" "#frame-title"
              frameInnerText "#fixture-frame" "#frame-title"
              frameGetAttribute "#fixture-frame" "#frame-link" "data-state"
              frameComputedStyle "#fixture-frame" "#frame-card" "display"
              frameProperty "#fixture-frame" "#frame-link" "href"
              frameCount "#fixture-frame" ".frame-item"
              frameLocatorCount "#fixture-frame" ".frame-item"
              frameBoundingBox "#fixture-frame" "#frame-card"
              frameAllTextContents "#fixture-frame" ".frame-item"
              frameAllInnerTexts "#fixture-frame" ".frame-item"
              frameEvaluate "#fixture-frame" "document.querySelector('#frame-status').textContent"
              frameHover "#fixture-frame" "#frame-card"
              frameToContainText "#fixture-frame" "#frame-title" "Ready"
            }
            """);

        var result = fixture.Cli.Run("run", script, "--trace", traceDir);

        result.ShouldPass();
        result.StdoutContains("TEST PASS runner frame actions");
        var trace = File.ReadAllText(Directory.EnumerateFiles(traceDir, "*.trace.json").Single());
        AssertTraceContains(trace, "FRAME ");
        AssertTraceContains(trace, "FRAME_VALUE");
        AssertTraceContains(trace, "FRAME_TEXT");
        AssertTraceContains(trace, "FRAME_ATTRIBUTE");
        AssertTraceContains(trace, "FRAME_STYLE");
        AssertTraceContains(trace, "FRAME_PROPERTY");
        AssertTraceContains(trace, "FRAME_COUNT");
        AssertTraceContains(trace, "FRAME_BOUNDING_BOX");
        AssertTraceContains(trace, "FRAME_TEXTS");
        AssertTraceContains(trace, "FRAME_EVALUATE");
    }

    [Fact]
    public void RunCommand_FrameFailureReportsActualStep()
    {
        var script = fixture.CreateScript("runner-frame-failure.cmgscript", $$"""
            test "runner frame failure" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              frameTextContent "#fixture-frame" "#missing-frame-child"
              caption "should not run"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=frameTextContent");
        result.StderrContains("#missing-frame-child");
    }

    [Fact]
    public void RunCommand_FrameWaitFailureReportsActualStep()
    {
        var script = fixture.CreateScript("runner-frame-wait-failure.cmgscript", $$"""
            test "runner frame wait failure" {
              navigate "{{fixture.FixtureHttpUri("index.html")}}" waitUntil=domcontentloaded
              frameWaitForElement "#fixture-frame" "#missing-frame-child" timeout=50
              caption "should not run"
            }
            """);

        var result = fixture.Cli.Run("run", script);

        result.ShouldFail();
        result.StderrContains("STEP FAIL");
        result.StderrContains("action=frameWaitForElement");
        result.StderrContains("#missing-frame-child");
    }

    private static void AssertTraceContains(string trace, string expected) =>
        Assert.Contains(expected, trace, StringComparison.Ordinal);
}
