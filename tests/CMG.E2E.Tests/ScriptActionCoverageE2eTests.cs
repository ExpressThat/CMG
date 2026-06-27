using CMG.E2E.Tests.Support;
using System.Text.RegularExpressions;

namespace CMG.E2E.Tests;

public sealed class ScriptActionCoverageE2eTests
{
    private static readonly Regex HeadingPattern = new("^#{2,3} ", RegexOptions.Compiled);
    private static readonly Regex InlineCodePattern = new("`(?<name>[^`]+)`", RegexOptions.Compiled);

    [Fact]
    public void DocumentedScriptActions_HaveDeclaredE2eCoverage()
    {
        var root = E2ePaths.RepositoryRoot();
        var covered = CoveredActions(root);
        var documented = DocumentedActions(root);
        var missing = documented
            .Where(action => !covered.Contains(action))
            .ToArray();

        Assert.True(covered.Count > 0, "docs/e2e-script-action-coverage.md must define coverage rows.");
        Assert.Empty(missing);
    }

    [Fact]
    public void DeclaredScriptActionCoverage_ReferencesExistingE2eFiles()
    {
        var root = E2ePaths.RepositoryRoot();
        var testRoot = Path.Combine(root, "tests", "CMG.E2E.Tests");
        var missing = CoverageRows(root)
            .SelectMany(row => row.Owners)
            .Distinct(StringComparer.Ordinal)
            .Where(owner => !File.Exists(Path.Combine(testRoot, owner)))
            .ToArray();

        Assert.Empty(missing);
    }

    private static SortedSet<string> DocumentedActions(string root)
    {
        var path = Path.Combine(root, "docs", "scripting", "actions.md");
        var actions = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var line in File.ReadLines(path).Where(line => HeadingPattern.IsMatch(line)))
        {
            foreach (Match match in InlineCodePattern.Matches(line))
            {
                actions.Add(match.Groups["name"].Value);
            }
        }

        return actions;
    }

    private static SortedSet<string> CoveredActions(string root)
    {
        var actions = new SortedSet<string>(StringComparer.Ordinal);
        foreach (var row in CoverageRows(root))
        {
            foreach (var action in row.Actions)
            {
                actions.Add(action);
            }
        }

        return actions;
    }

    private static IEnumerable<CoverageRow> CoverageRows(string root)
    {
        var path = Path.Combine(root, "docs", "e2e-script-action-coverage.md");
        foreach (var line in File.ReadLines(path))
        {
            var row = ParseCoverageRow(line);
            if (row is not null)
            {
                yield return row;
            }
        }
    }

    private static CoverageRow? ParseCoverageRow(string line)
    {
        if (!line.StartsWith('|') || line.Contains("---", StringComparison.Ordinal))
        {
            return null;
        }

        var cells = line.Trim('|').Split('|').Select(cell => cell.Trim()).ToArray();
        if (cells.Length < 3 || cells[0] is "Actions")
        {
            return null;
        }

        var actions = InlineCodePattern.Matches(cells[0])
            .Select(match => match.Groups["name"].Value)
            .ToArray();
        var owners = cells[1]
            .Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            .Select(owner => owner.Trim('`'))
            .ToArray();
        return new CoverageRow(actions, owners);
    }

    private sealed record CoverageRow(string[] Actions, string[] Owners);
}
