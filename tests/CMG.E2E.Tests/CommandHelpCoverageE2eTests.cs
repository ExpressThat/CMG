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

    [Fact]
    public void CommandDocs_AllExposeCommandHeadings()
    {
        var missing = CommandDocFiles()
            .Where(path => !CommandHeadingPattern.IsMatch(File.ReadLines(path).FirstOrDefault() ?? string.Empty))
            .Select(path => Path.GetRelativePath(E2ePaths.RepositoryRoot(), path).Replace('\\', '/'))
            .ToArray();

        Assert.Empty(missing);
    }

    [Fact]
    public void CommandDocHeadings_MatchTheirPaths()
    {
        var mismatches = CommandDocFiles()
            .Select(path => new
            {
                Path = Path.GetRelativePath(E2ePaths.RepositoryRoot(), path).Replace('\\', '/'),
                Expected = ExpectedCommandFromPath(path),
                Actual = CommandHeadingPattern.Match(File.ReadLines(path).FirstOrDefault() ?? string.Empty).Groups["command"].Value
            })
            .Where(doc => doc.Actual != doc.Expected)
            .Select(doc => $"{doc.Path}: expected '{doc.Expected}', got '{doc.Actual}'")
            .ToArray();

        Assert.Empty(mismatches);
    }

    private static IEnumerable<string[]> DocumentedCommands()
    {
        foreach (var path in CommandDocFiles())
        {
            var firstLine = File.ReadLines(path).FirstOrDefault() ?? string.Empty;
            var match = CommandHeadingPattern.Match(firstLine);
            if (match.Success)
            {
                yield return match.Groups["command"].Value.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            }
        }
    }

    private static IEnumerable<string> CommandDocFiles()
    {
        var commandDocs = Path.Combine(E2ePaths.RepositoryRoot(), "docs", "commands");
        return Directory.EnumerateFiles(commandDocs, "*.md", SearchOption.AllDirectories).Order();
    }

    private static string ExpectedCommandFromPath(string path)
    {
        var commandDocs = Path.Combine(E2ePaths.RepositoryRoot(), "docs", "commands");
        var parts = Path.GetRelativePath(commandDocs, path).Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        var commandParts = parts[^1] == "index.md"
            ? parts[..^1]
            : parts[..^1].Append(Path.GetFileNameWithoutExtension(parts[^1]));
        return string.Join(' ', commandParts);
    }
}
