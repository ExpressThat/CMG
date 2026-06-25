namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteWaitAlias(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        if (int.TryParse(action.Arguments[0], out _))
        {
            return ExecuteDelay(action with { Name = "delay" });
        }

        return ExecuteWaitForElement(remoteDebuggingUrl, automationClient, action with { Name = "waitForElement" });
    }
}
