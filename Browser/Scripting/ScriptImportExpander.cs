namespace CMG.Browser.Scripting;

public static class ScriptImportExpander
{
    public static ScriptImportResult Expand(string script, string baseDirectory) =>
        Expand(script, Path.GetFullPath(baseDirectory), []);

    private static ScriptImportResult Expand(string script, string baseDirectory, HashSet<string> stack)
    {
        var output = new List<string>();
        foreach (var line in script.ReplaceLineEndings("\n").Split('\n'))
        {
            var trimmed = line.Trim();
            if (!trimmed.StartsWith("import ", StringComparison.OrdinalIgnoreCase))
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

            var nested = Expand(File.ReadAllText(fullPath), Path.GetDirectoryName(fullPath) ?? baseDirectory, stack);
            stack.Remove(fullPath);
            if (!nested.Success)
            {
                return nested;
            }

            output.Add(nested.Script ?? string.Empty);
        }

        return ScriptImportResult.Ok(string.Join('\n', output));
    }

    private static string? ReadImportPath(string trimmed)
    {
        var value = trimmed["import ".Length..].Trim();
        return value.Length >= 2 && value[0] == '"' && value[^1] == '"'
            ? value[1..^1]
            : null;
    }
}

public sealed record ScriptImportResult(bool Success, string? Script, string? Error)
{
    public static ScriptImportResult Ok(string script) => new(true, script, null);

    public static ScriptImportResult Fail(string error) => new(false, null, error);
}
