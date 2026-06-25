namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteFileAction(BrowserScriptAction action, ScriptExecutionContext context)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "readfile" or "fixture" => ReadFileAction(action, context),
            "writefile" => WriteFileAction(action, append: false),
            "appendfile" => WriteFileAction(action, append: true),
            "expectfile" => ExpectFileAction(action),
            _ => throw new ScriptExecutionException($"Unknown file action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> ReadFileAction(BrowserScriptAction action, ScriptExecutionContext context)
    {
        RequireArgumentCount(action, 1, 1);
        var path = RequiredFilePath(action);
        if (!File.Exists(path))
        {
            throw new ScriptExecutionException($"File '{path}' was not found.");
        }

        var value = ReadFile(path, action);
        context.Variables[action.Arguments[0]] = value;
        return [$"FILE_READ {action.LineNumber:000} {action.Arguments[0]} {path}"];
    }

    private static IReadOnlyList<string> WriteFileAction(BrowserScriptAction action, bool append)
    {
        var path = RequiredFilePath(action);
        var text = action.Options.TryGetValue("text", out var optionText)
            ? optionText
            : action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        Directory.CreateDirectory(Path.GetDirectoryName(path) ?? Directory.GetCurrentDirectory());
        if (append)
        {
            File.AppendAllText(path, text);
            return [$"FILE_APPENDED {action.LineNumber:000} {path}"];
        }

        File.WriteAllText(path, text);
        return [$"FILE_WRITTEN {action.LineNumber:000} {path}"];
    }

    private static IReadOnlyList<string> ExpectFileAction(BrowserScriptAction action)
    {
        var path = RequiredFilePath(action);
        if (!File.Exists(path))
        {
            throw new ScriptExecutionException($"Expected file '{path}' to exist.");
        }

        if (action.Options.TryGetValue("contains", out var expected) &&
            !File.ReadAllText(path).Contains(expected, StringComparison.Ordinal))
        {
            throw new ScriptExecutionException($"Expected file '{path}' to contain '{expected}'.");
        }

        return [$"FILE_OK {action.LineNumber:000} {path}"];
    }

    private static string RequiredFilePath(BrowserScriptAction action)
    {
        if (!action.Options.TryGetValue("path", out var value) || string.IsNullOrWhiteSpace(value))
        {
            throw new ScriptExecutionException($"{action.Name} requires path=<file>.");
        }

        return Path.GetFullPath(value);
    }

    private static string ReadFile(string path, BrowserScriptAction action) =>
        action.Options.TryGetValue("encoding", out var encoding) &&
        encoding.Equals("base64", StringComparison.OrdinalIgnoreCase)
            ? Convert.ToBase64String(File.ReadAllBytes(path))
            : File.ReadAllText(path);
}
