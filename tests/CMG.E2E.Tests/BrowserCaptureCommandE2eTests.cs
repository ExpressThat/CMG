using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class BrowserCaptureCommandE2eTests : IClassFixture<CmgBrowserFixture>
{
    private readonly CmgBrowserFixture fixture;

    public BrowserCaptureCommandE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void CaptureCommands_WriteImagePdfAndVisualAssertionArtifacts()
    {
        Navigate();
        var elementShot = fixture.OutputPath("element.jpeg");
        var directElementShot = fixture.OutputPath("get-element.png");
        var pageShot = fixture.OutputPath("page-clipped.jpeg");
        var baseline = fixture.OutputPath("baseline.png");
        var actual = fixture.OutputPath("actual.png");
        var pageBaseline = fixture.OutputPath("page-baseline.png");
        var pageActual = fixture.OutputPath("page-actual.png");
        var pdf = fixture.OutputPath("advanced.pdf");
        var style = fixture.OutputPath("hide-secret.css");
        File.WriteAllText(style, "#masked-secret{visibility:hidden!important}");

        Run("browser", "control", "capture", "screenshot", "#visible-target", "--output", elementShot, "--type", "jpeg", "--quality", "80");
        Run("browser", "control", "capture", "getElement", "#visible-target", "--screenshot", "--output", directElementShot);
        Run("browser", "control", "capture", "screenshot", "#visible-target", "--output", baseline, "--mask", "#masked-secret", "--mask-color", "#000000");
        Run("browser", "control", "capture", "toHaveScreenshot", "#visible-target", "--baseline", baseline, "--output", actual, "--tolerance", "0");
        Run("browser", "control", "capture", "screenshotPage", "--output", pageShot, "--type", "jpg", "--quality", "75", "--clip-x", "0", "--clip-y", "0", "--clip-width", "320", "--clip-height", "240", "--style-path", style, "--animations", "disabled", "--caret", "hide");
        Run("browser", "control", "capture", "screenshotPage", "--output", pageBaseline, "--full-page", "--mask", "#masked-secret", "--mask-color", "#000000");
        Run("browser", "control", "capture", "expectScreenshot", "--baseline", pageBaseline, "--output", pageActual, "--full-page", "--mask", "#masked-secret", "--mask-color", "#000000", "--tolerance", "0.05");
        Run("browser", "control", "capture", "printPdf", "--path", pdf, "--format", "A4", "--margin-top", "10mm", "--margin-bottom", "10mm", "--page-ranges", "1", "--prefer-css-page-size");

        CmgE2eAssert.FileExists(elementShot);
        CmgE2eAssert.FileExists(directElementShot);
        CmgE2eAssert.FileExists(pageShot);
        CmgE2eAssert.FileExists(actual);
        CmgE2eAssert.FileExists(pageActual);
        CmgE2eAssert.FileExists(pdf);
    }

    [Fact]
    public void CaptureFailure_ReturnsScreenshotValidationReason()
    {
        Navigate();

        var result = fixture.Cli.Run("browser", "control", "capture", "screenshotPage", "--type", "png", "--quality", "80");

        result.ShouldFail();
        result.StderrContains("quality= is only valid when type=jpeg");
    }

    private void Navigate()
    {
        Run("browser", "control", "navigation", "navigate", fixture.FixtureHttpUri("index.html"), "--wait-until", "domcontentloaded");
    }

    private CmgResult Run(params string[] args)
    {
        var result = fixture.Cli.Run(args);
        result.ShouldPass();
        return result;
    }
}
