namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteStorageAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "localstorage" => WebStorage(remoteDebuggingUrl, automationClient, action, "localStorage", "LOCAL_STORAGE"),
            "sessionstorage" => WebStorage(remoteDebuggingUrl, automationClient, action, "sessionStorage", "SESSION_STORAGE"),
            "cookie" => Cookie(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown storage action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> WebStorage(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string storage,
        string output)
    {
        var (operation, key, value) = ParseStorageArguments(action, allowGetAll: false);
        var result = automationClient.Evaluate(remoteDebuggingUrl, BrowserStorageScripts.WebStorage(storage, operation, key, value));
        return operation is "get"
            ? [$"{output} {action.LineNumber:000} get {key} {result}"]
            : [$"{output} {action.LineNumber:000} {operation} {key}".TrimEnd()];
    }

    private static IReadOnlyList<string> Cookie(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        var (operation, key, value) = ParseStorageArguments(action, allowGetAll: true);
        var result = automationClient.Evaluate(remoteDebuggingUrl, BrowserStorageScripts.Cookie(operation, key, value));
        return operation is "get"
            ? [$"COOKIE {action.LineNumber:000} get {key} {result}".TrimEnd()]
            : [$"COOKIE {action.LineNumber:000} {operation} {key}".TrimEnd()];
    }

    private static (string Operation, string Key, string Value) ParseStorageArguments(BrowserScriptAction action, bool allowGetAll)
    {
        var operation = action.Arguments.Count > 0 ? action.Arguments[0].ToLowerInvariant() : "get";
        if (operation is not ("get" or "set" or "remove" or "clear"))
        {
            throw new ScriptExecutionException($"{action.Name} expects get, set, remove, or clear.");
        }

        var min = operation switch
        {
            "get" => allowGetAll ? 1 : 2,
            "set" => 3,
            "remove" => 2,
            _ => 1
        };
        var max = operation is "set" ? 3 : operation is "get" && allowGetAll ? 2 : min;
        if (action.Arguments.Count < min || action.Arguments.Count > max)
        {
            throw new ScriptExecutionException($"{action.Name} {operation} expects {DescribeStorageArguments(operation, allowGetAll)}.");
        }

        return (
            operation,
            action.Arguments.Count > 1 ? action.Arguments[1] : string.Empty,
            action.Arguments.Count > 2 ? action.Arguments[2] : string.Empty);
    }

    private static string DescribeStorageArguments(string operation, bool allowGetAll) =>
        operation switch
        {
            "get" => allowGetAll ? "an optional key" : "a key",
            "set" => "a key and value",
            "remove" => "a key",
            _ => "no key or value"
        };
}
