using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserDownloadFailureE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserDownloadFailureE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void WaitForDownload_ReportsClearFailureWhenNoFileAppears()
    {
        var result = fixture.Cli.Run(
            "browser",
            "control",
            "events",
            "waitForDownload",
            "--directory",
            fixture.OutputDirectory,
            "--pattern",
            "missing-download.txt",
            "--timeout",
            "10");

        result.ShouldFail();
        result.StderrContains("No download matching 'missing-download.txt'");
    }
}
