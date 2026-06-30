namespace CMG.Browser.Scripting;

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
