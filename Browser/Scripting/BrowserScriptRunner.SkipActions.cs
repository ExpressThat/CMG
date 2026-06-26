namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteSkip(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, int.MaxValue);
        var reason = action.Arguments.Count is 0 ? "Skipped by script." : string.Join(' ', action.Arguments);
        throw new ScriptSkipException(action.LineNumber, reason);
    }
}
