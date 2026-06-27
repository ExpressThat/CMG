using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class BrowserClockAccessibilityAliasE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public BrowserClockAccessibilityAliasE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void ClockRestoreAlias_RestoresNativePageTime()
    {
        Navigate();
        Run("browser", "control", "clock", "install", "--now", "1000").StdoutContains("CLOCK 001");
        Run("browser", "control", "clock", "tick", "250").StdoutContains("now=1250");
        Run("browser", "control", "page", "evaluate", "Date.now()").StdoutContains("1250");
        Run("browser", "control", "clock", "restoreClock").StdoutContains("CLOCK_RESTORED 001");

        var afterRestore = Run("browser", "control", "page", "evaluate", "Date.now() > 1250");
        afterRestore.StdoutContains("True");
    }

    [Fact]
    public void AccessibilityAliases_RunAgainstRealAccessibilityTree()
    {
        Navigate();
        var snapshot = fixture.OutputPath("accessibility-alias.json");

        Run("browser", "control", "accessibility", "accessibilitySnapshot", "#visible-target", "--output", snapshot)
            .StdoutContains(snapshot);
        CmgE2eAssert.FileExists(snapshot);
        Assert.Contains("Visible target", File.ReadAllText(snapshot), StringComparison.Ordinal);

        Run("browser", "control", "accessibility", "expectAccessible", "--role", "button", "--name", "Visible target")
            .StdoutContains("ACCESSIBLE 001 role=button");

        var missing = fixture.Cli.Run("browser", "control", "accessibility", "expectAccessible", "--role", "button", "--name", "Missing accessible name");
        missing.ShouldFail();
        missing.StderrContains("Missing accessible name");
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
