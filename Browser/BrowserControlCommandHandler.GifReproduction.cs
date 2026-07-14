using CMG.Browser.Scripting;
using CMG.Browser.Scripting.Recording;

namespace CMG.Browser;

public sealed partial class BrowserControlCommandHandler
{
    private static ScriptRunResult AddGifReproduction(
        ScriptRunResult result,
        FileInfo? commandGif,
        Func<string, bool, string> command)
    {
        var paths = result.StdoutLines
            .Where(line => line.StartsWith("GIF ", StringComparison.Ordinal))
            .Select(line => line[4..].Trim())
            .Where(path => path.Length > 0)
            .Distinct(StringComparer.OrdinalIgnoreCase)
            .ToArray();
        if (paths.Length == 0) return result;
        var commandPath = commandGif?.FullName;
        var diagnostics = paths.Select(path =>
        {
            var wholeRun = commandPath is not null &&
                Path.GetFullPath(path).Equals(Path.GetFullPath(commandPath), StringComparison.OrdinalIgnoreCase);
            return GifReproductionCommand.Diagnostic(path, command(path, wholeRun));
        });
        return result with { StdoutLines = result.StdoutLines.Concat(diagnostics).ToArray() };
    }
}
