namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteClockAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "clock" => RunClock(remoteDebuggingUrl, automationClient, action),
            "tick" => RunTick(remoteDebuggingUrl, automationClient, action),
            "restoreclock" => RunRestoreClock(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown clock action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> RunClock(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var now = GetLongOption(action, "now", DateTimeOffset.UtcNow.ToUnixTimeMilliseconds());
        var result = automationClient.Evaluate(remoteDebuggingUrl, BrowserClockScripts.Install(now));
        return [$"CLOCK {action.LineNumber:000} {result}"];
    }

    private static IReadOnlyList<string> RunTick(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var milliseconds = ParseNonNegativeLong(action.Arguments[0], "tick");
        var result = automationClient.Evaluate(remoteDebuggingUrl, BrowserClockScripts.Tick(milliseconds));
        return [$"TICK {action.LineNumber:000} {milliseconds} now={result}"];
    }

    private static IReadOnlyList<string> RunRestoreClock(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserClockScripts.Restore());
        return [$"CLOCK_RESTORED {action.LineNumber:000}"];
    }

    private static long GetLongOption(BrowserScriptAction action, string name, long defaultValue) =>
        action.Options.TryGetValue(name, out var value) ? ParseNonNegativeLong(value, name) : defaultValue;

    private static long ParseNonNegativeLong(string value, string name)
    {
        if (!long.TryParse(value, out var number) || number < 0)
        {
            throw new ScriptExecutionException($"'{name}' must be a positive whole number.");
        }

        return number;
    }
}
