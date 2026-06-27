using CMG.E2E.Tests.Support;
using System.Text.RegularExpressions;

namespace CMG.E2E.Tests;

public sealed class CommandBehaviorCoverageE2eTests
{
    private static readonly Regex CommandHeadingPattern = new("^# `(?<command>[^`]+)`$", RegexOptions.Compiled);
    private static readonly string[] AllowedDimensions =
    [
        "artifact",
        "browser",
        "cleanup",
        "failure",
        "gif",
        "help",
        "network",
        "report",
        "state",
        "success",
        "trace",
        "visual",
    ];

    [Fact]
    public void DocumentedCommands_HaveDeclaredBehaviorCoverage()
    {
        var root = E2ePaths.RepositoryRoot();
        var patterns = CoveragePatterns(root);
        var commands = DocumentedCommands(root).ToArray();
        var missing = commands
            .Where(command => !patterns.Any(pattern => pattern.Covers(command)))
            .ToArray();

        Assert.True(patterns.Count > 0, "docs/e2e-command-coverage.md must define coverage rows.");
        Assert.Empty(missing);
    }

    [Fact]
    public void DeclaredBehaviorCoverage_ReferencesExistingE2eFiles()
    {
        var root = E2ePaths.RepositoryRoot();
        var testRoot = Path.Combine(root, "tests", "CMG.E2E.Tests");
        var missing = CoveragePatterns(root)
            .SelectMany(pattern => pattern.Owners)
            .Distinct(StringComparer.Ordinal)
            .Where(owner => !File.Exists(Path.Combine(testRoot, owner)))
            .ToArray();

        Assert.Empty(missing);
    }

    [Fact]
    public void DeclaredBehaviorCoverage_HasValidCoverageDimensions()
    {
        var root = E2ePaths.RepositoryRoot();
        var failures = CoveragePatterns(root)
            .Where(pattern => pattern.Dimensions.Length is 0 ||
                              pattern.Dimensions.Except(AllowedDimensions, StringComparer.Ordinal).Any())
            .Select(pattern => pattern.Pattern)
            .ToArray();

        Assert.Empty(failures);
    }

    [Fact]
    public void DeclaredBehaviorCoverage_HasMinimumExpectedDimensions()
    {
        var root = E2ePaths.RepositoryRoot();
        var failures = CoveragePatterns(root)
            .Where(pattern => !HasAny(pattern, "success", "failure", "help") ||
                              Needs(pattern, "network") && !Has(pattern, "network") ||
                              Needs(pattern, "capture") && !HasAll(pattern, "artifact", "visual") ||
                              pattern.Pattern is "run" && !HasAll(pattern, "report", "gif", "trace") ||
                              pattern.Pattern is "browser control script" && !HasAll(pattern, "gif", "trace"))
            .Select(pattern => pattern.Pattern)
            .ToArray();

        Assert.Empty(failures);
    }

    private static List<CoveragePattern> CoveragePatterns(string root)
    {
        var path = Path.Combine(root, "docs", "e2e-command-coverage.md");
        return File.ReadLines(path)
            .Select(ParseCoverageRow)
            .Where(pattern => pattern is not null)
            .Cast<CoveragePattern>()
            .ToList();
    }

    private static CoveragePattern? ParseCoverageRow(string line)
    {
        if (!line.StartsWith('|') || line.Contains("---", StringComparison.Ordinal))
        {
            return null;
        }

        var cells = line.Trim('|')
            .Split('|')
            .Select(cell => cell.Trim())
            .ToArray();
        if (cells.Length < 4 || cells[0] is "Command pattern")
        {
            return null;
        }

        var pattern = cells[0].Trim('`');
        var isPrefix = pattern.EndsWith(" *", StringComparison.Ordinal);
        if (isPrefix)
        {
            pattern = pattern[..^2];
        }

        var owners = cells[1]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(owner => owner.Trim('`'))
            .ToArray();
        var dimensions = cells[2]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(dimension => dimension.Trim('`'))
            .ToArray();
        return new CoveragePattern(pattern, isPrefix, owners, dimensions);
    }

    private static IEnumerable<string> DocumentedCommands(string root)
    {
        var commandDocs = Path.Combine(root, "docs", "commands");
        foreach (var path in Directory.EnumerateFiles(commandDocs, "*.md", SearchOption.AllDirectories))
        {
            var firstLine = File.ReadLines(path).FirstOrDefault() ?? string.Empty;
            var match = CommandHeadingPattern.Match(firstLine);
            if (match.Success)
            {
                yield return match.Groups["command"].Value;
            }
        }
    }

    private sealed record CoveragePattern(string Pattern, bool IsPrefix, string[] Owners, string[] Dimensions)
    {
        public bool Covers(string command) =>
            IsPrefix
                ? command.Equals(Pattern, StringComparison.Ordinal) ||
                  command.StartsWith(Pattern + " ", StringComparison.Ordinal)
                : command.Equals(Pattern, StringComparison.Ordinal);
    }

    private static bool Has(CoveragePattern pattern, string dimension) =>
        pattern.Dimensions.Contains(dimension, StringComparer.Ordinal);

    private static bool HasAny(CoveragePattern pattern, params string[] dimensions) =>
        dimensions.Any(dimension => Has(pattern, dimension));

    private static bool HasAll(CoveragePattern pattern, params string[] dimensions) =>
        dimensions.All(dimension => Has(pattern, dimension));

    private static bool Needs(CoveragePattern pattern, string term) =>
        pattern.Pattern.Contains(term, StringComparison.Ordinal);
}
