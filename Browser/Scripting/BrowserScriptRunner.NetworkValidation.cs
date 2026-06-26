using System.Text.RegularExpressions;

namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static void ValidateRouteOptions(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("times", out var times) &&
            (!int.TryParse(times, out var parsed) || parsed <= 0))
        {
            throw new ScriptExecutionException($"{action.Name} option times= must be a positive integer.");
        }

        if (action.Options.TryGetValue("delay", out var delay) &&
            (!int.TryParse(delay, out var parsedDelay) || parsedDelay < 0))
        {
            throw new ScriptExecutionException($"{action.Name} option delay= must be a non-negative integer.");
        }

        ValidateNetworkUrlMatchOptions(action);
        ValidateRouteHeaderOptions(action);
    }

    private static void ValidateNetworkWaitOptions(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("timeout", out var timeout) &&
            (!int.TryParse(timeout, out var parsedTimeout) || parsedTimeout < 0))
        {
            throw new ScriptExecutionException($"{action.Name} option timeout= must be a non-negative integer.");
        }

        if (action.Options.TryGetValue("status", out var status) &&
            (!int.TryParse(status, out var parsedStatus) || parsedStatus < 100 || parsedStatus > 999))
        {
            throw new ScriptExecutionException($"{action.Name} option status= must be a numeric HTTP status.");
        }

        if (action.Options.TryGetValue("mocked", out var mocked) && !bool.TryParse(mocked, out _))
        {
            throw new ScriptExecutionException($"{action.Name} option mocked= must be true or false.");
        }

        if (action.Options.TryGetValue("headerValue", out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue) &&
            !action.Options.ContainsKey("header") &&
            !action.Options.ContainsKey("headerName"))
        {
            throw new ScriptExecutionException($"{action.Name} option headerValue= requires header= or headerName=.");
        }

        ValidateNetworkUrlMatchOptions(action);
    }

    private static void ValidateNetworkUrlMatchOptions(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("ignoreCase", out var ignoreCase) && !bool.TryParse(ignoreCase, out _))
        {
            throw new ScriptExecutionException($"{action.Name} option ignoreCase= must be true or false.");
        }

        var match = action.Options.GetValueOrDefault("match") ?? action.Options.GetValueOrDefault("mode");
        if (string.IsNullOrWhiteSpace(match))
        {
            return;
        }

        if (!IsNetworkMatchMode(match))
        {
            throw new ScriptExecutionException($"{action.Name} option match= must be contains, exact, or regex.");
        }

        if (match.Equals("regex", StringComparison.OrdinalIgnoreCase))
        {
            ValidateNetworkRegex(action);
        }
    }

    private static void ValidateRouteHeaderOptions(BrowserScriptAction action)
    {
        if (action.Options.TryGetValue("headerValue", out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue) &&
            !action.Options.ContainsKey("header") &&
            !action.Options.ContainsKey("headerName"))
        {
            throw new ScriptExecutionException($"{action.Name} option headerValue= requires header= or headerName=.");
        }

        ValidateRouteHeader(action, action.Options.GetValueOrDefault("header"), "header");
        foreach (var header in (action.Options.GetValueOrDefault("headers") ?? string.Empty).Split(';', StringSplitOptions.RemoveEmptyEntries))
        {
            ValidateRouteHeader(action, header, "headers");
        }
    }

    private static void ValidateRouteHeader(BrowserScriptAction action, string? header, string option)
    {
        if (string.IsNullOrWhiteSpace(header))
        {
            return;
        }

        if (header.IndexOf(':') <= 0)
        {
            throw new ScriptExecutionException($"{action.Name} option {option}= headers must be formatted as Name: value.");
        }
    }

    private static bool IsNetworkMatchMode(string match) =>
        match.Equals("contains", StringComparison.OrdinalIgnoreCase) ||
        match.Equals("exact", StringComparison.OrdinalIgnoreCase) ||
        match.Equals("regex", StringComparison.OrdinalIgnoreCase);

    private static void ValidateNetworkRegex(BrowserScriptAction action)
    {
        try
        {
            _ = new Regex(action.Arguments[0]);
        }
        catch (ArgumentException ex)
        {
            throw new ScriptExecutionException($"Invalid network regex '{action.Arguments[0]}': {ex.Message}");
        }
    }
}
