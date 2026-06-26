
namespace CMG.Browser.Scripting;

public sealed record ScriptRunResult(bool Success, IReadOnlyList<string> StdoutLines, string? Error, bool Skipped = false)
{
    public static ScriptRunResult Ok(IReadOnlyList<string> stdoutLines) => new(true, stdoutLines, null);

    public static ScriptRunResult Fail(string error, IReadOnlyList<string>? stdoutLines = null) => new(false, stdoutLines ?? [], error);

    public static ScriptRunResult Skip(string reason, IReadOnlyList<string>? stdoutLines = null) => new(false, stdoutLines ?? [], reason, true);
}

public sealed record ScriptTimeoutOptions(
    int? DefaultTimeout = null,
    int? NavigationTimeout = null,
    int? AssertionTimeout = null);

internal sealed class ScriptExecutionContext
{
    private List<Dictionary<string, string>> variableScopes = [new(StringComparer.Ordinal)];
    private readonly List<string> selectorScopes = [];
    private readonly List<string> frameScopes = [];

    public Dictionary<string, ScriptMacro> Macros { get; } = new(StringComparer.OrdinalIgnoreCase);

    public BrowserScriptTraceSession? Trace { get; set; }

    public int? DefaultTimeout { get; set; }

    public int? NavigationTimeout { get; set; }

    public int? AssertionTimeout { get; set; }

    public List<string> SoftFailures { get; } = [];

    public int CurrentVariableScopeIndex => variableScopes.Count - 1;

    public string? CurrentSelectorScope => selectorScopes.Count is 0 ? null : selectorScopes[^1];

    public string? CurrentFrameScope => frameScopes.Count is 0 ? null : frameScopes[^1];

    public bool TryGetVariable(string name, out string value)
    {
        for (var index = variableScopes.Count - 1; index >= 0; index--)
        {
            if (variableScopes[index].TryGetValue(name, out value!))
            {
                return true;
            }
        }

        value = string.Empty;
        return false;
    }

    public void SetVariable(string name, string value) => variableScopes[^1][name] = value;

    public void RemoveLocalVariable(string name) => variableScopes[^1].Remove(name);

    public IReadOnlyList<Dictionary<string, string>> CaptureVariableScopes(int maxIndex) =>
        variableScopes.Take(maxIndex + 1)
            .Select(scope => new Dictionary<string, string>(scope, StringComparer.Ordinal))
            .ToArray();

    public void WithVariableScopes(IReadOnlyList<Dictionary<string, string>> scopes, Action body)
    {
        var previous = variableScopes;
        variableScopes = scopes.Select(scope => new Dictionary<string, string>(scope, StringComparer.Ordinal)).ToList();
        try
        {
            body();
        }
        finally
        {
            variableScopes = previous;
        }
    }

    public void PushVariableScope(IEnumerable<(string Key, string Value)> values, Action body)
    {
        variableScopes.Add(values.ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal));
        try
        {
            body();
        }
        finally
        {
            variableScopes.RemoveAt(variableScopes.Count - 1);
        }
    }

    public void PushSelectorScope(string selector, Action body)
    {
        selectorScopes.Add(selector);
        try
        {
            body();
        }
        finally
        {
            selectorScopes.RemoveAt(selectorScopes.Count - 1);
        }
    }

    public void PushFrameScope(string selector, Action body)
    {
        frameScopes.Add(selector);
        try
        {
            body();
        }
        finally
        {
            frameScopes.RemoveAt(frameScopes.Count - 1);
        }
    }
}

internal sealed record ScriptMacro(BrowserScriptAction Action, int DefinitionScopeIndex);

internal sealed record BrowserScriptTraceStep(
    int LineNumber,
    string Name,
    bool Success,
    string? Error,
    IReadOnlyList<string> Output);

internal sealed record ScriptReadResult(bool Success, string? Script, string? Error)
{
    public static ScriptReadResult Ok(string script) => new(true, script, null);

    public static ScriptReadResult Fail(string error) => new(false, null, error);
}

public sealed class ScriptExecutionException : Exception
{
    public ScriptExecutionException(string message)
        : base(message)
    {
    }
}

internal sealed class ScriptActionFailedException : Exception
{
    public ScriptActionFailedException(string message)
        : base(message)
    {
    }
}

internal sealed class LoopControlException : Exception
{
    public LoopControlException(string kind)
        : base(kind)
    {
        Kind = kind;
    }

    public string Kind { get; }
}

internal sealed class ScriptSkipException : Exception
{
    public ScriptSkipException(int lineNumber, string reason)
        : base(reason)
    {
        LineNumber = lineNumber;
        Reason = reason;
    }

    public int LineNumber { get; }

    public string Reason { get; }
}
