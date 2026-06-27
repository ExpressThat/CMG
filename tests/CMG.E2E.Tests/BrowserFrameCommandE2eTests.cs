using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserFrameCommandE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserFrameCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void FrameCommands_RunAgainstSameOriginIframe()
    {
        Navigate();

        Run("browser", "control", "frames", "waitForSelector", "#fixture-frame", "#frame-title", "--timeout", "5000");
        Run("browser", "control", "frames", "frameWaitForElement", "#fixture-frame", "#frame-input", "--timeout", "5000");
        Run("browser", "control", "frames", "hover", "#fixture-frame", "#frame-card");
        Run("browser", "control", "frames", "fill", "#fixture-frame", "#frame-input", "filled");
        Run("browser", "control", "frames", "inputValue", "#fixture-frame", "#frame-input")
            .StdoutContains("FRAME_VALUE 001 filled");
        Run("browser", "control", "frames", "type", "#fixture-frame", "#frame-input", " plus");
        Run("browser", "control", "frames", "frameInputValue", "#fixture-frame", "#frame-input")
            .StdoutContains("filled plus");
        Run("browser", "control", "frames", "click", "#fixture-frame", "#frame-button");
        Run("browser", "control", "frames", "frameAssertText", "#fixture-frame", "#frame-status", "frame clicked");
        Run("browser", "control", "frames", "frameToContainText", "#fixture-frame", "#frame-status", "clicked");
        Run("browser", "control", "frames", "evaluate", "#fixture-frame", "document.querySelector('#frame-status').textContent")
            .StdoutContains("frame clicked");
        Run("browser", "control", "frames", "textContent", "#fixture-frame", "#frame-title")
            .StdoutContains("Frame Ready");
        Run("browser", "control", "frames", "innerText", "#fixture-frame", "#frame-card")
            .StdoutContains("Frame card");
        Run("browser", "control", "frames", "getAttribute", "#fixture-frame", "#frame-link", "href")
            .StdoutContains("/frame-profile");
        Run("browser", "control", "frames", "computedStyle", "#fixture-frame", "#frame-card", "display")
            .StdoutContains("block");
        Run("browser", "control", "frames", "property", "#fixture-frame", "#frame-link", "dataset.state")
            .StdoutContains("ready");
        Run("browser", "control", "frames", "count", "#fixture-frame", ".frame-item")
            .StdoutContains("FRAME_COUNT 001 2");
        Run("browser", "control", "frames", "locatorCount", "#fixture-frame", ".frame-item")
            .StdoutContains("FRAME_COUNT 001 2");
        Run("browser", "control", "frames", "boundingBox", "#fixture-frame", "#frame-card")
            .StdoutContains("FRAME_BOUNDING_BOX 001");
        Run("browser", "control", "frames", "allTextContents", "#fixture-frame", ".frame-item")
            .StdoutContains("First frame row");
        Run("browser", "control", "frames", "frameAllInnerTexts", "#fixture-frame", ".frame-item")
            .StdoutContains("Second frame row");
    }

    [Fact]
    public void FrameCommandFailure_ReportsMissingSelector()
    {
        Navigate();
        var missing = fixture.Cli.Run("browser", "control", "frames", "textContent", "#fixture-frame", "#missing-frame-child");

        missing.ShouldFail();
        missing.StderrContains("#missing-frame-child");
    }

    [Fact]
    public void FrameWaitFailure_ReportsMissingSelector()
    {
        Navigate();
        var missing = fixture.Cli.Run(
            "browser",
            "control",
            "frames",
            "frameWaitForElement",
            "#fixture-frame",
            "#missing-frame-child",
            "--timeout",
            "50");

        missing.ShouldFail();
        missing.StderrContains("#missing-frame-child");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }

    private void Navigate() =>
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
}
