namespace CMG.Browser.Scripting.Recording;

public static class GifTimelinePath
{
    public static string? Resolve(string gifPath, string? requestedPath)
    {
        if (string.IsNullOrWhiteSpace(requestedPath))
        {
            return null;
        }

        if (IsDisabled(requestedPath))
        {
            return null;
        }

        return requestedPath.Equals("true", StringComparison.OrdinalIgnoreCase)
            ? DefaultFor(gifPath)
            : ResolveRequested(gifPath, requestedPath);
    }

    public static string DefaultFor(string gifPath) =>
        Path.ChangeExtension(Path.GetFullPath(gifPath), ".timeline.json");

    private static string ResolveRequested(string gifPath, string requestedPath)
    {
        var full = Path.GetFullPath(requestedPath);
        var isJsonFile = Path.GetExtension(full).Equals(".json", StringComparison.OrdinalIgnoreCase);
        if (isJsonFile)
        {
            return full;
        }

        var name = $"{Path.GetFileNameWithoutExtension(gifPath)}.timeline.json";
        return Path.Combine(full, name);
    }

    private static bool IsDisabled(string value) =>
        value.Equals("false", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("no", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("off", StringComparison.OrdinalIgnoreCase) ||
        value.Equals("none", StringComparison.OrdinalIgnoreCase);
}
