namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteCoverageAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "startcoverage" => StartCoverage(remoteDebuggingUrl, automationClient, action),
            "stopcoverage" => StopCoverage(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown coverage action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> StartCoverage(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var options = new CoverageOptions(
            GetCoverageBool(action, "js", defaultValue: true),
            GetCoverageBool(action, "css", defaultValue: true));
        automationClient.StartCoverage(remoteDebuggingUrl, options);
        return [$"COVERAGE_STARTED {action.LineNumber:000} js={options.JavaScript.ToString().ToLowerInvariant()} css={options.Css.ToString().ToLowerInvariant()}"];
    }

    private static IReadOnlyList<string> StopCoverage(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var json = automationClient.StopCoverage(remoteDebuggingUrl);
        if (action.Options.TryGetValue("path", out var path) && !string.IsNullOrWhiteSpace(path))
        {
            var fullPath = Path.GetFullPath(path);
            Directory.CreateDirectory(Path.GetDirectoryName(fullPath) ?? Directory.GetCurrentDirectory());
            File.WriteAllText(fullPath, json);
            return [$"COVERAGE {action.LineNumber:000} {fullPath}"];
        }

        return [$"COVERAGE {action.LineNumber:000} {json}"];
    }

    private static bool GetCoverageBool(BrowserScriptAction action, string name, bool defaultValue)
    {
        if (!action.Options.TryGetValue(name, out var value))
        {
            return defaultValue;
        }

        return bool.TryParse(value, out var parsed)
            ? parsed
            : throw new ScriptExecutionException($"{action.Name} option {name}= must be true or false.");
    }
}
