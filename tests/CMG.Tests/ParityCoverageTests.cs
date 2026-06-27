using System.CommandLine;
using System.Text.RegularExpressions;
using CMG.Browser;
using CMG.Browser.Scripting;
using CMG.Commands;

namespace CMG.Tests;

public sealed partial class ParityCoverageTests
{
    [Fact]
    public void BrowserControlLeafCommands_HaveDocumentationPages()
    {
        var root = RepositoryRoot();
        var command = new BrowserControlCommandBuilder(new CapturingHandler()).Build(BrowserOptions());
        var missing = LeafCommandPaths(command)
            .Select(path => new
            {
                Path = path,
                Doc = Path.Combine([root, "docs", "commands", "browser", .. path[..^1], $"{path[^1]}.md"])
            })
            .Where(item => !File.Exists(item.Doc))
            .Select(item => string.Join(" ", item.Path))
            .Order(StringComparer.Ordinal)
            .ToArray();

        Assert.True(missing.Length is 0, "Missing command docs:" + Environment.NewLine + string.Join(Environment.NewLine, missing));
    }

    [Fact]
    public void DispatchedScriptActions_AreDocumented()
    {
        var root = RepositoryRoot();
        var dispatch = File.ReadAllText(Path.Combine(root, "Browser", "Scripting", "BrowserScriptRunner.ActionDispatch.cs"));
        var docs = string.Join(Environment.NewLine, [
            File.ReadAllText(Path.Combine(root, "docs", "scripting", "actions.md")),
            File.ReadAllText(Path.Combine(root, "docs", "scripting", "syntax.md")),
            File.ReadAllText(Path.Combine(root, "docs", "scripting", "gif-recording.md"))
        ]).ToLowerInvariant();
        var missing = ActionTokenRegex().Matches(dispatch)
            .Select(match => match.Groups[1].Value)
            .Where(IsActionToken)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .Where(action => !docs.Contains(action.ToLowerInvariant(), StringComparison.Ordinal))
            .Order(StringComparer.OrdinalIgnoreCase)
            .ToArray();

        Assert.True(missing.Length is 0, "Missing scripting docs for actions:" + Environment.NewLine + string.Join(Environment.NewLine, missing));
    }

    private static IEnumerable<string[]> LeafCommandPaths(Command command) =>
        LeafCommandPaths(command, [command.Name]);

    private static IEnumerable<string[]> LeafCommandPaths(Command command, string[] path)
    {
        if (command.Subcommands.Count is 0)
        {
            yield return path;
            yield break;
        }

        foreach (var child in command.Subcommands)
        {
            foreach (var leaf in LeafCommandPaths(child, [.. path, child.Name]))
            {
                yield return leaf;
            }
        }
    }

    private static BrowserSelectionOptions BrowserOptions()
    {
        var chrome = new Option<bool>("--chrome");
        return new BrowserSelectionOptions(chrome, new Option<bool>("--edge"), new Option<bool>("--firefox"));
    }

    private static bool IsActionToken(string value) =>
        value.Any(char.IsLetter) &&
        value.All(character => char.IsLetterOrDigit(character) || character is '.');

    private static string RepositoryRoot()
    {
        var directory = new DirectoryInfo(AppContext.BaseDirectory);
        while (directory is not null && !File.Exists(Path.Combine(directory.FullName, "CMG.csproj")))
        {
            directory = directory.Parent;
        }

        return directory?.FullName ?? throw new InvalidOperationException("Could not locate repository root.");
    }

    [GeneratedRegex("\"([A-Za-z][A-Za-z0-9.]*)\"")]
    private static partial Regex ActionTokenRegex();

    private sealed class CapturingHandler : IBrowserControlCommandHandler
    {
        public int GetElement(BrowserKind browserKind, string selector, bool html, bool screenshot, FileInfo? output) => 0;

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif) => 0;

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace) => 0;

        public int RunScript(BrowserKind browserKind, string file, FileInfo? gif, FileInfo? trace, ScriptTimeoutOptions? timeouts) => 0;

        public int RunScript(
            BrowserKind browserKind,
            string file,
            FileInfo? gif,
            FileInfo? trace,
            ScriptTimeoutOptions? timeouts,
            string? baseUrl,
            IReadOnlyDictionary<string, string> variables) => 0;

        public int RunScriptAction(BrowserKind browserKind, string scriptLine) => 0;

        public int ValidateScript(string file) => 0;
    }
}
