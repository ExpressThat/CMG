namespace CMG.Browser.Scripting;

public sealed class BrowserScriptValidator
{
    private readonly BrowserScriptParser parser;

    public BrowserScriptValidator(BrowserScriptParser parser)
    {
        this.parser = parser;
    }

    public ScriptValidationResult ValidateFile(string file)
    {
        var readResult = ReadScript(file);
        if (!readResult.Success)
        {
            return ScriptValidationResult.Fail(readResult.Error ?? "Could not read script.");
        }

        return ValidateText(readResult.Script ?? string.Empty);
    }

    public ScriptValidationResult ValidateText(string script)
    {
        var parseResult = parser.Parse(script);
        return parseResult.Success
            ? ScriptValidationResult.Ok(parseResult.Actions.Count)
            : ScriptValidationResult.Fail(parseResult.Error ?? "Could not parse script.");
    }

    private static ScriptReadResult ReadScript(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return ScriptReadResult.Fail("Script file was not provided.");
        }

        if (file is "-")
        {
            return ScriptReadResult.Ok(Console.In.ReadToEnd());
        }

        if (!File.Exists(file))
        {
            return ScriptReadResult.Fail($"Script file '{file}' was not found.");
        }

        var fullPath = Path.GetFullPath(file);
        var expanded = ScriptImportExpander.Expand(
            File.ReadAllText(fullPath),
            Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory());
        return expanded.Success
            ? ScriptReadResult.Ok(expanded.Script ?? string.Empty)
            : ScriptReadResult.Fail(expanded.Error ?? "Could not import script.");
    }
}

public sealed record ScriptValidationResult(bool Success, int ActionCount, string? Error)
{
    public static ScriptValidationResult Ok(int actionCount) => new(true, actionCount, null);

    public static ScriptValidationResult Fail(string error) => new(false, 0, error);
}
