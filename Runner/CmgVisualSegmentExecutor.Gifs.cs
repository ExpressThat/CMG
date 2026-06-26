namespace CMG.Runner;

public sealed partial class CmgVisualSegmentExecutor
{
    private static FileInfo? ResolveGifPath(CmgTestCase test, CmgNode action, CmgRunOptions options)
    {
        if (action.Options.TryGetValue("output", out var output) && !string.IsNullOrWhiteSpace(output))
        {
            return new FileInfo(output);
        }

        var name = action.Arguments.Count > 0 ? action.Arguments[0] : test.Name;
        var safeName = string.Concat(name.Select(character => char.IsLetterOrDigit(character) ? character : '-'));
        var directory = options.GifDirectory?.FullName ?? Directory.GetCurrentDirectory();
        return new FileInfo(Path.Combine(directory, $"{safeName}.gif"));
    }

    private static FileInfo? BuildGifPath(CmgTestCase test, CmgRunOptions options, int attempt)
    {
        if (options.GifDirectory is null)
        {
            return null;
        }

        var path = CmgRunService.BuildGifPath(test, options);
        if (attempt <= 1 || path is null)
        {
            return path;
        }

        var name = Path.GetFileNameWithoutExtension(path.Name);
        return new FileInfo(Path.Combine(path.DirectoryName ?? string.Empty, $"{name}-attempt-{attempt}.gif"));
    }
}
