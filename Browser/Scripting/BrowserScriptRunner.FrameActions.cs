namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteFrameAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        var script = action.Name.ToLowerInvariant() switch
        {
            "frameclick" => FrameElement(action, BrowserFrameScripts.Click),
            "framehover" => FrameElement(action, BrowserFrameScripts.Hover),
            "frametype" => FrameText(action, BrowserFrameScripts.Type),
            "framefill" => FrameText(action, BrowserFrameScripts.Fill),
            "frameasserttext" => FrameAssertText(action),
            "framewaitforelement" => FrameWait(action),
            "frameevaluate" => FrameEvaluate(action),
            _ => throw new ScriptExecutionException($"Unknown frame action '{action.Name}'.")
        };

        var result = automationClient.Evaluate(remoteDebuggingUrl, script);
        return action.Name.Equals("frameEvaluate", StringComparison.OrdinalIgnoreCase)
            ? [$"FRAME_EVALUATE {action.LineNumber:000} {result}"]
            : [$"FRAME {action.LineNumber:000} {action.Name}"];
    }

    private static string FrameElement(BrowserScriptAction action, Func<string, string, string> build)
    {
        RequireArgumentCount(action, 2, 2);
        return build(action.Arguments[0], action.Arguments[1]);
    }

    private static string FrameText(BrowserScriptAction action, Func<string, string, string, string> build)
    {
        RequireArgumentCount(action, 3, 3);
        return build(action.Arguments[0], action.Arguments[1], action.Arguments[2]);
    }

    private static string FrameAssertText(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 3, 3);
        ValidateTextMatchOptions(action, action.Arguments[2]);
        return BrowserFrameScripts.AssertText(
            action.Arguments[0],
            action.Arguments[1],
            action.Arguments[2],
            EventTextMatchMode(action),
            GetBoolOption(action, "ignoreCase"));
    }

    private static string FrameWait(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        return BrowserFrameScripts.WaitForElement(
            action.Arguments[0],
            action.Arguments[1],
            GetIntOption(action, "timeout", 5_000));
    }

    private static string FrameEvaluate(BrowserScriptAction action)
    {
        RequireArgumentCount(action, 2, 2);
        return BrowserFrameScripts.Evaluate(action.Arguments[0], action.Arguments[1]);
    }
}
