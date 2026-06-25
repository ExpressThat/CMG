using System.Text.Json;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteDialogAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "capturedialogs" => CaptureDialogs(remoteDebuggingUrl, automationClient, action),
            "setdialogbehavior" => SetDialogBehavior(remoteDebuggingUrl, automationClient, action),
            "waitfordialog" => WaitForDialog(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown dialog action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> CaptureDialogs(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        InstallDialogScript(remoteDebuggingUrl, automationClient, action, behavior: "accept");
        return [$"DIALOG_CAPTURE {action.LineNumber:000}"];
    }

    private static IReadOnlyList<string> SetDialogBehavior(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var behavior = action.Arguments[0].ToLowerInvariant();
        if (behavior is not ("accept" or "dismiss"))
        {
            throw new ScriptExecutionException("setDialogBehavior expects accept or dismiss.");
        }

        InstallDialogScript(remoteDebuggingUrl, automationClient, action, behavior);
        return [$"DIALOG_BEHAVIOR {action.LineNumber:000} {behavior}"];
    }

    private static IReadOnlyList<string> WaitForDialog(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var result = automationClient.Evaluate(remoteDebuggingUrl, BrowserDialogScripts.WaitForDialog(action.Arguments[0], timeout));
        return [$"DIALOG {action.LineNumber:000} {ParseDialogResult(result)}"];
    }

    private static void InstallDialogScript(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action, string behavior)
    {
        var promptText = action.Options.TryGetValue("promptText", out var value) ? value : string.Empty;
        var script = BrowserDialogScripts.Install(behavior, promptText);
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
    }

    private static string ParseDialogResult(string result)
    {
        using var document = JsonDocument.Parse(result);
        var root = document.RootElement;
        if (root.TryGetProperty("success", out var success) && success.GetBoolean())
        {
            return root.TryGetProperty("value", out var value) ? value.GetRawText() : "{}";
        }

        var error = root.TryGetProperty("error", out var reason)
            ? reason.GetString() ?? "Dialog wait failed."
            : "Dialog wait failed.";
        throw new ScriptExecutionException(error);
    }
}
