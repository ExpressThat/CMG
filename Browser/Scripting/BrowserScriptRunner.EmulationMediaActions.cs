namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteEmulateMedia(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        ValidateMediaOption(action, "media", ["screen", "print"]);
        ValidateMediaOption(action, "colorScheme", ["light", "dark", "no-preference"]);
        ValidateMediaOption(action, "reducedMotion", ["reduce", "no-preference"]);
        ValidateMediaOption(action, "forcedColors", ["active", "none"]);
        ValidateMediaOption(action, "contrast", ["more", "less", "custom", "no-preference"]);
        if (action.Options.Count is 0)
        {
            throw new ScriptExecutionException("emulateMedia requires at least one media option.");
        }

        var script = BrowserEmulationScript.BuildMedia(action.Options);
        automationClient.AddInitScript(remoteDebuggingUrl, script);
        automationClient.Evaluate(remoteDebuggingUrl, script);
        return [$"MEDIA {action.LineNumber:000} {string.Join(' ', action.Options.Select(FormatOption))}"];
    }

    private static void ValidateMediaOption(BrowserScriptAction action, string name, IReadOnlyCollection<string> allowed)
    {
        if (!action.Options.TryGetValue(name, out var value) || allowed.Contains(value))
        {
            return;
        }

        throw new ScriptExecutionException($"{action.Name} option {name}= must be {string.Join(", ", allowed)}.");
    }

    private static string FormatOption(KeyValuePair<string, string> option) =>
        $"{option.Key}={option.Value}";
}
