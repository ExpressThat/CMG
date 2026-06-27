using CMG.E2E.Tests.Support;
using System.Text.RegularExpressions;

namespace CMG.E2E.Tests;

public sealed class CommandHelpCoverageE2eTests : IClassFixture<CmgCliFixture>
{
    private static readonly Regex CommandHeadingPattern = new("^# `(?<command>[^`]+)`$", RegexOptions.Compiled);
    private readonly CmgCliFixture fixture;

    public CommandHelpCoverageE2eTests(CmgCliFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void DocumentedCommands_HaveWorkingExternalHelp()
    {
        var failures = new List<string>();
        foreach (var command in DocumentedCommands())
        {
            var result = fixture.Cli.RunWithTimeout(TimeSpan.FromSeconds(20), [.. command, "--help"]);
            if (result.ExitCode is not 0 || !result.Stdout.Contains("Usage:", StringComparison.Ordinal))
            {
                failures.Add($"{string.Join(' ', command)} => exit {result.ExitCode}\n{result.Stdout}\n{result.Stderr}");
            }
        }

        Assert.Empty(failures);

        Assert.False(
            Directory.Exists(Path.Combine(fixture.LocalAppData, "CMG")),
            "Help commands must not create browser state or require a launched browser.");
    }

    private static IEnumerable<string[]> DocumentedCommands()
    {
        var commandDocs = Path.Combine(E2ePaths.RepositoryRoot(), "docs", "commands");
        foreach (var path in Directory.EnumerateFiles(commandDocs, "*.md", SearchOption.AllDirectories).Order())
        {
            var firstLine = File.ReadLines(path).FirstOrDefault() ?? string.Empty;
            var match = CommandHeadingPattern.Match(firstLine);
            if (match.Success)
            {
                yield return match.Groups["command"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }
}
