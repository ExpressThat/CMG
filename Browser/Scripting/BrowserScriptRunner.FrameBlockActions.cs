namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private void ExecuteFrameBlock(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        ScriptExecutionContext context,
        Recording.ScriptGifRecorder? recorder,
        List<string> output)
    {
        RequireArgumentCount(action, 1, 1);
        var frameSelector = action.Arguments[0];
        context.PushFrameScope(frameSelector, () => ExecuteActions(remoteDebuggingUrl, automationClient, action.Children, context, recorder, output));
    }

    private static BrowserScriptAction ApplyFrameScope(BrowserScriptAction action, ScriptExecutionContext context)
    {
        var frame = context.CurrentFrameScope;
        if (string.IsNullOrWhiteSpace(frame) || action.Name.Equals("frame", StringComparison.OrdinalIgnoreCase) ||
            action.Name.Equals("frameLocator", StringComparison.OrdinalIgnoreCase) || action.Name.StartsWith("frame", StringComparison.OrdinalIgnoreCase))
        {
            return action;
        }

        return action.Name.ToLowerInvariant() switch
        {
            "click" => FrameAction(action, "frameClick", frame, min: 1, max: 1),
            "hover" => FrameAction(action, "frameHover", frame, min: 1, max: 1),
            "type" or "presssequentially" => FrameAction(action, "frameType", frame, min: 2, max: 2),
            "fill" => FrameAction(action, "frameFill", frame, min: 2, max: 2),
            "waitforelement" or "waitforselector" or "assertvisible" => FrameAction(action, "frameWaitForElement", frame, min: 1, max: 1),
            "asserttext" or "expecttext" or "tohavetext" or "tocontaintext" or "containstext" or "waitfortext" =>
                FrameTextAssertion(action, "frameAssertText", frame),
            "contains" => FrameContains(action, frame),
            "evaluate" => FrameAction(action, "frameEvaluate", frame, min: 1, max: 1),
            _ => action
        };
    }

    private static BrowserScriptAction FrameAction(BrowserScriptAction action, string name, string frame, int min, int max)
    {
        if (action.Arguments.Count < min || action.Arguments.Count > max)
        {
            return action;
        }

        return action with { Name = name, Arguments = [frame, .. action.Arguments] };
    }

    private static BrowserScriptAction FrameTextAssertion(BrowserScriptAction action, string name, string frame)
    {
        if (action.Arguments.Count is not 2)
        {
            return action;
        }

        return action with { Name = name, Arguments = [frame, .. action.Arguments] };
    }

    private static BrowserScriptAction FrameContains(BrowserScriptAction action, string frame) =>
        action.Arguments.Count switch
        {
            1 => action with { Name = "frameAssertText", Arguments = [frame, "body", action.Arguments[0]] },
            2 => action with { Name = "frameAssertText", Arguments = [frame, .. action.Arguments] },
            _ => action
        };
}
