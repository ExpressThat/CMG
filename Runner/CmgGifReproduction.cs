using CMG.Browser.Scripting.Recording;

namespace CMG.Runner;

internal sealed record CmgGifReproduction(string GifPath, string Command);

internal static class CmgGifReproductions
{
    public static IReadOnlyList<CmgGifReproduction> For(CmgTestResult test) =>
        Paths(test.GifPath).Select(path => new CmgGifReproduction(
            path,
            GifReproductionCommand.Runner(
                test.Browser,
                test.BrowserPort,
                test.SourcePath,
                test.Name,
                test.Project,
                path,
                PathsEqual(path, test.CommandGifPath)))).ToArray();

    public static IEnumerable<string> Diagnostics(IEnumerable<CmgTestResult> tests) =>
        tests.SelectMany(For).Select(item => GifReproductionCommand.Diagnostic(item.GifPath, item.Command));

    private static IEnumerable<string> Paths(string? value) =>
        string.IsNullOrWhiteSpace(value)
            ? []
            : value.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);

    private static bool PathsEqual(string left, string? right) =>
        right is not null && Path.GetFullPath(left).Equals(Path.GetFullPath(right), StringComparison.OrdinalIgnoreCase);
}
