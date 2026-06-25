namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteAddInitScript(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        if (action.Arguments.Count > 1)
        {
            throw new ScriptExecutionException($"{action.Name} expects at most 1 positional argument.");
        }

        var source = ReadInitScriptSource(action);
        if (string.IsNullOrWhiteSpace(source))
        {
            throw new ScriptExecutionException($"{action.Name} requires inline script text or path=<file>.");
        }

        var id = automationClient.AddInitScript(remoteDebuggingUrl, source);
        return [$"INIT_SCRIPT {action.LineNumber:000} {id}"];
    }

    private static string ReadInitScriptSource(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("path", out var path) && !string.IsNullOrWhiteSpace(path))
        {
            var fullPath = Path.GetFullPath(path);
            if (!File.Exists(fullPath))
            {
                throw new ScriptExecutionException($"Init script file '{fullPath}' was not found.");
            }

            return File.ReadAllText(fullPath);
        }

        return action.Arguments.Count is 1 ? action.Arguments[0] : string.Empty;
    }
}
