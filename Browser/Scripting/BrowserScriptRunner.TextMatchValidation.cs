using System.Text.RegularExpressions;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static void ValidateTextMatchOptions(BrowserScriptAction action, string pattern)
    {
        if (action.Options.TryGetValue("ignoreCase", out var ignoreCase) && !bool.TryParse(ignoreCase, out _))
        {
            throw new ScriptExecutionException($"{action.Name} option ignoreCase= must be true or false.");
        }

        var match = EventTextMatchMode(action);
        if (match == "regex" && !string.IsNullOrEmpty(pattern))
        {
            ValidateTextRegex(action, pattern);
        }
    }

    private static string EventTextMatchMode(BrowserScriptAction action)
    {
        var match = TextMatchMode(action);
        return match == "matches" ? "regex" : match;
    }

    private static void ValidateTextRegex(BrowserScriptAction action, string pattern)
    {
        try
        {
            _ = new Regex(pattern);
        }
        catch (ArgumentException ex)
        {
            throw new ScriptExecutionException($"Invalid text regex '{pattern}': {ex.Message}");
        }
    }
}
