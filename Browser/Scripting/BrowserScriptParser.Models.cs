namespace CMG.Browser.Scripting;

public sealed record BrowserScriptAction(
    int LineNumber,
    string RawLine,
    string Name,
    IReadOnlyList<string> Arguments,
    IReadOnlyDictionary<string, string> Options,
    IReadOnlyList<BrowserScriptAction> Children);

public sealed record ScriptParseResult(bool Success, IReadOnlyList<BrowserScriptAction> Actions, string? Error)
{
    public static ScriptParseResult Ok(IReadOnlyList<BrowserScriptAction> actions) => new(true, actions, null);

    public static ScriptParseResult Fail(string error) => new(false, [], error);
}

internal sealed record TokenizeResult(bool Success, IReadOnlyList<string> Tokens, string? Error)
{
    public static TokenizeResult Ok(IReadOnlyList<string> tokens) => new(true, tokens, null);

    public static TokenizeResult Fail(string error) => new(false, [], error);
}

internal sealed record ActionListParseResult(
    bool Success,
    IReadOnlyList<BrowserScriptAction> Actions,
    int NextIndex,
    string? Error)
{
    public static ActionListParseResult Ok(IReadOnlyList<BrowserScriptAction> actions, int nextIndex) =>
        new(true, actions, nextIndex, null);

    public static ActionListParseResult Fail(string error) => new(false, [], 0, error);
}
