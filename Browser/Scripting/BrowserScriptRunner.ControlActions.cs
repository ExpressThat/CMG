namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private IReadOnlyList<string> ExecuteControlAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder)
    {
        var output = new List<string>();
        switch (action.Name.ToLowerInvariant())
        {
            case "macro":
                RegisterMacro(action, context);
                return [$"MACRO {action.LineNumber:000} {action.Arguments[0]}"];
            case "call":
                ExecuteMacro(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "if":
                ExecuteIf(remoteDebuggingUrl, automationClient, [action], context, recorder, output);
                return output;
            case "elseif":
            case "else":
                return [];
            case "for":
                ExecuteFor(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "foreach":
                ExecuteForEach(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "foreachselector":
                ExecuteForEachSelector(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            default:
                throw new ScriptExecutionException($"Unknown control action '{action.Name}'.");
        }
    }

    private static bool IsControlAction(string name) =>
        name.Equals("macro", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("if", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("elseif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("else", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("for", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("foreach", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("foreachSelector", StringComparison.OrdinalIgnoreCase);
}
