using System.Text.Json;
using CMG.Browser;

namespace CMG.Runner;

public sealed class CmgStorageStateRunner
{
    public CmgStepResult Run(CmgNode action, string remoteDebuggingUrl, IBrowserAutomationClient automationClient)
    {
        if (action.Arguments.Count < 1)
        {
            return Fail(action, "storageState requires save or load.");
        }

        var operation = action.Arguments[0].ToLowerInvariant();
        var path = action.Options.TryGetValue("path", out var optionPath)
            ? optionPath
            : action.Arguments.ElementAtOrDefault(1) ?? "cmg-storage-state.json";

        try
        {
            return operation switch
            {
                "save" => Save(action, path, remoteDebuggingUrl, automationClient),
                "load" => Load(action, path, remoteDebuggingUrl, automationClient),
                _ => Fail(action, $"Unknown storageState operation '{operation}'.")
            };
        }
        catch (Exception exception) when (exception is IOException or JsonException or ChromeDevToolsException)
        {
            return Fail(action, exception.Message);
        }
    }

    private static CmgStepResult Save(CmgNode action, string path, string remoteDebuggingUrl, IBrowserAutomationClient automationClient)
    {
        var json = automationClient.Evaluate(
            remoteDebuggingUrl,
            "JSON.stringify({ localStorage: Object.entries(localStorage), sessionStorage: Object.entries(sessionStorage), cookies: document.cookie })");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(path)) ?? Directory.GetCurrentDirectory());
        File.WriteAllText(path, json);
        return Pass(action, $"STORAGE_STATE {action.LineNumber:000} saved {Path.GetFullPath(path)}");
    }

    private static CmgStepResult Load(CmgNode action, string path, string remoteDebuggingUrl, IBrowserAutomationClient automationClient)
    {
        if (!File.Exists(path))
        {
            return Fail(action, $"Storage state file '{path}' was not found.");
        }

        var json = File.ReadAllText(path);
        automationClient.Evaluate(remoteDebuggingUrl, BuildLoadScript(json));
        return Pass(action, $"STORAGE_STATE {action.LineNumber:000} loaded {Path.GetFullPath(path)}");
    }

    private static string BuildLoadScript(string json) =>
        $"(() => {{ const state = JSON.parse({QuoteJs(json)}); localStorage.clear(); sessionStorage.clear(); for (const [k,v] of state.localStorage || []) localStorage.setItem(k,v); for (const [k,v] of state.sessionStorage || []) sessionStorage.setItem(k,v); if (state.cookies) document.cookie = state.cookies; return true; }})()";

    private static string QuoteJs(string value) =>
        $"\"{value.Replace("\\", "\\\\", StringComparison.Ordinal).Replace("\"", "\\\"", StringComparison.Ordinal).Replace("\r", "\\r", StringComparison.Ordinal).Replace("\n", "\\n", StringComparison.Ordinal)}\"";

    private static CmgStepResult Pass(CmgNode action, string output) => new(action.LineNumber, action.Kind, true, [output], null, null);

    private static CmgStepResult Fail(CmgNode action, string error) => new(action.LineNumber, action.Kind, false, [], error, null);
}
