using CMG.Runner;

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
        var runnerParse = new CmgDslParser().Parse("<stdin>", script);
        if (runnerParse.Success && runnerParse.Document is not null)
        {
            return ScriptValidationResult.Runner(
                CountNodes(runnerParse.Document.Nodes, IsSuite),
                CountNodes(runnerParse.Document.Nodes, IsTest),
                CountNodes(runnerParse.Document.Nodes, IsMacro));
        }

        var parseResult = parser.Parse(script);
        return parseResult.Success
            ? ScriptValidationResult.Ok(parseResult.Actions.Count)
            : ScriptValidationResult.Fail(parseResult.Error ?? "Could not parse script.");
    }

    private static int CountNodes(IEnumerable<CmgNode> nodes, Func<CmgNode, bool> predicate) =>
        nodes.Sum(node => (predicate(node) ? 1 : 0) + CountNodes(node.Children, predicate));

    private static bool IsSuite(CmgNode node) =>
        IsAny(node, "suite", "describe", "context");

    private static bool IsTest(CmgNode node) =>
        IsAny(node, "test", "it", "specify");

    private static bool IsMacro(CmgNode node) =>
        node.Kind.Equals("macro", StringComparison.OrdinalIgnoreCase);

    private static bool IsAny(CmgNode node, params string[] names) =>
        names.Any(name => node.Kind.Equals(name, StringComparison.OrdinalIgnoreCase));

    private static ScriptReadResult ReadScript(string file)
    {
        if (string.IsNullOrWhiteSpace(file))
        {
            return ScriptReadResult.Fail("Script file was not provided.");
        }

        if (file is "-")
        {
            var script = Console.In.ReadToEnd();
            return string.IsNullOrEmpty(script)
                ? ScriptReadResult.Fail("No script text was provided on stdin for --file -.")
                : ScriptReadResult.Ok(script);
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

public sealed record ScriptValidationResult(
    bool Success,
    int ActionCount,
    string? Error,
    bool IsRunner = false,
    int SuiteCount = 0,
    int TestCount = 0,
    int MacroCount = 0)
{
    public static ScriptValidationResult Ok(int actionCount) => new(true, actionCount, null);

    public static ScriptValidationResult Runner(int suiteCount, int testCount, int macroCount) =>
        new(true, 0, null, true, suiteCount, testCount, macroCount);

    public static ScriptValidationResult Fail(string error) => new(false, 0, error);
}
