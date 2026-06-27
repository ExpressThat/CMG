namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteSetViewport(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        action = NormalizeViewportAction(action);
        automationClient.SetViewport(remoteDebuggingUrl, GetViewportOptions(action));
        return [];
    }

    private static BrowserScriptAction NormalizeViewportAction(BrowserScriptAction action)
    {
        if (action.Arguments.Count is 0)
        {
            return action;
        }

        if (action.Arguments.Count is 2 &&
            !action.Options.ContainsKey("width") &&
            !action.Options.ContainsKey("height"))
        {
            var options = new Dictionary<string, string>(action.Options)
            {
                ["width"] = action.Arguments[0],
                ["height"] = action.Arguments[1]
            };
            return action with { Arguments = [], Options = options };
        }

        throw new ScriptExecutionException($"{action.Name} expects width=<pixels> height=<pixels> or '<width>' '<height>'.");
    }

    private static ViewportOptions GetViewportOptions(BrowserScriptAction action) =>
        new(
            GetIntOption(action, "width", required: true),
            GetIntOption(action, "height", required: true),
            GetDoubleOption(action, "deviceScaleFactor", 1),
            GetBoolOption(action, "isMobile"),
            GetBoolOption(action, "hasTouch"));
}
