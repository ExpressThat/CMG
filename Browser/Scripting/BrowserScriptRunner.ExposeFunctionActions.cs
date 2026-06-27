namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteExposeFunction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        var name = action.Arguments[0];
        if (!IsSafeIdentifier(name))
        {
            throw new ScriptExecutionException($"{action.Name} requires a valid JavaScript identifier name.");
        }

        var script = BrowserExposeFunctionScript.Build(name, action.Arguments[1], includeSource: IsExposeBinding(action));
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"EXPOSED_FUNCTION {action.LineNumber:000} {name}"];
    }

    private static bool IsExposeBinding(BrowserScriptAction action) =>
        string.Equals(action.Name, "exposeBinding", StringComparison.OrdinalIgnoreCase);

    private static bool IsSafeIdentifier(string value)
    {
        if (string.IsNullOrWhiteSpace(value) || !IsIdentifierStart(value[0]))
        {
            return false;
        }

        return value.Skip(1).All(IsIdentifierPart);
    }

    private static bool IsIdentifierStart(char value) =>
        value is '_' or '$' || char.IsAsciiLetter(value);

    private static bool IsIdentifierPart(char value) =>
        IsIdentifierStart(value) || char.IsAsciiDigit(value);
}
