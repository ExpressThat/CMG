namespace CMG.Browser.Scripting;

public static class ScriptImportExpander
{
    public static ScriptImportResult Expand(string script, string baseDirectory)
    {
        var result = ExpandWithSourceLines(script, baseDirectory);
        return result.Success
            ? result with { Script = string.Join('\n', result.Lines.Select(line => line.Text)) }
            : result;
    }

    public static ScriptImportResult ExpandWithSourceLines(string script, string baseDirectory) =>
        ExpandLines(script, Path.GetFullPath(baseDirectory), []);

    private static ScriptImportResult ExpandLines(string script, string baseDirectory, HashSet<string> stack)
    {
        var output = new List<ScriptNormalizedLine>();
        foreach (var line in ScriptLineNormalizer.NormalizeWithSourceLines(script))
        {
            var trimmed = line.Text.Trim();
            if (!StartsWithImport(trimmed))
            {
                output.Add(line);
                continue;
            }

            var path = ReadImportPath(trimmed);
            if (path is null)
            {
                return ScriptImportResult.Fail($"Invalid import syntax '{trimmed}'. Use import \"path\".");
            }

            var fullPath = Path.GetFullPath(Path.Combine(baseDirectory, path));
            if (!File.Exists(fullPath))
            {
                return ScriptImportResult.Fail($"Imported script '{fullPath}' was not found.");
            }

            if (!stack.Add(fullPath))
            {
                return ScriptImportResult.Fail($"Import cycle detected for '{fullPath}'.");
            }

            var nested = ExpandLines(File.ReadAllText(fullPath), Path.GetDirectoryName(fullPath) ?? baseDirectory, stack);
            stack.Remove(fullPath);
            if (!nested.Success)
            {
                return nested;
            }

            output.AddRange(nested.Lines);
        }

        return ScriptImportResult.Ok(output);
    }

    private static string? ReadImportPath(string trimmed)
    {
        var value = trimmed["import".Length..].Trim();
        return value.Length >= 2 && value[0] == '"' && value[^1] == '"'
            ? value[1..^1]
            : null;
    }

    private static bool StartsWithImport(string trimmed) =>
        trimmed.Length > "import".Length &&
        trimmed.StartsWith("import", StringComparison.OrdinalIgnoreCase) &&
        char.IsWhiteSpace(trimmed["import".Length]);
}

public sealed record ScriptImportResult(
    bool Success,
    string? Script,
    string? Error,
    IReadOnlyList<ScriptNormalizedLine>? SourceLines = null)
{
    public IReadOnlyList<ScriptNormalizedLine> Lines => SourceLines ?? [];

    public static ScriptImportResult Ok(IReadOnlyList<ScriptNormalizedLine> lines) => new(true, null, null, lines);

    public static ScriptImportResult Fail(string error) => new(false, null, error);
}
