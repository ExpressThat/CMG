using CMG.E2E.Tests.Support;

namespace CMG.E2E.Tests;

[Collection(CmgE2eCollection.Name)]
public sealed class CommandHelpCoverageE2eTests
{
    private readonly CmgBrowserFixture fixture;

    public CommandHelpCoverageE2eTests(CmgBrowserFixture fixture)
    {
        this.fixture = fixture;
    }

    [Fact]
    public void EveryDocumentedLeafCommand_HasWorkingHelp()
    {
        var failures = new List<string>();
        foreach (var command in DocumentedLeafCommands())
        {
            var result = fixture.Cli.Run([.. command, "--help"]);
            if (result.ExitCode is 0 && result.Stdout.Contains("Usage:", StringComparison.Ordinal))
            {
                continue;
            }

            failures.Add($"{string.Join(' ', command)} => exit {result.ExitCode}\n{result.Stdout}\n{result.Stderr}");
        }

        Assert.True(failures.Count is 0, string.Join("\n---\n", failures));
    }

    private static IEnumerable<string[]> DocumentedLeafCommands()
    {
        var commandDocs = Path.Combine(E2ePaths.RepositoryRoot(), "docs", "commands");
        foreach (var file in Directory.EnumerateFiles(commandDocs, "*.md", SearchOption.AllDirectories).OrderBy(value => value, StringComparer.Ordinal))
        {
            if (Path.GetFileName(file).Equals("index.md", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }

            var relative = Path.GetRelativePath(commandDocs, Path.ChangeExtension(file, null));
            yield return relative.Split(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
        }
    }
}
