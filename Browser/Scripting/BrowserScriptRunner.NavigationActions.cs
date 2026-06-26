using System.Text.RegularExpressions;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static BrowserScriptAction NormalizeNavigationAlias(BrowserScriptAction action) =>
        action.Name.ToLowerInvariant() switch
        {
            "tohaveurl" => action with { Name = "expectUrl" },
            "tohavetitle" => action with { Name = "expectTitle" },
            _ => action
        };

    private static IReadOnlyList<string> ExecuteNavigationAction(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "reload" => Reload(remoteDebuggingUrl, automationClient, action),
            "goback" => MoveHistory(remoteDebuggingUrl, automationClient, action, "back", "BACK"),
            "goforward" => MoveHistory(remoteDebuggingUrl, automationClient, action, "forward", "FORWARD"),
            "waitforurl" => WaitForUrl(remoteDebuggingUrl, automationClient, action),
            "waitfortitle" => WaitForTitle(remoteDebuggingUrl, automationClient, action),
            "expecturl" => ExpectUrl(remoteDebuggingUrl, automationClient, action),
            "expecttitle" => ExpectTitle(remoteDebuggingUrl, automationClient, action),
            "waitforloadstate" => WaitForLoadState(remoteDebuggingUrl, automationClient, action),
            "waitfornavigation" => WaitForNavigation(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown navigation action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> Reload(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        var currentUrl = automationClient.Evaluate(remoteDebuggingUrl, "location.href");
        automationClient.Evaluate(remoteDebuggingUrl, "location.reload(); location.href");
        Thread.Sleep(100);
        return [$"RELOADED {action.LineNumber:000} {currentUrl}"];
    }

    private static IReadOnlyList<string> MoveHistory(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action,
        string direction,
        string output)
    {
        RequireArgumentCount(action, 0, 0);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var url = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.History(direction, timeout));
        return [$"{output} {action.LineNumber:000} {url}"];
    }

    private static IReadOnlyList<string> WaitForUrl(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var url = PollPageValue(
            remoteDebuggingUrl,
            automationClient,
            "location.href",
            action.Arguments[0],
            timeout,
            "URL",
            "URL",
            action);
        return [$"URL {action.LineNumber:000} {url}"];
    }

    private static IReadOnlyList<string> ExpectUrl(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var url = automationClient.Evaluate(remoteDebuggingUrl, "location.href");
        if (!NavigationMatches(url, action.Arguments[0], action))
        {
            throw new ScriptExecutionException(NavigationFailure("URL", action.Arguments[0], url, action));
        }

        return [$"URL {action.LineNumber:000} {url}"];
    }

    private static IReadOnlyList<string> WaitForTitle(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var timeout = GetIntOption(action, "timeout", 5_000);
        var title = PollPageValue(
            remoteDebuggingUrl,
            automationClient,
            "document.title",
            action.Arguments[0],
            timeout,
            "Title",
            "title",
            action);
        return [$"TITLE {action.LineNumber:000} {title}"];
    }

    private static string PollPageValue(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        string expression,
        string expected,
        int timeout,
        string label,
        string lastValueLabel,
        BrowserScriptAction action)
    {
        var deadline = DateTimeOffset.UtcNow.AddMilliseconds(timeout);
        var actual = string.Empty;
        do
        {
            actual = automationClient.Evaluate(remoteDebuggingUrl, expression);
            if (NavigationMatches(actual, expected, action))
            {
                return actual;
            }

            Thread.Sleep(50);
        }
        while (DateTimeOffset.UtcNow < deadline);

        throw new ScriptExecutionException(
            $"{label} did not match {expected} within {timeout}ms using {NavigationMatchMode(action)} match. Last {lastValueLabel}: {actual}");
    }

    private static IReadOnlyList<string> ExpectTitle(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        RequireArgumentCount(action, 1, 1);
        var title = automationClient.Evaluate(remoteDebuggingUrl, "document.title");
        if (!NavigationMatches(title, action.Arguments[0], action))
        {
            throw new ScriptExecutionException(NavigationFailure("title", action.Arguments[0], title, action));
        }

        return [$"TITLE {action.LineNumber:000} {title}"];
    }

    private static bool NavigationMatches(string actual, string expected, BrowserScriptAction action)
    {
        var comparison = GetBoolOption(action, "ignoreCase") ? StringComparison.OrdinalIgnoreCase : StringComparison.Ordinal;
        return NavigationMatchMode(action) switch
        {
            "exact" => string.Equals(actual, expected, comparison),
            "regex" or "matches" => RegexMatchesNavigation(actual, expected, action),
            _ => actual.Contains(expected, comparison)
        };
    }

    private static string NavigationMatchMode(BrowserScriptAction action)
    {
        var mode = (action.Options.GetValueOrDefault("match") ?? action.Options.GetValueOrDefault("mode") ?? "contains").ToLowerInvariant();
        if (mode is "contains" or "exact" or "regex" or "matches")
        {
            return mode;
        }

        throw new ScriptExecutionException($"{action.Name} option match= must be contains, exact, or regex.");
    }

    private static bool RegexMatchesNavigation(string actual, string pattern, BrowserScriptAction action)
    {
        try
        {
            return Regex.IsMatch(actual, pattern, GetBoolOption(action, "ignoreCase") ? RegexOptions.IgnoreCase : RegexOptions.None);
        }
        catch (ArgumentException exception)
        {
            throw new ScriptExecutionException($"Invalid navigation regex '{pattern}': {exception.Message}");
        }
    }

    private static string NavigationFailure(string label, string expected, string actual, BrowserScriptAction action) =>
        $"Expected {label} to match {expected} using {NavigationMatchMode(action)} match, got {actual}.";

    private static IReadOnlyList<string> WaitForLoadState(string remoteDebuggingUrl, IBrowserAutomationClient automationClient, BrowserScriptAction action)
    {
        if (action.Arguments.Count > 1)
        {
            throw new ScriptExecutionException("waitForLoadState expects at most one state argument.");
        }

        var state = action.Arguments.Count > 0 ? action.Arguments[0] : "load";
        if (state is not ("loading" or "interactive" or "complete" or "load" or "networkidle"))
        {
            throw new ScriptExecutionException("waitForLoadState expects loading, interactive, complete, load, or networkidle.");
        }

        var timeout = GetIntOption(action, "timeout", 5_000);
        var actual = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.WaitForLoadState(state, timeout));
        return [$"LOAD_STATE {action.LineNumber:000} {actual}"];
    }

    private static IReadOnlyList<string> WaitForNavigation(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        if (action.Arguments.Count > 1)
        {
            throw new ScriptExecutionException("waitForNavigation expects at most one URL substring argument.");
        }

        var waitUntil = action.Options.TryGetValue("waitUntil", out var waitUntilValue)
            ? waitUntilValue
            : action.Options.GetValueOrDefault("state", "load");
        if (waitUntil is not ("load" or "domcontentloaded" or "networkidle" or "commit"))
        {
            throw new ScriptExecutionException("waitForNavigation waitUntil= expects load, domcontentloaded, networkidle, or commit.");
        }

        var timeout = GetIntOption(action, "timeout", 5_000);
        var expected = action.Arguments.Count > 0 ? action.Arguments[0] : string.Empty;
        var json = automationClient.Evaluate(remoteDebuggingUrl, BrowserNavigationScripts.WaitForNavigation(expected, waitUntil, timeout));
        return [$"NAVIGATION {action.LineNumber:000} {json}"];
    }
}
