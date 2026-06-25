namespace CMG.Browser.Scripting;

public sealed partial class BrowserScriptRunner
{
    private static IReadOnlyList<string> ExecuteEmulate(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        ApplyViewport(remoteDebuggingUrl, automationClient, action);
        var script = BrowserEmulationScript.Build(action.Options);
        if (!string.IsNullOrWhiteSpace(script))
        {
            automationClient.Evaluate(remoteDebuggingUrl, script);
        }

        return [$"EMULATE {action.LineNumber:000} {string.Join(' ', action.Options.Keys)}".TrimEnd()];
    }

    private static IReadOnlyList<string> ExecuteGeolocationOrPermission(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        return action.Name.ToLowerInvariant() switch
        {
            "setgeolocation" => ExecuteSetGeolocation(remoteDebuggingUrl, automationClient, action),
            "grantpermissions" => ExecuteGrantPermissions(remoteDebuggingUrl, automationClient, action),
            "clearpermissions" => ExecuteClearPermissions(remoteDebuggingUrl, automationClient, action),
            _ => throw new ScriptExecutionException($"Unknown emulation action '{action.Name}'.")
        };
    }

    private static IReadOnlyList<string> ExecuteSetGeolocation(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        var (latitude, longitude) = GetCoordinates(action);
        var accuracy = action.Options.TryGetValue("accuracy", out var value) && double.TryParse(value, out var parsed)
            ? parsed
            : 1;
        automationClient.Evaluate(remoteDebuggingUrl, BrowserEmulationScript.BuildGeolocation(latitude, longitude, accuracy));
        return [$"GEOLOCATION {action.LineNumber:000} {latitude},{longitude} accuracy={accuracy}"];
    }

    private static IReadOnlyList<string> ExecuteGrantPermissions(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        var permissions = action.Arguments.Count > 0
            ? action.Arguments
            : SplitOption(action, "permissions");
        if (permissions.Count is 0)
        {
            throw new ScriptExecutionException("grantPermissions requires at least one permission name.");
        }

        automationClient.Evaluate(remoteDebuggingUrl, BrowserEmulationScript.BuildPermissions(permissions));
        return [$"PERMISSIONS {action.LineNumber:000} {string.Join(',', permissions)}"];
    }

    private static IReadOnlyList<string> ExecuteClearPermissions(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        RequireArgumentCount(action, 0, 0);
        automationClient.Evaluate(remoteDebuggingUrl, BrowserEmulationScript.BuildPermissions([]));
        return [$"PERMISSIONS_CLEARED {action.LineNumber:000}"];
    }

    private static void ApplyViewport(
        string remoteDebuggingUrl,
        IBrowserAutomationClient automationClient,
        BrowserScriptAction action)
    {
        var hasWidth = action.Options.ContainsKey("width");
        var hasHeight = action.Options.ContainsKey("height");
        if (hasWidth != hasHeight)
        {
            throw new ScriptExecutionException("emulate requires both width and height when overriding viewport.");
        }

        if (hasWidth)
        {
            automationClient.SetViewport(
                remoteDebuggingUrl,
                GetIntOption(action, "width", required: true),
                GetIntOption(action, "height", required: true));
        }
    }

    private static (double Latitude, double Longitude) GetCoordinates(BrowserScriptAction action)
    {
        if (action.Arguments.Count is 1)
        {
            return ParseCoordinates(action.Arguments[0]);
        }

        if (action.Options.TryGetValue("latitude", out var lat) &&
            action.Options.TryGetValue("longitude", out var lng) &&
            double.TryParse(lat, out var latitude) &&
            double.TryParse(lng, out var longitude))
        {
            return (latitude, longitude);
        }

        throw new ScriptExecutionException("setGeolocation requires '<latitude>,<longitude>' or latitude=<value> longitude=<value>.");
    }

    private static (double Latitude, double Longitude) ParseCoordinates(string value)
    {
        var parts = value.Split(',', StringSplitOptions.TrimEntries);
        if (parts.Length is 2 && double.TryParse(parts[0], out var latitude) && double.TryParse(parts[1], out var longitude))
        {
            return (latitude, longitude);
        }

        throw new ScriptExecutionException("setGeolocation requires '<latitude>,<longitude>' or latitude=<value> longitude=<value>.");
    }

    private static IReadOnlyList<string> SplitOption(BrowserScriptAction action, string name) =>
        action.Options.TryGetValue(name, out var value)
            ? value.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries)
            : [];
}
