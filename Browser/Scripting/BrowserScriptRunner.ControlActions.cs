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
            case "step":
                ExecuteStep(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "narrate":
                ExecuteNarrate(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "macro":
                RegisterMacro(action, context);
                return [$"MACRO {action.LineNumber:000} {action.Arguments[0]}"];
            case "call":
                ExecuteMacro(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "return":
                ExecuteReturn(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "within":
                ExecuteWithin(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "frame":
            case "framelocator":
                ExecuteFrameBlock(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "recording":
            case "withrecording":
                ExecuteRecordingScope(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "showkeystrokes":
                var keystrokeOptions = new Dictionary<string, string>(action.Options, StringComparer.OrdinalIgnoreCase)
                {
                    ["showKeystrokes"] = "true"
                };
                ExecuteRecordingScope(remoteDebuggingUrl, automationClient,
                    action with { Name = "recording", Options = keystrokeOptions }, context, recorder, output);
                return output;
            case "hidefromgif":
            case "cutgif":
            case "speedupgif":
            case "slowdowngif":
                ExecuteTimelineBlock(remoteDebuggingUrl, automationClient, action, context, recorder, output);
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
            case "foreachjson":
                ExecuteForEachJson(remoteDebuggingUrl, automationClient, action, context, recorder, output);
                return output;
            case "foreachlist":
                ExecuteForEachList(remoteDebuggingUrl, automationClient, action, context, recorder, output);
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
            case "withtimeout":
            case "withdefaulttimeout":
            case "withnavigationtimeout":
            case "withassertiontimeout":
            case "withexpecttimeout":
                ExecuteScopedTimeout(remoteDebuggingUrl, automationClient, action, context, recorder, output);
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
        name.Equals("step", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("narrate", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("macro", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("call", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("return", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("within", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("frame", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("frameLocator", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("recording", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("withRecording", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("showKeystrokes", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("hideFromGif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("cutGif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("speedUpGif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("slowDownGif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("if", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("elseif", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("else", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("for", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("foreach", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("foreachJson", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("foreachList", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("foreachSelector", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("while", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("until", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("doWhile", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("doUntil", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("repeat", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("retry", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("toPass", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("withTimeout", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("withDefaultTimeout", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("withNavigationTimeout", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("withAssertionTimeout", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("withExpectTimeout", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("break", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("continue", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("try", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("catch", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("finally", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("switch", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("case", StringComparison.OrdinalIgnoreCase) ||
        name.Equals("default", StringComparison.OrdinalIgnoreCase);

    private void ExecuteStep(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, 1);
        if (recorder is null)
        {
            automationClient.ShowMessageBar(remoteDebuggingUrl, action.Arguments[0], BrowserCaptionOptions.FromOptions(action.Options, action.Name));
        }
        recorder?.EnterStep(action);
        try
        {
            context.PushExecutionContext($"step {action.Arguments[0]}", () =>
                ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
        }
        finally { recorder?.ExitStep(action); }
    }

    private void ExecuteNarrate(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, 1);
        var options = new Dictionary<string, string>(action.Options, StringComparer.OrdinalIgnoreCase);
        options.TryAdd("captionStyle", "teaching");
        var captionAction = action with { Name = "caption", Options = options, Children = [] };
        automationClient.ShowMessageBar(remoteDebuggingUrl, action.Arguments[0], BrowserCaptionOptions.FromOptions(options, action.Name));
        if (recorder is not null)
        {
            recorder.AfterAction(captionAction);
        }
        output.Add($"NARRATE {action.LineNumber:000} {QuoteField(action.Arguments[0])}");
        context.PushExecutionContext($"narrate {action.Arguments[0]}", () =>
            ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
    }
}
