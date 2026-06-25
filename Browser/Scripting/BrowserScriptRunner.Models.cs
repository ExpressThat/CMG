
namespace CMG.Browser.Scripting;

public sealed record ScriptRunResult(bool Success, IReadOnlyList<string> StdoutLines, string? Error)
{
    public static ScriptRunResult Ok(IReadOnlyList<string> stdoutLines) => new(true, stdoutLines, null);

    public static ScriptRunResult Fail(string error, IReadOnlyList<string>? stdoutLines = null) => new(false, stdoutLines ?? [], error);
}

internal sealed class ScriptExecutionContext
{
    public Dictionary<string, string> Variables { get; } = new(StringComparer.Ordinal);
    public Dictionary<string, BrowserScriptAction> Macros { get; } = new(StringComparer.OrdinalIgnoreCase);
}

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
