using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

public sealed class CommandHelpCoverageE2eTests : IClassFixture<CmgCliFixture>
{
    private readonly CmgCliFixture fixture;

    public CommandHelpCoverageE2eTests(CmgCliFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void RepresentativeCommands_HaveWorkingExternalHelp()
    {
        foreach (var command in RepresentativeCommands())
        {
            var result = fixture.Cli.RunWithTimeout(TimeSpan.FromSeconds(20), [.. command, "--help"]);
            Assert.True(
                result.ExitCode is 0 && result.Stdout.Contains("Usage:", StringComparison.Ordinal),
                $"{string.Join(' ', command)} => exit {result.ExitCode}\n{result.Stdout}\n{result.Stderr}");
        }

        Assert.False(
            Directory.Exists(Path.Combine(fixture.LocalAppData, "CMG")),
            "Help commands must not create browser state or require a launched browser.");
    }

    private static IEnumerable<string[]> RepresentativeCommands()
    {
        yield return ["browser"];
        yield return ["browser", "launch"];
        yield return ["browser", "control", "script"];
        yield return ["browser", "control", "input", "click"];
        yield return ["browser", "control", "network", "route"];
        yield return ["run"];
        yield return ["files", "read"];
        yield return ["api", "request"];
    }
}
