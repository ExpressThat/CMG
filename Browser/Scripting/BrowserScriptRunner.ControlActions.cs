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
            case "return":
                return ReturnValue(action);
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
            case "while":
                ExecuteWhile(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "until":
                ExecuteUntil(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "dowhile":
                ExecutePostConditionLoop(remoteDebuggingUrl, automationClient, action, context, recorder, output, repeatWhenConditionIsTrue: true);
                return output;
            case "dountil":
                ExecutePostConditionLoop(remoteDebuggingUrl, automationClient, action, context, recorder, output, repeatWhenConditionIsTrue: false);
                return output;
            case "repeat":
                ExecuteRepeat(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "retry":
            case "topass":
                ExecuteRetry(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "break":
                RequireArgumentCount(action, 0, 0);
                throw new LoopControlException("break");
            case "continue":
                RequireArgumentCount(action, 0, 0);
                throw new LoopControlException("continue");
            case "try":
                ExecuteTry(remoteDebuggingUrl, automationClient, [action], context, recorder, output);
                return output;
            case "catch":
            case "finally":
                return [];
            case "switch":
                ExecuteSwitch(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "case":
            case "default":
                return [];
            default:
                throw new ScriptExecutionException($"Unknown control action '{action.Name}'.");
        }
    }

    private static bool IsControlAction(string name) =>
        name.Equals("macro", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("call", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("return", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("if", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("elseif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("else", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("for", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("foreach", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("foreachSelector", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("while", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("until", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("doWhile", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("doUntil", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("repeat", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("retry", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("toPass", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("break", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("continue", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("try", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("catch", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("finally", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("switch", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("case", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("default", StringComparison.OrdinalIgnoreCase);
}
