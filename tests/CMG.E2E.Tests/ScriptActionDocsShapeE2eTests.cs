using CMG.E2E.Tests.Support;
using System.Text.RegularExpressions;

namespace CMG.E2E.Tests;

public sealed class ScriptActionDocsShapeE2eTests
{
    private static readonly Regex HeadingPattern = new("^#{2,3} (?<heading>.+)$", RegexOptions.Compiled);
    private static readonly string[] AllowedNonActionHeadings =
    [
        "Default Timeouts",
        "Clipboard Actions",
        "Frame Actions",
        "Runner-Only Structural Actions",
        "Runner Convenience Actions",
        "Element Assertion Aliases",
        "Imports, Control Flow, Loops, And Macros",
        "Unknown Actions",
        "Locator Support",
        "Actionability",
    ];

    [Fact]
    public void ScriptActionDocHeadings_ExposeActionNamesOrKnownGroupSections()
    {
        var unexpected = File.ReadLines(ActionsDocPath())
            .Select((line, index) => new { Line = line, Number = index + 1 })
            .Select(item => new { item.Number, Match = HeadingPattern.Match(item.Line) })
            .Where(item => item.Match.Success)
            .Where(item => !item.Match.Groups["heading"].Value.Contains('`'))
            .Where(item => !AllowedNonActionHeadings.Contains(item.Match.Groups["heading"].Value, StringComparer.Ordinal))
            .Select(item => $"line {item.Number}: {item.Match.Groups["heading"].Value}")
            .ToArray();

        Assert.Empty(unexpected);
    }

    private static string ActionsDocPath() =>
        Path.Combine(E2ePaths.RepositoryRoot(), "docs", "scripting", "actions.md");
}
